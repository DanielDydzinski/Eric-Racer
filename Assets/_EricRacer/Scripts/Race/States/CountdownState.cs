using EricRacer.Core;

namespace EricRacer.Race.States
{
    /// <summary>3-2-1: every PC counts down to the same synced server time.</summary>
    internal class CountdownState : IState
    {
        private readonly RaceManager m_Race;

        public CountdownState(RaceManager race) => m_Race = race;

        public void Enter() => m_Race.BeginCountdown();

        public void Tick()
        {
            if (m_Race.Now >= m_Race.PhaseEndsAt)
                m_Race.ChangeState(RaceStateId.Racing);
        }

        public void Exit() { }
    }
}
