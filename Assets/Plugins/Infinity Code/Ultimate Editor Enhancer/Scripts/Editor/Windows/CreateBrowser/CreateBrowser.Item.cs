/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class CreateBrowser
    {
        public abstract class Item : SearchableItem
        {
            private static int hash = "CreateBrowserItem".GetHashCode();

            protected GUIContent _content;
            internal string label;
            internal FolderItem parent;

            internal GUIContent content
            {
                get
                {
                    if (_content == null) InitContent();
                    return _content;
                }
            }

            public virtual void Dispose()
            {
                _content = null;
                parent = null;
            }

            public virtual void Draw()
            {
                Rect r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(18));

                Event e = Event.current;

                if (e.type == EventType.Repaint)
                {
                    if (selectedItem == this) GUI.DrawTexture(r, Styles.selectedRowTexture);
                    int cid = GUIUtility.GetControlID(hash, FocusType.Passive);
                    EditorStyles.label.Draw(r, content, cid, false, false);
                }

                if (allowSelect && r.Contains(e.mousePosition))
                {
                    selectedItem = this;
                    instance.UpdateSelectedIndex();
                    GUI.changed = true;
                }

                if (e.type == EventType.MouseDown && r.Contains(e.mousePosition))
                {
                    if (e.button == 0) OnClick();
                    e.Use();
                }
            }

            public virtual void Filter(string pattern, List<Item> filteredItems)
            {
                if (Match(pattern)) filteredItems.Add(this);
            }

            protected override int GetSearchCount()
            {
                return 1;
            }

            protected override string GetSearchString(int index)
            {
                return label;
            }

            protected abstract void InitContent();

            public abstract void OnClick();
        }
    }
}