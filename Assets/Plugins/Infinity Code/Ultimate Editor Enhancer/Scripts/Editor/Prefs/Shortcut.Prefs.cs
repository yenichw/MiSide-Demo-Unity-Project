/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public class Shortcut
        {
            public string action;
            public string context;
            public string shortcut;

            public Shortcut(string action, string context, string shortcut)
            {
                this.action = action;
                this.context = context;
                this.shortcut = shortcut;
            }

            public Shortcut(string action, string context, EventModifiers modifiers): this(action, context, ModifierToString(modifiers))
            {

            }

            public Shortcut(string action, string context, KeyCode keyCode) : this(action, context, ModifierToString(EventModifiers.None, keyCode))
            {

            }

            public Shortcut(string action, string context, EventModifiers modifiers, KeyCode keyCode) : this(action, context, ModifierToString(modifiers, keyCode))
            {

            }

            public Shortcut(string action, string context, EventModifiers modifiers, string extra) : this(action, context, ModifierToString(modifiers, extra))
            {

            }
        }
    }
}