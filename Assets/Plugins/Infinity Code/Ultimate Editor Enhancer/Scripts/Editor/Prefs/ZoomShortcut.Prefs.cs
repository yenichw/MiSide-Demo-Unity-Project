/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool zoomShortcut = true;

        public static KeyCode zoomInShortcutKeyCode = KeyCode.Equals;
        public static KeyCode zoomOutShortcutKeyCode = KeyCode.Minus;

#if !UNITY_EDITOR_OSX
        public static EventModifiers zoomShortcutModifiers = EventModifiers.Control;
        public static EventModifiers zoomBoostShortcutModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers zoomShortcutModifiers = EventModifiers.Command;
        public static EventModifiers zoomBoostShortcutModifiers = EventModifiers.Command | EventModifiers.Shift;

#endif

        public class ZoomShortcutManager : StandalonePrefManager<ZoomShortcutManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Zoom By Shortcut",
                        "Zoom In Hot Key",
                        "Zoom Out Hot Key",
                        "Boost"
                    };
                }
            }

            public override float order
            {
                get { return -34; }
            }

            public override void Draw()
            {
                zoomShortcut = EditorGUILayout.ToggleLeft("Zoom By Shortcut", zoomShortcut);
                
                EditorGUI.BeginDisabledGroup(!zoomShortcut);
                EditorGUI.indentLevel++;

                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = LabelWidth + 5;
                zoomInShortcutKeyCode = (KeyCode)EditorGUILayout.EnumPopup("Zoom In Hot Key", zoomInShortcutKeyCode, GUILayout.Width(420));
                zoomOutShortcutKeyCode = (KeyCode)EditorGUILayout.EnumPopup("Zoom Out Hot Key", zoomOutShortcutKeyCode, GUILayout.Width(420));
                EditorGUIUtility.labelWidth = oldLabelWidth;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.Label("Modifiers", GUILayout.Width(ModifierLabelWidth + 15));
                zoomShortcutModifiers = DrawModifiers(zoomShortcutModifiers);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.Label("Boost Modifiers", GUILayout.Width(ModifierLabelWidth + 15));
                zoomBoostShortcutModifiers = DrawModifiers(zoomBoostShortcutModifiers);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!zoomShortcut) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Fast Move Forward", "Scene View", zoomShortcutModifiers, zoomInShortcutKeyCode), 
                    new Shortcut("Fast Move Backward", "Scene View", zoomShortcutModifiers, zoomOutShortcutKeyCode),
                    new Shortcut("Super Fast Move Forward", "Scene View", zoomBoostShortcutModifiers, zoomInShortcutKeyCode),
                    new Shortcut("Super Fast Move Backward", "Scene View", zoomBoostShortcutModifiers, zoomOutShortcutKeyCode),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                zoomShortcut = state;
            }
        }
    }
}