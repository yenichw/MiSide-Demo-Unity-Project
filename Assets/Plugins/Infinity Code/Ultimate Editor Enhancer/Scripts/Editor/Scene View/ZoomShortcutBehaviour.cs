/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class ZoomShortcutBehaviour
    {
        private const int Speed = 10;

        static ZoomShortcutBehaviour()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidate;
            binding.OnPress += OnInvoke;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            if (e.modifiers != Prefs.zoomShortcutModifiers && e.modifiers != Prefs.zoomBoostShortcutModifiers) return;

            if (e.keyCode == Prefs.zoomInShortcutKeyCode) MoveCamera(-1);
            else if (e.keyCode == Prefs.zoomOutShortcutKeyCode) MoveCamera(1);
        }

        private static void MoveCamera(int direction)
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (Event.current.modifiers == Prefs.zoomBoostShortcutModifiers) direction *= 5;
            if (!view.orthographic)
            {
                try
                {
                    view.pivot += view.rotation * new Vector3(0, 0, direction * -30);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
            else
            {
                view.size = Mathf.Abs(view.size) * (float)(direction * Speed * 0.0149999996647239 + 1.0);
            }

            Event.current.Use();
        }

        private static bool OnValidate()
        {
            if (!Prefs.zoomShortcut) return false;
            if (!(EditorWindow.focusedWindow is SceneView)) return false;
            return true;
        }
    }
}