using System;
using System.Collections.Generic;
using EricRacer.Core;
using EricRacer.Race.States;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EricRacer.Race
{
    /// <summary>
    /// Runs one race: the server drives a state machine (Grid → Countdown → Racing → Results) and owns all rules
    /// (gate order, laps, finishing, positions). Every PC observes the synced state, times and per-kart progress.
    /// </summary>
    public class RaceManager : NetworkBehaviour
    {
        [SerializeField] private RaceSettings settings;
        [SerializeField] private TrackLayout track;
        [SerializeField] private KartSpawner spawner;
        [SerializeField] private RaceRoster roster;
        [SerializeField] private RaceStateEventChannel stateChannel;

        private readonly NetworkVariable<RaceStateId> m_State = new NetworkVariable<RaceStateId>();
        private readonly NetworkVariable<double> m_PhaseEndsAt = new NetworkVariable<double>();
        private readonly NetworkVariable<double> m_RaceStartedAt = new NetworkVariable<double>();
        private readonly NetworkVariable<int> m_Laps = new NetworkVariable<int>();

        private StateMachine<RaceStateId> m_Machine;
        private readonly List<RaceProgress> m_Ranking = new List<RaceProgress>();
        private int m_FinishedCount;
        private float m_NextRankingTime;

        public RaceStateId State => m_State.Value;
        public int Laps => m_Laps.Value;
        public int CheckpointCount => track.Count;
        public RaceRoster Roster => roster;
        /// <summary>Server time the countdown ends (valid during Countdown).</summary>
        public double PhaseEndsAt => m_PhaseEndsAt.Value;
        public double RaceStartedAt => m_RaceStartedAt.Value;
        public double Now => NetworkManager.ServerTime.Time;

        public event Action<RaceStateId> StateChanged;

        // --- Server-side data the states read ---
        internal RaceSettings Settings => settings;
        internal bool AllClientsLoaded { get; private set; }
        internal int FinishedCount => m_FinishedCount;
        internal float FirstFinishRealtime { get; private set; } = -1f;
        internal bool EveryoneHasAKart => roster.Racers.Count >= NetworkManager.ConnectedClientsIds.Count
            && roster.Racers.Count >= LaunchOptions.ExpectedPlayers;
        internal bool EveryoneFinished => roster.Racers.Count > 0 && m_FinishedCount >= roster.Racers.Count;

        public override void OnNetworkSpawn()
        {
            m_State.OnValueChanged += OnStateValueChanged;
            Broadcast(m_State.Value);

            if (!IsServer)
                return;

            m_Laps.Value = LaunchOptions.Laps > 0 ? LaunchOptions.Laps : settings.Laps;
            spawner.KartSpawned += ServerRegister;
            NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;

            m_Machine = new StateMachine<RaceStateId>();
            m_Machine.Add(RaceStateId.Grid, new GridState(this));
            m_Machine.Add(RaceStateId.Countdown, new CountdownState(this));
            m_Machine.Add(RaceStateId.Racing, new RacingState(this));
            m_Machine.Add(RaceStateId.Results, new ResultsState(this));
            m_Machine.Changed += id => m_State.Value = id;
            m_Machine.ChangeTo(RaceStateId.Grid);

            spawner.Begin(NetworkManager);
        }

        public override void OnNetworkDespawn()
        {
            m_State.OnValueChanged -= OnStateValueChanged;
            stateChannel.Raise(RaceStateId.None);

            if (!IsServer)
                return;
            spawner.End();
            spawner.KartSpawned -= ServerRegister;
            if (NetworkManager.SceneManager != null)
                NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }

        void Update()
        {
            if (IsServer && m_Machine != null)
                m_Machine.Tick();
        }

        void OnStateValueChanged(RaceStateId _, RaceStateId next) => Broadcast(next);

        void Broadcast(RaceStateId state)
        {
            Debug.Log($"[Race] {state}");
            stateChannel.Raise(state);
            StateChanged?.Invoke(state);
        }

        void OnLoadEventCompleted(string sceneName, LoadSceneMode mode, List<ulong> completed, List<ulong> timedOut)
        {
            if (sceneName == gameObject.scene.name)
                AllClientsLoaded = true;
        }

        void ServerRegister(NetworkObject kart, ulong clientId)
        {
            // Phase 4 replaces this with the name each player typed.
            kart.GetComponent<RaceProgress>().ServerBind(this, $"Player {clientId + 1}");
        }

        // --- Called by states (server only) ---

        internal void ChangeState(RaceStateId next) => m_Machine.ChangeTo(next);

        internal void BeginCountdown() => m_PhaseEndsAt.Value = Now + settings.CountdownSeconds;

        internal void BeginRace()
        {
            m_RaceStartedAt.Value = Now;
            m_FinishedCount = 0;
            FirstFinishRealtime = -1f;
        }

        internal void ServerOnCheckpoint(RaceProgress racer, int index)
        {
            if (State != RaceStateId.Racing || racer.HasFinished)
                return;
            if (index != racer.NextCheckpoint(track.Count))
                return; // Wrong gate (shortcut, driving backwards, or the gate behind the grid): ignore.

            racer.ServerAdvance();

            if (racer.LapsCompleted(track.Count) >= Laps)
            {
                m_FinishedCount++;
                if (FirstFinishRealtime < 0f)
                    FirstFinishRealtime = Time.realtimeSinceStartup;
                racer.ServerFinish(m_FinishedCount, (float)(Now - RaceStartedAt));
                Debug.Log($"[Race] {racer.DisplayName} finished #{m_FinishedCount} in {racer.FinishTime:F2}s");
            }
            UpdateRanking(force: true);
        }

        internal void UpdateRanking(bool force = false)
        {
            if (!force && Time.time < m_NextRankingTime)
                return;
            m_NextRankingTime = Time.time + settings.PositionUpdateInterval;

            m_Ranking.Clear();
            m_Ranking.AddRange(roster.Racers);
            m_Ranking.Sort(CompareStanding);
            for (int i = 0; i < m_Ranking.Count; i++)
                m_Ranking[i].ServerSetPosition(i + 1);
        }

        int CompareStanding(RaceProgress a, RaceProgress b)
        {
            if (a.HasFinished || b.HasFinished)
            {
                if (a.HasFinished && b.HasFinished)
                    return a.FinishPlace.CompareTo(b.FinishPlace);
                return a.HasFinished ? -1 : 1;
            }
            if (a.CheckpointsPassed != b.CheckpointsPassed)
                return b.CheckpointsPassed.CompareTo(a.CheckpointsPassed);
            return DistanceToNext(a).CompareTo(DistanceToNext(b));
        }

        float DistanceToNext(RaceProgress racer) =>
            Vector3.Distance(racer.transform.position, track.CheckpointPosition(racer.NextCheckpoint(track.Count)));

        // --- Requests from players ---

        /// <summary>Host only: run the same track again.</summary>
        public void RequestRestart()
        {
            if (IsServer && State == RaceStateId.Results)
                NetworkManager.SceneManager.LoadScene(gameObject.scene.name, LoadSceneMode.Single);
        }
    }
}
