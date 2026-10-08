using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace EricRacer.Editor
{
    /// <summary>
    /// House style for UI, re-runnable from the menu after adding new UI:
    /// rounded panels/buttons and one TextMeshPro material preset per text category.
    /// Presets are named "<font> - <Category>" so they appear in TMP's Material Preset dropdown.
    /// </summary>
    public static class UiStyleTools
    {
        private const string k_Root = "Assets/_EricRacer";
        private const string k_RoundedSprite = k_Root + "/Art/UI/RoundedRect.png";
        private const string k_FontDir = k_Root + "/Art/Fonts";
        private const int k_SpriteSize = 128;
        private const int k_CornerPixels = 48;

        public enum TextCategory { Banner, Hud, UI, WorldLabel }

        // Which texts are which: banners and in-race HUD by name; any 3D text is a world label; everything else is UI.
        private static readonly HashSet<string> k_BannerNames = new HashSet<string> { "Banner", "Title", "Subtitle" };
        private static readonly HashSet<string> k_HudNames = new HashSet<string> { "LapText", "PositionText", "CountdownText", "MessageText" };

        public const string ArrowSprite = k_Root + "/Art/UI/Arrow.png";

        [MenuItem("Eric Racer/UI/Create Style Assets")]
        public static void CreateStyleAssets()
        {
            CreateRoundedSprite();
            CreateArrowSprite();
            CreateTextPresets();
        }

        /// <summary>A soft, chunky right-pointing arrow (flip X for left), used by the lobby's 3D browse arrows.</summary>
        static void CreateArrowSprite()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            // Triangle with rounded feel: inside if left of both slanted edges and right of the back edge.
            var a = new Vector2(24, 12); var b = new Vector2(24, 116); var tip = new Vector2(112, 64);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Mathf.Min(EdgeDistance(p, a, tip), Mathf.Min(EdgeDistance(p, tip, b), EdgeDistance(p, b, a)));
                float alpha = Mathf.Clamp01(d - 6f + 0.5f); // 6px inset rounds the corners off visually
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            File.WriteAllBytes(ArrowSprite, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(ArrowSprite);
            var importer = (TextureImporter)AssetImporter.GetAtPath(ArrowSprite);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128;
            importer.mipmapEnabled = true;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        // Signed distance to the edge from->to; positive on the inside of a counter-clockwise triangle.
        static float EdgeDistance(Vector2 p, Vector2 from, Vector2 to)
        {
            Vector2 edge = (to - from).normalized;
            Vector2 normal = new Vector2(-edge.y, edge.x);
            return Vector2.Dot(p - from, normal);
        }

        [MenuItem("Eric Racer/UI/Apply Style To All Scenes And Prefabs")]
        public static void ApplyEverywhere()
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(k_RoundedSprite);
            var presets = LoadPresets();

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { k_Root + "/Prefabs" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                if (Apply(root.transform, sprite, presets))
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }

            string openScene = EditorSceneManager.GetActiveScene().path;
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { k_Root + "/Scenes" }))
            {
                var scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid), OpenSceneMode.Single);
                bool changed = false;
                foreach (var root in scene.GetRootGameObjects())
                    if (!PrefabUtility.IsPartOfPrefabInstance(root))
                        changed |= Apply(root.transform, sprite, presets);
                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            if (!string.IsNullOrEmpty(openScene))
                EditorSceneManager.OpenScene(openScene, OpenSceneMode.Single);
        }

        static bool Apply(Transform root, Sprite rounded, Dictionary<TextCategory, Material> presets)
        {
            bool changed = false;

            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                var rect = (RectTransform)image.transform;
                bool fullScreen = rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one;
                bool isInputCaret = image.GetComponent<TMP_SelectionCaret>() != null;
                if (fullScreen || isInputCaret || (image.sprite != null && image.sprite != rounded))
                    continue;

                float smallestSide = Mathf.Min(Mathf.Abs(rect.sizeDelta.x), Mathf.Abs(rect.sizeDelta.y));
                float radius = smallestSide > 0f ? Mathf.Clamp(smallestSide * 0.3f, 10f, 32f) : 24f;
                image.sprite = rounded;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = k_CornerPixels / radius;
                EditorUtility.SetDirty(image);
                changed = true;
            }

            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                var material = presets[Categorize(text)];
                if (text.fontSharedMaterial == material)
                    continue;
                text.fontSharedMaterial = material;
                EditorUtility.SetDirty(text);
                changed = true;
            }
            return changed;
        }

        public static TextCategory Categorize(TMP_Text text)
        {
            string name = text.gameObject.name.Replace("Shadow", string.Empty);
            if (k_BannerNames.Contains(name))
                return TextCategory.Banner;
            if (k_HudNames.Contains(name))
                return TextCategory.Hud;
            return text is TextMeshPro ? TextCategory.WorldLabel : TextCategory.UI;
        }

        static Dictionary<TextCategory, Material> LoadPresets()
        {
            var font = TMP_Settings.defaultFontAsset;
            var presets = new Dictionary<TextCategory, Material>();
            foreach (TextCategory category in System.Enum.GetValues(typeof(TextCategory)))
                presets[category] = AssetDatabase.LoadAssetAtPath<Material>(PresetPath(font, category));
            return presets;
        }

        static string PresetPath(TMP_FontAsset font, TextCategory category) => $"{k_FontDir}/{font.name} - {category}.mat";

        static void CreateTextPresets()
        {
            Directory.CreateDirectory(k_FontDir);
            var font = TMP_Settings.defaultFontAsset;
            var navy = new Color(0.05f, 0.08f, 0.25f, 1f);

            // Outline width / colour, drop shadow (underlay) and glyph thickening per category.
            Create(font, TextCategory.Banner, outline: 0.30f, navy, underlay: true, dilate: 0.25f);
            Create(font, TextCategory.Hud, outline: 0.25f, navy, underlay: true, dilate: 0.2f);
            Create(font, TextCategory.UI, outline: 0.15f, new Color(0f, 0f, 0f, 0.85f), underlay: false, dilate: 0.1f);
            Create(font, TextCategory.WorldLabel, outline: 0.25f, Color.black, underlay: false, dilate: 0.15f);
            AssetDatabase.SaveAssets();
        }

        static void Create(TMP_FontAsset font, TextCategory category, float outline, Color outlineColor, bool underlay, float dilate)
        {
            string path = PresetPath(font, category);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(font.material) { name = Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(material, path);
            }

            material.EnableKeyword(ShaderUtilities.Keyword_Outline);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, outline);
            material.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
            material.SetFloat(ShaderUtilities.ID_FaceDilate, dilate);
            if (underlay)
            {
                material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.45f));
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.6f);
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.6f);
                material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.4f);
            }
            else
            {
                material.DisableKeyword(ShaderUtilities.Keyword_Underlay);
            }
            EditorUtility.SetDirty(material);
        }

        static void CreateRoundedSprite()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(k_RoundedSprite));
            var texture = new Texture2D(k_SpriteSize, k_SpriteSize, TextureFormat.RGBA32, false);
            float r = k_CornerPixels;
            for (int y = 0; y < k_SpriteSize; y++)
            for (int x = 0; x < k_SpriteSize; x++)
            {
                // Distance from the nearest corner circle centre; inside the straight edges the distance is 0.
                float cx = Mathf.Clamp(x + 0.5f, r, k_SpriteSize - r);
                float cy = Mathf.Clamp(y + 0.5f, r, k_SpriteSize - r);
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                float alpha = Mathf.Clamp01(r - distance + 0.5f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
            File.WriteAllBytes(k_RoundedSprite, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(k_RoundedSprite);

            var importer = (TextureImporter)AssetImporter.GetAtPath(k_RoundedSprite);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = new Vector4(k_CornerPixels, k_CornerPixels, k_CornerPixels, k_CornerPixels);
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
    }
}
