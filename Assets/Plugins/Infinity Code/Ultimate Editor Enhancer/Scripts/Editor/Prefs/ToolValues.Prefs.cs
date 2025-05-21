/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool showToolValues = true;
#if !UNITY_EDITOR_OSX
        public static EventModifiers toolValuesModifiers = EventModifiers.Control;
#else
        public static EventModifiers toolValuesModifiers = EventModifiers.Command;
#endif

        public class ToolValuesManager : StandalonePrefManager<ToolValuesManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Show Tool Values In Scene View"
                    };
                }
            }

            public override void Draw()
            {
                showToolValues = EditorGUILayout.ToggleLeft("Show Tool Values In SceneView", showToolValues);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Modifiers", GUILayout.Width(ModifierLabelWidth + 15));
                toolValuesModifiers = DrawModifiers(toolValuesModifiers, true);
                EditorGUILayout.EndHorizontal();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!group) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Show Tool Values", "Scene View", toolValuesModifiers),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                showToolValues = state;
            }
        }
    }
}