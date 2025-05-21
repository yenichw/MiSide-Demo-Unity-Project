/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool renameByShortcut = true;

        public class RenameManager : StandalonePrefManager<RenameManager>, IHasShortcutPref
        {
            public override void Draw()
            {
                renameByShortcut = EditorGUILayout.ToggleLeft("Rename By Shortcut (F2)", renameByShortcut);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                return new[]
                {
                    new Shortcut("Rename Selected Items", "Everywhere", "F2"),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                renameByShortcut = state;
            }
        }
    }
}