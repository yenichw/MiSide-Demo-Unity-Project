/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Integration;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class MaximizeGameView
    {
        static MaximizeGameView()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += () => Prefs.improveMaximizeGameViewBehaviour;
            binding.OnPress += OnInvoke;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            EditorWindow wnd = EditorWindow.focusedWindow;
            if (wnd != null && 
                ((wnd.GetType() == GameViewRef.type && e.keyCode == KeyCode.Space && e.modifiers == EventModifiers.Shift) || 
                 (!FullscreenEditor.isPresent && e.keyCode == KeyCode.F11 && e.modifiers == EventModifiers.FunctionKey)))
            {
                if (EditorApplication.isPlaying && LocalSettings.askMaximizeGameView)
                {
                    LocalSettings.askMaximizeGameView = false;

                    if (!EditorUtility.DisplayDialog(
                        "Select Behavior",
                        "Ultimate Editor Enhancer can maximize GameView at runtime. Do you want to use it?\nYou can change this selection in the asset settings.",
                        "Maximize", "Ignore"))
                    {
                        Prefs.improveMaximizeGameViewBehaviour = false;
                        Prefs.Save();
                        return;
                    }
                }

                WindowsHelper.SetMaximized(wnd, !wnd.maximized);
                Event.current.Use();
            }
        }
    }
}