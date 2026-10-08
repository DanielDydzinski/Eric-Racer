using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace EricRacer.Net
{
    public readonly struct DiscoveredHost
    {
        public readonly string Address;
        public readonly ushort Port;
        public readonly string HostName;
        public readonly int Players;
        public readonly int MaxPlayers;
        public readonly float LastSeen;

        public DiscoveredHost(string address, ushort port, string hostName, int players, int maxPlayers, float lastSeen)
        {
            Address = address;
            Port = port;
            HostName = hostName;
            Players = players;
            MaxPlayers = maxPlayers;
            LastSeen = lastSeen;
        }
    }

    /// <summary>
    /// Finds games on the home network with UDP broadcasts, so nobody has to type an IP address.
    /// The host advertises once a second; menus listen and get <see cref="HostsChanged"/>.
    /// Runs on the main thread by polling the socket, which is plenty for one packet a second.
    /// </summary>
    public class LanDiscovery : MonoBehaviour
    {
        [SerializeField] private ushort discoveryPort = 47777;
        [SerializeField] private float advertiseInterval = 1f;
        [SerializeField] private float hostTimeout = 3.5f;

        private const string k_Magic = "ERICRACER1";

        public IReadOnlyList<DiscoveredHost> Hosts => m_Hosts;
        public event Action HostsChanged;

        private readonly List<DiscoveredHost> m_Hosts = new List<DiscoveredHost>();
        private readonly List<IPEndPoint> m_BroadcastTargets = new List<IPEndPoint>();
        private UdpClient m_Sender;
        private UdpClient m_Listener;
        private Func<string> m_BuildAdvert;
        private float m_NextAdvertTime;

        public bool IsAdvertising => m_Sender != null;
        public bool IsListening => m_Listener != null;

        public void StartAdvertising(string hostName, ushort gamePort, Func<int> playerCount, int maxPlayers)
        {
            StopAdvertising();
            string safeName = hostName.Replace('|', ' ');
            m_BuildAdvert = () => $"{k_Magic}|{gamePort}|{playerCount()}|{maxPlayers}|{safeName}";
            m_Sender = new UdpClient { EnableBroadcast = true };
            CollectBroadcastTargets();
            m_NextAdvertTime = 0f;
        }

        public void StopAdvertising()
        {
            m_Sender?.Close();
            m_Sender = null;
        }

        public void StartListening()
        {
            if (m_Listener != null)
                return;
            try
            {
                // Reuse lets several game windows on one PC listen at the same time while testing.
                var listener = new UdpClient();
                listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                listener.Client.Bind(new IPEndPoint(IPAddress.Any, discoveryPort));
                m_Listener = listener;
            }
            catch (SocketException e)
            {
                Debug.LogWarning($"[LanDiscovery] Can't listen on port {discoveryPort}: {e.Message}");
            }
        }

        public void StopListening()
        {
            m_Listener?.Close();
            m_Listener = null;
            if (m_Hosts.Count > 0)
            {
                m_Hosts.Clear();
                HostsChanged?.Invoke();
            }
        }

        void Update()
        {
            if (m_Sender != null && Time.unscaledTime >= m_NextAdvertTime)
            {
                m_NextAdvertTime = Time.unscaledTime + advertiseInterval;
                SendAdvert();
            }

            if (m_Listener != null)
            {
                ReceiveAdverts();
                ExpireStaleHosts();
            }
        }

        void OnDestroy()
        {
            StopAdvertising();
            StopListening();
        }

        void SendAdvert()
        {
            byte[] data = Encoding.UTF8.GetBytes(m_BuildAdvert());
            foreach (var target in m_BroadcastTargets)
            {
                try { m_Sender.Send(data, data.Length, target); }
                catch (SocketException) { /* An adapter that can't broadcast (VPN, disconnected) isn't a problem. */ }
            }
        }

        void ReceiveAdverts()
        {
            try
            {
                while (m_Listener.Available > 0)
                {
                    IPEndPoint from = null;
                    byte[] data = m_Listener.Receive(ref from);
                    if (TryParse(Encoding.UTF8.GetString(data), from.Address.ToString(), out var host))
                        Upsert(host);
                }
            }
            catch (SocketException e)
            {
                Debug.LogWarning($"[LanDiscovery] Receive failed: {e.Message}");
            }
        }

        bool TryParse(string message, string address, out DiscoveredHost host)
        {
            host = default;
            string[] parts = message.Split(new[] { '|' }, 5);
            if (parts.Length != 5 || parts[0] != k_Magic
                || !ushort.TryParse(parts[1], out ushort port)
                || !int.TryParse(parts[2], out int players)
                || !int.TryParse(parts[3], out int max))
                return false;

            host = new DiscoveredHost(address, port, parts[4], players, max, Time.unscaledTime);
            return true;
        }

        void Upsert(DiscoveredHost host)
        {
            for (int i = 0; i < m_Hosts.Count; i++)
            {
                if (m_Hosts[i].Address != host.Address || m_Hosts[i].Port != host.Port)
                    continue;
                bool changed = m_Hosts[i].Players != host.Players || m_Hosts[i].HostName != host.HostName;
                m_Hosts[i] = host;
                if (changed)
                    HostsChanged?.Invoke();
                return;
            }
            m_Hosts.Add(host);
            HostsChanged?.Invoke();
        }

        void ExpireStaleHosts()
        {
            int removed = m_Hosts.RemoveAll(h => Time.unscaledTime - h.LastSeen > hostTimeout);
            if (removed > 0)
                HostsChanged?.Invoke();
        }

        void CollectBroadcastTargets()
        {
            m_BroadcastTargets.Clear();
            m_BroadcastTargets.Add(new IPEndPoint(IPAddress.Broadcast, discoveryPort));

            // 255.255.255.255 can leave through the wrong adapter (VPN, Hyper-V), so also aim at each LAN's own broadcast address.
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;
                foreach (var unicast in nic.GetIPProperties().UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily != AddressFamily.InterNetwork || unicast.IPv4Mask == null)
                        continue;
                    byte[] ip = unicast.Address.GetAddressBytes();
                    byte[] mask = unicast.IPv4Mask.GetAddressBytes();
                    for (int i = 0; i < 4; i++)
                        ip[i] = (byte)(ip[i] | ~mask[i]);
                    m_BroadcastTargets.Add(new IPEndPoint(new IPAddress(ip), discoveryPort));
                }
            }
        }

        /// <summary>This PC's addresses on the home network, shown to the host in case someone needs to type one.</summary>
        public static List<string> GetLocalAddresses()
        {
            var result = new List<string>();
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up || nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;
                foreach (var unicast in nic.GetIPProperties().UnicastAddresses)
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                        result.Add(unicast.Address.ToString());
            }
            return result;
        }
    }
}
