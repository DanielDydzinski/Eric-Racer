using Unity.Netcode;
using UnityEngine;

namespace EricRacer.Kart
{
    /// <summary>
    /// Splits a kart into "mine" and "someone else's". The owner's PC runs input and physics;
    /// every other PC just shows the kart where the owner says it is (NetworkTransform, owner authority).
    /// </summary>
    public class NetworkKart : NetworkBehaviour
    {
        [Tooltip("Only the PC that drives this kart runs these (input strategies, ArcadeKart physics, respawn).")]
        [SerializeField] private Behaviour[] ownerOnly;
        [SerializeField] private LocalKartAnnouncer announcer;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                foreach (var behaviour in ownerOnly)
                    behaviour.enabled = false;
                return;
            }

            // Netcode places the transform after instantiating; an interpolated Rigidbody would otherwise snap back to its own (origin) pose.
            var body = GetComponent<Rigidbody>();
            body.position = transform.position;
            body.rotation = transform.rotation;

            announcer.Announce();
        }
    }
}
