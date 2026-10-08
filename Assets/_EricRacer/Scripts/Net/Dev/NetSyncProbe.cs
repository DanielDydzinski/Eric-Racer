using System.Text;
using EricRacer.Core;
using EricRacer.Kart;
using Unity.Netcode;
using UnityEngine;

namespace EricRacer.Net.Dev
{
    /// <summary>
    /// Test-only (<c>-netlog</c>): every couple of seconds logs where this PC sees every kart,
    /// so the logs of several windows can be compared to check they agree.
    /// </summary>
    public class NetSyncProbe : MonoBehaviour
    {
        [SerializeField] private float interval = 2f;

        private float m_Next;
        private readonly StringBuilder m_Line = new StringBuilder();

        void Awake()
        {
            if (!LaunchOptions.NetLog)
                enabled = false;
        }

        void Update()
        {
            if (Time.unscaledTime < m_Next || NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
                return;
            m_Next = Time.unscaledTime + interval;

            m_Line.Clear();
            m_Line.Append($"[NetSync] me={NetworkManager.Singleton.LocalClientId} t={NetworkManager.Singleton.ServerTime.Time:F1}");
            foreach (var kart in FindObjectsByType<NetworkKart>(FindObjectsSortMode.None))
            {
                Vector3 p = kart.transform.position;
                m_Line.Append($" | kart{kart.OwnerClientId}{(kart.IsOwner ? "*" : "")} ({p.x:F1},{p.y:F1},{p.z:F1})");
            }
            Debug.Log(m_Line.ToString());
        }
    }
}
