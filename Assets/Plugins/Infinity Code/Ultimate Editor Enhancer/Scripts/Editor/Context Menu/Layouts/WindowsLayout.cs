/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.EditorMenus.PopupWindows;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Layouts
{
    public class WindowsLayout: MainLayoutItem<WindowsLayout, PopupWindowItem>
    {
        public override float order
        {
            get { return -1000; }
        }

        protected override void CalculateRect(ref Vector2 position, ref Vector2 offset, ref bool flipHorizontal, ref bool flipVertical)
        {
            rect = new Rect(position, Vector2.zero);
            foreach (PopupWindowItem item in items)
            {
                if (!item.isActive) continue;
                
                Vector2 size = item.size;
                rect.width = Mathf.Max(size.x, rect.width);
                rect.height += size.y;
            }

            rect.height += GUI.skin.button.margin.top;
            rect.width += 10;

#if !UNITY_EDITOR_OSX
            Resolution resolution = Screen.currentResolution;
            int width = resolution.width;
            int height = resolution.height;

            if (rect.xMin % width + rect.width > width - 10) flipHorizontal = true;
            if (rect.yMax % height > height - 30) flipVertical = true;
#endif
            if (position.y < 10) offset.y = Mathf.Max(offset.y, 10);
        }

        protected override bool CheckPrefs()
        {
            return Prefs.popupWindows;
        }

        public override void OnGUI()
        {
            if (items == null) return;

            foreach (PopupWindowItem item in items)
            {
                if (!item.isActive) continue;

                try
                {
                    item.Draw();
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }

        public override void SetPosition(Vector2 position, Vector2 offset, bool flipHorizontal, bool flipVertical)
        {
            int ox = 10;
            int oy = -10;
            rect.position = position + offset + new Vector2(ox, oy);

            if (flipHorizontal) rect.x -= rect.width + ox * 2;
            if (flipVertical) rect.y -= rect.height + oy * 2;
        }

        public override void Show()
        {
            wnd = LayoutWindow.Show(this, rect);
        }
    }
}