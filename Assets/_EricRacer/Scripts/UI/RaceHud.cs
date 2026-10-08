using EricRacer.Core;
using EricRacer.Race;
using KartGame.KartSystems;
using TMPro;
using UnityEngine;

namespace EricRacer.UI
{
    /// <summary>The player's own race info: lap, position, 3-2-1-GO and the finish message.</summary>
    public class RaceHud : MonoBehaviour
    {
        [SerializeField] private RaceManager race;
        [SerializeField] private KartEventChannel localKart;
        [SerializeField] private TMP_Text lapText;
        [SerializeField] private TMP_Text positionText;
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private GameObject resultsPanel;

        [Header("Sounds")]
        [SerializeField] private AudioSource sfx;
        [SerializeField] private AudioClip countdownBeep;
        [SerializeField] private AudioClip goSound;
        [SerializeField] private float goDisplaySeconds = 1.2f;

        private RaceProgress m_Local;
        private int m_LastCountdownNumber;
        private float m_HideGoAt;

        void OnEnable()
        {
            race.StateChanged += OnRaceStateChanged;
            localKart.Raised += Bind;
            if (localKart.HasValue && localKart.LastValue != null)
                Bind(localKart.LastValue);
            OnRaceStateChanged(race.State);
        }

        void OnDisable()
        {
            race.StateChanged -= OnRaceStateChanged;
            localKart.Raised -= Bind;
            Unbind();
        }

        void Bind(ArcadeKart kart)
        {
            Unbind();
            m_Local = kart.GetComponent<RaceProgress>();
            m_Local.Changed += Refresh;
            Refresh();
        }

        void Unbind()
        {
            if (m_Local != null)
                m_Local.Changed -= Refresh;
            m_Local = null;
        }

        void OnRaceStateChanged(RaceStateId state)
        {
            resultsPanel.SetActive(state == RaceStateId.Results);
            if (state == RaceStateId.Countdown)
                m_LastCountdownNumber = 0;
            if (state == RaceStateId.Racing)
            {
                m_HideGoAt = Time.time + goDisplaySeconds;
                Play(goSound);
            }
            Refresh();
        }

        void Update()
        {
            // Purely time-driven: the countdown ticks against the synced server clock.
            if (race.State == RaceStateId.Countdown)
            {
                int number = Mathf.CeilToInt((float)(race.PhaseEndsAt - race.Now));
                if (number > 0 && number != m_LastCountdownNumber)
                {
                    m_LastCountdownNumber = number;
                    countdownText.text = number.ToString();
                    Play(countdownBeep);
                }
            }
            else
            {
                countdownText.text = Time.time < m_HideGoAt ? "GO!" : string.Empty;
            }
        }

        void Refresh()
        {
            bool racing = race.State == RaceStateId.Racing || race.State == RaceStateId.Results;
            if (m_Local == null || race.Laps == 0)
            {
                lapText.text = positionText.text = messageText.text = string.Empty;
                return;
            }

            int lap = Mathf.Min(m_Local.LapsCompleted(race.CheckpointCount) + 1, race.Laps);
            lapText.text = $"LAP {lap}/{race.Laps}";
            positionText.text = racing ? RaceText.Ordinal(m_Local.Position) : string.Empty;
            messageText.text = m_Local.HasFinished ? $"You finished {RaceText.Ordinal(m_Local.FinishPlace)}!" : string.Empty;
        }

        void Play(AudioClip clip)
        {
            if (sfx != null && clip != null)
                sfx.PlayOneShot(clip);
        }
    }
}
