using EricRacer.Core;
using UnityEngine;

namespace EricRacer.Race.States
{
    /// <summary>GO! Keeps positions fresh and ends the race when everyone is home, or a while after the winner.</summary>
    internal class RacingState : IState
    {
        private readonly RaceManager m_Race;

        public RacingState(RaceManager race) => m_Race = race;

        public void Enter() => m_Race.BeginRace();

        public void Tick()
        {
            m_Race.UpdateRanking();

            bool winnerWaitOver = m_Race.FirstFinishRealtime >= 0f
                && Time.realtimeSinceStartup - m_Race.FirstFinishRealtime >= m_Race.Settings.FinishTimeoutSeconds;
            if (m_Race.EveryoneFinished || winnerWaitOver)
                m_Race.ChangeState(RaceStateId.Results);
        }

        public void Exit() { }
    }
}
