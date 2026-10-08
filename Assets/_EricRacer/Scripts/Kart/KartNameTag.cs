using TMPro;
using UnityEngine;

namespace EricRacer.Kart
{
    /// <summary>Floating name above other players' karts (not your own), always facing the camera.</summary>
    public class KartNameTag : MonoBehaviour
    {
        [SerializeField] private KartAppearance appearance;
        [SerializeField] private TextMeshPro label;

        private Camera m_Camera;

        void OnEnable()
        {
            appearance.Refreshed += Refresh;
            Refresh();
        }

        void OnDisable() => appearance.Refreshed -= Refresh;

        void Refresh()
        {
            var owner = appearance.Owner;
            bool show = owner != null && !appearance.IsOwner;
            label.gameObject.SetActive(show);
            if (!show)
                return;
            label.text = owner.PlayerName;
            label.color = Color.Lerp(owner.Character.KartColor, Color.white, 0.35f);
        }

        void LateUpdate()
        {
            if (!label.gameObject.activeSelf)
                return;
            if (m_Camera == null)
                m_Camera = Camera.main;
            if (m_Camera != null)
                label.transform.rotation = Quaternion.LookRotation(label.transform.position - m_Camera.transform.position);
        }
    }
}
