/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool actions = true;
        public static bool actionsAddComponent = false;

        private class ActionsManager : StandalonePrefManager<ActionsManager>
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Actions", "Add Component Action For Single GameObject In Actions" }; }
            }

            public override float order
            {
                get { return -80; }
            }

            public override void Draw()
            {
                actions = EditorGUILayout.ToggleLeft("Actions", actions, EditorStyles.label);

                EditorGUI.BeginDisabledGroup(!actions);
                EditorGUI.indentLevel++;
                actionsAddComponent = EditorGUILayout.ToggleLeft("Add Component Action For Single GameObject In Actions", actionsAddComponent);
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                actions = state;
            }
        }
    }
}