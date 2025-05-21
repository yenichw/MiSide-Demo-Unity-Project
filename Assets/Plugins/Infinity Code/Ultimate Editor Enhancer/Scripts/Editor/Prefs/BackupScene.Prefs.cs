/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool backupScene = true;
        public static int backupSceneIntervalHours = 24;
        
        public class BackupSceneManager : PrefManager
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Backup Scene" }; }
            }

            public override float order
            {
                get { return Order.BackupScene; }
            }

            public override void Draw()
            {
                backupScene = EditorGUILayout.ToggleLeft("Backup Scene", backupScene);
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(!backupScene);
                backupSceneIntervalHours = EditorGUILayout.IntField("Interval (hours)", backupSceneIntervalHours);
                if (backupSceneIntervalHours < 0) backupSceneIntervalHours = 0;
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                backupScene = state;
            }
        }
    }
}