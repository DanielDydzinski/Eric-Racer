using UnityEngine;

namespace EricRacer.Race
{
    /// <summary>
    /// What the host picked in the lobby (track, laps). Lives on the host's PC between the lobby and the race;
    /// other PCs see these values through the lobby and race managers' synced variables.
    /// </summary>
    [CreateAssetMenu(menuName = "Eric Racer/Race Setup")]
    public class RaceSetup : ScriptableObject
    {
        public const int MinLaps = 1;
        public const int MaxLaps = 9;

        [SerializeField] private RaceSettings defaults;
        [SerializeField] private TrackCatalog tracks;

        private int m_Laps;
        private int m_TrackIndex;

        public TrackCatalog Tracks => tracks;

        public int Laps
        {
            get => m_Laps > 0 ? m_Laps : defaults.Laps;
            set => m_Laps = Mathf.Clamp(value, MinLaps, MaxLaps);
        }

        public int TrackIndex
        {
            get => m_TrackIndex;
            set => m_TrackIndex = tracks.Wrap(value);
        }

        public TrackDefinition Track => tracks.Get(m_TrackIndex);

        void OnDisable()
        {
            m_Laps = 0;
            m_TrackIndex = 0;
        }
    }
}
