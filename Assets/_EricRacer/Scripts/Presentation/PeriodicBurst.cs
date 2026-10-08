using UnityEngine;

namespace EricRacer.Presentation
{
    /// <summary>Fires a particle effect every few seconds (title screen confetti).</summary>
    public class PeriodicBurst : MonoBehaviour
    {
        [SerializeField] private ParticleSystem effect;
        [SerializeField] private float interval = 2.5f;

        private float m_Next;

        void Update()
        {
            if (Time.time < m_Next)
                return;
            m_Next = Time.time + interval;
            effect.Play(true);
        }
    }
}
