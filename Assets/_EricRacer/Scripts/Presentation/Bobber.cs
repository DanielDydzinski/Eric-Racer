using UnityEngine;

namespace EricRacer.Presentation
{
    /// <summary>Gentle float and sway, for balloons.</summary>
    public class Bobber : MonoBehaviour
    {
        [SerializeField] private float height = 0.3f;
        [SerializeField] private float speed = 1f;
        [SerializeField] private float swayDegrees = 6f;

        private Vector3 m_Start;
        private float m_Phase;

        void Awake()
        {
            m_Start = transform.localPosition;
            m_Phase = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            float t = Time.time * speed + m_Phase;
            transform.localPosition = m_Start + Vector3.up * Mathf.Sin(t) * height;
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.7f) * swayDegrees);
        }
    }
}
