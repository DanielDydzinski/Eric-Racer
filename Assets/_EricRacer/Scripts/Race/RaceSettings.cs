using UnityEngine;

namespace EricRacer.Race
{
    [CreateAssetMenu(menuName = "Eric Racer/Race Settings")]
    public class RaceSettings : ScriptableObject
    {
        [SerializeField, Range(1, 10)] private int laps = 3;
        [SerializeField] private float countdownSeconds = 3f;
        [Tooltip("Minimum time on the grid so everyone can see who's racing.")]
        [SerializeField] private float minGridSeconds = 2f;
        [Tooltip("Start anyway if someone's PC is still loading after this long.")]
        [SerializeField] private float maxGridSeconds = 15f;
        [Tooltip("After the winner crosses the line, the others get this long before results.")]
        [SerializeField] private float finishTimeoutSeconds = 45f;
        [SerializeField] private float positionUpdateInterval = 0.2f;
        [SerializeField] private string lobbyScene = "Lobby";

        public int Laps => laps;
        public float CountdownSeconds => countdownSeconds;
        public float MinGridSeconds => minGridSeconds;
        public float MaxGridSeconds => maxGridSeconds;
        public float FinishTimeoutSeconds => finishTimeoutSeconds;
        public float PositionUpdateInterval => positionUpdateInterval;
        public string LobbyScene => lobbyScene;
    }
}
