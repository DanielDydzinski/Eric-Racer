using UnityEngine;

namespace EricRacer.Core
{
    /// <summary>
    /// Lets scene objects reach a long-lived service (that survives scene loads) through an asset reference,
    /// instead of searching for it. The service provides itself on Awake.
    /// </summary>
    public abstract class RuntimeAnchor<T> : ScriptableObject where T : Object
    {
        public T Value { get; private set; }
        public bool IsSet => Value != null;

        public void Provide(T value) => Value = value;

        public void Release(T value)
        {
            if (Value == value)
                Value = null;
        }

        void OnDisable() => Value = null;
    }
}
