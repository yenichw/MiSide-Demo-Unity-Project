/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool checkForUpdates = true;

        public class UpdaterManager : PrefManager
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[] { "Check For Updates" };
                }
            }

            public override float order
            {
                get { return Order.Updater; }
            }

            public override void Draw()
            {
                checkForUpdates = EditorGUILayout.ToggleLeft("Check For Updates", checkForUpdates);
            }

            public override void SetState(bool state)
            {
                base.SetState(state);

                checkForUpdates = state;
            }
        }
    }
}