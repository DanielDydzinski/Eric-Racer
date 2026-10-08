using KartGame.KartSystems;
using UnityEngine;

namespace EricRacer.Kart
{
    /// <summary>Data asset for kart handling, so tuning lives in assets instead of being copied into every prefab.</summary>
    [CreateAssetMenu(menuName = "Eric Racer/Kart Tuning")]
    public class KartTuning : ScriptableObject
    {
        [SerializeField] private ArcadeKart.Stats stats = new ArcadeKart.Stats
        {
            TopSpeed = 10f,
            Acceleration = 5f,
            AccelerationCurve = 4f,
            Braking = 10f,
            ReverseAcceleration = 5f,
            ReverseSpeed = 5f,
            Steer = 5f,
            CoastingDrag = 4f,
            Grip = .95f,
            AddedGravity = 1f,
        };

        public ArcadeKart.Stats Stats => stats;
    }
}
