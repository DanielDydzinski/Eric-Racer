using EricRacer.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EricRacer.UI
{
    /// <summary>
    /// Small overlay to leave the game. The game keeps running behind it (it's multiplayer).
    /// "Resume" is selected first so a stray A press just closes it.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField] private ConnectionManagerAnchor connection;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private TMP_Text leaveLabel;

        void Awake()
        {
            resumeButton.onClick.AddListener(Close);
            leaveButton.onClick.AddListener(Leave);
            panel.SetActive(false);
        }

        void OnEnable()
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnToggle;
        }

        void OnDisable() => toggleAction.action.performed -= OnToggle;

        void OnToggle(InputAction.CallbackContext _)
        {
            if (panel.activeSelf)
                Close();
            else
                Open();
        }

        void Open()
        {
            bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
            leaveLabel.text = isHost ? "END GAME FOR EVERYONE" : "LEAVE GAME";
            panel.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(resumeButton.gameObject);
        }

        void Close()
        {
            panel.SetActive(false);
            EventSystem.current?.SetSelectedGameObject(null);
        }

        void Leave() => connection.Value.Leave();
    }
}
