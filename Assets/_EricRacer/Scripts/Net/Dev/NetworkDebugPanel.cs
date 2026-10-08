using EricRacer.Core;
using UnityEngine;

namespace EricRacer.Net.Dev
{
    /// <summary>
    /// Bare-bones Host/Join screen for testing networking until the real title screen and lobby exist (Phase 4).
    /// </summary>
    public class NetworkDebugPanel : MonoBehaviour
    {
        [SerializeField] private ConnectionManager connection;
        [SerializeField] private LanDiscovery discovery;
        [SerializeField] private float scale = 1.5f;

        private string m_ManualAddress = "127.0.0.1";
        private string m_Name;
        private Vector2 m_Scroll;

        void Start() => m_Name = LocalPlayerProfile.Name;

        void OnGUI()
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            // Full panel in the menu; a small strip in the bottom-left corner once in a game, clear of the race HUD.
            float height = Screen.height / scale;
            Rect area = connection.State == ConnectionState.Offline
                ? new Rect(10, 10, 340, height - 20)
                : new Rect(10, height - 130, 340, 120);
            GUILayout.BeginArea(area, GUI.skin.box);

            GUILayout.Label($"<b>Eric Racer</b> · {connection.State}", new GUIStyle(GUI.skin.label) { richText = true });

            switch (connection.State)
            {
                case ConnectionState.Offline: DrawOffline(); break;
                case ConnectionState.Hosting: DrawHosting(); break;
                case ConnectionState.Connected: DrawConnected(); break;
                default:
                    GUILayout.Label("Working on it...");
                    if (GUILayout.Button("Cancel")) connection.Leave();
                    break;
            }

            GUILayout.EndArea();
        }

        void DrawOffline()
        {
            if (!string.IsNullOrEmpty(connection.LastDisconnectReason))
                GUILayout.Label(connection.LastDisconnectReason);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", GUILayout.Width(50));
            string edited = GUILayout.TextField(m_Name, LocalPlayerProfile.MaxNameLength);
            if (edited != m_Name)
            {
                m_Name = edited;
                LocalPlayerProfile.Name = edited;
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Host a game", GUILayout.Height(40)))
                connection.StartHost();

            GUILayout.Space(8);
            GUILayout.Label("Games on this Wi-Fi:");
            m_Scroll = GUILayout.BeginScrollView(m_Scroll, GUILayout.Height(140));
            if (discovery.Hosts.Count == 0)
                GUILayout.Label("  (looking...)");
            foreach (var host in discovery.Hosts)
            {
                if (GUILayout.Button($"Join {host.HostName}  ({host.Players}/{host.MaxPlayers})", GUILayout.Height(32)))
                    connection.StartClient(host.Address);
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            m_ManualAddress = GUILayout.TextField(m_ManualAddress);
            if (GUILayout.Button("Join IP", GUILayout.Width(80)))
                connection.StartClient(m_ManualAddress);
            GUILayout.EndHorizontal();
        }

        void DrawHosting()
        {
            GUILayout.Label($"Players: {connection.PlayerCount}/{connection.MaxPlayers}");
            GUILayout.Label("This PC: " + string.Join(", ", LanDiscovery.GetLocalAddresses()));
            if (GUILayout.Button("End game", GUILayout.Height(32)))
                connection.Leave();
        }

        void DrawConnected()
        {
            if (GUILayout.Button("Leave game", GUILayout.Height(32)))
                connection.Leave();
        }
    }
}
