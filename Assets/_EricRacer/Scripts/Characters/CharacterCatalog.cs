using UnityEngine;

namespace EricRacer.Characters
{
    /// <summary>The characters players can pick. The network only ever sends an index into this list.</summary>
    [CreateAssetMenu(menuName = "Eric Racer/Character Catalog")]
    public class CharacterCatalog : ScriptableObject
    {
        [SerializeField] private CharacterDefinition[] characters;

        public int Count => characters.Length;

        public CharacterDefinition Get(int index) => characters[Wrap(index)];

        /// <summary>Keeps browsing going round in a circle: past the last character comes the first.</summary>
        public int Wrap(int index) => ((index % Count) + Count) % Count;
    }
}
