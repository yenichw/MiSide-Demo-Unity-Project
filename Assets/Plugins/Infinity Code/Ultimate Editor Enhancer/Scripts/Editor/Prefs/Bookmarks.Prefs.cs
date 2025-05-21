/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool bookmarksHotKey = true;
        public static KeyCode bookmarksKeyCode = KeyCode.B;
        public static EventModifiers bookmarksModifiers = EventModifiers.Shift | EventModifiers.Alt;

        private class BookmarksManager : PrefManager, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Bookmarks" }; }
            }

            public override float order
            {
                get { return Order.Bookmarks; }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("Bookmarks", ref bookmarksHotKey, ref bookmarksKeyCode, ref bookmarksModifiers, EditorStyles.label, 17);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!bookmarksHotKey) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Open Bookmarks", "Everywhere", bookmarksModifiers, bookmarksKeyCode),
                };
            }
            
            public override void SetState(bool state)
            {
                base.SetState(state);
                
                bookmarksHotKey = state;
            }
        }
    }
}