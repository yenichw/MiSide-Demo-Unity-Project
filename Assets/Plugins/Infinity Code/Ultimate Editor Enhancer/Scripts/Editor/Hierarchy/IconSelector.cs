/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class IconSelector
    {
        static IconSelector()
        {
            HierarchyItemDrawer.Register("IconSelector", OnHierarchyGUI, HierarchyToolOrder.IconSelector);
        }

        private static void OnHierarchyGUI(HierarchyItem item)
        {
            if (item.gameObject == null || !item.hovered) return;

            Event e = Event.current;
            if (e.type != EventType.ContextClick || e.button != 1) return;

            Rect r = new Rect(item.rect.x, item.rect.y, 16, 16);
            if (!r.Contains(e.mousePosition)) return;

            GameObjectHierarchySettings.ShowAtPosition(Selection.gameObjects, r);
            e.Use();
        }
    }
}