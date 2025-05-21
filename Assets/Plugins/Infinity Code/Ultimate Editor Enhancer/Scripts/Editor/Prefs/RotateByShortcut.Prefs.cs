/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool rotateByShortcut = true;

        private class RotateByShortcutManager : StandalonePrefManager<RotateByShortcutManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Rotate By Shortcut" }; }
            }

            public override void Draw()
            {
                rotateByShortcut = EditorGUILayout.ToggleLeft("Rotate By Shortcut", rotateByShortcut);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!rotateByShortcut) return new Shortcut[0];

#if !UNITY_EDITOR_OSX
                EventModifiers modifiers = EventModifiers.Control | EventModifiers.Shift;
#else
                EventModifiers modifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

                return new[]
                {
                    new Shortcut("Rotate Selection Y -90°", "Scene View", modifiers, KeyCode.LeftArrow),
                    new Shortcut("Rotate Selection Y +90°", "Scene View", modifiers, KeyCode.RightArrow),
                    new Shortcut("Rotate Selection From Myself -90°", "Scene View", modifiers, KeyCode.UpArrow),
                    new Shortcut("Rotate Selection To Myself +90°", "Scene View", modifiers, KeyCode.DownArrow),
                    new Shortcut("Rotate Selection By View Clockwise -90°", "Scene View", modifiers, KeyCode.PageUp),
                    new Shortcut("Rotate Selection By View Counterclockwise +90°", "Scene View", modifiers, KeyCode.PageDown),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                rotateByShortcut = state;
            }
        }
    }
}