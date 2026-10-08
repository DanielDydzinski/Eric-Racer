using System.Collections;
using EricRacer.Core;
using UnityEngine;

namespace EricRacer.Dev
{
    /// <summary>
    /// Test-only (<c>-screenshot 20</c>): saves a screenshot next to the log after that many seconds,
    /// so a built game's visuals can be checked without anyone looking at the screen.
    /// </summary>
    public class DevScreenshot : MonoBehaviour
    {
        IEnumerator Start()
        {
            if (LaunchOptions.ScreenshotAfterSeconds <= 0f)
                yield break;
            yield return new WaitForSecondsRealtime(LaunchOptions.ScreenshotAfterSeconds);
            string file = System.IO.Path.Combine(Application.persistentDataPath, $"shot_{LocalPlayerProfile.Name}.png");
            ScreenCapture.CaptureScreenshot(file);
            Debug.Log($"[Dev] Screenshot saved to {file}");
        }
    }
}
