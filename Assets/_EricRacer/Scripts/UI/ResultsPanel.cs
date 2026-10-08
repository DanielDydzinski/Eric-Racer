using System.Collections.Generic;
using EricRacer.Race;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EricRacer.UI
{
    /// <summary>Final standings. The host gets a "Race again" button; everyone else waits for the host.</summary>
    public class ResultsPanel : MonoBehaviour
    {
        [SerializeField] private RaceManager race;
        [SerializeField] private TMP_Text[] rows;
        [SerializeField] private Button raceAgainButton;
        [SerializeField] private TMP_Text waitingText;

        private readonly List<RaceProgress> m_Sorted = new List<RaceProgress>();
        private readonly List<RaceProgress> m_Watched = new List<RaceProgress>();

        void Awake() => raceAgainButton.onClick.AddListener(race.RequestRestart);

        void OnEnable()
        {
            race.Roster.Changed += Rewatch;
            Rewatch();

            bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
            raceAgainButton.gameObject.SetActive(isHost);
            waitingText.gameObject.SetActive(!isHost);
            if (isHost && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(raceAgainButton.gameObject);
        }

        void OnDisable()
        {
            race.Roster.Changed -= Rewatch;
            Unwatch();
        }

        void Rewatch()
        {
            Unwatch();
            m_Watched.AddRange(race.Roster.Racers);
            foreach (var racer in m_Watched)
                racer.Changed += Refresh;
            Refresh();
        }

        void Unwatch()
        {
            foreach (var racer in m_Watched)
                if (racer != null)
                    racer.Changed -= Refresh;
            m_Watched.Clear();
        }

        void Refresh()
        {
            m_Sorted.Clear();
            m_Sorted.AddRange(race.Roster.Racers);
            m_Sorted.Sort((a, b) => a.Position.CompareTo(b.Position));

            for (int i = 0; i < rows.Length; i++)
            {
                bool used = i < m_Sorted.Count;
                rows[i].gameObject.SetActive(used);
                if (!used)
                    continue;
                var racer = m_Sorted[i];
                string time = racer.HasFinished ? RaceText.Time(racer.FinishTime) : "still racing";
                rows[i].text = $"{RaceText.Ordinal(racer.Position),-4} {racer.DisplayName,-16} {time}";
            }
        }
    }
}
