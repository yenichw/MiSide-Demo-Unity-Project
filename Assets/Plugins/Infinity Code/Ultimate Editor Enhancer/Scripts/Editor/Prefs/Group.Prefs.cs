/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool group = true;

        public static KeyCode groupKeyCode = KeyCode.G;

#if !UNITY_EDITOR_OSX
        public static EventModifiers groupModifiers = EventModifiers.Control;
#else
        public static EventModifiers groupModifiers = EventModifiers.Command;
#endif

        public class GroupManager : StandalonePrefManager<GroupManager>, IHasShortcutPref
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Group" }; }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("Group", ref group, ref groupKeyCode, ref groupModifiers, EditorStyles.label, 17);
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!group) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Group GameObjects", "Everywhere", groupModifiers, groupKeyCode),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                group = state;
            }
        }
    }
}