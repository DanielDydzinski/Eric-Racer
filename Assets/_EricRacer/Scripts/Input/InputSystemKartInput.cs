using KartGame.KartSystems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EricRacer.Input
{
    /// <summary>
    /// Human driver strategy for <see cref="ArcadeKart"/>: reads keyboard/gamepad through the Input System.
    /// Disable this component on karts the local player doesn't own.
    /// </summary>
    public class InputSystemKartInput : BaseInput
    {
        [SerializeField] private InputActionReference steer;
        [SerializeField] private InputActionReference accelerate;
        [SerializeField] private InputActionReference brake;

        void OnEnable()
        {
            steer.action.Enable();
            accelerate.action.Enable();
            brake.action.Enable();
        }

        public override InputData GenerateInput()
        {
            if (!enabled)
                return default;

            return new InputData
            {
                TurnInput = Mathf.Clamp(steer.action.ReadValue<float>(), -1f, 1f),
                Accelerate = accelerate.action.IsPressed(),
                Brake = brake.action.IsPressed()
            };
        }
    }
}
