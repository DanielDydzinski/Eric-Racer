using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace EricRacer.Net
{
    /// <summary>
    /// Owns starting/stopping a session (host or client) and gatekeeps who may join.
    /// A small state machine: every transition goes through <see cref="SetState"/> and is broadcast via <see cref="StateChanged"/>.
    /// </summary>
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private ushort port = 7777;
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private int tickRate = 60;
        [Tooltip("How long a silent player (crashed laptop, Wi-Fi gone) stays in the game. Long enough to survive a slow scene load.")]
        [SerializeField] private int disconnectTimeoutMs = 10000;

        public ConnectionState State { get; private set; } = ConnectionState.Offline;
        public string LastDisconnectReason { get; private set; }
        public ushort Port => port;
        public int MaxPlayers => maxPlayers;
        public int PlayerCount => m_Network.IsServer ? m_Network.ConnectedClientsIds.Count : 0;

        /// <summary>Turned off by the race once it starts so nobody drops in mid-race.</summary>
        public bool AcceptingNewPlayers { get; set; } = true;

        public event Action<ConnectionState> StateChanged;

        private NetworkManager m_Network;
        private UnityTransport m_Transport;

        void Awake()
        {
            m_Network = GetComponent<NetworkManager>();
            m_Transport = GetComponent<UnityTransport>();
            m_Network.NetworkConfig.ConnectionApproval = true;
            m_Network.NetworkConfig.TickRate = (uint)tickRate;
            m_Transport.DisconnectTimeoutMS = disconnectTimeoutMs;
            m_Network.ConnectionApprovalCallback = ApproveConnection;
        }

        void OnEnable()
        {
            m_Network.OnServerStarted += OnServerStarted;
            m_Network.OnClientConnectedCallback += OnClientConnected;
            m_Network.OnClientDisconnectCallback += OnClientDisconnected;
            m_Network.OnClientStopped += OnClientStopped;
            m_Network.OnTransportFailure += OnTransportFailure;
        }

        void OnDisable()
        {
            if (m_Network == null)
                return;
            m_Network.OnServerStarted -= OnServerStarted;
            m_Network.OnClientConnectedCallback -= OnClientConnected;
            m_Network.OnClientDisconnectCallback -= OnClientDisconnected;
            m_Network.OnClientStopped -= OnClientStopped;
            m_Network.OnTransportFailure -= OnTransportFailure;
        }

        public bool StartHost()
        {
            if (State != ConnectionState.Offline)
                return false;

            LastDisconnectReason = null;
            AcceptingNewPlayers = true;
            // Listen on every network card so laptops on the Wi-Fi can reach us.
            m_Transport.SetConnectionData("127.0.0.1", port, "0.0.0.0");
            SetState(ConnectionState.StartingHost);
            if (!m_Network.StartHost())
            {
                LastDisconnectReason = "Couldn't start the game. Is another copy already hosting?";
                SetState(ConnectionState.Offline);
                return false;
            }
            return true;
        }

        public bool StartClient(string address)
        {
            if (State != ConnectionState.Offline || string.IsNullOrWhiteSpace(address))
                return false;

            LastDisconnectReason = null;
            m_Transport.SetConnectionData(address.Trim(), port);
            SetState(ConnectionState.Connecting);
            if (!m_Network.StartClient())
            {
                LastDisconnectReason = "Couldn't connect.";
                SetState(ConnectionState.Offline);
                return false;
            }
            return true;
        }

        public void Leave()
        {
            if (State == ConnectionState.Offline || State == ConnectionState.ShuttingDown)
                return;
            SetState(ConnectionState.ShuttingDown);
            m_Network.Shutdown();
        }

        void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.CreatePlayerObject = false;

            if (request.ClientNetworkId == NetworkManager.ServerClientId)
            {
                response.Approved = true;
                return;
            }

            if (m_Network.ConnectedClientsIds.Count >= maxPlayers)
            {
                response.Approved = false;
                response.Reason = "The game is full.";
            }
            else if (!AcceptingNewPlayers)
            {
                response.Approved = false;
                response.Reason = "The race has already started. Join the next one!";
            }
            else
            {
                response.Approved = true;
            }
        }

        void OnServerStarted()
        {
            if (m_Network.IsHost)
                SetState(ConnectionState.Hosting);
        }

        void OnClientConnected(ulong clientId)
        {
            if (!m_Network.IsServer && clientId == m_Network.LocalClientId)
                SetState(ConnectionState.Connected);
        }

        void OnClientDisconnected(ulong clientId)
        {
            if (m_Network.IsServer || clientId != m_Network.LocalClientId)
                return;
            LastDisconnectReason = string.IsNullOrEmpty(m_Network.DisconnectReason)
                ? (State == ConnectionState.Connecting ? "Couldn't find that game." : "The host left the game.")
                : m_Network.DisconnectReason;
        }

        void OnClientStopped(bool wasHost) => SetState(ConnectionState.Offline);

        void OnTransportFailure()
        {
            LastDisconnectReason = "Network problem. Check the Wi-Fi.";
            SetState(ConnectionState.Offline);
        }

        void SetState(ConnectionState next)
        {
            if (State == next)
                return;
            State = next;
            Debug.Log($"[Connection] {next}{(string.IsNullOrEmpty(LastDisconnectReason) ? "" : $" ({LastDisconnectReason})")}");
            StateChanged?.Invoke(next);
        }
    }
}
