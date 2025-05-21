/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool showDistanceInScene = true;

        public class DistanceToolManager : StandalonePrefManager<DistanceToolManager>
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Show Distance In Scene View" }; }
            }

            public override void Draw()
            {
                showDistanceInScene = EditorGUILayout.ToggleLeft("Show Distance In Scene View", showDistanceInScene);
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                showDistanceInScene = state;
            }
        }
    }
}