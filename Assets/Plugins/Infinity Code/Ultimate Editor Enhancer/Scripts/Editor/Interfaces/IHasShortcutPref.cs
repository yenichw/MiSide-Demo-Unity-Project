/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;

namespace InfinityCode.UltimateEditorEnhancer
{
    public interface IHasShortcutPref
    {
        IEnumerable<Prefs.Shortcut> GetShortcuts();
    }
}