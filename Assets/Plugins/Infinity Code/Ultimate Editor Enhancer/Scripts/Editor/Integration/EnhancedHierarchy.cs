/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Integration
{
    [InitializeOnLoad]
    public class EnhancedHierarchy: EditorWindow
    {
        const string PrefKey = Prefs.Prefix + "EnhancedHierarchy";

        private bool disableErrorIcons = true;
        private bool hideNativeIcons = true;
        private static FieldInfo rightMarginField;
        private static FieldInfo disableNativeIconField;

        public static bool isPresent { get; }

        static EnhancedHierarchy()
        {
            Assembly assembly = Reflection.GetAssembly("EnhancedHierarchyEditor");
            if (assembly == null) return;

            Type prefType = assembly.GetType("EnhancedHierarchy.Preferences");
            if (prefType == null) return;

            rightMarginField = prefType.GetField("RightMargin", Reflection.StaticLookup);
            disableNativeIconField = prefType.GetField("DisableNativeIcon", Reflection.StaticLookup);

            if (rightMarginField == null || disableNativeIconField == null) return;

            isPresent = true;

            EditorApplication.delayCall += ResolveConflicts;
        }

        public static int GetRightMargin()
        {
            if (!isPresent) return 0;

            object rm = rightMarginField.GetValue(null);
            object wrapper = Reflection.GetFieldValue(rm, "wrapper");
            return (int) Reflection.GetFieldValue(wrapper, "value");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Ultimate Editor Enhancer has detected the presence of Enhanced Hierarchy in the project.\nFor the best work of both assets together, we recommend the following changes in the settings:", MessageType.Warning);
            disableErrorIcons = EditorGUILayout.ToggleLeft("Ultimate Editor Enhancer / Show Error Icons - OFF", disableErrorIcons);
            hideNativeIcons = EditorGUILayout.ToggleLeft("Enhanced Hierarchy / Hide Native Icon - ON", hideNativeIcons);

            if (GUILayout.Button("Apply"))
            {
                if (disableErrorIcons)
                {
                    Prefs.hierarchyErrorIcons = false;
                    Prefs.Save();
                }

                if (hideNativeIcons)
                {
                    SetHideNativeIcons(true);
                }

                Close();
            }
        }

        public static void OpenWindow()
        {
            EnhancedHierarchy wnd = GetWindow<EnhancedHierarchy>(true);
            wnd.titleContent = new GUIContent("Optimize Settings");
            Rect rect = wnd.position;
            rect.width = 500;
            rect.height = 110;
            rect.x = (Screen.currentResolution.width - rect.width) / 2;
            rect.y = (Screen.currentResolution.height - rect.height) / 2;
            wnd.position = rect; 
        }

        private static void ResolveConflicts()
        {
            if (LocalSettings.enhancedHierarchyShown) return;
            OpenWindow();
            LocalSettings.enhancedHierarchyShown = true;
        }

        private static void SetHideNativeIcons(bool value)
        {
            if (!isPresent) return;
            object p = disableNativeIconField.GetValue(null);
            Reflection.SetPropertyValue(p, "Value", value);
        }

        public static void SetRightMargin(int margin)
        {
            if (!isPresent) return;

            object rm = rightMarginField.GetValue(null);
            object wrapper = Reflection.GetFieldValue(rm, "wrapper");
            Reflection.SetFieldValue(wrapper, "value", margin);
            Reflection.SetFieldValue(rm, "wrapper", wrapper);
        }
    }
}