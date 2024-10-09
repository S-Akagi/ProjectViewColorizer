using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Alto.Editor
{
    public class ProjectViewColorizerWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "Project View Colorizer";
        private const string MENU_ITEM_PATH = "Tools/ProjectViewColorizer/Configure";
        private const string DEFAULT_GROUP_NAME = "新しいグループ";
        private const int SCROLL_VIEW_HEIGHT = 300;

        private List<ColorGroup> colorGroups;
        private Vector2 scrollPos;

        [MenuItem(MENU_ITEM_PATH)]
        public static void ShowWindow()
        {
            GetWindow<ProjectViewColorizerWindow>(WINDOW_TITLE);
        }

        private void OnEnable()
        {
            colorGroups = ProjectViewColorizer.GetColorGroups();
        }

        private void OnGUI()
        {
            DrawRecommendedSettingsSection();
            DrawColorGroupSettingsSection();
            DrawAddGroupButton();
            DrawSaveSettingsButton();
        }

        private void DrawRecommendedSettingsSection()
        {
            GUILayout.Label("おすすめの設定を作成", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("注意: このボタンを押すと、デフォルトのカラーグループとフォルダが作成されます。\n　　　　すでにカラーグループがある場合は、それらが削除されます。", MessageType.Warning);

            if (GUILayout.Button("カラーグループとフォルダを作成"))
            {
                CreateDefaultGroupsAndFolders();
            }

            GUILayout.Space(20);
        }

        private void CreateDefaultGroupsAndFolders()
        {
            colorGroups.Clear();
            ProjectViewColorizer.InitializeDefaultGroups();
            colorGroups = ProjectViewColorizer.GetColorGroups();
            ProjectViewColorizer.UpdateSettings(colorGroups);

            CreateDefaultFolders();

            EditorUtility.DisplayDialog("成功", "カラーグループとフォルダが作成されました。", "OK");
        }

        private void DrawColorGroupSettingsSection()
        {
            GUILayout.Label("ディレクトリカラーの設定", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(SCROLL_VIEW_HEIGHT));

            for (int i = 0; i < colorGroups.Count; i++)
            {
                DrawColorGroupRow(i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawColorGroupRow(int index)
        {
            EditorGUILayout.BeginHorizontal();
            colorGroups[index].groupName = EditorGUILayout.TextField("グループ名", colorGroups[index].groupName);
            colorGroups[index].groupColor = EditorGUILayout.ColorField("グループカラー", colorGroups[index].groupColor);

            if (GUILayout.Button("削除", GUILayout.Width(40)))
            {
                colorGroups.RemoveAt(index);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAddGroupButton()
        {
            if (GUILayout.Button("グループを追加"))
            {
                colorGroups.Add(new ColorGroup { groupName = DEFAULT_GROUP_NAME, groupColor = Color.white });
            }
        }

        private void DrawSaveSettingsButton()
        {
            EditorGUILayout.HelpBox("保存しないと設定が反映されません。", MessageType.Warning);
            if (GUILayout.Button("設定を保存"))
            {
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            if (ValidateGroupNames())
            {
                ProjectViewColorizer.UpdateSettings(colorGroups);
                EditorUtility.DisplayDialog("成功", "設定が保存されました。", "OK");
            }
        }

        private bool ValidateGroupNames()
        {
            for (int i = 0; i < colorGroups.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(colorGroups[i].groupName))
                {
                    EditorUtility.DisplayDialog("無効な入力", "グループ名は空白にできません。", "OK");
                    return false;
                }
            }
            return true;
        }

        private void CreateDefaultFolders()
        {
            string[] folders = { 
                "Assets/XR", "Assets/Packages", "Assets/Editor", "Assets/Avatar", 
                "Assets/Cloth", "Assets/Props", "Assets/Scripts", "Assets/Tools", 
                "Assets/Scenes", "Assets/Shaders" 
            };

            foreach (string folder in folders)
            {
                CreateFolderIfNotExists(folder);
            }

            AssetDatabase.Refresh();
        }

        private void CreateFolderIfNotExists(string folderPath)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parent = Path.GetDirectoryName(folderPath);
                string newFolderName = Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parent, newFolderName);
            }
        }
    }
}