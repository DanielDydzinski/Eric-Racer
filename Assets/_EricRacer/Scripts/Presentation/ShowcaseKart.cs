using EricRacer.Characters;
using EricRacer.Core;
using UnityEngine;

namespace EricRacer.Presentation
{
    /// <summary>Title screen kart: shows this PC's last-picked racer, turning slowly and hopping now and then.</summary>
    public class ShowcaseKart : MonoBehaviour
    {
        [SerializeField] private KartCharacterSlot kart;
        [SerializeField] private CharacterCatalog catalog;
        [SerializeField] private float spinDegreesPerSecond = 30f;
        [SerializeField] private float hopInterval = 3f;

        private float m_NextHop;

        void Start()
        {
            kart.Apply(catalog.Get(LocalPlayerProfile.CharacterIndex));
            m_NextHop = Time.time + hopInterval;
        }

        void Update()
        {
            transform.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
            if (Time.time >= m_NextHop && kart.Current != null)
            {
                m_NextHop = Time.time + hopInterval;
                kart.Current.Hop();
            }
        }
    }
}
