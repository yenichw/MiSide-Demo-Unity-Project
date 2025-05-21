/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool switchCustomTool = true;
        public static KeyCode switchCustomToolKeyCode = KeyCode.U;
        public static EventModifiers switchCustomToolModifiers = EventModifiers.None;

        private class SwitchCustomToolManager : StandalonePrefManager<SwitchCustomToolManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Select Next Custom Tool"
                    };
                }
            }

            public override void Draw()
            {
                switchCustomTool = EditorGUILayout.ToggleLeft("Select Next Custom Tool", switchCustomTool);
                EditorGUI.indentLevel++;

                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = LabelWidth + 5;
                switchCustomToolKeyCode = (KeyCode)EditorGUILayout.EnumPopup("Hot Key", switchCustomToolKeyCode, GUILayout.Width(420));
                EditorGUIUtility.labelWidth = oldLabelWidth;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.Label("Modifiers", GUILayout.Width(ModifierLabelWidth + 15));
                switchCustomToolModifiers = DrawModifiers(switchCustomToolModifiers, true);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                List<Shortcut> shortcuts = new List<Shortcut>
                {
                    new Shortcut("Select Next Custom Tool", "Everywhere", switchCustomToolModifiers, switchCustomToolKeyCode)
                };
                return shortcuts;
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                switchCustomTool = state;
            }
        }
    }
}