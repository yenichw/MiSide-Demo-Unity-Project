// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ClothParam????????????????????
    /// </summary>
    public static class EditorPresetUtility
    {
        const string configName = "preset folder";

        public static void DrawPresetButton(MonoBehaviour owner, ClothParams clothParam)
        {
            using (var horizontalScope = new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);

                GUI.backgroundColor = Color.green;
                if (EditorGUILayout.DropdownButton(new GUIContent("Preset"), FocusType.Keyboard, GUILayout.Width(70), GUILayout.Height(16)))
                {
                    CreatePresetPopupMenu(owner, clothParam);
                    GUI.backgroundColor = Color.white;
                    GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Save", GUILayout.Width(40), GUILayout.Height(16)))
                {
                    SavePreset(owner, clothParam);
                    GUIUtility.ExitGUI();
                }
                if (GUILayout.Button("Load", GUILayout.Width(40), GUILayout.Height(16)))
                {
                    LoadPreset(owner, clothParam);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private static string GetComponentTypeName(MonoBehaviour owner)
        {
            string componentTypeName = string.Empty;
            if (owner is MagicaBoneCloth)
                componentTypeName = "BoneCloth";
            else if (owner is MagicaBoneSpring)
                componentTypeName = "BoneSpring";
            else if (owner is MagicaMeshCloth)
                componentTypeName = "MeshCloth";
            else if (owner is MagicaMeshSpring)
                componentTypeName = "MeshSpring";

            return componentTypeName;
        }


        private class PresetInfo
        {
            public string presetPath;
            public string presetName;
            public TextAsset text;
        }

        private static void CreatePresetPopupMenu(MonoBehaviour owner, ClothParams clothParam)
        {
            // ?????????????????????????
            string presetTypeName = GetComponentTypeName(owner);
            if (string.IsNullOrEmpty(presetTypeName))
                return;

            var guidArray = AssetDatabase.FindAssets($"{presetTypeName} t:" + nameof(TextAsset));
            if (guidArray == null)
                return;

            Dictionary<string, List<PresetInfo>> dict = new Dictionary<string, List<PresetInfo>>();
            foreach (var guid in guidArray)
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guid);

                // json??
                if (filePath.EndsWith(".json") == false)
                    continue;

                var text = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
                if (text)
                {
                    var info = new PresetInfo();
                    info.presetPath = filePath;
                    var fname = Path.GetFileNameWithoutExtension(filePath);
                    fname = fname.Replace(presetTypeName, "");
                    if (fname.StartsWith("_"))
                        fname = fname.Remove(0, 1); // ??_?????
                    info.presetName = fname;
                    info.text = text;

                    // ?????????????
                    var dirName = Path.GetDirectoryName(filePath);
                    if (dict.ContainsKey(dirName) == false)
                    {
                        dict.Add(dirName, new List<PresetInfo>());
                    }
                    dict[dirName].Add(info);
                }
            }

            // ?????????????
            // ??????????????????????
            var menu = new GenericMenu();
            int line = 0;
            foreach (var kv in dict)
            {
                if (line > 0)
                {
                    menu.AddSeparator("");
                }
                foreach (var info in kv.Value)
                {
                    var textAsset = info.text;
                    var presetName = info.presetName;
                    var presetPath = info.presetPath;
                    menu.AddItem(new GUIContent(presetName), false, () =>
                    {
                        var json = textAsset.text;
                        //Debug.Log(json);

                        // load
                        Debug.Log("Load preset file:" + presetPath);
                        LoadClothParam(owner, clothParam, json);
                        Debug.Log("Complete.");
                    });
                }
                line++;
            }
            menu.ShowAsContext();
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="clothParam"></param>
        private static void SavePreset(MonoBehaviour owner, ClothParams clothParam)
        {
            // ?????????
            string folder = EditorUserSettings.GetConfigValue(configName);

            // ???
            string presetTypeName = GetComponentTypeName(owner);

            // ???????
            string path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                "Save Preset",
                $"{presetTypeName}_xxx",
                "json",
                "Enter a name for the preset json.",
                folder
                );
            if (string.IsNullOrEmpty(path))
                return;

            // ???????
            folder = Path.GetDirectoryName(path);
            EditorUserSettings.SetConfigValue(configName, folder);

            Debug.Log("Save preset file:" + path);

            // json
            string json = JsonUtility.ToJson(clothParam);

            // save
            File.WriteAllText(path, json);

            AssetDatabase.Refresh();

            Debug.Log("Complete.");
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="clothParam"></param>
        private static void LoadPreset(MonoBehaviour owner, ClothParams clothParam)
        {
            // ?????????
            string folder = EditorUserSettings.GetConfigValue(configName);

            // ?????????
            string path = UnityEditor.EditorUtility.OpenFilePanel("Load Preset", folder, "json");
            if (string.IsNullOrEmpty(path))
                return;

            // ???????
            folder = Path.GetDirectoryName(path);
            EditorUserSettings.SetConfigValue(configName, folder);

            // json
            Debug.Log("Load preset file:" + path);
            string json = File.ReadAllText(path);

            // load
            LoadClothParam(owner, clothParam, json);

            Debug.Log("Complete.");
        }

        /// <summary>
        /// json????????????????
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="clothParam"></param>
        /// <param name="json"></param>
        private static void LoadClothParam(MonoBehaviour owner, ClothParams clothParam, string json)
        {
            if (string.IsNullOrEmpty(json) == false)
            {
                // ??????????????
                Transform influenceTarget = clothParam.GetInfluenceTarget();
                Transform disableReferenceObject = clothParam.DisableReferenceObject;
                //Transform directionalDampingObject = clothParam.DirectionalDampingObject;

                // undo
                Undo.RecordObject(owner, "Load preset");

                JsonUtility.FromJsonOverwrite(json, clothParam);

                // ????????????????
                clothParam.SetInfluenceTarget(influenceTarget);
                clothParam.DisableReferenceObject = disableReferenceObject;
                //clothParam.DirectionalDampingObject = directionalDampingObject;
            }
        }
    }
}
