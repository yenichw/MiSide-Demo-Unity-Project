/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class FlatSelectorWindow : EditorWindow
    {
        private const string SearchFieldName = "UEEFlatSelectorSearchTextField";
        private const int ItemHeight = 16;
        private const int ExtraHeight = 28;

        public Action<int> OnSelect;

        private GUIContent[] contents;
        private int selected;
        private string filterText;
        private bool resetSelection = true;
        private ListView listView;
        private static FlatSelectorWindow instance;
        private List<int> items;
        private bool ignoreItemSelect;
        private Texture selectedTexture;

        static FlatSelectorWindow()
        {
            EventManager.AddBinding(EventManager.ClosePopupEvent).OnInvoke += b =>
            {
                if (instance != null) instance.Close(); 
            };
        }

        private void DrawFilterTextField()
        {
            GUI.SetNextControlName(SearchFieldName);
            EditorGUI.BeginChangeCheck();
            filterText = GUILayoutUtils.ToolbarSearchField(filterText);
            if (EditorGUI.EndChangeCheck()) UpdateFilteredItems();

            if (resetSelection && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl(SearchFieldName);
                resetSelection = false;
            }
        }

        private void Init()
        {
            items = new List<int>();
            for (int i = 0; i < contents.Length; i++) items.Add(i);

            Func<VisualElement> makeItem = () =>
            {
                VisualElement el = new VisualElement();
                el.AddToClassList("flatselector-item");

                Image image = new Image();
                image.AddToClassList("flatselector-item-image");

                el.Add(image);
                el.Add(new Label());
                return el;
            };

            if (selectedTexture == null) selectedTexture = EditorGUIUtility.IconContent("FilterSelectedOnly").image;

            Action<VisualElement, int> bindItem = (el, i) =>
            {
                Image image = el[0] as Image;

                int itemIndex = items[i];
                image.image = selected == itemIndex ? selectedTexture : null;

                GUIContent content = contents[itemIndex];
                Label label = el[1] as Label;
                label.text = content.text;
                label.tooltip = content.tooltip;

                if (string.IsNullOrEmpty(content.text))
                {
                    el.AddToClassList("flatselector-separator");
                    el.style.height = 1;
                    el.style.marginTop = 7;
                    el.style.marginBottom = 8;
                }
                else
                {
                    el.RemoveFromClassList("flatselector-separator");
                    el.style.height = 16;
                    el.style.marginTop = 0;
                    el.style.marginBottom = 0;
                }
            };

            listView = new ListView(items, ItemHeight, makeItem, bindItem);
            listView.selectionType = SelectionType.Single;
            listView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;

#if !UNITY_2022_2_OR_NEWER
            listView.onSelectionChange += objects =>
#else
            listView.selectionChanged += objects =>
#endif
            {
                if (ignoreItemSelect) return;
                Event e = Event.current;
                if (e.type == EventType.KeyDown)
                {
                    if (e.keyCode != KeyCode.KeypadEnter && e.keyCode != KeyCode.Return && e.keyCode != KeyCode.Space) return;
                }
                if (OnSelect != null) OnSelect(items[listView.selectedIndex]);
                Close();
            };

            listView.AddToClassList("flatselector-listview");

            rootVisualElement.styleSheets.Add(StyleSheets.flatSelector);
            rootVisualElement.AddToClassList("flatselector");

            rootVisualElement.Add(new IMGUIContainer(DrawFilterTextField));
            rootVisualElement.Add(listView);

            ignoreItemSelect = true;
            listView.selectedIndex = selected;
            ignoreItemSelect = false;
        }

        private void OnDestroy()
        {
            contents = null;
            listView = null;
            instance = null;
            OnSelect = null;
        }

        private void OnGUI()
        {
            if (focusedWindow != this) Close();

            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape) Close();
                else if (e.keyCode == KeyCode.DownArrow)
                {
                    ignoreItemSelect = true;
                    e.Use();
                    if (listView.selectedIndex == items.Count - 1) listView.selectedIndex = 0;
                    else listView.selectedIndex++;
                    ignoreItemSelect = false;
                }
                else if (e.keyCode == KeyCode.UpArrow)
                {
                    ignoreItemSelect = true;
                    e.Use();
                    if (listView.selectedIndex == 0 || listView.selectedIndex == -1) listView.selectedIndex = items.Count - 1;
                    else listView.selectedIndex--;
                    ignoreItemSelect = false;
                    
                }
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Space || e.keyCode == KeyCode.KeypadEnter)
                {
                    if (listView.selectedIndex != -1 && OnSelect != null) OnSelect(items[listView.selectedIndex]);
                    Close();
                }
            }
        }

        public static FlatSelectorWindow Show(Rect rect, GUIContent[] contents, int selected)
        {
            if (instance != null) instance.Close();
            if (contents == null || contents.Length == 0) return null;

            float width = rect.width;

            GUIStyle style = EditorStyles.label;

            for (int i = 0; i < contents.Length; i++)
            {
                Vector2 size = style.CalcSize(contents[i]);
                if (size.x + 50 > width) width = size.x + 50;
            }

            FlatSelectorWindow wnd = instance = CreateInstance<FlatSelectorWindow>();
            rect.y += rect.height;
            rect.width = width;
            rect.height = Mathf.Min(Prefs.defaultWindowSize.y, contents.Length * ItemHeight + ExtraHeight);
            rect.position = GUIUtility.GUIToScreenPoint(rect.position);
            wnd.minSize = Vector2.one;
            wnd.position = rect;
            wnd.contents = contents;
            wnd.selected = selected;
            wnd.ShowPopup();
            wnd.Init();
            wnd.Focus();

            return wnd;
        }

        private void UpdateFilteredItems()
        {
            items = new List<int>();
            if (string.IsNullOrEmpty(filterText))
            {
                for (int i = 0; i < contents.Length; i++) items.Add(i);
            }
            else
            {
                string pattern = SearchableItem.GetPattern(filterText);

                for (int i = 0; i < contents.Length; i++)
                {
                    if (SearchableItem.Match(pattern, contents[i].text))
                    {
                        items.Add(i);
                    }
                }
            }

            listView.itemsSource = items;
#if UNITY_2021_2_OR_NEWER
            listView.Rebuild();
#else
            listView.Refresh();
#endif

            ignoreItemSelect = true;
            listView.selectedIndex = items.IndexOf(selected);
            ignoreItemSelect = false;

            Rect rect = position;
            rect.height = Mathf.Min(Prefs.defaultWindowSize.y, items.Count * ItemHeight + ExtraHeight);
            position = rect;
        }
    }
}