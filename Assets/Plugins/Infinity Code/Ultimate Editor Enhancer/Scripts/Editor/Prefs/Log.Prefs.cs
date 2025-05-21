/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool showExceptionsInConsole = true;

        private class LogManager : PrefManager
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Log",
                        "Show Exceptions"
                    };
                }
            }

            public override float order
            {
                get { return Order.Log; }
            }

            public override void Draw()
            {
                EditorGUILayout.LabelField("Log");

                EditorGUI.indentLevel++;
                showExceptionsInConsole = EditorGUILayout.ToggleLeft("Show Exceptions", showExceptionsInConsole);
                EditorGUI.indentLevel--;
            }

            public override void SetState(bool state)
            {
                base.SetState(state);

                showExceptionsInConsole = state;
            }
        }
    }
}