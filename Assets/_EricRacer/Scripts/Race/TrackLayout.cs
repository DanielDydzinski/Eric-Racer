using UnityEngine;

namespace EricRacer.Race
{
    /// <summary>The ordered gates of a track. Element 0 is the start/finish line.</summary>
    public class TrackLayout : MonoBehaviour
    {
        [SerializeField] private Checkpoint[] checkpoints;

        public int Count => checkpoints.Length;
        public Vector3 CheckpointPosition(int index) => checkpoints[index].transform.position;

        void Awake()
        {
            for (int i = 0; i < checkpoints.Length; i++)
                checkpoints[i].Index = i;
        }
    }
}
