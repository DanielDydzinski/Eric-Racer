using UnityEngine;

namespace EricRacer.Race
{
    [CreateAssetMenu(menuName = "Eric Racer/Track Catalog")]
    public class TrackCatalog : ScriptableObject
    {
        [SerializeField] private TrackDefinition[] tracks;

        public int Count => tracks.Length;
        public TrackDefinition Get(int index) => tracks[Wrap(index)];
        public int Wrap(int index) => ((index % Count) + Count) % Count;
    }
}
