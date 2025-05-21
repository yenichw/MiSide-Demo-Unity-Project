/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class SelectCustomTool
    {
        const double waitTime = 0.5;
        
        private static Type lastCustomTool;
        private static double pressTime = 0;

        static SelectCustomTool()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidateShortcut;
            binding.OnPress += StartWaitShowMenu;
            binding.OnRelease += Select;

#if UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanged += OnActiveToolChanged;
#else
            EditorTools.activeToolChanged += OnActiveToolChanged;
#endif
        }

        private static Type GetActiveToolType()
        {
#if UNITY_2020_2_OR_NEWER
            return ToolManager.activeToolType;
#else
            return EditorTools.activeToolType;
#endif
        }

        private static void OnActiveToolChanged()
        {
            List<Type> tools = EditorToolUtilityRef.GetCustomEditorToolsForType(null);
            Type activeToolType = GetActiveToolType();
            if (tools != null && tools.Any(t => t == activeToolType)) lastCustomTool = activeToolType;
        }

        private static bool OnValidateShortcut()
        {
            if (!Prefs.switchCustomTool) return false;
            if (EditorWindow.focusedWindow != null && !(EditorWindow.focusedWindow is SceneView)) return false;
            Event e = Event.current;
            if (e.keyCode != Prefs.switchCustomToolKeyCode || e.modifiers != Prefs.switchCustomToolModifiers) return false;
            return !EditorGUIRef.IsEditingTextField();
        }

        private static void Select()
        {
            EditorApplication.update -= WaitShowMenu;
            pressTime = 0;

            if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == GameViewRef.type)
            {
                return;
            }
            
            List<Type> tools = EditorToolUtilityRef.GetCustomEditorToolsForType(null);
            
            if (tools.Any(t => t == GetActiveToolType()))
            {
                int index = tools.IndexOf(lastCustomTool) + 1;
                SetTool(tools[index < tools.Count? index: 0]);
            }
            else
            {
                if (lastCustomTool == null) SetTool(tools[0]);
                else SetTool(lastCustomTool);
            }
        }

        private static void SetTool(Type type)
        {
#if UNITY_2020_2_OR_NEWER
            ToolManager.SetActiveTool(type);
#else
            EditorTools.SetActiveTool(type);
#endif
            string label;
            object[] attributes = type.GetCustomAttributes(typeof(EditorToolAttribute), true);
            if (attributes.Length > 0)
            {
                label = (attributes[0] as EditorToolAttribute).displayName;
            }
            else label = type.Name;

            foreach (SceneView view in SceneView.sceneViews)
            {
                view.ShowNotification(TempContent.Get(label), 1);
            }
        }

        private static void ShowMenu()
        {
            pressTime = 0;
            List<Type> tools = EditorToolUtilityRef.GetCustomEditorToolsForType(null);
            Type activeToolType = GetActiveToolType();

            GenericMenuEx menu = GenericMenuEx.Start();
            for (int i = 0; i < tools.Count; i++)
            {
                Type type = tools[i];
                string label;

                object[] attributes = type.GetCustomAttributes(typeof(EditorToolAttribute), true);
                if (attributes.Length > 0) label = (attributes[0] as EditorToolAttribute).displayName;
                else label = type.Name;

                menu.Add(label, type == activeToolType, () => SetTool(type));
            }

            menu.Show();
        }

        private static void StartWaitShowMenu()
        {
            if (pressTime != 0) return;
            if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == GameViewRef.type) return;
            
            pressTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += WaitShowMenu;
        }

        private static void WaitShowMenu()
        {
            if (EditorApplication.timeSinceStartup - pressTime < waitTime) return;

            EditorApplication.update -= WaitShowMenu;
            SceneViewManager.OnNextGUI += ShowMenu;
            
            SceneView.RepaintAll();
        }
    }
}