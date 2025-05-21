/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool ungroup = true;

        public static KeyCode ungroupKeyCode = KeyCode.G;

#if !UNITY_EDITOR_OSX
        public static EventModifiers ungroupModifiers = EventModifiers.Control | EventModifiers.Alt;
#else
        public static EventModifiers ungroupModifiers = EventModifiers.Command | EventModifiers.Alt;
#endif

        private class UngroupManager : StandalonePrefManager<UngroupManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Ungroup"
                    };
                }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("Ungroup", ref ungroup, ref ungroupKeyCode, ref ungroupModifiers, EditorStyles.label, 17);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!ungroup) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Ungroup GameObjects", "Everywhere", ungroupModifiers, ungroupKeyCode),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                ungroup = state;
            }
        }
    }
}