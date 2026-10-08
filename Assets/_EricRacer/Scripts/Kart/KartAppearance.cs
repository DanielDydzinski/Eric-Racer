using EricRacer.Characters;
using EricRacer.Net;
using Unity.Netcode;
using UnityEngine;

namespace EricRacer.Kart
{
    /// <summary>Dresses a race kart as its owner's chosen character, on every PC.</summary>
    [RequireComponent(typeof(KartCharacterSlot))]
    public class KartAppearance : NetworkBehaviour
    {
        [SerializeField] private SessionRoster sessions;

        private KartCharacterSlot m_Slot;
        private PlayerSession m_Owner;

        public PlayerSession Owner => m_Owner;

        /// <summary>Raised whenever the owner's look or name is (re)applied.</summary>
        public event System.Action Refreshed;

        void Awake() => m_Slot = GetComponent<KartCharacterSlot>();

        public override void OnNetworkSpawn()
        {
            sessions.Changed += FindOwner;
            FindOwner();
        }

        public override void OnNetworkDespawn()
        {
            sessions.Changed -= FindOwner;
            if (m_Owner != null)
                m_Owner.Changed -= Apply;
            m_Owner = null;
        }

        // The owner's session may arrive a moment after the kart on a joining PC, so keep looking until found.
        void FindOwner()
        {
            if (m_Owner != null)
                return;
            m_Owner = sessions.Find(OwnerClientId);
            if (m_Owner == null)
                return;
            m_Owner.Changed += Apply;
            Apply();
        }

        void Apply()
        {
            m_Slot.Apply(m_Owner.Character);
            Refreshed?.Invoke();
        }
    }
}
