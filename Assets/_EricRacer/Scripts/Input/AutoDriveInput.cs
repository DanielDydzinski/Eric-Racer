using EricRacer.Core;
using KartGame.KartSystems;
using UnityEngine;

namespace EricRacer.Input
{
    /// <summary>
    /// Test-only driver strategy: holds the throttle and weaves gently, so multiplayer sync can be
    /// checked with several game windows nobody is holding a controller for. Enabled by <c>-autodrive</c>.
    /// Must sit after <see cref="InputSystemKartInput"/> on the kart so it takes priority when enabled.
    /// </summary>
    public class AutoDriveInput : BaseInput
    {
        [SerializeField] private float weaveSpeed = 0.6f;
        [SerializeField] private float weaveAmount = 0.5f;

        void Awake()
        {
            if (!LaunchOptions.AutoDrive)
                enabled = false;
        }

        public override InputData GenerateInput()
        {
            return new InputData
            {
                Accelerate = true,
                TurnInput = Mathf.Sin(Time.time * weaveSpeed) * weaveAmount
            };
        }
    }
}
