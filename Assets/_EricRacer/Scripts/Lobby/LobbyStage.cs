using EricRacer.Net;
using UnityEngine;

namespace EricRacer.Lobby
{
    /// <summary>Gives each player a pedestal, in join order (the host is always first).</summary>
    public class LobbyStage : MonoBehaviour
    {
        [SerializeField] private SessionRoster sessions;
        [SerializeField] private LobbySlotView[] slots;

        void OnEnable()
        {
            sessions.Changed += Rebind;
            Rebind();
        }

        void OnDisable() => sessions.Changed -= Rebind;

        void Rebind()
        {
            for (int i = 0; i < slots.Length; i++)
                slots[i].Bind(i < sessions.Players.Count ? sessions.Players[i] : null);
        }
    }
}
