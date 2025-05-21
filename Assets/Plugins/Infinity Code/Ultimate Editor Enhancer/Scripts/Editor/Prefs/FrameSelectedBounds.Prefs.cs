/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public partial class Prefs
    {
        public static bool frameSelectedBounds = true;

        public class FrameSelectedBoundsManager : StandalonePrefManager<FrameSelectedBoundsManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Frame", "Selected", "Bounds"
                    };
                }
            }

            public override void Draw()
            {
                frameSelectedBounds = EditorGUILayout.ToggleLeft("Frame Selected Bounds", frameSelectedBounds);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!frameSelectedBounds) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Frame Selected Bounds", "Scene View", "SHIFT + F"),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                frameSelectedBounds = state;
            }
        }
    }
}