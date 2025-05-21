/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.PostHeader
{
    public class HeaderBookmark : PostHeaderItem
    {
        public override int rowOrder
        {
            get
            {
                return 1;
            }
        }

        protected override GUIStyle style
        {
            get
            {
                if (_style == null)
                {
                    _style = new GUIStyle(EditorStyles.toolbarButton);
                    _style.padding = new RectOffset(2, 2, 2, 2);
                }

                return _style;
            }
        }

        public override void OnRowGUI(Object target)
        {
            GameObject go = target as GameObject;
            if (go == null) return;

            Texture emptyTexture = Styles.isProSkin? Icons.starWhite: Icons.starBlack;
            bool contain = Bookmarks.Contain(target);
            Texture texture = contain? Icons.starYellow : emptyTexture;
            GUIContent content = TempContent.Get(texture, contain? "Remove Bookmark": "Add Bookmark");

            ButtonEvent buttonEvent = GUILayoutUtils.Button(content, style, GUILayout.Width(20), GUILayout.Height(20));
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
        }
    }
}