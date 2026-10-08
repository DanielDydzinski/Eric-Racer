using EricRacer.Core;

namespace EricRacer.Race.States
{
    /// <summary>Final standings are frozen; the host decides when to race again.</summary>
    internal class ResultsState : IState
    {
        private readonly RaceManager m_Race;

        public ResultsState(RaceManager race) => m_Race = race;

        public void Enter() => m_Race.UpdateRanking(force: true);
        public void Tick() { }
        public void Exit() { }
    }
}
