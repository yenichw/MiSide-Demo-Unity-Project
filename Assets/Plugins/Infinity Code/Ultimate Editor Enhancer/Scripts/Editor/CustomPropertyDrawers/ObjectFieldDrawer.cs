/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.PropertyDrawers
{
#if !UEE_DISABLE_OBJECT_FIELD_DRAWER 
    [CustomPropertyDrawer(typeof(Object), true)]
#endif
    public class ObjectFieldDrawer : PropertyDrawer
    {
        public delegate void GUIDelegate(Rect area, SerializedProperty property, GUIContent label);

        public static GUIDelegate OnGUIBefore;
        public static GUIDelegate OnGUIAfter;

        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference || property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(area, property, label);
                return;
            }

            DelegateHelper.InvokeGUI(OnGUIBefore, area, property, label);
            EditorGUI.PropertyField(area, property, label, property.isExpanded);
            DelegateHelper.InvokeGUI(OnGUIAfter, area, property, label);
        }
    }
}