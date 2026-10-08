using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EricRacer.Editor
{
    /// <summary>
    /// Lists every empty object reference on our own components (EricRacer.*) in our scenes and prefabs.
    /// Menu: Eric Racer/Validate References. Fields that are optional by design are skipped.
    /// </summary>
    public static class ReferenceValidator
    {
        private const string k_Root = "Assets/_EricRacer";
        private static readonly HashSet<string> k_Optional = new HashSet<string>
        {
            "KartCharacterSlot.kartAnimator",
            "CharacterDefinition.portrait",
            "TrackDefinition.thumbnail",
        };

        [MenuItem("Eric Racer/Validate References")]
        public static void ValidateFromMenu()
        {
            var problems = Validate();
            if (problems.Count == 0)
                Debug.Log("[Validate] All references set.");
            foreach (var problem in problems)
                Debug.LogWarning("[Validate] " + problem);
        }

        public static List<string> Validate()
        {
            var problems = new List<string>();

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { k_Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (var behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
                    Check(behaviour, path, problems);
            }

            string openScene = EditorSceneManager.GetActiveScene().path;
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { k_Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                        Check(behaviour, path, problems);
            }
            if (!string.IsNullOrEmpty(openScene))
                EditorSceneManager.OpenScene(openScene, OpenSceneMode.Single);

            return problems;
        }

        static void Check(MonoBehaviour behaviour, string where, List<string> problems)
        {
            if (behaviour == null)
            {
                problems.Add($"{where}: missing script");
                return;
            }
            var type = behaviour.GetType();
            if (type.Namespace == null || !type.Namespace.StartsWith("EricRacer"))
                return;

            var property = new SerializedObject(behaviour).GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = property.propertyType != SerializedPropertyType.String;
                if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script")
                    continue;
                if (property.objectReferenceValue != null || k_Optional.Contains($"{type.Name}.{property.name}"))
                    continue;
                problems.Add($"{where}: {behaviour.gameObject.name}/{type.Name}.{property.propertyPath} is empty");
            }
        }
    }
}
