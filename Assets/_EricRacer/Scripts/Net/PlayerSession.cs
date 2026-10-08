using System;
using EricRacer.Characters;
using EricRacer.Core;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace EricRacer.Net
{
    /// <summary>
    /// One per connected player (Netcode's player object), alive from joining until leaving, across lobby and races.
    /// The single source of who a player is: name, chosen character and whether they're ready.
    /// The owner writes its own choices; on a family LAN there's nothing to protect.
    /// </summary>
    public class PlayerSession : NetworkBehaviour
    {
        [SerializeField] private SessionRoster roster;
        [SerializeField] private CharacterCatalog catalog;

        private readonly NetworkVariable<FixedString32Bytes> m_Name =
            new NetworkVariable<FixedString32Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<byte> m_Character =
            new NetworkVariable<byte>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<bool> m_Ready =
            new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public event Action Changed;

        /// <summary>Owner only: the player flicked left (-1) or right (+1) through the racers.</summary>
        public event Action<int> Browsed;

        public string PlayerName => m_Name.Value.ToString();
        public int CharacterIndex => m_Character.Value;
        public CharacterDefinition Character => catalog.Get(m_Character.Value);
        public bool IsReady => m_Ready.Value;

        public override void OnNetworkSpawn()
        {
            DontDestroyOnLoad(gameObject);
            m_Name.OnValueChanged += OnNameChanged;
            m_Character.OnValueChanged += OnByteChanged;
            m_Ready.OnValueChanged += OnBoolChanged;

            if (IsOwner)
            {
                m_Name.Value = new FixedString32Bytes(LocalPlayerProfile.Name);
                m_Character.Value = (byte)catalog.Wrap(LocalPlayerProfile.CharacterIndex);
                m_Ready.Value = false;
            }
            roster.Add(this);
        }

        public override void OnNetworkDespawn()
        {
            m_Name.OnValueChanged -= OnNameChanged;
            m_Character.OnValueChanged -= OnByteChanged;
            m_Ready.OnValueChanged -= OnBoolChanged;
            roster.Remove(this);
        }

        void OnNameChanged(FixedString32Bytes _, FixedString32Bytes __) => Changed?.Invoke();
        void OnByteChanged(byte _, byte __) => Changed?.Invoke();
        void OnBoolChanged(bool _, bool __) => Changed?.Invoke();

        // --- Owner only ---

        public void BrowseCharacter(int step)
        {
            if (!IsOwner || IsReady)
                return;
            int next = catalog.Wrap(CharacterIndex + step);
            m_Character.Value = (byte)next;
            LocalPlayerProfile.CharacterIndex = next;
            Browsed?.Invoke(step);
        }

        public void SetReady(bool ready)
        {
            if (IsOwner)
                m_Ready.Value = ready;
        }
    }
}
