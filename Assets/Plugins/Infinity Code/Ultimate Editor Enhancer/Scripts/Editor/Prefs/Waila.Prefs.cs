/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool waila = true;
        public static bool wailaShowNameUnderCursor = true;
        public static bool wailaShowAllNamesUnderCursor = true;
        public static bool wailaSmartSelection = true;
        public static SmartSelectionStyle wailaSmartSelectionStyle = SmartSelectionStyle.flat;

#if !UNITY_EDITOR_OSX
        public static EventModifiers wailaShowNameUnderCursorModifiers = EventModifiers.Control;
        public static EventModifiers wailaShowAllNamesUnderCursorModifiers = EventModifiers.Control | EventModifiers.Shift;
        public static KeyCode wailaSmartSelectionKeyCode = KeyCode.Space;
        public static EventModifiers wailaSmartSelectionModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers wailaShowNameUnderCursorModifiers = EventModifiers.Command;
        public static EventModifiers wailaShowAllNamesUnderCursorModifiers = EventModifiers.Command | EventModifiers.Shift;
        public static KeyCode wailaSmartSelectionKeyCode = KeyCode.Space;
        public static EventModifiers wailaSmartSelectionModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        public class WailaManager : StandalonePrefManager<WailaManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Show All Names Under Cursor",
                        "Smart Selection",
                        "Waila (What Am I Looking At)",
                        "Show Name Under Cursor"
                    };
                }
            }

            public override float order
            {
                get { return -50; }
            }

            public override void Draw()
            {
                waila = EditorGUILayout.ToggleLeft("Waila (What Am I Looking At)", waila);
                EditorGUI.indentLevel++;

                EditorGUI.BeginDisabledGroup(!waila);

                DrawFieldWithModifiers("Show Name Under Cursor", ref wailaShowNameUnderCursor, ref wailaShowNameUnderCursorModifiers, LabelWidth + 40);
                DrawFieldWithModifiers("Show All Names Under Cursor", ref wailaShowAllNamesUnderCursor, ref wailaShowAllNamesUnderCursorModifiers, LabelWidth + 40);
                DrawFieldWithHotKey("Smart Selection", ref wailaSmartSelection, ref wailaSmartSelectionKeyCode, ref wailaSmartSelectionModifiers);
                wailaSmartSelectionStyle = (SmartSelectionStyle)EditorGUILayout.EnumPopup("Smart Selection Style", wailaSmartSelectionStyle);

                EditorGUI.indentLevel--;

                EditorGUI.EndDisabledGroup();
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!waila) return new Shortcut[0];

                List<Shortcut> shortcuts = new List<Shortcut>();

                if (wailaShowNameUnderCursor)
                {
                    shortcuts.Add(new Shortcut("Show Name Of GameObject Under Cursor", "Scene View", wailaShowNameUnderCursorModifiers));
                }

                if (wailaShowAllNamesUnderCursor)
                {
                    shortcuts.Add(new Shortcut("Show Names Of All GameObject Under Cursor", "Scene View", wailaShowAllNamesUnderCursorModifiers));
                }

                if (wailaSmartSelection)
                {
                    shortcuts.Add(new Shortcut("Start Smart Selection", "Scene View", wailaSmartSelectionModifiers, wailaSmartSelectionKeyCode));
                }

                return shortcuts;
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                waila = state;
            }
        }
    }
}