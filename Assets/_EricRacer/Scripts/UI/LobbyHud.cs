using System.Collections.Generic;
using EricRacer.Lobby;
using EricRacer.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EricRacer.UI
{
    /// <summary>Lobby screen overlay: how to pick and ready up, plus the host's race settings and START.</summary>
    public class LobbyHud : MonoBehaviour
    {
        [SerializeField] private LobbyManager lobby;
        [SerializeField] private SessionRoster sessions;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private TMP_Text statusText;

        [Header("Host only")]
        [SerializeField] private GameObject hostPanel;
        [SerializeField] private TMP_Text trackText;
        [SerializeField] private TMP_Text lapsText;
        [SerializeField] private Button lapsDown;
        [SerializeField] private Button lapsUp;
        [SerializeField] private Button startButton;

        private readonly List<PlayerSession> m_Watched = new List<PlayerSession>();

        void Awake()
        {
            lapsDown.onClick.AddListener(() => lobby.ChangeLaps(-1));
            lapsUp.onClick.AddListener(() => lobby.ChangeLaps(1));
            startButton.onClick.AddListener(lobby.StartRace);
        }

        void OnEnable()
        {
            lobby.SettingsChanged += Refresh;
            sessions.Changed += Rewatch;
            Rewatch();
        }

        void OnDisable()
        {
            lobby.SettingsChanged -= Refresh;
            sessions.Changed -= Rewatch;
            Unwatch();
        }

        void Rewatch()
        {
            Unwatch();
            m_Watched.AddRange(sessions.Players);
            foreach (var player in m_Watched)
                player.Changed += Refresh;
            Refresh();
        }

        void Unwatch()
        {
            foreach (var player in m_Watched)
                if (player != null)
                    player.Changed -= Refresh;
            m_Watched.Clear();
        }

        void Refresh()
        {
            var me = sessions.Local;
            hintText.text = me != null && me.IsReady
                ? "You're ready!   <size=70%>(B / Backspace to change racer)</size>"
                : "<  >  pick your racer        A / Space  =  READY!";

            int ready = 0;
            foreach (var player in sessions.Players)
                if (player.IsReady)
                    ready++;

            bool everyoneReady = sessions.AllReady;
            statusText.text = everyoneReady
                ? (lobby.IsHost ? "Everyone's ready! Press START" : "Everyone's ready! Waiting for the host...")
                : $"Ready: {ready} / {sessions.Players.Count}";

            hostPanel.SetActive(lobby.IsHost);
            trackText.text = $"Track: {lobby.Track.DisplayName}";
            lapsText.text = $"Laps: {lobby.Laps}";
            startButton.interactable = lobby.CanStart;
        }
    }
}
