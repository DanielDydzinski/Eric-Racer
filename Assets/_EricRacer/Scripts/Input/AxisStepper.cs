namespace EricRacer.Input
{
    /// <summary>Turns a held axis (stick, d-pad, arrow keys) into single steps: one flick = one step.</summary>
    public class AxisStepper
    {
        private const float k_Press = 0.6f;
        private const float k_Release = 0.3f;
        private bool m_Held;

        /// <returns>-1 or +1 on the frame the axis is pushed, otherwise 0.</returns>
        public int Step(float value)
        {
            float magnitude = value < 0f ? -value : value;
            if (m_Held)
            {
                if (magnitude < k_Release)
                    m_Held = false;
                return 0;
            }
            if (magnitude < k_Press)
                return 0;
            m_Held = true;
            return value > 0f ? 1 : -1;
        }
    }
}
