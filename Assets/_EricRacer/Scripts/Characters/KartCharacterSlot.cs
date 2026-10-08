using System;
using KartGame.KartSystems;
using UnityEngine;

namespace EricRacer.Characters
{
    /// <summary>
    /// Puts a character in the driver's seat of a kart (race karts and lobby display karts alike) and paints the kart.
    /// </summary>
    public class KartCharacterSlot : MonoBehaviour
    {
        [Tooltip("The placeholder driver that ships with the kart model; its pose marks the seat.")]
        [SerializeField] private Transform defaultDriver;
        [Tooltip("Optional: the kart's animator driver, so steering animates the new character.")]
        [SerializeField] private KartPlayerAnimator kartAnimator;
        [SerializeField] private Renderer kartBody;

        private static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");

        private Transform m_Seat;
        private Vector3 m_SeatPosition;
        private Quaternion m_SeatRotation;
        private MaterialPropertyBlock m_Block;

        public CharacterView Current { get; private set; }
        public CharacterDefinition Definition { get; private set; }
        public event Action<CharacterView> CharacterChanged;

        void Awake()
        {
            m_Seat = defaultDriver.parent;
            m_SeatPosition = defaultDriver.localPosition;
            m_SeatRotation = defaultDriver.localRotation;
        }

        public void Apply(CharacterDefinition definition)
        {
            if (definition == null || definition == Definition)
                return;
            Definition = definition;

            if (defaultDriver != null)
                defaultDriver.gameObject.SetActive(false);
            if (Current != null)
                Destroy(Current.gameObject);

            Current = Instantiate(definition.DriverPrefab, m_Seat);
            Current.transform.SetLocalPositionAndRotation(m_SeatPosition + Current.SeatOffset, m_SeatRotation);
            Current.SetGrounded(true);
            if (kartAnimator != null)
                kartAnimator.PlayerAnimator = Current.Animator;

            if (kartBody != null)
            {
                m_Block ??= new MaterialPropertyBlock();
                kartBody.GetPropertyBlock(m_Block);
                m_Block.SetColor(k_BaseColor, definition.KartColor);
                kartBody.SetPropertyBlock(m_Block);
            }

            CharacterChanged?.Invoke(Current);
        }
    }
}
