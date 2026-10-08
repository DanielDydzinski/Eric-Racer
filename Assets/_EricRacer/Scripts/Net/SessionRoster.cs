using System;
using System.Collections.Generic;
using UnityEngine;

namespace EricRacer.Net
{
    /// <summary>Runtime set of everyone in the game, on every PC, ordered by join order (client id).</summary>
    [CreateAssetMenu(menuName = "Eric Racer/Session Roster")]
    public class SessionRoster : ScriptableObject
    {
        private readonly List<PlayerSession> m_Players = new List<PlayerSession>();

        public IReadOnlyList<PlayerSession> Players => m_Players;
        public event Action Changed;

        public PlayerSession Local
        {
            get
            {
                foreach (var player in m_Players)
                    if (player.IsOwner)
                        return player;
                return null;
            }
        }

        public PlayerSession Find(ulong clientId)
        {
            foreach (var player in m_Players)
                if (player.OwnerClientId == clientId)
                    return player;
            return null;
        }

        public bool AllReady
        {
            get
            {
                if (m_Players.Count == 0)
                    return false;
                foreach (var player in m_Players)
                    if (!player.IsReady)
                        return false;
                return true;
            }
        }

        public void Add(PlayerSession player)
        {
            if (m_Players.Contains(player))
                return;
            m_Players.Add(player);
            m_Players.Sort((a, b) => a.OwnerClientId.CompareTo(b.OwnerClientId));
            Changed?.Invoke();
        }

        public void Remove(PlayerSession player)
        {
            if (m_Players.Remove(player))
                Changed?.Invoke();
        }

        void OnDisable() => m_Players.Clear();
    }
}
