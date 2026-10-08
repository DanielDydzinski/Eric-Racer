using UnityEngine;

namespace EricRacer.Characters
{
    /// <summary>
    /// Everything that makes a racer look like themselves. Adding a family member = a Humanoid prefab with
    /// <see cref="CharacterView"/> + one of these assets added to the <see cref="CharacterCatalog"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Eric Racer/Character")]
    public class CharacterDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Racer";
        [SerializeField] private Sprite portrait;
        [SerializeField] private CharacterView driverPrefab;
        [Tooltip("Tint applied to the kart body so racers are easy to tell apart.")]
        [SerializeField] private Color kartColor = Color.white;

        public string DisplayName => displayName;
        public Sprite Portrait => portrait;
        public CharacterView DriverPrefab => driverPrefab;
        public Color KartColor => kartColor;
    }
}
