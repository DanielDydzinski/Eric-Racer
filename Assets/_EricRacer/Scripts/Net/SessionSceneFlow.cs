using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EricRacer.Net
{
    /// <summary>
    /// Moves between the menu and the online scenes as the connection changes:
    /// Boot → menu, hosting → first online scene (clients follow the host automatically), offline → back to the menu.
    /// </summary>
    public class SessionSceneFlow : MonoBehaviour
    {
        [SerializeField] private ConnectionManager connection;
        [SerializeField] private string menuScene = "MainMenu";
        [Tooltip("Scene the host opens once the game is created (the lobby, once it exists).")]
        [SerializeField] private string sessionStartScene = "Race_Oval";

        void OnEnable() => connection.StateChanged += OnStateChanged;
        void OnDisable() => connection.StateChanged -= OnStateChanged;

        void Start()
        {
            if (connection.State == ConnectionState.Offline && SceneManager.GetActiveScene().name != menuScene)
                SceneManager.LoadScene(menuScene);
        }

        void OnStateChanged(ConnectionState state)
        {
            switch (state)
            {
                case ConnectionState.Hosting:
                    NetworkManager.Singleton.SceneManager.LoadScene(sessionStartScene, LoadSceneMode.Single);
                    break;
                case ConnectionState.Offline:
                    if (SceneManager.GetActiveScene().name != menuScene)
                        SceneManager.LoadScene(menuScene);
                    break;
            }
        }
    }
}
