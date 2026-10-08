using Cinemachine;
using EricRacer.Core;
using KartGame.KartSystems;
using UnityEngine;

namespace EricRacer.Cameras
{
    /// <summary>Points the scene's Cinemachine camera at whichever kart this PC's player drives.</summary>
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class FollowLocalKartCamera : MonoBehaviour
    {
        [SerializeField] private KartEventChannel localKartChannel;

        private CinemachineVirtualCamera m_Camera;

        void Awake() => m_Camera = GetComponent<CinemachineVirtualCamera>();

        void OnEnable()
        {
            localKartChannel.Raised += Follow;
            if (localKartChannel.HasValue && localKartChannel.LastValue != null)
                Follow(localKartChannel.LastValue);
        }

        void OnDisable() => localKartChannel.Raised -= Follow;

        void Follow(ArcadeKart kart)
        {
            m_Camera.Follow = kart.transform;
            m_Camera.LookAt = kart.transform;
        }
    }
}
