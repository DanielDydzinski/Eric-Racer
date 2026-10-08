using System;
using EricRacer.Core;
using EricRacer.Net;
using EricRacer.Race;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EricRacer.Lobby
{
    /// <summary>
    /// The lobby's shared state: the host's track and lap choice, synced to everyone, and starting the race
    /// once every player is ready.
    /// </summary>
    public class LobbyManager : NetworkBehaviour
    {
        [SerializeField] private RaceSetup setup;
        [SerializeField] private SessionRoster sessions;

        private readonly NetworkVariable<byte> m_Laps = new NetworkVariable<byte>();
        private readonly NetworkVariable<byte> m_Track = new NetworkVariable<byte>();
        private bool m_Starting;

        public int Laps => m_Laps.Value;
        public TrackDefinition Track => setup.Tracks.Get(m_Track.Value);
        public bool CanStart => sessions.AllReady && !m_Starting;

        public event Action SettingsChanged;

        public override void OnNetworkSpawn()
        {
            m_Laps.OnValueChanged += OnSettingChanged;
            m_Track.OnValueChanged += OnSettingChanged;
            if (IsServer)
            {
                m_Laps.Value = (byte)setup.Laps;
                m_Track.Value = (byte)setup.TrackIndex;
            }
            SettingsChanged?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            m_Laps.OnValueChanged -= OnSettingChanged;
            m_Track.OnValueChanged -= OnSettingChanged;
        }

        void OnSettingChanged(byte _, byte __) => SettingsChanged?.Invoke();

        void Update()
        {
            // Test runs (-players N with -autodrive) start by themselves once everyone has readied up.
            if (IsServer && LaunchOptions.ExpectedPlayers > 0 && sessions.Players.Count >= LaunchOptions.ExpectedPlayers && CanStart)
                StartRace();
        }

        // --- Host only ---

        public void ChangeLaps(int step)
        {
            if (!IsServer || step == 0)
                return;
            setup.Laps = Laps + step;
            m_Laps.Value = (byte)setup.Laps;
        }

        public void ChangeTrack(int step)
        {
            if (!IsServer || step == 0)
                return;
            setup.TrackIndex = m_Track.Value + step;
            m_Track.Value = (byte)setup.TrackIndex;
        }

        public void StartRace()
        {
            if (!IsServer || !CanStart)
                return;
            m_Starting = true;
            var lineup = new System.Text.StringBuilder();
            foreach (var player in sessions.Players)
                lineup.Append($" {player.PlayerName}={player.Character.DisplayName}");
            Debug.Log($"[Lobby] Starting {setup.Track.DisplayName}, {Laps} laps:{lineup}");
            NetworkManager.SceneManager.LoadScene(setup.Track.SceneName, LoadSceneMode.Single);
        }
    }
}
