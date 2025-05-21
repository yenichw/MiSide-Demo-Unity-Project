/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool dropToFloor = true;
        public static bool advancedDropToFloor = true;
        public static KeyCode dropToFloorKeyCode = KeyCode.End;

        public static EventModifiers dropToFloorModifiers = EventModifiers.Shift;

#if !UNITY_EDITOR_OSX
        public static EventModifiers advancedDropToFloorModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers advancedDropToFloorModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        public class DropToFloorManager : StandalonePrefManager<DropToFloorManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get 
                { 
                    return new[]
                    {
                        "Drop To Floor",
                        "Advanced"
                    };
                }
            }

            public override void Draw()
            {
                dropToFloor = EditorGUILayout.ToggleLeft("Drop To Floor", dropToFloor, EditorStyles.label);

                EditorGUI.BeginDisabledGroup(!dropToFloor);
                EditorGUI.indentLevel++;

                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = LabelWidth + 5;
                dropToFloorKeyCode = (KeyCode)EditorGUILayout.EnumPopup("Hot Key", dropToFloorKeyCode, GUILayout.Width(420));
                EditorGUIUtility.labelWidth = oldLabelWidth;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.Label("Modifiers", GUILayout.Width(ModifierLabelWidth + 15));
                dropToFloorModifiers = DrawModifiers(dropToFloorModifiers, true);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                GUILayout.Label("Advanced Modifiers", GUILayout.Width(ModifierLabelWidth + 15));
                advancedDropToFloorModifiers = DrawModifiers(advancedDropToFloorModifiers, true);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                List<Shortcut> shortcuts = new List<Shortcut>();

                if (dropToFloor)
                {
                    shortcuts.Add(new Shortcut("Drop Selected GameObject To Floor", "Scene View", dropToFloorModifiers, dropToFloorKeyCode));
                }

                if (advancedDropToFloor)
                {
                    shortcuts.Add(new Shortcut("Drop Selected GameObject To Floor (Advanced)", "Scene View", advancedDropToFloorModifiers, dropToFloorKeyCode));
                }

                return shortcuts;
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                dropToFloor = state;
                advancedDropToFloor = state;
            }
        }
    }
}