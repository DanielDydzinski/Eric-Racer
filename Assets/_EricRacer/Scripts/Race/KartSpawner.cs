using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace EricRacer.Race
{
    /// <summary>Server-side: gives every connected player a kart on a free grid slot, owned by that player.</summary>
    public class KartSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject kartPrefab;
        [SerializeField] private Transform[] gridSlots;

        public event Action<NetworkObject, ulong> KartSpawned;

        private readonly Dictionary<ulong, int> m_SlotByClient = new Dictionary<ulong, int>();
        private NetworkManager m_Network;

        /// <summary>Spawns karts for everyone connected now and for anyone who joins later, until <see cref="End"/>.</summary>
        public void Begin(NetworkManager network)
        {
            m_Network = network;
            m_Network.OnClientConnectedCallback += SpawnFor;
            m_Network.OnClientDisconnectCallback += FreeSlot;
            foreach (ulong clientId in m_Network.ConnectedClientsIds)
                SpawnFor(clientId);
        }

        public void End()
        {
            if (m_Network == null)
                return;
            m_Network.OnClientConnectedCallback -= SpawnFor;
            m_Network.OnClientDisconnectCallback -= FreeSlot;
            m_Network = null;
        }

        void OnDestroy() => End();

        void SpawnFor(ulong clientId)
        {
            if (m_SlotByClient.ContainsKey(clientId))
                return;

            int slot = FirstFreeSlot();
            if (slot < 0)
            {
                Debug.LogWarning($"[KartSpawner] No grid slot left for client {clientId}.");
                return;
            }

            m_SlotByClient[clientId] = slot;
            Transform spot = gridSlots[slot];
            var kart = kartPrefab.InstantiateAndSpawn(m_Network, clientId, destroyWithScene: true, position: spot.position, rotation: spot.rotation);
            KartSpawned?.Invoke(kart, clientId);
        }

        // The kart itself is despawned by Netcode when its owner disconnects.
        void FreeSlot(ulong clientId) => m_SlotByClient.Remove(clientId);

        int FirstFreeSlot()
        {
            for (int slot = 0; slot < gridSlots.Length; slot++)
                if (!m_SlotByClient.ContainsValue(slot))
                    return slot;
            return -1;
        }
    }
}
