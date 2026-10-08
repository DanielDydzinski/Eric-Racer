using UnityEngine;

namespace EricRacer.Race
{
    /// <summary>A gate on the track. The PC driving a kart reports crossing it; the server decides if it counts.</summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        public int Index { get; set; }

        void OnTriggerEnter(Collider other)
        {
            var body = other.attachedRigidbody;
            if (body == null)
                return;
            var progress = body.GetComponent<RaceProgress>();
            if (progress != null && progress.IsOwner)
                progress.ReportCheckpoint(Index);
        }
    }
}
