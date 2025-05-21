/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.Editors;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [InitializeOnLoad]
    public class MigrationWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        static MigrationWindow()
        {
#if UCONTEXT
            EditorApplication.delayCall += () =>
            {
                MigrationWindow wnd = GetWindow<MigrationWindow>(true, "Migration to Ultimate Editor Enhancer", true);
                wnd.minSize = new Vector2(500, 300);
            };
#endif
        }

        private static void DeleteFolders(int step)
        {
            EditorGUILayout.LabelField($"{step}. Delete Assets / Plugins / Infinity Code / uContext folder");
#if UCONTEXT_PRO
            EditorGUILayout.LabelField($"{step}.1. Delete Assets / Plugins / Infinity Code / uContext Pro folder");
#endif
            if (GUILayout.Button("Delete Folder(s)"))
            {
                for (int i = 0; i < 2; i++)
                {
                    string basicPath = "Assets/Plugins/Infinity Code/uContext";
                    if (Directory.Exists(basicPath)) AssetDatabase.DeleteAsset(basicPath);

                    string proPath = basicPath + " Pro";
                    if (Directory.Exists(proPath)) AssetDatabase.DeleteAsset(proPath);
                }
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private static void FinalMessage()
        {
            EditorGUILayout.LabelField("This is all. Enjoy using Ultimate Editor Enhancer.");
        }

        private static void MigrateSceneReferences(int step)
        {
            EditorGUILayout.LabelField($"{step}. Migrate scene references:");
            EditorGUILayout.LabelField($"{step}.1. Open a scene that contains hierarchy backgrounds, bookmarks for objects in the scene, or view states and click Update References button.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Update References")) UpdateReferences();
            EditorGUILayout.LabelField($"{step}.2. Repeat step {step}.1 for every scene that has scene references.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Note: If you skip this step, all hierarchy backgrounds and bookmarks will be lost.\nView states can be restored by selecting the GameObject containing them and using Missing Script Fixer.", EditorStyles.wordWrappedLabel);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private static void MigrateSettings(int step)
        {
            EditorGUILayout.LabelField($"{step}. Migrate settings and references to objects in the project:");
            EditorGUILayout.LabelField($"{step}.1. Open uContext settings");
            if (GUILayout.Button("Open Settings")) SettingsService.OpenProjectSettings("Project/uContext");
            EditorGUILayout.LabelField($"{step}.2. File / Export / Settings");
            EditorGUILayout.LabelField($"{step}.3. File / Export / Items / Everything");
            EditorGUILayout.LabelField($"{step}.4. Open Ultimate Editor Enhancer settings");
            if (GUILayout.Button("Open Settings")) Settings.OpenSettings();
            EditorGUILayout.LabelField($"{step}.5. File / Import / Settings");
            EditorGUILayout.LabelField($"{step}.6. File / Import / Items");

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField(@"Ultimate Editor Enhancer has detected that you are using uContext in the project.
These assets are incompatible but upgradable.
To properly upgrade uContext to Ultimate Editor Enhancer, follow these steps:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            MigrateSettings(1);
            MigrateSceneReferences(2);
            DeleteFolders(3);
            RemoveDefineSymbols(4);
            FinalMessage();

            EditorGUILayout.EndScrollView();
        }

        private static void RemoveDefineSymbols(int step)
        {
#if UCONTEXT_PRO
            string symbolStr = "UCONTEXT and UCONTEXT_PRO";
#else
            string symbolStr = "UCONTEXT";
#endif

            EditorGUILayout.LabelField($"{step}. Open Player Settings and remove {symbolStr} define symbols");
            if (GUILayout.Button("Remove Symbol(s)"))
            {
                BuildTargetGroup g = EditorUserBuildSettings.selectedBuildTargetGroup;
            
#if UNITY_2023_1_OR_NEWER
                UnityEditor.Build.NamedBuildTarget buildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(g);
                string currentDefinitions = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
#else
                string currentDefinitions = PlayerSettings.GetScriptingDefineSymbolsForGroup(g);
#endif
                
                if (!string.IsNullOrEmpty(currentDefinitions))
                {
                    List<string> keys = currentDefinitions.Split(';').ToList();
                    keys.Remove("UCONTEXT");
                    keys.Remove("UCONTEXT_PRO");

                    currentDefinitions = string.Join(";", keys);
#if UNITY_2023_1_OR_NEWER
                    PlayerSettings.SetScriptingDefineSymbols(buildTarget, currentDefinitions);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(g, currentDefinitions);
#endif
                }
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private static void ReplaceAllScripts(Type oldType, Type newType)
        {
            if (oldType == null) return;

            MonoScript script = MissedScriptEditor.scripts.FirstOrDefault(s => s.GetClass() == newType);
            Object[] objects = ObjectHelper.FindObjectsOfType(oldType, true);
            foreach (Object obj in objects)
            {
                SerializedObject so = new SerializedObject(obj);
                try
                {
                    SerializedProperty sp = so.FindProperty("m_Script");
                    sp.objectReferenceValue = script;
                    so.ApplyModifiedProperties();
                }
                catch
                {
                    
                }
            }
        }

        private static void UpdateReferences()
        {
            Assembly assembly = Assembly.Load("uContext");
            if (assembly == null) return;

            Type sceneReferencesType = assembly.GetType("InfinityCode.uContext.SceneReferences");
            if (sceneReferencesType != null)
            {
                Object[] sceneReferences = ObjectHelper.FindObjectsOfType(sceneReferencesType, true);

                foreach (Object sr in sceneReferences)
                {
                    (sr as Component).gameObject.name = "Ultimate Editor Enhancer References";
                }

                ReplaceAllScripts(sceneReferencesType, typeof(SceneReferences));
            }

            ReplaceAllScripts(assembly.GetType("InfinityCode.uContext.TemporaryContainer"), typeof(TemporaryContainer));
            ReplaceAllScripts(assembly.GetType("InfinityCode.uContext.ViewState"), typeof(ViewState));
            ReplaceAllScripts(assembly.GetType("InfinityCode.uContext.HideInPreview"), typeof(HideInPreview));
            ReplaceAllScripts(assembly.GetType("InfinityCode.uContext.FlattenCollection"), typeof(FlattenCollection));
        }
    }
}