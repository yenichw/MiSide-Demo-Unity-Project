/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.HierarchyTools;
using InfinityCode.UltimateEditorEnhancer.SceneTools;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    public class FlatSmartSelectionWindow : AutoSizePopupWindow
    {
        private static Waila.SceneViewItem lastSceneViewItem;
        private static string filterText;
        private static bool resetSelection;
        private static List<FlatItem> activeItems;
        private static List<FlatItem> flatItems;
        private static FlatSmartSelectionWindow _instance;

        public static FlatSmartSelectionWindow instance
        {
            get { return _instance; }
        }

        private static TreeViewState treeViewState;
        private static FlatTreeView treeView;
        private GameObject highlightGO;
        private GameObject lastHighlightGO;

        private static bool DrawFilterTextField()
        {
            GUI.SetNextControlName("UEESmartSelectionSearchTextField");
            EditorGUI.BeginChangeCheck();
            filterText = GUILayoutUtils.ToolbarSearchField(filterText);
            bool changed = EditorGUI.EndChangeCheck();

            if (resetSelection && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("UEESmartSelectionSearchTextField");
                resetSelection = false;
            }

            return changed;
        }

        protected override void OnContentGUI()
        {
            Event e = Event.current;
            EventType type = e.type;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Select GameObject");
            if (GUILayoutUtils.ToolbarButton("?")) Links.OpenDocumentation("smart-selection");
            EditorGUILayout.EndHorizontal();

            bool filterChanged = DrawFilterTextField();

            bool needReload = false;

            if (filterChanged || activeItems == null)
            {
                if (string.IsNullOrEmpty(filterText)) activeItems = flatItems;
                else
                {
                    string pattern = SearchableItem.GetPattern(filterText);
                    activeItems = flatItems.Where(p => p.Match(pattern)).ToList();
                }

                needReload = true;
            }

            if (treeView == null)
            {
                treeViewState = new TreeViewState();
                treeView = new FlatTreeView(treeViewState);
                needReload = false;
            }

            if (needReload) treeView.Reload();

            highlightGO = null;
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, Mathf.Min(Prefs.defaultWindowSize.y - lastRect.y, treeView.totalHeight));
            treeView.OnGUI(rect);

            if (highlightGO != lastHighlightGO)
            {
                lastHighlightGO = highlightGO;
                SceneTools.Highlighter.Highlight(highlightGO);
                GUI.changed = true;
            }

            if (type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape) Close();
                else if (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.DownArrow)
                {
                    if (GUI.GetNameOfFocusedControl() == "UEESmartSelectionSearchTextField")
                    {
                        treeView.SetFocusAndEnsureSelectedItem();
                        e.Use();
                    }
                }
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    IList<int> selection = treeView.GetSelection();
                    int id = selection.Count > 0 ? selection.First() : 0;
                    SelectByDisplayID(id);
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (FlatItem item in flatItems) item.Dispose();
            flatItems.Clear();

            lastSceneViewItem.mode = 0;
            lastSceneViewItem = null;
            UnityEditor.Tools.hidden = false;
            filterText = null;
            activeItems = null;
            scrollPosition = Vector2.zero;
            treeView = null;
            treeViewState = null;

            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Focus();
        }

        private void OnEnable()
        {
            resetSelection = true;
            SceneTools.Highlighter.Highlight(null);
        }

        public GameObject Get(int id)
        {
            if (id < treeView.items.Count) return treeView.items[id];
            return null;
        }

        private void SelectByDisplayID(int id)
        {
            if (id < treeView.items.Count)
            {
                GameObject target = treeView.items[id];
                if (Event.current.control || Event.current.shift) SelectionRef.Add(target);
                else Selection.activeGameObject = target;
            }

            Close();
        }

        public static FlatSmartSelectionWindow Show(Waila.SceneViewItem sceneViewItem)
        {
            lastSceneViewItem = sceneViewItem;

            if (flatItems == null) flatItems = new List<FlatItem>();
            else flatItems.Clear();

            flatItems.AddRange(sceneViewItem.targets.Select(t => new FlatItem(t)));
            if (flatItems.Count == 0) return null;

            Vector2 size = Prefs.defaultWindowSize;
            Vector2 position = Event.current.mousePosition - new Vector2(size.x / 2, -30);

            position = GUIUtility.GUIToScreenPoint(position);

            Rect rect = new Rect(position, size);

            _instance = CreateInstance<FlatSmartSelectionWindow>();
            _instance.minSize = Vector2.zero;
            _instance.position = rect;
            _instance.adjustHeight = AutoSize.top;
            _instance.ShowPopup();
            _instance.Focus();
            _instance.wantsMouseMove = true;
            return _instance;
        }

        internal class FlatTreeView : TreeView
        {
            public List<GameObject> items;

            public FlatTreeView(TreeViewState state) : base(state)
            {
                Reload();
            }

            public FlatTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
            {
                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
                var allItems = new List<TreeViewItem>();

                int nextID = 0;
                items = new List<GameObject>();

                for (int i = 0; i < activeItems.Count; i++)
                {
                    FlatItem flatItem = activeItems[i];
                    TreeViewItem item = new TreeViewItem {id = nextID++, depth = 0, displayName = flatItem.name, icon = BestIconDrawer.GetGameObjectIcon(flatItem.target) as Texture2D };
                    allItems.Add(item);
                    items.Add(flatItem.target);

                    Transform parent = flatItem.target.transform.parent;
                    while (parent != null)
                    {
                        GameObject go = parent.gameObject;
                        item = new TreeViewItem { id = nextID++, depth = 1, displayName = go.name, icon = BestIconDrawer.GetGameObjectIcon(go) as Texture2D};
                        items.Add(go);
                        allItems.Add(item);
                        parent = parent.parent;
                    }
                }

                SetupParentsAndChildrenFromDepths(root, allItems);

                return root;
            }

            protected override bool CanStartDrag(CanStartDragArgs args)
            {
                return true;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                Event e = Event.current;
                
                if (args.rowRect.Contains(e.mousePosition))
                {
                    GameObject go = instance.highlightGO = instance.Get(args.item.id);
                    
                    if (e.type == EventType.Repaint)
                    {
                        GUI.DrawTexture(args.rowRect, Styles.selectedRowTexture, ScaleMode.StretchToFill);
                    }
                    else if (e.type == EventType.MouseDown)
                    {
                        if (e.button == 1)
                        {
                            GameObjectUtils.ShowContextMenu(false, go);
                            e.Use();
                        }
                        else if (e.button == 2)
                        {
                            Undo.RecordObject(go, "Toggle Active");
                            go.SetActive(!go.activeSelf);
                            e.Use();
                        }
                    }
                }

                base.RowGUI(args);
            }

            protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
            {
                GameObject go = instance.Get(args.draggedItemIDs[0]);
                if (go == null) return;

                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { go };
                DragAndDrop.StartDrag("Drag " + go.name);
                Event.current.Use();
                instance.Close();
            }

            protected override void SingleClickedItem(int id)
            {
                instance.SelectByDisplayID(id);
            }
        }

        internal class FlatItem: SearchableItem
        {
            public string name;
            public GameObject target;

            public FlatItem(GameObject target)
            {
                this.target = target;
                name = target.name;
            }

            public void Dispose()
            {
                target = null;
            }

            protected override int GetSearchCount()
            {
                return 1;
            }

            protected override string GetSearchString(int index)
            {
                return name;
            }
        }
    }
}