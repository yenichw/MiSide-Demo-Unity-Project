/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool objectPlacer = true;

#if !UNITY_EDITOR_OSX
        public static EventModifiers objectPlacerModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers objectPlacerModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        public class ObjectPlacerManager : StandalonePrefManager<ObjectPlacerManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Object Placer"
                    };
                }
            }

            public override float order
            {
                get { return -42; }
            }

            public override void Draw()
            {
                objectPlacer = EditorGUILayout.ToggleLeft("Object Placer", objectPlacer, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                EditorGUI.BeginDisabledGroup(!objectPlacer);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Modifiers", GUILayout.Width(LabelWidth));
                objectPlacerModifiers = DrawModifiers(objectPlacerModifiers);
                EditorGUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel--;
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!objectPlacer) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Place Object", "Scene View", objectPlacerModifiers, "RMB"),
                    new Shortcut("Place With Alignment", "Create Browser", "ENTER"),
                    new Shortcut("Place Without Alignment", "Create Browser", "SHIFT + ENTER"),

                    new Shortcut("Place In Root With Alignment", "Create Browser", 
#if !UNITY_EDITOR_OSX
                        "CTRL + ENTER"
#else
                        "CMD + ENTER"
#endif
                        ),
                    new Shortcut("Place In Root Without Alignment", "Create Browser", 
#if !UNITY_EDITOR_OSX
                        "CTRL + SHIFT + ENTER"
#else
                        "CMD + SHIFT + ENTER"
#endif
                        ),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                objectPlacer = state;
            }
        }
    }
}