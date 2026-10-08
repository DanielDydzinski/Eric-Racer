using System.Collections;
using EricRacer.Core;
using UnityEngine;

namespace EricRacer.Net.Dev
{
    /// <summary>Starts hosting or joining straight away when the game is launched with <c>-host</c> or <c>-join ip</c>.</summary>
    public class LaunchOptionsConnector : MonoBehaviour
    {
        [SerializeField] private ConnectionManager connection;

        IEnumerator Start()
        {
            // Let Boot hand over to the menu first, exactly as when a person clicks Host/Join.
            yield return null;

            if (LaunchOptions.AutoHost)
                connection.StartHost();
            else if (!string.IsNullOrEmpty(LaunchOptions.JoinAddress))
                connection.StartClient(LaunchOptions.JoinAddress);
        }
    }
}
