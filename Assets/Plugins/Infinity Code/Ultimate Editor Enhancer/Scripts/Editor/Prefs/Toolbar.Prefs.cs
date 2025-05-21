/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool toolbar = true;
        public static TimerMode timerMode = TimerMode.icon;
        public static bool showViewStateToolbarIcon = true;

        public class ToolbarManager : StandalonePrefManager<ToolbarManager>, IStateablePref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return ToolbarWindowsManager.GetKeywords().Concat(new []
                    {
                        "Show icon on toolbar if selection has View State",
                        "Timer",
                    });
                }
            }

            public override void Draw()
            {
                toolbar = EditorGUILayout.ToggleLeft("Toolbar", toolbar);
                EditorGUI.BeginDisabledGroup(!toolbar);
                EditorGUI.indentLevel++;
                showViewStateToolbarIcon = EditorGUILayout.ToggleLeft("Show Icon If Selection Has View State", showViewStateToolbarIcon);
                timerMode = (TimerMode)EditorGUILayout.EnumPopup("Timer Mode", timerMode);
                ToolbarWindowsManager.Draw(null);
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }

            public string GetMenuName()
            {
                return "Toolbar";
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                toolbar = state;
            }
        }
    }
}