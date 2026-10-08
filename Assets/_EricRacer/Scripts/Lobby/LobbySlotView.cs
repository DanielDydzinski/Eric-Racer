using EricRacer.Characters;
using EricRacer.Net;
using TMPro;
using UnityEngine;

namespace EricRacer.Lobby
{
    /// <summary>
    /// One pedestal on the lobby stage: shows a player's kart and racer spinning slowly, their name, and whether
    /// they're ready. Picking a new racer makes the kart pop; getting ready sets off confetti and a green spotlight.
    /// </summary>
    public class LobbySlotView : MonoBehaviour
    {
        [SerializeField] private KartCharacterSlot kart;
        [SerializeField] private Transform turntable;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Light spotlight;
        [SerializeField] private ParticleSystem readyBurst;
        [SerializeField] private float spinDegreesPerSecond = 25f;
        [SerializeField] private Color idleLight = new Color(1f, 0.95f, 0.85f);
        [SerializeField] private Color readyLight = new Color(0.3f, 1f, 0.35f);
        [SerializeField] private float popScale = 1.25f;

        [Header("Browse arrows (only on this PC's own pedestal, until ready)")]
        [SerializeField] private Transform leftArrow;
        [SerializeField] private Transform rightArrow;
        [SerializeField] private float arrowPopScale = 1.6f;
        [SerializeField] private float arrowBobHeight = 0.08f;

        private PlayerSession m_Session;
        private float m_LeftPop;
        private float m_RightPop;
        private Vector3 m_LeftRest;
        private Vector3 m_RightRest;
        private bool m_Bound;
        private int m_ShownCharacter = -1;
        private bool m_ShownReady;
        private float m_Pop;

        public void Bind(PlayerSession session)
        {
            if (m_Bound && session == m_Session)
                return;
            if (m_Session != null)
            {
                m_Session.Changed -= Refresh;
                m_Session.Browsed -= OnBrowsed;
            }

            m_Bound = true;
            m_Session = session;
            m_ShownCharacter = -1;
            m_ShownReady = false;
            if (m_Session != null)
            {
                m_Session.Changed += Refresh;
                m_Session.Browsed += OnBrowsed;
            }
            Refresh();
        }

        void Awake()
        {
            m_LeftRest = leftArrow.localPosition;
            m_RightRest = rightArrow.localPosition;
        }

        void OnBrowsed(int step)
        {
            if (step < 0)
                m_LeftPop = 1f;
            else
                m_RightPop = 1f;
        }

        void OnDestroy() => Bind(null);

        void Refresh()
        {
            bool occupied = m_Session != null;
            kart.gameObject.SetActive(occupied);
            bool showArrows = occupied && m_Session.IsOwner && !m_Session.IsReady;
            leftArrow.gameObject.SetActive(showArrows);
            rightArrow.gameObject.SetActive(showArrows);
            if (!occupied)
            {
                nameText.text = string.Empty;
                statusText.text = "Join in!";
                spotlight.color = idleLight * 0.4f;
                return;
            }

            nameText.text = m_Session.PlayerName;
            statusText.text = m_Session.IsReady ? "READY!" : m_Session.Character.DisplayName;
            spotlight.color = m_Session.IsReady ? readyLight : idleLight;

            if (m_Session.CharacterIndex != m_ShownCharacter)
            {
                bool firstShow = m_ShownCharacter < 0;
                m_ShownCharacter = m_Session.CharacterIndex;
                kart.Apply(m_Session.Character);
                if (!firstShow)
                    Celebrate(pop: true);
            }

            if (m_Session.IsReady && !m_ShownReady)
            {
                readyBurst.Play();
                Celebrate(pop: false);
            }
            m_ShownReady = m_Session.IsReady;
        }

        void Celebrate(bool pop)
        {
            if (pop)
                m_Pop = 1f;
            if (kart.Current != null)
                kart.Current.Hop();
        }

        void Update()
        {
            turntable.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
            m_Pop = Mathf.MoveTowards(m_Pop, 0f, Time.deltaTime * 4f);
            float scale = 1f + (popScale - 1f) * Mathf.Sin(m_Pop * Mathf.PI);
            turntable.localScale = Vector3.one * scale;

            if (leftArrow.gameObject.activeSelf)
            {
                AnimateArrow(leftArrow, m_LeftRest, ref m_LeftPop, -1f);
                AnimateArrow(rightArrow, m_RightRest, ref m_RightPop, 1f);
            }
        }

        // Gentle bob while waiting; a pop outwards when the player browses that way.
        void AnimateArrow(Transform arrow, Vector3 rest, ref float pop, float side)
        {
            pop = Mathf.MoveTowards(pop, 0f, Time.deltaTime * 4f);
            float punch = Mathf.Sin(pop * Mathf.PI);
            float bob = Mathf.Sin(Time.time * 3f) * arrowBobHeight;
            arrow.localPosition = rest + new Vector3(side * punch * 0.25f, bob, 0f);
            arrow.localScale = Vector3.one * (1f + (arrowPopScale - 1f) * punch);
        }
    }
}
