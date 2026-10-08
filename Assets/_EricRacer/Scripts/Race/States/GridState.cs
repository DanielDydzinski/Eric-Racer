using EricRacer.Core;
using UnityEngine;

namespace EricRacer.Race.States
{
    /// <summary>Karts sit on the grid until every PC has loaded the track (or it takes too long).</summary>
    internal class GridState : IState
    {
        private readonly RaceManager m_Race;
        private float m_EnteredAt;

        public GridState(RaceManager race) => m_Race = race;

        public void Enter() => m_EnteredAt = Time.realtimeSinceStartup;

        public void Tick()
        {
            float waited = Time.realtimeSinceStartup - m_EnteredAt;
            bool ready = m_Race.AllClientsLoaded && m_Race.EveryoneHasAKart && waited >= m_Race.Settings.MinGridSeconds;
            if (ready || waited >= m_Race.Settings.MaxGridSeconds)
                m_Race.ChangeState(RaceStateId.Countdown);
        }

        public void Exit() { }
    }
}
