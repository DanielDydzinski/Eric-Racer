using System;
using UnityEngine;

namespace EricRacer.Core
{
    /// <summary>
    /// ScriptableObject observer channel: lets objects in different scenes or prefabs talk without references to each other.
    /// Remembers the last value so late subscribers (e.g. a camera that loads after the kart spawned) can catch up.
    /// </summary>
    public abstract class EventChannel<T> : ScriptableObject
    {
        public event Action<T> Raised;

        public T LastValue { get; private set; }
        public bool HasValue { get; private set; }

        public void Raise(T value)
        {
            LastValue = value;
            HasValue = true;
            Raised?.Invoke(value);
        }

        public void Clear()
        {
            LastValue = default;
            HasValue = false;
        }

        // SO state survives play mode in the editor; start each session clean.
        void OnDisable() => Clear();
    }
}
