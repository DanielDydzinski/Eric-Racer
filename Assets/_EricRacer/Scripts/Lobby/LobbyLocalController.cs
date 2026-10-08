using EricRacer.Core;
using EricRacer.Input;
using EricRacer.Net;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EricRacer.Lobby
{
    /// <summary>
    /// This PC's lobby controls: flick left/right to browse racers, A to get ready, B to change your mind.
    /// The host also gets laps (shoulders / Q,E) and START.
    /// </summary>
    public class LobbyLocalController : MonoBehaviour
    {
        [SerializeField] private SessionRoster sessions;
        [SerializeField] private LobbyManager lobby;
        [SerializeField] private InputActionReference browse;
        [SerializeField] private InputActionReference ready;
        [SerializeField] private InputActionReference unready;
        [SerializeField] private InputActionReference hostLaps;
        [SerializeField] private InputActionReference hostStart;

        private readonly AxisStepper m_Browse = new AxisStepper();
        private readonly AxisStepper m_Laps = new AxisStepper();
        private PlayerSession m_ResetFor;

        void OnEnable()
        {
            foreach (var action in new[] { browse, ready, unready, hostLaps, hostStart })
                action.action.Enable();
            ready.action.performed += OnReady;
            unready.action.performed += OnUnready;
            hostStart.action.performed += OnHostStart;
        }

        void OnDisable()
        {
            ready.action.performed -= OnReady;
            unready.action.performed -= OnUnready;
            hostStart.action.performed -= OnHostStart;
        }

        void Update()
        {
            var me = sessions.Local;
            if (me == null)
                return;

            // Sessions outlive scenes: everyone starts each lobby visit un-ready.
            if (m_ResetFor != me)
            {
                m_ResetFor = me;
                me.SetReady(false);
            }

            int step = m_Browse.Step(browse.action.ReadValue<float>());
            if (step != 0)
                me.BrowseCharacter(step);

            if (lobby.IsHost)
                lobby.ChangeLaps(m_Laps.Step(hostLaps.action.ReadValue<float>()));

            if (LaunchOptions.AutoDrive && !me.IsReady)
                me.SetReady(true);
        }

        void OnReady(InputAction.CallbackContext _) => sessions.Local?.SetReady(true);
        void OnUnready(InputAction.CallbackContext _) => sessions.Local?.SetReady(false);
        void OnHostStart(InputAction.CallbackContext _) => lobby.StartRace();
    }
}
