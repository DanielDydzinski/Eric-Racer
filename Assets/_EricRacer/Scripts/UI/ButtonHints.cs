using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EricRacer.UI
{
    /// <summary>
    /// Turns tokens like {A}, {LB} or {Space} in a message into coloured button labels, so every
    /// "press this" in the game looks the same: B/X/Y in their pad colours, A and the other pad buttons
    /// in pink-purple, keyboard keys as yellow [Key]. Unknown tokens are left as typed.
    /// </summary>
    public static class ButtonHints
    {
        private const string k_PadOther = "#C9A7FF";
        private const string k_Key = "#FFD84D";

        private static readonly Dictionary<string, string> k_Styles = new Dictionary<string, string>
        {
            // Gamepad face buttons (Xbox colours)
            { "A", Pad("A", k_PadOther) },
            { "B", Pad("B", "#FF4B4B") },
            { "X", Pad("X", "#3D9BFF") },
            { "Y", Pad("Y", "#FFD23F") },
            // Other gamepad buttons
            { "LB", Pad("LB", k_PadOther) },
            { "RB", Pad("RB", k_PadOther) },
            { "START", Pad("START", k_PadOther) },
            { "VIEW", Pad("VIEW", k_PadOther) },
            { "DPad", Pad("D-pad", k_PadOther) },
            { "Left", Pad("<", k_PadOther) },
            { "Right", Pad(">", k_PadOther) },
            // Keyboard keys
            { "Space", Key("Space") },
            { "Enter", Key("Enter") },
            { "Esc", Key("Esc") },
            { "Backspace", Key("Backspace") },
            { "Arrows", Key("Arrows") },
            { "Q", Key("Q") },
            { "E", Key("E") },
            { "R", Key("R") },
            { "WASD", Key("WASD") },
        };

        private static readonly Regex k_Token = new Regex(@"\{(\w+)\}");

        public static string Format(string template)
        {
            if (string.IsNullOrEmpty(template))
                return template;
            return k_Token.Replace(template, match =>
                k_Styles.TryGetValue(match.Groups[1].Value, out string styled) ? styled : match.Value);
        }

        static string Pad(string label, string colour) => $"<b><color={colour}>{label}</color></b>";
        static string Key(string label) => $"<b><color={k_Key}>[{label}]</color></b>";
    }
}
