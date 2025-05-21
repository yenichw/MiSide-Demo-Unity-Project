/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Layouts
{
    public abstract class MainLayoutItem<T, U> : MainLayoutItem
        where T: MainLayoutItem<T, U>
        where U: LayoutItem
    {
        protected List<U> items;
        protected int countActiveItems;

        public override bool isActive
        {
            get { return countActiveItems > 0; }
        }

        protected abstract void CalculateRect(ref Vector2 position, ref Vector2 offset, ref bool flipHorizontal, ref bool flipVertical);

        protected abstract bool CheckPrefs();

        protected void LoadItems()
        {
            if (items != null) return;
            items = Reflection.GetInheritedItems<U>();
        }

        public override void Prepare(GameObject[] targets, Vector2 position, ref Vector2 offset, ref bool flipHorizontal, ref bool flipVertical)
        {
            if (wnd != null) wnd.Close();

            countActiveItems = 0;

            if (!CheckPrefs()) return;
            LoadItems();
            PrepareItems();

            if (!isActive) return;

            CalculateRect(ref position, ref offset, ref flipHorizontal, ref flipVertical);
        }

        protected void PrepareItems()
        {
            if (items == null) return;

            GameObject[] targets = Selection.gameObjects;

            foreach (U item in items)
            {
                item.Prepare(targets);
                PrepareItemLate(item);
                if (item.isActive) countActiveItems++;
            }
        }

        protected virtual void PrepareItemLate(U item)
        {

        }
    }
}