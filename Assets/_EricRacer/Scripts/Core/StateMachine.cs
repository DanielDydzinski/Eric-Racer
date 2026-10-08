using System;
using System.Collections.Generic;

namespace EricRacer.Core
{
    public interface IState
    {
        void Enter();
        void Tick();
        void Exit();
    }

    /// <summary>Minimal state machine: states are small classes, identified by an enum so the current one can be synced.</summary>
    public class StateMachine<TId>
    {
        private readonly Dictionary<TId, IState> m_States = new Dictionary<TId, IState>();
        private IState m_Current;

        public TId CurrentId { get; private set; }
        public event Action<TId> Changed;

        public void Add(TId id, IState state) => m_States.Add(id, state);

        public void ChangeTo(TId id)
        {
            m_Current?.Exit();
            CurrentId = id;
            m_Current = m_States[id];
            Changed?.Invoke(id);
            m_Current.Enter();
        }

        public void Tick() => m_Current?.Tick();
    }
}
