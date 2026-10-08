using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace EricRacer.Race
{
    /// <summary>
    /// One racer's standing, synced to everyone. Only the server writes it; the owning PC reports gates it drives through.
    /// Laps and the next gate are derived from a single counter (gates passed in order) so they can never disagree.
    /// </summary>
    public class RaceProgress : NetworkBehaviour
    {
        [SerializeField] private RaceRoster roster;

        private readonly NetworkVariable<int> m_CheckpointsPassed = new NetworkVariable<int>();
        private readonly NetworkVariable<byte> m_Position = new NetworkVariable<byte>();
        private readonly NetworkVariable<byte> m_FinishPlace = new NetworkVariable<byte>();
        private readonly NetworkVariable<float> m_FinishTime = new NetworkVariable<float>();
        private readonly NetworkVariable<FixedString32Bytes> m_DisplayName = new NetworkVariable<FixedString32Bytes>();

        private RaceManager m_Race;

        public event Action Changed;

        public int CheckpointsPassed => m_CheckpointsPassed.Value;
        public int Position => m_Position.Value;
        public int FinishPlace => m_FinishPlace.Value;
        public bool HasFinished => m_FinishPlace.Value > 0;
        public float FinishTime => m_FinishTime.Value;
        public string DisplayName => m_DisplayName.Value.ToString();

        /// <summary>Passing the line the first time starts lap 1, so laps only count once a full loop follows it.</summary>
        public int LapsCompleted(int checkpointCount) => CheckpointsPassed == 0 ? 0 : (CheckpointsPassed - 1) / checkpointCount;
        public int NextCheckpoint(int checkpointCount) => CheckpointsPassed % checkpointCount;

        public override void OnNetworkSpawn()
        {
            m_CheckpointsPassed.OnValueChanged += OnIntChanged;
            m_Position.OnValueChanged += OnByteChanged;
            m_FinishPlace.OnValueChanged += OnByteChanged;
            m_DisplayName.OnValueChanged += OnNameChanged;
            roster.Add(this);
        }

        public override void OnNetworkDespawn()
        {
            m_CheckpointsPassed.OnValueChanged -= OnIntChanged;
            m_Position.OnValueChanged -= OnByteChanged;
            m_FinishPlace.OnValueChanged -= OnByteChanged;
            m_DisplayName.OnValueChanged -= OnNameChanged;
            roster.Remove(this);
        }

        void OnIntChanged(int _, int __) => Changed?.Invoke();
        void OnByteChanged(byte _, byte __) => Changed?.Invoke();
        void OnNameChanged(FixedString32Bytes _, FixedString32Bytes __) => Changed?.Invoke();

        public void ReportCheckpoint(int index)
        {
            if (IsOwner)
                ReportCheckpointRpc(index);
        }

        [Rpc(SendTo.Server)]
        void ReportCheckpointRpc(int index)
        {
            if (m_Race != null)
                m_Race.ServerOnCheckpoint(this, index);
        }

        internal void ServerBind(RaceManager race, string displayName)
        {
            m_Race = race;
            m_DisplayName.Value = new FixedString32Bytes(displayName);
        }

        internal void ServerAdvance() => m_CheckpointsPassed.Value++;
        internal void ServerSetPosition(int position) => m_Position.Value = (byte)position;

        internal void ServerFinish(int place, float time)
        {
            m_FinishTime.Value = time;
            m_FinishPlace.Value = (byte)place;
        }
    }
}
