/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool selectionHistory = true;

        public static KeyCode selectionHistoryPrevKeyCode = KeyCode.Comma;
        public static KeyCode selectionHistoryNextKeyCode = KeyCode.Period;
#if !UNITY_EDITOR_OSX
        public static EventModifiers selectionHistoryModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers selectionHistoryModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        private class SelectionHistoryManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Selection History",
                        "Set Prev Selection",
                        "Set Next Selection"
                    };
                }
            }

            public override float order
            {
                get { return Order.SelectionHistory; }
            }

            public override void Draw()
            {
                selectionHistory = EditorGUILayout.ToggleLeft("Selection History", selectionHistory);
                EditorGUI.indentLevel++;

                EditorGUI.BeginDisabledGroup(!selectionHistory);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(17);
                GUILayout.Label("Set Prev Selection", GUILayout.Width(ModifierLabelWidth - 15));
                selectionHistoryPrevKeyCode = (KeyCode)EditorGUILayout.EnumPopup(selectionHistoryPrevKeyCode, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(17);
                GUILayout.Label("Set Next Selection", GUILayout.Width(ModifierLabelWidth - 15));
                selectionHistoryNextKeyCode = (KeyCode)EditorGUILayout.EnumPopup(selectionHistoryNextKeyCode, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Modifiers", GUILayout.Width(ModifierLabelWidth + 15));
                selectionHistoryModifiers = DrawModifiers(selectionHistoryModifiers);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;

                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!selectionHistory) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Set Prev GameObject Selection", "Everywhere", selectionHistoryModifiers, selectionHistoryPrevKeyCode),
                    new Shortcut("Set Next GameObject Selection", "Everywhere", selectionHistoryModifiers, selectionHistoryNextKeyCode),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                selectionHistory = state;
            }
        }
    }
}