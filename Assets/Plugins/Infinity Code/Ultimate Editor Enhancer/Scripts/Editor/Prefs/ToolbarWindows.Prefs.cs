/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.Tools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool windowsToolbarIcon = true;
        public static bool recentWindowsInToolbar = true;
        public static bool favoriteWindowsInToolbar = true;
        public static int maxRecentWindows = 10;

        private class ToolbarWindowsManager : StandalonePrefManager<ToolbarWindowsManager>
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Windows In Toolbar",
                        "Favorite Windows",
                        "Recent Windows",
                        "Max Recent Windows"
                    };
                }
            }

            public override void Draw()
            {
                EditorGUI.BeginChangeCheck();
                windowsToolbarIcon = EditorGUILayout.ToggleLeft("Windows", windowsToolbarIcon);
                if (EditorGUI.EndChangeCheck())
                {
                    ToolbarWindows.Reinit();
                    GUI.changed = true;
                }

                EditorGUI.BeginDisabledGroup(!windowsToolbarIcon);
                EditorGUI.indentLevel++;

                recentWindowsInToolbar = EditorGUILayout.ToggleLeft("Recent Windows", recentWindowsInToolbar);
                EditorGUI.BeginDisabledGroup(!recentWindowsInToolbar);
                maxRecentWindows = EditorGUILayout.IntField("Max Recent Windows", maxRecentWindows);
                EditorGUI.EndDisabledGroup();
                favoriteWindowsInToolbar = EditorGUILayout.ToggleLeft("Favorite Windows", favoriteWindowsInToolbar);

                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }
            
            public override void SetState(bool state)
            {
                base.SetState(state);

                windowsToolbarIcon = state;
            }
        }
    }
}