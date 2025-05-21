/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool preview = true;

        public static KeyCode previewKeyCode = KeyCode.Q;
        public static EventModifiers previewModifiers = EventModifiers.Control | EventModifiers.Shift;

        public class PreviewManager : StandalonePrefManager<PreviewManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Preview"
                    };
                }
            }

            public override float order
            {
                get { return -33; }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("Preview", ref preview, ref previewKeyCode, ref previewModifiers, EditorStyles.label, 17);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!preview) return new Shortcut[0];

                return new []
                {
                    new Shortcut("Preview of Cameras", "Scene View", previewModifiers, previewKeyCode),
                    new Shortcut("Set View", "Preview of Cameras", "F"),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                preview = state;
            }
        }
    }
}