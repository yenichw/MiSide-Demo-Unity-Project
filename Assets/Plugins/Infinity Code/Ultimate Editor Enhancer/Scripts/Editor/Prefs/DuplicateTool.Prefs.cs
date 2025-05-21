/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static int duplicateToolMaxCopies = 1000;

        public class DuplicateToolManager : PrefManager
        {
            public override IEnumerable<string> keywords
            {
                get { return new[] { "Duplicate Tool" }; }
            }

            public override float order
            {
                get { return Order.DuplicateTool; }
            }

            public override void Draw()
            {
                EditorGUILayout.LabelField("Duplicate Tool");

                EditorGUI.indentLevel++;
                
                EditorGUI.BeginChangeCheck();
                duplicateToolMaxCopies = EditorGUILayout.IntField("Max Copies", duplicateToolMaxCopies);
                if (EditorGUI.EndChangeCheck())
                {
                    if (duplicateToolMaxCopies < 1) duplicateToolMaxCopies = 1;
                }
                
                EditorGUI.indentLevel--;
            }
        }
    }
}