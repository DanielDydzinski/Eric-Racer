using System;
using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EricRacer.Kart
{
    /// <summary>
    /// Puts the kart back on the track when it falls off, ends up upside down, or the player presses Respawn.
    /// Only active on the kart this PC simulates (the owner).
    /// </summary>
    [RequireComponent(typeof(ArcadeKart))]
    public class KartRespawner : MonoBehaviour
    {
        [SerializeField] private InputActionReference respawnAction;
        [SerializeField] private LayerMask trackLayers = (1 << 9) | (1 << 11); // Ground, Track
        [SerializeField] private float fallBelowY = -20f;
        [SerializeField] private float flippedSecondsBeforeRespawn = 1.5f;
        [SerializeField] private float safePoseInterval = 0.5f;
        [Tooltip("Respawn at a pose recorded at least this long ago, so we don't land right back on the edge we fell off.")]
        [SerializeField] private float safePoseMinAge = 1f;

        public event Action Respawned;

        private const int k_History = 8;
        private readonly Pose[] m_SafePoses = new Pose[k_History];
        private readonly float[] m_SafePoseTimes = new float[k_History];
        private int m_SafeCount;
        private int m_SafeHead;
        private float m_NextSampleTime;
        private float m_FlippedTime;

        private ArcadeKart m_Kart;
        private Rigidbody m_Body;

        void Awake()
        {
            m_Kart = GetComponent<ArcadeKart>();
            m_Body = GetComponent<Rigidbody>();
        }

        // Start, not Awake: a networked kart is moved onto its grid slot after Awake.
        void Start() => RecordSafePose(new Pose(transform.position, transform.rotation));

        void OnEnable()
        {
            if (respawnAction != null)
            {
                respawnAction.action.Enable();
                respawnAction.action.performed += OnRespawnPressed;
            }
        }

        void OnDisable()
        {
            if (respawnAction != null)
                respawnAction.action.performed -= OnRespawnPressed;
        }

        void OnRespawnPressed(InputAction.CallbackContext _) => Respawn();

        void FixedUpdate()
        {
            if (transform.position.y < fallBelowY)
            {
                Respawn();
                return;
            }

            bool upsideDown = Vector3.Dot(transform.up, Vector3.up) < 0.2f && m_Body.linearVelocity.sqrMagnitude < 4f;
            m_FlippedTime = upsideDown ? m_FlippedTime + Time.fixedDeltaTime : 0f;
            if (m_FlippedTime > flippedSecondsBeforeRespawn)
            {
                Respawn();
                return;
            }

            if (Time.time >= m_NextSampleTime && IsSafe())
            {
                m_NextSampleTime = Time.time + safePoseInterval;
                RecordSafePose(new Pose(transform.position, Quaternion.Euler(0f, transform.eulerAngles.y, 0f)));
            }
        }

        bool IsSafe()
        {
            return m_Kart.GroundPercent >= 1f
                && Vector3.Dot(transform.up, Vector3.up) > 0.9f
                && Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, 2f, trackLayers, QueryTriggerInteraction.Ignore);
        }

        void RecordSafePose(Pose pose)
        {
            m_SafePoses[m_SafeHead] = pose;
            m_SafePoseTimes[m_SafeHead] = Time.time;
            m_SafeHead = (m_SafeHead + 1) % k_History;
            m_SafeCount = Mathf.Min(m_SafeCount + 1, k_History);
        }

        Pose PickRespawnPose()
        {
            // Newest pose that is old enough; fall back to the oldest one we have.
            for (int i = 1; i <= m_SafeCount; i++)
            {
                int index = (m_SafeHead - i + k_History) % k_History;
                if (Time.time - m_SafePoseTimes[index] >= safePoseMinAge)
                    return m_SafePoses[index];
            }
            return m_SafePoses[(m_SafeHead - m_SafeCount + k_History) % k_History];
        }

        /// <summary>Optional external override, e.g. the last checkpoint once the race system exists.</summary>
        public void Respawn(Pose pose)
        {
            m_Body.linearVelocity = Vector3.zero;
            m_Body.angularVelocity = Vector3.zero;
            m_Body.position = pose.position + Vector3.up * 0.5f;
            m_Body.rotation = pose.rotation;
            transform.SetPositionAndRotation(m_Body.position, m_Body.rotation);
            m_FlippedTime = 0f;
            Respawned?.Invoke();
        }

        public void Respawn() => Respawn(PickRespawnPose());
    }
}
