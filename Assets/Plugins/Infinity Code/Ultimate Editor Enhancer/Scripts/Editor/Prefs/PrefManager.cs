/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public abstract class PrefManager
        {
            public const int LabelWidth = 210;
            public const int ModifierLabelWidth = LabelWidth - 30;
            public const int SpaceBeforeModifiers = 35;

            public virtual IEnumerable<string> keywords
            {
                get { return new string[0]; }
            }

            public virtual float order
            {
                get { return 0; }
            }

            public virtual void Draw()
            {
            }

            protected static EventModifiers DrawModifiers(EventModifiers modifier, bool hideFunction = false)
            {
                bool control = (modifier & EventModifiers.Control) == EventModifiers.Control;
                control = GUILayout.Toggle(control, "Control", GUILayout.ExpandWidth(false));

                bool shift = (modifier & EventModifiers.Shift) == EventModifiers.Shift;
                shift = GUILayout.Toggle(shift, "Shift", GUILayout.ExpandWidth(false));

                bool alt = (modifier & EventModifiers.Alt) == EventModifiers.Alt;
                alt = GUILayout.Toggle(alt, "Alt", GUILayout.ExpandWidth(false));

                bool function = (modifier & EventModifiers.FunctionKey) == EventModifiers.FunctionKey;
                if (!hideFunction) function = GUILayout.Toggle(function, "Function", GUILayout.ExpandWidth(false));

                bool command = (modifier & EventModifiers.Command) == EventModifiers.Command;
                command = GUILayout.Toggle(command, "Command", GUILayout.ExpandWidth(false));

                modifier = 0;
                if (control) modifier |= EventModifiers.Control;
                if (shift) modifier |= EventModifiers.Shift;
                if (alt) modifier |= EventModifiers.Alt;
                if (command) modifier |= EventModifiers.Command;
                if (function) modifier |= EventModifiers.FunctionKey;
                return modifier;
            }

            protected static void DrawFieldWithHotKey(string label, ref bool field, ref KeyCode hotKey, ref EventModifiers modifiers, GUIStyle labelStyle = null, int spaceBeforeLabels = SpaceBeforeModifiers)
            {
                if (labelStyle == null) labelStyle = EditorStyles.label;
                field = EditorGUILayout.ToggleLeft(label, field, labelStyle);
                EditorGUI.BeginDisabledGroup(!field);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(spaceBeforeLabels);
                GUILayout.Label("Hot Key", GUILayout.Width(EditorGUI.indentLevel == 0? ModifierLabelWidth: ModifierLabelWidth - 15));
                hotKey = (KeyCode)EditorGUILayout.EnumPopup(hotKey, GUILayout.ExpandWidth(false));
                modifiers = DrawModifiers(modifiers);
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }

            protected static void DrawFieldWithModifiers(string label, ref bool field, ref EventModifiers modifiers, int labelWidth = LabelWidth)
            {
                EditorGUILayout.BeginHorizontal();
                field = EditorGUILayout.ToggleLeft(label, field, GUILayout.Width(labelWidth));
                EditorGUI.BeginDisabledGroup(!field);
                modifiers = DrawModifiers(modifiers);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            public virtual void SetState(bool state)
            {
                
            }
        }
    }
}