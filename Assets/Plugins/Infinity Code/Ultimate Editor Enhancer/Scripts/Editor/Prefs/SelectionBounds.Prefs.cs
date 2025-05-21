/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool selectionBounds = true;

        public class SelectionBoundsManager : StandalonePrefManager<SelectionBoundsManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Display Bounds of Selected Renderers (Hotkey - CapsLock)"
                    };
                }
            }

            public override void Draw()
            {
                selectionBounds = EditorGUILayout.ToggleLeft("Display Bounds of Selected Renderers (Hotkey - CapsLock)", selectionBounds);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!selectionBounds) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Display Bounds of Selected Renderers", "Scene View", "CapsLock"),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                selectionBounds = state;
            }
        }
    }
}