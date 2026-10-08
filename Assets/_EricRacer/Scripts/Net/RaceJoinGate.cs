using EricRacer.Race;
using UnityEngine;

namespace EricRacer.Net
{
    /// <summary>Lets new players in only while nobody is mid-race.</summary>
    public class RaceJoinGate : MonoBehaviour
    {
        [SerializeField] private ConnectionManager connection;
        [SerializeField] private RaceStateEventChannel raceState;

        void OnEnable() => raceState.Raised += Apply;
        void OnDisable() => raceState.Raised -= Apply;

        void Apply(RaceStateId state) =>
            connection.AcceptingNewPlayers = state == RaceStateId.None || state == RaceStateId.Grid;
    }
}
