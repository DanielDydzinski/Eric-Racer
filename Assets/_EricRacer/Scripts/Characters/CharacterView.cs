using UnityEngine;

namespace EricRacer.Characters
{
    /// <summary>
    /// Root of every playable character prefab: a Humanoid model driven by the shared PlayerController animator
    /// (Steering float, Grounded bool). New family models only need a Humanoid rig to work with it.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterView : MonoBehaviour
    {
        private static readonly int k_Steering = Animator.StringToHash("Steering");
        private static readonly int k_Grounded = Animator.StringToHash("Grounded");
        private static readonly int k_JumpState = Animator.StringToHash("PlayerJump");

        [Tooltip("Nudges this model in the kart seat (kart space). Models with different proportions sit slightly differently.")]
        [SerializeField] private Vector3 seatOffset;

        private Animator m_Animator;

        public Vector3 SeatOffset => seatOffset;

        public Animator Animator => m_Animator != null ? m_Animator : (m_Animator = GetComponent<Animator>());

        public void SetSteering(float steering) => Animator.SetFloat(k_Steering, steering);
        public void SetGrounded(bool grounded) => Animator.SetBool(k_Grounded, grounded);

        /// <summary>A little hop: used when picked in the lobby and to celebrate.</summary>
        public void Hop() => Animator.Play(k_JumpState, 0, 0f);
    }
}
