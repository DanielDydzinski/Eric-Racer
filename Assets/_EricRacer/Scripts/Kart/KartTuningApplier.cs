using KartGame.KartSystems;
using UnityEngine;

namespace EricRacer.Kart
{
    // Runs before ArcadeKart.Awake, which caches grip from baseStats.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(ArcadeKart))]
    public class KartTuningApplier : MonoBehaviour
    {
        [SerializeField] private KartTuning tuning;

        void Awake()
        {
            if (tuning != null)
                GetComponent<ArcadeKart>().baseStats = tuning.Stats;
        }
    }
}
