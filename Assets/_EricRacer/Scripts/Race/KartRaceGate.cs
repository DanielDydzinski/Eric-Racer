using KartGame.KartSystems;
using UnityEngine;

namespace EricRacer.Race
{
    /// <summary>Holds the kart on the grid until GO. After the finish everyone can keep driving around for fun.</summary>
    [RequireComponent(typeof(ArcadeKart))]
    public class KartRaceGate : MonoBehaviour
    {
        [SerializeField] private RaceStateEventChannel raceState;

        private ArcadeKart m_Kart;

        void Awake() => m_Kart = GetComponent<ArcadeKart>();

        void OnEnable()
        {
            raceState.Raised += Apply;
            Apply(raceState.HasValue ? raceState.LastValue : RaceStateId.None);
        }

        void OnDisable() => raceState.Raised -= Apply;

        void Apply(RaceStateId state) => m_Kart.SetCanMove(state == RaceStateId.Racing || state == RaceStateId.Results);
    }
}
