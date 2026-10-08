using System;

namespace EricRacer.Core
{
    /// <summary>
    /// Command line switches for testing several game windows on one PC:
    /// <c>-host</c>, <c>-join 127.0.0.1</c>, <c>-name Kamil</c>, <c>-autodrive</c>, <c>-netlog</c>, <c>-laps 1</c>, <c>-players 3</c>.
    /// </summary>
    public static class LaunchOptions
    {
        public static bool AutoHost { get; }
        public static string JoinAddress { get; }
        public static string PlayerName { get; }
        public static bool AutoDrive { get; }
        public static bool NetLog { get; }
        /// <summary>Overrides the lap count for quick tests; 0 means use the race settings.</summary>
        public static int Laps { get; }
        /// <summary>Test runs: the grid waits for this many players before the countdown (the lobby does this for real games).</summary>
        public static int ExpectedPlayers { get; }
        /// <summary>Test runs: start with this character instead of the saved one (-1 = not set).</summary>
        public static int CharacterIndex { get; } = -1;
        /// <summary>Test runs: save a screenshot after this many seconds (0 = off).</summary>
        public static float ScreenshotAfterSeconds { get; }

        static LaunchOptions()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string next = i + 1 < args.Length ? args[i + 1] : null;
                switch (args[i].ToLowerInvariant())
                {
                    case "-host": AutoHost = true; break;
                    case "-join": JoinAddress = next; break;
                    case "-name": PlayerName = next; break;
                    case "-autodrive": AutoDrive = true; break;
                    case "-netlog": NetLog = true; break;
                    case "-laps": Laps = int.TryParse(next, out int laps) ? laps : 0; break;
                    case "-players": ExpectedPlayers = int.TryParse(next, out int players) ? players : 0; break;
                    case "-character": CharacterIndex = int.TryParse(next, out int character) ? character : -1; break;
                    case "-screenshot": ScreenshotAfterSeconds = float.TryParse(next, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float seconds) ? seconds : 0f; break;
                }
            }
        }
    }
}
