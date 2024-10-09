using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Alto.Editor
{
    [System.Serializable]
    public class ColorGroup
    {
        public string groupName;
        public Color groupColor;
    }

    public static class ProjectViewColorizer
    {
        private const string MENU_PATH = "Tools/ProjectViewColorizer/Active";
        private const string SETTINGS_KEY_PREFIX = "ProjectViewColorizer_";
        private static readonly Color DEFAULT_COLOR = new Color(0, 0, 0, 1f);

        private static List<ColorGroup> colorGroups = new List<ColorGroup>();

        static ProjectViewColorizer()
        {
            LoadSettings();
            Initialize();
        }

        [MenuItem(MENU_PATH)]
        private static void ToggleEnabled()
        {
            bool isEnabled = Menu.GetChecked(MENU_PATH);
            Menu.SetChecked(MENU_PATH, !isEnabled);
            Initialize();
        }

        public static void Initialize()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowGUI;
            if (Menu.GetChecked(MENU_PATH))
            {
                EditorApplication.projectWindowItemOnGUI += OnProjectWindowGUI;
            }
        }

        private static void OnProjectWindowGUI(string guid, Rect selectionRect)
        {
            if (!Menu.GetChecked(MENU_PATH))
                return;

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            int directoryLevel = GetDirectoryLevel(assetPath);
            Color color = GetColorByDirectoryLevel(directoryLevel);

            DrawColoredBox(selectionRect, color);
        }

        private static void DrawColoredBox(Rect selectionRect, Color color)
        {
            if (color != DEFAULT_COLOR)
            {
                Color originalColor = GUI.color;
                GUI.color = color;
                GUI.Box(selectionRect, GUIContent.none);
                GUI.color = originalColor;
            }
        }

        private static int GetDirectoryLevel(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            if (parts.Length >= 1)
            {
                string topLevel = parts.Length == 1 ? parts[0] : parts[1];
                return GetGroupIndex(topLevel);
            }
            return -1;
        }

        private static int GetGroupIndex(string groupName)
        {
            return colorGroups.FindIndex(group => group.groupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
        }

        private static Color GetColorByDirectoryLevel(int directoryLevel)
        {
            return (directoryLevel >= 0 && directoryLevel < colorGroups.Count) 
                ? colorGroups[directoryLevel].groupColor 
                : DEFAULT_COLOR;
        }

        private static Color HexToRGBA(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.clear;
        }

        private static void SaveSettings()
        {
            for (int i = 0; i < colorGroups.Count; i++)
            {
                EditorPrefs.SetString($"{SETTINGS_KEY_PREFIX}GroupName_{i}", colorGroups[i].groupName);
                EditorPrefs.SetString($"{SETTINGS_KEY_PREFIX}GroupColor_{i}", ColorUtility.ToHtmlStringRGBA(colorGroups[i].groupColor));
            }
            EditorPrefs.SetInt($"{SETTINGS_KEY_PREFIX}GroupCount", colorGroups.Count);
        }

        private static void LoadSettings()
        {
            int count = EditorPrefs.GetInt($"{SETTINGS_KEY_PREFIX}GroupCount", 0);
            colorGroups.Clear();

            for (int i = 0; i < count; i++)
            {
                string name = EditorPrefs.GetString($"{SETTINGS_KEY_PREFIX}GroupName_{i}", string.Empty);
                string colorString = EditorPrefs.GetString($"{SETTINGS_KEY_PREFIX}GroupColor_{i}", "FFFFFFFF");
                
                if (!string.IsNullOrEmpty(name) && ColorUtility.TryParseHtmlString("#" + colorString, out Color color))
                {
                    colorGroups.Add(new ColorGroup { groupName = name, groupColor = color });
                }
            }
        }

        public static void InitializeDefaultGroups()
        {
            string[] defaultGroupNames = { "Assets", "XR", "Packages", "Editor", "Avatar", "Cloth", "Props", "Scripts", "Tools", "Scenes", "Shaders" };
            string[] defaultGroupColors = {
                "#1abc9cFF", "#1abc9cFF", "#3498dbFF", "#9b59b6FF", "#e74c3cFF",
                "#e67e22FF", "#f1c40fFF", "#2ecc71FF", "#0ff119FF", "#d90ff1FF", "#95a5a6FF"
            };

            colorGroups.Clear();
            for (int i = 0; i < defaultGroupNames.Length; i++)
            {
                Color color = HexToRGBA(defaultGroupColors[i]);
                colorGroups.Add(new ColorGroup { groupName = defaultGroupNames[i], groupColor = color });
            }
        }

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            Initialize();
        }

        public static void UpdateSettings(List<ColorGroup> newGroups)
        {
            colorGroups = newGroups;
            SaveSettings();
            Initialize();
        }

        public static List<ColorGroup> GetColorGroups()
        {
            return new List<ColorGroup>(colorGroups);
        }
    }
}