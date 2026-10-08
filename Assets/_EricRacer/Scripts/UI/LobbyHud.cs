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
        [SerializeField] private TMP_Text startLabel;
        [SerializeField] private string startWaitingText = "WAITING...";
        [SerializeField] private string startReadyText = "START!";
        [Tooltip("How much the START button grows and shrinks when everyone is ready.")]
        [SerializeField] private float startPulse = 0.06f;

        [Header("Messages ({A}, {Space}... become coloured button labels)")]
        [SerializeField, TextArea] private string choosingHint = "Press {A} / {Space} to ready!";
        [SerializeField, TextArea] private string readyHint = "You're ready!   •   {B} / {Backspace} change racer";
        [Tooltip("{0} = players ready, {1} = players in the lobby")]
        [SerializeField] private string readyCount = "Ready: {0} / {1}";
        [SerializeField] private string everyoneReadyHost = "Everyone's ready! Press {START} / {Enter}";
        [SerializeField] private string everyoneReadyGuest = "Everyone's ready! Waiting for the host...";
        [Tooltip("{0} = track name")]
        [SerializeField] private string trackLabel = "Track: {0}";
        [Tooltip("{0} = number of laps")]
        [SerializeField] private string lapsLabel = "Laps: {0}";

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
            hintText.text = ButtonHints.Format(me != null && me.IsReady ? readyHint : choosingHint);

            int ready = 0;
            foreach (var player in sessions.Players)
                if (player.IsReady)
                    ready++;

            string status = sessions.AllReady
                ? (lobby.IsHost ? everyoneReadyHost : everyoneReadyGuest)
                : string.Format(readyCount, ready, sessions.Players.Count);
            statusText.text = ButtonHints.Format(status);

            hostPanel.SetActive(lobby.IsHost);
            trackText.text = string.Format(trackLabel, lobby.Track.DisplayName);
            lapsText.text = string.Format(lapsLabel, lobby.Laps);
            startButton.interactable = lobby.CanStart;
            startLabel.text = lobby.CanStart ? startReadyText : startWaitingText;
        }

        void Update()
        {
            // Purely cosmetic: a gentle pulse so the host notices START once it's available.
            float scale = startButton.interactable ? 1f + Mathf.Sin(Time.time * 5f) * startPulse : 1f;
            startButton.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
