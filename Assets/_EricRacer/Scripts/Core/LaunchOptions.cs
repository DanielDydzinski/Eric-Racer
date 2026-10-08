using System;

namespace EricRacer.Core
{
    /// <summary>
    /// Command line switches for testing several game windows on one PC:
    /// <c>-host</c>, <c>-join 127.0.0.1</c>, <c>-name Kamil</c>, <c>-autodrive</c>, <c>-netlog</c>.
    /// </summary>
    public static class LaunchOptions
    {
        public static bool AutoHost { get; }
        public static string JoinAddress { get; }
        public static string PlayerName { get; }
        public static bool AutoDrive { get; }
        public static bool NetLog { get; }

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
                }
            }
        }
    }
}
