using System.Collections.Generic;
using EricRacer.Core;
using EricRacer.Net;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EricRacer.UI
{
    /// <summary>
    /// The birthday title screen menu: type your name, host a game, or join one found on the Wi-Fi
    /// (or by IP as a fallback). Works with mouse, keyboard and gamepad.
    /// </summary>
    public class TitleMenu : MonoBehaviour
    {
        [SerializeField] private ConnectionManagerAnchor connection;
        [SerializeField] private LanDiscoveryAnchor discovery;
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private Button hostButton;
        [SerializeField] private RectTransform gamesList;
        [SerializeField] private Button gameButtonTemplate;
        [SerializeField] private TMP_Text noGamesText;
        [SerializeField] private TMP_InputField ipField;
        [SerializeField] private Button joinIpButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TMP_Text messageText;

        private readonly List<Button> m_GameButtons = new List<Button>();

        void Awake()
        {
            gameButtonTemplate.gameObject.SetActive(false);
            hostButton.onClick.AddListener(Host);
            joinIpButton.onClick.AddListener(() => Join(ipField.text));
            quitButton.onClick.AddListener(Application.Quit);
            nameField.characterLimit = LocalPlayerProfile.MaxNameLength;
            nameField.onEndEdit.AddListener(SaveName);
        }

        void OnEnable()
        {
            connection.Value.StateChanged += OnConnectionState;
            discovery.Value.HostsChanged += RebuildGames;
        }

        void OnDisable()
        {
            if (connection.IsSet)
                connection.Value.StateChanged -= OnConnectionState;
            if (discovery.IsSet)
                discovery.Value.HostsChanged -= RebuildGames;
        }

        void Start()
        {
            nameField.text = LocalPlayerProfile.Name;
            if (string.IsNullOrEmpty(ipField.text))
                ipField.text = "192.168.";
            messageText.text = connection.Value.LastDisconnectReason ?? string.Empty;
            RebuildGames();
        }

        void Update()
        {
            // Gamepads need something selected to navigate from; mouse clicks on empty space clear it.
            var events = EventSystem.current;
            if (events != null && (events.currentSelectedGameObject == null || !events.currentSelectedGameObject.activeInHierarchy))
                events.SetSelectedGameObject(hostButton.gameObject);
        }

        void SaveName(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                LocalPlayerProfile.Name = value;
        }

        void Host()
        {
            SaveName(nameField.text);
            messageText.text = "Starting the game...";
            connection.Value.StartHost();
        }

        void Join(string address)
        {
            SaveName(nameField.text);
            messageText.text = "Joining...";
            connection.Value.StartClient(address);
        }

        void OnConnectionState(ConnectionState state)
        {
            bool idle = state == ConnectionState.Offline;
            hostButton.interactable = idle;
            joinIpButton.interactable = idle;
            foreach (var button in m_GameButtons)
                button.interactable = idle;
            if (idle)
                messageText.text = connection.Value.LastDisconnectReason ?? string.Empty;
        }

        void RebuildGames()
        {
            foreach (var button in m_GameButtons)
                Destroy(button.gameObject);
            m_GameButtons.Clear();

            foreach (var host in discovery.Value.Hosts)
            {
                var button = Instantiate(gameButtonTemplate, gamesList);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<TMP_Text>().text = $"Join {host.HostName}   ({host.Players}/{host.MaxPlayers})";
                string address = host.Address;
                button.onClick.AddListener(() => Join(address));
                m_GameButtons.Add(button);
            }
            noGamesText.gameObject.SetActive(m_GameButtons.Count == 0);
        }
    }
}
