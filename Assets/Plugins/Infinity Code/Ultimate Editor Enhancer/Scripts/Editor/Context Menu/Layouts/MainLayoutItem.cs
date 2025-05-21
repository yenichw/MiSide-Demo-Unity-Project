/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Layouts
{
    public abstract class MainLayoutItem
    {
        protected LayoutWindow wnd;
        protected Rect rect;

        public LayoutWindow window
        {
            get { return wnd; }
        }

        public abstract bool isActive { get; }

        public virtual float order
        {
            get { return 0; }
        }

        public virtual void Close()
        {
            if (wnd != null)
            {
                wnd.Close();
                wnd = null;
            }
        }

        public abstract void OnGUI();
        public abstract void Prepare(GameObject[] targets, Vector2 position, ref Vector2 offset, ref bool flipHorizontal, ref bool flipVertical);
        public abstract void SetPosition(Vector2 position, Vector2 offset, bool flipHorizontal, bool flipVertical);
        public abstract void Show();
    }
}