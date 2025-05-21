/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Integration;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class BookmarkButton
    {
        private static Texture2D offTexture;

        static BookmarkButton()
        {
            HierarchyItemDrawer.Register("BookmarkButton", OnHierarchyItem, HierarchyToolOrder.Bookmark);
        }

        private static void OnHierarchyItem(HierarchyItem item)
        {
            if (!Prefs.hierarchyBookmarks) return;
            if (item.gameObject == null) return;

            Event e = Event.current;

            bool contain = Bookmarks.Contain(item.gameObject);
            if (!contain)
            {
                if (Prefs.hierarchyIconsDisplayRule != HierarchyIconsDisplayRule.always && !item.hovered) return;
            }

            if (offTexture == null) offTexture = Styles.isProSkin ? (Texture2D)Icons.starWhite : (Texture2D)Icons.starBlack;

            Rect rect = item.rect;
            Rect r = new Rect(rect.xMax - 16, rect.y, 16, rect.height);

            if (Cinemachine.ContainBrain(item.gameObject)) r.x -= 16;

            Texture2D texture = offTexture;
            string tooltip = "Add Bookmark";

            if (contain)
            {
                texture = (Texture2D)Icons.starYellow;
                tooltip = "Remove Bookmark";
            }

            GUIContent content = TempContent.Get(texture, tooltip);
            
            if (e.type == EventType.MouseUp && e.button == 1 && r.Contains(e.mousePosition))
            {
                Bookmarks.ShowWindow();
                e.Use();
            }

            if (GUI.Button(r, content, GUIStyle.none))
            {
                if (contain) Bookmarks.Remove(item.gameObject);
                else Bookmarks.Add(item.gameObject);
            }

            item.rect.xMax -= 16;
        }
    }
}