/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Attributes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ComponentHeader
{
    public static class ComponentBookmark
    {
        [ComponentHeaderButton(ComponentHeaderButtonOrder.Bookmark)]
        public static bool DrawHeaderButton(Rect rect, Object[] targets)
        {
            if (targets.Length != 1)
            {
                return false;
            }

            Object target = targets[0];
            Component component = target as Component;
            if (component == null) return false;
            
            Texture emptyTexture = Styles.isProSkin? Icons.starWhite: Icons.starBlack;
            bool contain = Bookmarks.Contain(target);
            Texture texture = contain? Icons.starYellow : emptyTexture;
            GUIContent content = TempContent.Get(texture, contain? "Remove Bookmark": "Add Bookmark");

            ButtonEvent buttonEvent = GUILayoutUtils.Button(rect, content, GUIStyle.none);
            if (buttonEvent == ButtonEvent.click)
            {
                Event e = Event.current;
                if (e.button == 0)
                {
                    if (contain) Bookmarks.Remove(target);
                    else Bookmarks.Add(target);
                }
                else if (e.button == 1)
                {
                    Bookmarks.ShowWindow();
                }
                e.Use();
            }

            return true;
        }
    }
}