/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class CreateBrowser: EditorWindow
    {
        public const int PreviewHeight = 90;

        public Action<CreateBrowser> OnClose;
        public Action<string> OnSelectCreate;
        public Action<string> OnSelectPrefab;

        public string helpMessage;

        private static CreateBrowser instance;
        private static Item selectedItem;
        private static FolderItem selectedFolder;
        private static Editor previewEditor;
        private static PrefabItem previewPrefab;
        private static Rect? scrollRect;
        private static Vector2 scrollPosition;
        private static double loadTimer;
        private static Texture previewBackground;

        public string createLabel = "Create";
        public string prefabsLabel = "Prefabs";
        
        private bool resetSelection = true;
        private int selectedIndex = 0;

        [NonSerialized]
        private string searchText;

        private List<Provider> providers;
        private List<Item> filterItems;
        private int totalItems;
        private static bool allowSelect;

        private bool DrawFilterTextField()
        {
            GUI.SetNextControlName("UEECreateSearchTextField");
            EditorGUI.BeginChangeCheck();
            searchText = GUILayoutUtils.ToolbarSearchField(searchText);
            bool changed = EditorGUI.EndChangeCheck();

            if (resetSelection && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("UEECreateSearchTextField");
                resetSelection = false;
                Repaint();
            }

            return changed;
        }

        private void DrawFilteredItems()
        {
            if (filterItems.Count > 0)
            {
                foreach (Item item in filterItems)
                {
                    if (instance == null) return;
                    item.Draw();
                }
            }
            else EditorGUILayout.LabelField("Nothing found.");
        }

        private void DrawRootItems()
        {
            foreach (Provider provider in providers)
            {
                if (instance == null) return;
                provider.Draw();
            }
        }

        private void DrawSubItems()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            FolderItem current = selectedFolder;

            if (selectedFolder.parent != null)
            {
                if (GUILayoutUtils.ToolbarButton(EditorIconContents.animationFirstKey))
                {
                    selectedFolder = null;
                    selectedItem = null;
                    return;
                }
            }

            if (GUILayoutUtils.ToolbarButton(EditorIconContents.animationPrevKey))
            {
                SelectParent();
            }

            EditorGUILayout.LabelField(current.title);
            EditorGUILayout.EndHorizontal();

            foreach (Item item in current.children)
            {
                item.Draw();
                if (instance == null) return;
            }
        }

        private void InitProviders()
        {
            providers = new List<Provider>();
            providers.Add(new CreateProvider());
            providers.Add(new PrefabProvider());
            providers.Add(new BookmarkProvider());

            providers = providers.OrderBy(p => p.order).ToList();

            totalItems = providers.Sum(p => p.count);
        }

        private void OnDestroy()
        {
            if (OnClose != null) OnClose(this);

            if (providers != null)
            {
                foreach (Provider p in providers) p.Dispose();
            }

            filterItems = null;

            instance = null;
            OnSelectCreate = null;
            OnClose = null;
            OnSelectPrefab = null;
            selectedFolder = null;
            selectedItem = null;
            previewPrefab = null;
            previewEditor = null;
            if (previewEditor != null)
            {
                DestroyImmediate(previewEditor);
                previewEditor = null;
            }
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
            instance = this;
            filterItems = new List<Item>();

            InitProviders();

            selectedIndex = 0;
            foreach (Provider provider in providers)
            {
                if (provider.items.Count == 0) continue;
                selectedItem = provider.items[0];
                break;
            }
        }

        private void OnGUI()
        {
            if (!ProcessEvents()) return;

            EditorGUILayout.BeginHorizontal();
            bool filterChanged = DrawFilterTextField();
            if (GUILayoutUtils.ToolbarButton("?")) Links.OpenDocumentation("object-placer");
            EditorGUILayout.EndHorizontal();

            PrefabItem currentPrefab = selectedItem as PrefabItem;
            Event e = Event.current;
            allowSelect = e.type == EventType.MouseMove && !e.control && !e.command && scrollRect.HasValue && scrollRect.Value.Contains(e.mousePosition);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            loadTimer = EditorApplication.timeSinceStartup;

            if (filterItems.Count == 0 && string.IsNullOrEmpty(searchText))
            {
                if (selectedFolder == null) DrawRootItems();
                else DrawSubItems();
            }
            else DrawFilteredItems();

            EditorGUILayout.EndScrollView();

            if (e.type != EventType.Layout && e.type != EventType.Used)
            {
                scrollRect = GUILayoutUtility.GetLastRect();
            }

            if (Prefs.createBrowserPreviewSelection)
            {
                if (currentPrefab != null) currentPrefab.DrawPreview();
                else
                {
                    GUILayoutUtility.GetRect(1, PreviewHeight);
                    Rect rect = GUILayoutUtility.GetLastRect();
                    if (previewBackground == null) previewBackground = Resources.CreateSinglePixelTexture(0.33f);
                    GUI.DrawTexture(rect, previewBackground);
                }
            }

            if (!string.IsNullOrEmpty(helpMessage)) EditorGUILayout.HelpBox(helpMessage, MessageType.Info);

            if (filterChanged) UpdateFilterItems();

            if (GUI.changed) Repaint();
        }

        public static CreateBrowser OpenWindow()
        {
            instance = GetWindow<CreateBrowser>(true, "Create Browser");
            instance.Focus();
            instance.wantsMouseMove = true;
            return instance;
        }

        private bool ProcessEvents()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.DownArrow) SelectNext();
                else if (e.keyCode == KeyCode.UpArrow) SelectPrev();
                else if (e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return)
                {
                    return SelectCurrent();
                }
                else if (e.keyCode == KeyCode.Backspace && string.IsNullOrEmpty(searchText)) SelectParent();
            }

            return true;
        }

        private void SelectParent()
        {
            if (selectedFolder == null) return;

            FolderItem f = selectedFolder;
            selectedFolder = selectedFolder.parent;
            if (selectedFolder != null) selectedIndex = selectedFolder.children.IndexOf(f);
            else
            {
                int nextStart = 0;
                foreach (Provider provider in providers)
                {
                    int index = provider.IndexOf(f);
                    if (index != -1)
                    {
                        selectedIndex = index + nextStart;
                        break;
                    }

                    nextStart += provider.count;
                }
            }
            selectedItem = f;
            Event.current.Use();
            Repaint();
        }

        private bool SelectCurrent()
        {
            if (selectedItem == null) return false;

            bool ret = selectedItem is FolderItem;

            selectedItem.OnClick();
            Event.current.Use();
            Repaint();

            return ret;
        }

        private void SelectNext()
        {
            selectedIndex++;
            if (!string.IsNullOrEmpty(searchText))
            {
                if (filterItems.Count == 0)
                {
                    selectedItem = null;
                    selectedIndex = -1;
                }
                else
                {
                    if (selectedIndex >= filterItems.Count) selectedIndex = 0;
                    selectedItem = filterItems[selectedIndex];
                }
            }
            else if (selectedFolder != null)
            {
                if (selectedIndex >= selectedFolder.children.Count) selectedIndex = 0;
                selectedItem = selectedFolder.children[selectedIndex];
            }
            else
            {
                if (selectedIndex >= totalItems) selectedIndex = 0;

                int remain = selectedIndex;

                foreach (Provider provider in providers)
                {
                    if (provider.count > remain)
                    {
                        selectedItem = provider.items[remain];
                        break;
                    }

                    remain -= provider.count;
                }
            }

            previewPrefab = null;
            DestroyImmediate(previewEditor);
            Event.current.Use();
            Repaint();
        }

        private void SelectPrev()
        {
            selectedIndex--;
            if (!string.IsNullOrEmpty(searchText))
            {
                if (filterItems.Count == 0)
                {
                    selectedItem = null;
                    selectedIndex = -1;
                }
                else
                {
                    if (selectedIndex < 0) selectedIndex = filterItems.Count - 1;
                    selectedItem = filterItems[selectedIndex];
                }
            }
            else if (selectedFolder != null)
            {
                if (selectedIndex < 0) selectedIndex = selectedFolder.children.Count - 1;
                selectedItem = selectedFolder.children[selectedIndex];
            }
            else
            {
                if (selectedIndex < 0) selectedIndex = totalItems - 1;

                int remain = selectedIndex;

                foreach (Provider provider in providers)
                {
                    if (provider.count > remain)
                    {
                        selectedItem = provider.items[remain];
                        break;
                    }

                    remain -= provider.count;
                }
            }
            Event.current.Use();
            Repaint();
        }

        private void UpdateFilterItems()
        {
            filterItems.Clear();
            if (string.IsNullOrEmpty(searchText)) return;

            string pattern = SearchableItem.GetPattern(searchText);

            foreach (Provider provider in providers) provider.Filter(pattern, filterItems);

            filterItems = filterItems.Take(Prefs.createBrowserMaxFilterItems).ToList();

            selectedIndex = filterItems.IndexOf(selectedItem);
            if (selectedIndex == -1)
            {
                if (filterItems.Count > 0)
                {
                    selectedIndex = 0;
                    selectedItem = filterItems[0];
                }
                else
                {
                    selectedItem = null;
                }
            }
        }

        private void UpdateSelectedIndex()
        {
            if (!string.IsNullOrEmpty(searchText))
            {
                if (filterItems.Count == 0) selectedIndex = -1;
                else selectedIndex = filterItems.IndexOf(selectedItem);
            }
            else if (selectedFolder != null)
            {
                selectedIndex = selectedFolder.children.IndexOf(selectedItem);
            }
            else
            {
                selectedIndex = 0;
                bool match = false;
                int nextStart = 0;
                foreach (Provider provider in providers)
                {
                    int index = provider.IndexOf(selectedItem);
                    if (index != -1)
                    {
                        match = true;
                        selectedIndex = index + nextStart;
                        break;
                    }

                    nextStart += provider.count;
                }

                if (!match)
                {
                    selectedIndex = 0;
                    selectedItem = providers[0].items[0];
                }
            }
        }
    }
}