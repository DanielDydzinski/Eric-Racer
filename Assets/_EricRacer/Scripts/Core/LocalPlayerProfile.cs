using System;
using UnityEngine;

namespace EricRacer.Core
{
    /// <summary>The name this PC's player goes by. Remembered between sessions.</summary>
    public static class LocalPlayerProfile
    {
        private const string k_NameKey = "player_name";
        public const int MaxNameLength = 16;

        public static string Name
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(LaunchOptions.PlayerName))
                    return LaunchOptions.PlayerName;
                string saved = PlayerPrefs.GetString(k_NameKey, string.Empty);
                return string.IsNullOrWhiteSpace(saved) ? Environment.UserName : saved;
            }
            set
            {
                string trimmed = (value ?? string.Empty).Trim();
                if (trimmed.Length > MaxNameLength)
                    trimmed = trimmed.Substring(0, MaxNameLength);
                PlayerPrefs.SetString(k_NameKey, trimmed);
                PlayerPrefs.Save();
            }
        }
    }
}
