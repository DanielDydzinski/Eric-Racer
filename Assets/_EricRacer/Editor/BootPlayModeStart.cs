using UnityEditor;
using UnityEditor.SceneManagement;

namespace EricRacer.Editor
{
    /// <summary>
    /// Pressing Play always starts from the Boot scene, so networking services exist no matter which scene is open.
    /// </summary>
    [InitializeOnLoad]
    static class BootPlayModeStart
    {
        private const string k_BootScene = "Assets/_EricRacer/Scenes/Boot.unity";

        static BootPlayModeStart()
        {
            EditorApplication.delayCall += () =>
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(k_BootScene);
        }
    }
}
