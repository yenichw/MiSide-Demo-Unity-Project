/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class RowEnable
    {
        static RowEnable()
        {
            HierarchyItemDrawer.Register("RowEnable", OnHierarchyItem, HierarchyToolOrder.RowEnable);
            HierarchyItemDrawer.Register("RowEnableMiddle", OnHierarchyItemMiddle, HierarchyToolOrder.RowEnable);
        }

        private static void OnHierarchyItemMiddle(HierarchyItem item)
        {
            if (!Prefs.hierarchyEnableMiddleClick) return;
            if (item.gameObject == null || !item.hovered) return;

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 2)
            {
                Undo.RecordObject(item.gameObject, "Modified Property in " + item.gameObject.name);
                item.gameObject.SetActive(!item.gameObject.activeSelf);
                EditorUtility.SetDirty(item.gameObject);
                e.Use();
            }
        }

        private static void OnHierarchyItem(HierarchyItem item)
        {
            if (!Prefs.hierarchyEnableGameObject) return;
            if (item.gameObject == null || !item.hovered) return;

            Rect rect = item.rect;
            Rect r = new Rect(32, rect.y, 16, rect.height);

            EditorGUI.BeginChangeCheck();
            bool v = EditorGUI.Toggle(r, GUIContent.none, item.gameObject.activeSelf);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(item.gameObject, "Modified Property in " + item.gameObject.name);
                item.gameObject.SetActive(v);
                EditorUtility.SetDirty(item.gameObject);
            }
        }
    }
}