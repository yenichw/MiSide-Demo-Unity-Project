/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class CreateBrowser
    {
        public abstract class Provider
        {
            public List<Item> items;

            public int count
            {
                get { return items.Count; }
            }

            public abstract float order { get; }

            public abstract string title { get; }

            public Provider()
            {
                Cache();
            }

            public abstract void Cache();

            public void Dispose()
            {
                if (items != null)
                {
                    foreach (Item item in items) item.Dispose();
                    items = null;
                }
            }

            public virtual void Draw()
            {
                if (items.Count == 0) return;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(title, Styles.multilineLabel);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();

                foreach (Item item in items)
                {
                    item.Draw();
                    if (instance == null) return;
                }

                EditorGUILayout.Space();
            }

            public virtual void Filter(string pattern, List<Item> filteredItems)
            {
                foreach (Item item in items) item.Filter(pattern, filteredItems);
            }

            public virtual int IndexOf(Item item)
            {
                return items.IndexOf(item);
            }
        }
    }
}