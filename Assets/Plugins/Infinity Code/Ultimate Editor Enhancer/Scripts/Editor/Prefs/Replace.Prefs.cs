/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool replace = true;

        public static KeyCode replaceKeyCode = KeyCode.H;
#if !UNITY_EDITOR_OSX
        public static EventModifiers replaceModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers replaceModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        public class ReplaceManager : StandalonePrefManager<ReplaceManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Replace"
                    };
                }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("Replace", ref replace, ref replaceKeyCode, ref replaceModifiers, EditorStyles.label, 17);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!replace) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Replace Selected GameObjects", "Everywhere", replaceModifiers, replaceKeyCode), 
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                replace = state; 
            }
        }
    }
}