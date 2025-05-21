/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class Search
    {
        internal abstract class Record : SearchableItem
        {
            internal const int MaxLabelLength = 65;
            
            protected long lastClickTime;
            protected string _label;
            protected string[] search;
            protected string _tooltip;
            private static List<string> tempSearch;
            public bool used;
            protected GUIContent _content;
            private Texture2D _assetPreview;

            protected GUIContent content
            {
                get
                {
                    if (_content == null) _content = new GUIContent(label, tooltip);
                    return _content;
                }
            }

            protected virtual Texture2D assetPreview
            {
                get
                {
                    if (_assetPreview == null) _assetPreview = AssetPreview.GetMiniThumbnail(target);
                    return _assetPreview;
                }
            }

            public virtual string label
            {
                get
                {
                    if (_label == null) InitLabel();
                    return _label;
                }
            }

            public abstract Object target { get; }
            public abstract string tooltip { get; }

            public abstract string type { get; }

            public virtual void Dispose()
            {
                _tooltip = null;
                _label = null;
                _content = null;
                _assetPreview = null;
                search = null;
            }

            public int Draw(int index)
            {
                Rect rect = new Rect(0, index * 20, instance.position.width, 20);
                GUI.Box(rect, GUIContent.none, bestRecordIndex == index ? Styles.selectedRow : Styles.transparentRow);
                if (assetPreview != null) GUI.DrawTexture(new Rect(2, rect.y + 2, 16, 16), assetPreview);
                GUI.Label(new Rect(20, rect.y, rect.width - 20, 20), content);

                Event e = Event.current;
                int state = 0;

                if (rect.Contains(e.mousePosition))
                {
                    if (e.type == EventType.MouseUp)
                    {
                        if (e.button == 0)
                        {
                            bestRecordIndex = index;
                            long ticks = DateTime.Now.Ticks;
                            if (ticks - lastClickTime < 5000000)
                            {
                                state = ProcessDoubleClick(e);
                            }
                            lastClickTime = ticks;
                            e.Use();
                        }
                        else if (e.button == 1)
                        {
                            bestRecordIndex = index;
                            ShowContextMenu(index);
                            e.Use();
                        }
                    }
                    else if (e.type == EventType.MouseDrag && target != null)
                    {
                        bestRecordIndex = index;
                        StartDrag(e);
                    }
                }

                return state;
            }

            protected override int GetSearchCount()
            {
                return search.Length;
            }

            protected override string GetSearchString(int index)
            {
                return search[index];
            }

            protected virtual void InitLabel()
            {
                if (tooltip.Length <= MaxLabelLength) _label = tooltip;
                else
                {
                    int start = tooltip.IndexOf('/', tooltip.Length - MaxLabelLength + 3);
                    if (start != -1) _label = "..." + tooltip.Substring(start);
                    else _label = "..." + tooltip.Substring(tooltip.Length - MaxLabelLength + 3);
                }
            }

            protected virtual void PrepareContextMenuExtraItems(GenericMenuEx menu)
            {
                
            }

            protected virtual int ProcessDoubleClick(Event e)
            {
                int state;
                if (e.modifiers == EventModifiers.Control) state = 2;
                else state = 1;
                return state;
            }

            public abstract void Select(int state);

            protected virtual void ShowContextMenu(int index)
            {
                GenericMenuEx menu = GenericMenuEx.Start();
                menu.Add("Select", () => SelectRecord(index, 1));
                menu.Add("Open", () => SelectRecord(index, 2));

                PrepareContextMenuExtraItems(menu);

                menu.Show();
            }

            protected virtual void StartDrag(Event e)
            {
                isDragStarted = true;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] {target};
                DragAndDrop.StartDrag("Dragging " + target.name);
                e.Use();
            }

            public bool Update(string pattern, string valueType)
            {
                if (!string.IsNullOrEmpty(valueType) && !Contains(type, valueType)) return false;
                return Match(pattern);
            }

            public virtual void UpdateGameObjectName(GameObject go)
            {

            }
        }
    }
}