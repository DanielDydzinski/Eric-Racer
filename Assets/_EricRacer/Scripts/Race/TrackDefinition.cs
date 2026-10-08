using UnityEngine;

namespace EricRacer.Race
{
    [CreateAssetMenu(menuName = "Eric Racer/Track")]
    public class TrackDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Oval";
        [Tooltip("Scene name as listed in Build Settings.")]
        [SerializeField] private string sceneName = "Race_Oval";
        [SerializeField] private Sprite thumbnail;

        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public Sprite Thumbnail => thumbnail;
    }
}
