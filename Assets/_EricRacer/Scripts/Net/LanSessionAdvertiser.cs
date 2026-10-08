using EricRacer.Core;
using UnityEngine;

namespace EricRacer.Net
{
    /// <summary>Advertises our game while hosting and looks for other games while offline.</summary>
    public class LanSessionAdvertiser : MonoBehaviour
    {
        [SerializeField] private ConnectionManager connection;
        [SerializeField] private LanDiscovery discovery;

        void OnEnable() => connection.StateChanged += Apply;
        void OnDisable() => connection.StateChanged -= Apply;
        void Start() => Apply(connection.State);

        void Apply(ConnectionState state)
        {
            if (state == ConnectionState.Hosting)
                discovery.StartAdvertising($"{LocalPlayerProfile.Name}'s race", connection.Port, () => connection.PlayerCount, connection.MaxPlayers);
            else
                discovery.StopAdvertising();

            if (state == ConnectionState.Offline)
                discovery.StartListening();
            else
                discovery.StopListening();
        }
    }
}
