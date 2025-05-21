/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.HierarchyTools;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool hierarchyRowBackground = true;
        public static HierarchyRowBackgroundStyle hierarchyRowBackgroundStyle = HierarchyRowBackgroundStyle.gradient;

        public class BackgroundManager : StandalonePrefManager<BackgroundManager>
        {
            private static SerializedObject so;
            private SerializedProperty prop;
            
            public override IEnumerable<string> keywords
            {
                get 
                { 
                    return new[] 
                    { 
                        "Row Backgrounds",
                        "Style",
                    };
                }
            }

            public override void Draw()
            {
                hierarchyRowBackground = EditorGUILayout.ToggleLeft("Row Background", hierarchyRowBackground);

                EditorGUI.BeginDisabledGroup(!hierarchyRowBackground);

                EditorGUI.BeginChangeCheck();
                hierarchyRowBackgroundStyle = (HierarchyRowBackgroundStyle)EditorGUILayout.EnumPopup("Style", hierarchyRowBackgroundStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    BackgroundDrawer.backgroundTexture = null;
                    EditorApplication.RepaintHierarchyWindow();
                }
                
                if (so == null) so = new SerializedObject(ReferenceManager.instance);
                so.Update();

                if (prop == null) prop = so.FindProperty("_backgroundRules");
                EditorGUILayout.PropertyField(prop, true);
                so.ApplyModifiedProperties();

                EditorGUI.EndDisabledGroup();
            }
            
            public override void SetState(bool state)
            {
                base.SetState(state);
                
                hierarchyHeaders = state;
            }
        }
    }
}