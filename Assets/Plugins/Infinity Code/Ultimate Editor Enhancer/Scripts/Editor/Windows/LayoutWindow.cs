/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.EditorMenus;
using InfinityCode.UltimateEditorEnhancer.EditorMenus.Layouts;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class LayoutWindow: PopupWindow
    {
        private MainLayoutItem item;

        protected void OnDestroy()
        {
            item = null;
        }

        protected override void OnGUI()
        {
            if (item == null)
            {
                EditorMenu.Close();
                return;
            }

            try
            {
                base.OnGUI();
                item.OnGUI();
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
        }

        public static LayoutWindow Show(MainLayoutItem item, Rect rect)
        {
            LayoutWindow wnd = CreateInstance<LayoutWindow>();
            wnd.item = item;
            wnd.ShowPopup();
            wnd.minSize = Vector2.zero;
            WindowsHelper.SetRect(wnd, rect);
            wnd.Focus();
            return wnd;
        }
    }
}
 