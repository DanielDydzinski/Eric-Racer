using EricRacer.Core;
using KartGame.KartSystems;
using UnityEngine;

namespace EricRacer.Kart
{
    /// <summary>
    /// Tells the scene (camera, HUD) which kart belongs to the player on this PC.
    /// Offline test scenes announce on Start; in multiplayer the network layer calls <see cref="Announce"/> for the owner.
    /// </summary>
    [RequireComponent(typeof(ArcadeKart))]
    public class LocalKartAnnouncer : MonoBehaviour
    {
        [SerializeField] private KartEventChannel localKartChannel;
        [SerializeField] private bool announceOnStart;

        void Start()
        {
            if (announceOnStart)
                Announce();
        }

        public void Announce() => localKartChannel.Raise(GetComponent<ArcadeKart>());
    }
}
