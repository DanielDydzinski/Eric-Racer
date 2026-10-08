using System;
using System.Collections.Generic;
using UnityEngine;

namespace EricRacer.Race
{
    /// <summary>
    /// Runtime set of everyone racing, kept on every PC. Karts add themselves on spawn and remove themselves on despawn,
    /// so the HUD, results and race manager can list racers without searching the scene.
    /// </summary>
    [CreateAssetMenu(menuName = "Eric Racer/Race Roster")]
    public class RaceRoster : ScriptableObject
    {
        private readonly List<RaceProgress> m_Racers = new List<RaceProgress>();

        public IReadOnlyList<RaceProgress> Racers => m_Racers;
        public event Action Changed;

        public void Add(RaceProgress racer)
        {
            if (m_Racers.Contains(racer))
                return;
            m_Racers.Add(racer);
            Changed?.Invoke();
        }

        public void Remove(RaceProgress racer)
        {
            if (m_Racers.Remove(racer))
                Changed?.Invoke();
        }

        void OnDisable() => m_Racers.Clear();
    }
}
