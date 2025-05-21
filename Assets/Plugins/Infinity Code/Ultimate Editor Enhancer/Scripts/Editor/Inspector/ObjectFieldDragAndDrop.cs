/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Interceptors;
using InfinityCode.UltimateEditorEnhancer.PropertyDrawers;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.InspectorTools
{
    [InitializeOnLoad]
    public static class ObjectFieldDragAndDrop
    {
        static ObjectFieldDragAndDrop()
        {
            if (Interceptor.isAppleM) ObjectFieldDrawer.OnGUIBefore += OnGUIBefore;
        }

        private static void OnGUIBefore(Rect area, SerializedProperty property, GUIContent label)
        {
            if (!Prefs.dragObjectFields) return;

            Object obj = property.objectReferenceValue;
            if (obj == null) return;

            Event e = Event.current;

            if (e.type == EventType.MouseDrag && area.Contains(e.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { obj };
                DragAndDrop.StartDrag("Drag " + obj.name);
                e.Use();
            }
        }
    }
}