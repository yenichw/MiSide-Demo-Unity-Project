/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using InfinityCode.UltimateEditorEnhancer.JSON;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [InitializeOnLoad]
    public partial class Bookmarks : EditorWindow
    {
        #region Fields

        private static ProjectFolderBookmark[] _folders;
        private static Dictionary<SceneReferences, List<SceneBookmark>> _sceneItems;
        private static string[] cachedLabels;
        private static GUIContent clearContent;
        private static List<BookmarkItem> filteredItems;
        private static GUIContent filterByLabelContent;
        private static GUIContent filterByTypeContent;
        private static List<ProjectBookmark> selectedFolderItems;
        private static List<BookmarkItem> selectedFoldersStack;
        private static double lastClickTime;
        private static Bookmarks instance;
        private static BookmarkItem removeItem;

        private string _filter = "";
        private string activeLabel;
        private bool focusOnSearch;
        private Vector2 scrollPosition;
        private bool showProjectItems = true;
        private bool showSceneItems = true;

        #endregion

        private static Texture2D emptyTexture { get; set; }

        public string filter
        {
            get => _filter;
            set
            {
                _filter = value;
                UpdateFilteredItems();
            }
        }

        private static ProjectFolderBookmark[] folders
        {
            get
            {
                if (_folders == null) InitProjectFolders();
                return _folders;
            }
        }

        public static GUIContent hiddenContent { get; private set; }

        public static JsonArray json
        {
            get
            {
                JsonArray jArr = new JsonArray();
                for (int i = 0; i < projectItems.Count; i++)
                {
                    JsonObject obj = new JsonObject();
                    obj.Add("path", AssetDatabase.GetAssetPath(projectItems[i].target), JsonValue.ValueType.STRING);
                    jArr.Add(obj);
                }

                return jArr;
            }
            set
            {
                if (value.count == 0) return;

                if (projectItems.Count > 0)
                {
                    // 0 - Replace, 1 - Ignore, 2 - Append
                    int action = EditorUtility.DisplayDialogComplex("Import Bookmarks", "Bookmarks already contain project items", "Replace", "Ignore", "Append");
                    if (action == 1) return;
                    if (action == 0)
                    {
                        ReferenceManager.bookmarks.Clear();
                    }
                }
                
                foreach (JsonItem v in value)
                {
                    string path = v.V<string>("path");
                    if (!File.Exists(path)) continue;
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (projectItems.Any(i => i.target == obj)) continue;

                    ProjectBookmark item = new ProjectBookmark(obj);
                    ReferenceManager.bookmarks.Add(item);
                }

                Redraw();
            }
        }

        public static List<ProjectBookmark> projectItems => ReferenceManager.bookmarks;

        public static Dictionary<SceneReferences, List<SceneBookmark>> sceneItems
        {
            get
            {
                if (_sceneItems == null)
                {
                    List<SceneReferences> sceneReferences = SceneReferences.instances;
                    _sceneItems = new Dictionary<SceneReferences, List<SceneBookmark>>();
                    foreach (SceneReferences sr in sceneReferences)
                    {
                        if (!sr) continue;
                        if (sr.bookmarks.Count > 0) _sceneItems.Add(sr, sr.bookmarks);
                    }

                    _sceneItems = _sceneItems.OrderBy(p => p.Key.gameObject.scene.name).ToDictionary(p => p.Key, p => p.Value);
                }

                return _sceneItems;
            }
        }

        public static GUIContent visibleContent { get; private set; }

        static Bookmarks()
        {
            SceneReferences.OnUpdateInstances += OnUpdateSceneReferences;

            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidate;
            binding.OnPress += () => ToggleWindow();
        }

        public static void Add(Object target) 
        {
            if (!target) return;

            Scene? scene = GetScene(target);
            if (scene.HasValue && scene.Value.name != null)
            {
                SceneReferences sceneReferences = SceneReferences.Get(scene.Value);
                if (sceneReferences.bookmarks.Any(i => i.target == target)) return;

                SceneBookmark item = new SceneBookmark(target);
                sceneReferences.bookmarks.Add(item);
                EditorUtility.SetDirty(sceneReferences);
                _sceneItems = null;
                sceneItemsList = null;
            }
            else
            {
                if (projectItems.Any(i => i.target == target)) return;
                ProjectBookmark item = new ProjectBookmark(target);
                projectItems.Add(item);
                projectItems.Sort(new ProjectBookmarkComparer());
                cachedLabels = null;
                _folders = null;
                Save();
            }
        }

        private void BottomBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginChangeCheck();
            float maxWidth = position.width - 15;
            Rect rect = GUILayoutUtility.GetRect(maxWidth, maxWidth, 20, 20);
            rect.xMin = rect.xMax - 100;
            rect.x -= 3;
            int id = FixedIDs.BookmarksGridSize;
            gridSize = (int) GUI.Slider(rect, gridSize, 0, minGridSize, maxGridSize, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, id);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(GridSizePref, gridSize);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void ClearFilter()
        {
            instance._filter = "";
            filteredItems = null;

            TextEditorRef.SetText(string.Empty);
        }

        public static bool Contain(Object target)
        {
            if (!target) return false;

            Scene? scene = GetScene(target);
            if (scene.HasValue && scene.Value.name != null)
            {
                SceneReferences sceneReferences = SceneReferences.Get(scene.Value, false);
                return sceneReferences && sceneReferences.bookmarks.Any(i => i.target == target);
            }

            return projectItems.Any(i => i.target == target);
        }

        private static void DisposeFolderItems()
        {
            if (selectedFolderItems == null) return;

            foreach (ProjectBookmark item in selectedFolderItems) item.Dispose();
            selectedFolderItems = null;
        }

        private void DrawClearButton()
        {
            if (clearContent == null) clearContent = new GUIContent(EditorIconContents.treeEditorTrash.image, "Delete all bookmarks");

            if (!GUILayoutUtils.ToolbarButton(clearContent)) return;
            if (!EditorUtility.DisplayDialog("Clear Bookmarks", "Do you really want to clear your bookmarks?", "Clear", "Cancel")) return;

            ReferenceManager.bookmarks.Clear();
            ReferenceManager.Save();

            foreach (SceneReferences sceneReference in SceneReferences.instances)
            {
                sceneReference.bookmarks.Clear();
                EditorUtility.SetDirty(sceneReference);
            }

            _sceneItems = null;
            _filter = string.Empty;
        }

        private void DrawFilterByLabel()
        {
            if (projectItems.Count == 0) return;

            if (cachedLabels == null)
            {
                var temp = new HashSet<string>();
                foreach (ProjectBookmark item in projectItems)
                {
                    string[] labels = AssetDatabase.GetLabels(item.target);
                    if (labels == null) continue;
                    
                    foreach (string label in labels)
                    {
                        if (!temp.Contains(label)) temp.Add(label);
                    }
                }

                foreach (ProjectFolderBookmark folder in folders)
                {
                    foreach (ProjectFolderBookmark.Item item in folder.items)
                    {
                        string[] labels = item.labels;
                        if (labels == null) continue;
                        
                        foreach (string label in labels)
                        {
                            if (!temp.Contains(label)) temp.Add(label);
                        }
                    }
                }

                cachedLabels = temp.OrderBy(v => v).ToArray();
            }

            if (cachedLabels.Length == 0) return;

            if (filterByLabelContent == null) filterByLabelContent = new GUIContent(EditorGUIUtility.IconContent("FilterByLabel").image, "Filter by Label");
            filterByLabelContent.text = activeLabel;
            if (!GUILayoutUtils.ToolbarButton(filterByLabelContent)) return;

            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add("None", string.IsNullOrEmpty(activeLabel), () =>
            {
                activeLabel = null;
                UpdateFilteredItems();
            });
            menu.AddSeparator();

            for (int i = 0; i < cachedLabels.Length; i++)
            {
                string label = cachedLabels[i];
                menu.Add(label, activeLabel == label, () =>
                {
                    activeLabel = label;
                    UpdateFilteredItems();
                });
            }
            menu.Show();
        }

        private void DrawFilterByType()
        {
            if (filterByTypeContent == null) filterByTypeContent = EditorGUIUtility.IconContent("FilterByType");
            if (!GUILayoutUtils.ToolbarButton(filterByTypeContent)) return;

            string[] names = GameObjectUtils.GetTypesDisplayNames();
            string assetType = "";
            Match match = Regex.Match(filter, @"(:)(\w*)");
            if (match.Success)
            {
                assetType = match.Groups[2].Value.ToUpperInvariant();
                if (assetType == "PREFAB") assetType = "GAMEOBJECT";
            }

            GenericMenuEx menu = GenericMenuEx.Start();
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                bool isSameType = name.ToUpperInvariant() == assetType;
                menu.Add(name, isSameType, () =>
                {
                    if (!string.IsNullOrEmpty(assetType))
                    {
                        if (isSameType) filter = Regex.Replace(filter, @"(:)(\w*)", "");
                        else filter = Regex.Replace(filter, @"(:)(\w*)", ":" + name);
                    }
                    else filter += ":" + name;
                });
            }
            menu.Show();
        }

        private void DrawItems()
        {
            if (filteredItems != null)
            {
                if (gridSize > minGridSize) DrawGridItems(filteredItems);
                else DrawTreeItems(filteredItems);
            }
            else if (selectedFolderItems != null)
            {
                if (gridSize > minGridSize) DrawGridItems(selectedFolderItems);
                else DrawTreeItems(selectedFolderItems);
            }
            else
            {
                if (sceneItems.Count > 0)
                {
                    showSceneItems = GUILayout.Toggle(showSceneItems, "Scene Items", EditorStyles.foldoutHeader);
                    if (showSceneItems)
                    {
                        if (sceneItemsList == null) sceneItemsList = new Dictionary<SceneReferences, ReorderableList>();
                        bool multiScene = sceneItems.Count > 1;
                        foreach (var pair in sceneItems)
                        {
                            if (pair.Key == null) continue;
                            
                            string label = multiScene? pair.Key.gameObject.scene.name: string.Empty;
                            if (gridSize > minGridSize) DrawGridItems(pair.Value, label);
                            else
                            {
                                ReorderableList list;
                                if (sceneItemsList.TryGetValue(pair.Key, out list))
                                {
                                    DrawTreeItems(ref list, pair.Value, label);
                                }
                                else
                                {
                                    DrawTreeItems(ref list, pair.Value, label);
                                    sceneItemsList[pair.Key] = list;
                                }
                            }
                        }
                    }
                }

                if (projectItems.Count > 0)
                {
                    showProjectItems = GUILayout.Toggle(showProjectItems, "Project Items", EditorStyles.foldoutHeader);
                    if (showProjectItems)
                    {
                        if (gridSize > minGridSize) DrawGridItems(projectItems);
                        else DrawTreeItems(ref projectItemsList, projectItems);
                    }
                }
            }
        }

        private void FolderItemsToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            BookmarkItem current = selectedFoldersStack.Last();

            if (selectedFoldersStack.Count > 1)
            {
                if (GUILayoutUtils.ToolbarButton(EditorIconContents.animationFirstKey))
                {
                    ClearFilter();

                    DisposeFolderItems();
                    selectedFoldersStack.Clear();
                    selectedFolderItems = null;
                    return;
                }
            }

            if (GUILayoutUtils.ToolbarButton(EditorIconContents.animationPrevKey))
            {
                ClearFilter();

                selectedFoldersStack.RemoveAt(selectedFoldersStack.Count - 1);
                if (selectedFoldersStack.Count > 0) SelectParentFolder(selectedFoldersStack.Last());
                else DisposeFolderItems();
            }

            EditorGUILayout.LabelField(current.title);
            EditorGUILayout.EndHorizontal();
        }

        private static Scene? GetScene(Object target)
        {
            if (target is GameObject) return (target as GameObject).scene;
            if (target is Component) return (target as Component).gameObject.scene;
            return null;
        }

        private static void InitFolderItems(BookmarkItem folderItem)
        {
            Object target = folderItem.target;
            DisposeFolderItems();

            string assetPath = AssetDatabase.GetAssetPath(target);
            IEnumerable<string> entries = Directory.GetFileSystemEntries(assetPath);
            List<ProjectBookmark> tempItems = new List<ProjectBookmark>();
            foreach (string entry in entries)
            {
                if (entry.EndsWith(".meta")) continue;

                Object asset = AssetDatabase.LoadAssetAtPath<Object>(entry);
                if (!asset) continue;

                ProjectBookmark item = new ProjectBookmark(asset);
                tempItems.Add(item);
            }

            selectedFolderItems = tempItems.ToList();
        }

        private static void InitPreview(BookmarkItem item)
        {
            if (item.isProjectItem)
            {
                if (item.target is GameObject)
                {
                    bool isLoading = AssetPreview.IsLoadingAssetPreview(item.target.GetInstanceID());
                    if (isLoading)
                    {
                        item.preview = EditorResources.prefabTexture;
                    }
                    else
                    {
                        item.preview = AssetPreview.GetAssetPreview(item.target);
                        if (!item.preview) item.preview = AssetPreview.GetMiniThumbnail(item.target);
                        item.previewLoaded = true;
                    }
                    
                    return;
                }

                if (item.target is UnityEngine.Tilemaps.Tile)
                {
                    UnityEngine.Tilemaps.Tile tile = item.target as UnityEngine.Tilemaps.Tile;
                    if (tile && tile.sprite)
                    {
                        item.preview = tile.sprite.texture;
                        item.previewLoaded = true;
                        return;
                    }
                }
            }

            item.preview = AssetPreview.GetMiniThumbnail(item.target);
            item.previewLoaded = true;
        }

        private static void InitProjectFolders()
        {
            _folders = projectItems
                .Where(i => i.target is DefaultAsset && AssetDatabase.IsValidFolder(i.path))
                .Select(i => new ProjectFolderBookmark(i)).ToArray();
        }

        public static void InsertBookmarkMenu(GenericMenuEx menu, Object target)
        {
            if (Contain(target)) menu.Add("Remove Bookmark", () => Remove(target));
            else menu.Add("Add Bookmark", () => Add(target));
        }

        private void OnDestroy()
        {
            if (selectedFolderItems != null)
            {
                foreach (ProjectBookmark item in selectedFolderItems) item.Dispose();
                selectedFolderItems = null;
            }

            selectedFoldersStack = null;
            if (_folders != null)
            {
                foreach (ProjectFolderBookmark folder in _folders) folder.Dispose();
                _folders = null;
            }

            cachedLabels = null;
            instance = null;
        }

        private void OnEnable()
        {
            instance = this;
            minSize = new Vector2(250, 250);

            gridSize = EditorPrefs.GetInt(GridSizePref, 16);

            showContent = new GUIContent(Styles.isProSkin? Icons.openNewWhite: Icons.openNewBlack, "Show");
            closeContent = new GUIContent(Styles.isProSkin ? Icons.closeWhite: Icons.closeBlack, "Remove");

            hiddenContent = EditorIconContents.sceneVisibilityHiddenHover;
            visibleContent = EditorIconContents.sceneVisibilityVisibleHover;
        }

        private void OnGUI()
        {
            if (!instance) instance = this;
            if (!emptyTexture) emptyTexture = Resources.CreateSinglePixelTexture(1f, 0f);
            if (showContentStyle == null || !showContentStyle.normal.background)
            {
                showContentStyle = new GUIStyle(Styles.transparentButton);
                showContentStyle.margin.top = 6;
                showContentStyle.margin.left = 6;
            }
            if (selectedStyle == null)
            {
                selectedStyle = new GUIStyle(Styles.selectedRow);
                selectedStyle.fixedHeight = 0;
            }

            if (removeItem != null) Remove(removeItem);

            ProcessEvents();
            Toolbar();

            if (selectedFoldersStack != null && selectedFoldersStack.Count > 0)
            {
                FolderItemsToolbar();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            removeItem = null;
            DrawItems();

            if (removeItem != null)
            {
                Remove(removeItem);
                removeItem = null;
                Save();
                UpdateFilteredItems();
            }

            EditorGUILayout.EndScrollView();

            BottomBar();
        }

        private static void OnUpdateSceneReferences()
        {
            _sceneItems = null;
            if (instance) instance.UpdateFilteredItems();
        }

        private static bool OnValidate()
        {
            return Prefs.bookmarksHotKey && Event.current.modifiers == Prefs.bookmarksModifiers && Event.current.keyCode == Prefs.bookmarksKeyCode;
        }

        private void ProcessEvents()
        {
            if (WindowsHelper.mouseOverWindow != this) return;
            if (selectedFolderItems != null) return;

            Event e = Event.current;
            if (e.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                e.Use();
            }
            else if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                e.Use();

                foreach (Object obj in DragAndDrop.objectReferences) Add(obj);
            }
            else if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.F && (e.modifiers == EventModifiers.Control || e.modifiers == EventModifiers.Command))
                {
                    focusOnSearch = true;
                    e.Use();
                }
            }
        }

        private void ProcessDoubleClick(BookmarkItem item)
        {
            if (EditorApplication.timeSinceStartup - lastClickTime > 0.3)
            {
                lastClickTime = EditorApplication.timeSinceStartup;
                return;
            }

            lastClickTime = 0;

            if (item.target is AudioClip)
            {
                AudioClip audioClip = item.target as AudioClip;
                if (AudioUtilsRef.IsClipPlaying(audioClip)) AudioUtilsRef.StopClip(audioClip);
                else AudioUtilsRef.PlayClip(audioClip);
            }
            else if (item.target is SceneAsset)
            {
                string path = AssetDatabase.GetAssetPath(item.target);

                if (SceneManagerHelper.AskForSave(SceneManager.GetActiveScene()))
                {
                    EditorSceneManager.OpenScene(path);
                }
            }
            else if (item.target is DefaultAsset)
            {
                FileAttributes attributes = File.GetAttributes(AssetDatabase.GetAssetPath(item.target));
                if ((attributes & FileAttributes.Directory) == FileAttributes.Directory) SelectFolder(item);
            }
            else if (item.target is Component)
            {
                ComponentWindow.Show(item.target as Component);
            }
            else if (item.target is GameObject)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(item.gameObject))
                {
                    GameObjectUtils.OpenPrefab(AssetDatabase.GetAssetPath(item.target));
                }
            }
            else EditorUtility.OpenWithDefaultApp(AssetDatabase.GetAssetPath(item.target));
        }

        public static void Redraw()
        {
            if (instance) instance.Repaint();
        }

        public static void Remove(Object item)
        {
            if (!item) return;

            Scene? scene = GetScene(item);
            if (scene.HasValue && scene.Value.name != null)
            {
                SceneReferences sceneReferences = SceneReferences.Get(scene.Value, false);
                if (!sceneReferences) return;

                if (sceneReferences.bookmarks.RemoveAll(i => i.target == item) > 0)
                {
                    EditorUtility.SetDirty(sceneReferences);
                    _sceneItems = null;
                }
            }
            else
            {
                projectItems.RemoveAll(i => i.target == item);
                cachedLabels = null;
                _folders = null;
                Save();
            }

            if (instance)
            {
                instance.UpdateFilteredItems();
            }
        }

        private static void Remove(BookmarkItem item)
        {
            if (item == null) return;

            if (!item.target)
            {
                if (item.isProjectItem) projectItems.Remove(item as ProjectBookmark);
                else
                {
                    List<SceneReferences> sceneReferencesList = SceneReferences.instances;
                    foreach (SceneReferences sceneReferences in sceneReferencesList)
                    {
                        if (sceneReferences.bookmarks.Remove(item as SceneBookmark))
                        {
                            EditorUtility.SetDirty(sceneReferences);
                        }
                    }
                }
            }
            else Remove(item.target);
        }

        private static void RemoveLate(BookmarkItem item)
        {
            removeItem = item;
        }

        private static void Save()
        {
            foreach (KeyValuePair<SceneReferences, List<SceneBookmark>> pair in sceneItems)
            {
                EditorUtility.SetDirty(pair.Key);
            }

            ReferenceManager.Save();
        }

        private static void SelectFolder(BookmarkItem folderItem)
        {
            ClearFilter();

            InitFolderItems(folderItem);

            if (selectedFoldersStack == null) selectedFoldersStack = new List<BookmarkItem>();
            selectedFoldersStack.Add(folderItem);
        }

        private void SelectParentFolder(BookmarkItem folderItem)
        {
            InitFolderItems(folderItem);
        }

        private void ShowContextMenu(BookmarkItem item)
        {
            if (item.target is Component)
            {
                ComponentUtils.ShowContextMenu(item.target as Component);
            }
            else if (item.target is GameObject)
            {
                if (!item.isProjectItem) GameObjectUtils.ShowContextMenu(false, item.target as GameObject);
                else ShowOtherContextMenu(item);
            }
            else if (item.target is SceneAsset)
            {
                GenericMenuEx menu = GenericMenuEx.Start();

                menu.Add("Open", () =>
                {
                    if (SceneManagerHelper.AskForSave(SceneManager.GetActiveScene()))
                    {
                        string path = AssetDatabase.GetAssetPath(item.target);
                        EditorSceneManager.OpenScene(path);
                    }
                });

                menu.Add("Open Additive", () =>
                {
                    string path = AssetDatabase.GetAssetPath(item.target);
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                });

                if (item.canBeRemoved)
                {
                    menu.AddSeparator();
                    menu.Add("Remove Bookmark", () => RemoveLate(item));
                }
                menu.Show();
            }
            else ShowOtherContextMenu(item);
        }

        public static EditorWindow ShowDropDownWindow(Vector2? mousePosition = null)
        {
            if (!mousePosition.HasValue) mousePosition = Event.current.mousePosition;
            Bookmarks wnd = CreateInstance<Bookmarks>();
            wnd.titleContent = new GUIContent("Bookmarks");
            Vector2 position = GUIUtility.GUIToScreenPoint(mousePosition.Value);
            Vector2 size = Prefs.defaultWindowSize;
            Rect rect = new Rect(position - size / 2, size);
            if (rect.y < 30) rect.y = 30;

            wnd.position = rect;
            wnd.ShowPopup();
            wnd.Focus();

            PinAndClose.Show(wnd, rect, wnd.Close, () =>
            {
                Rect wRect = wnd.position;
                wRect.yMin -= PinAndClose.Height;
                ShowWindow().position = wRect;
                wnd.Close();
            }, "Bookmarks");
            return wnd;
        }

        private void ShowOtherContextMenu(BookmarkItem item)
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            if (item.canBeRemoved) menu.Add("Remove Bookmark", () => RemoveLate(item));
            menu.Show();
        }

        [MenuItem(WindowsHelper.MenuPath + "Bookmarks", false, MenuItemOrder.Bookmarks)]
        public static EditorWindow ShowWindow()
        {
            return ShowWindow(null);
        }

        public static EditorWindow ShowWindow(Vector2? mousePosition)
        {
            Bookmarks wnd = CreateInstance<Bookmarks>();
            wnd.titleContent = new GUIContent("Bookmarks");
            wnd.Show();

            Vector2? mp = null;
            if (mousePosition.HasValue) mp = mousePosition.Value;
            else if (Event.current != null) mp = Event.current.mousePosition;

            if (mp.HasValue)
            {
                Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mp.Value);
                Vector2 size = Prefs.defaultWindowSize;
                wnd.position = new Rect(screenPoint - size / 2, size);
            }
            return wnd;
        }

        public static EditorWindow ShowUtilityWindow(Vector2? mousePosition = null)
        {
            if (!mousePosition.HasValue) mousePosition = Event.current.mousePosition;
            Bookmarks wnd = CreateInstance<Bookmarks>();
            wnd.titleContent = new GUIContent("Bookmarks");
            wnd.ShowUtility();
            wnd.Focus();
            Vector2 size = Prefs.defaultWindowSize;
            wnd.position = new Rect(GUIUtility.GUIToScreenPoint(mousePosition.Value) - size / 2, size);
            return wnd;
        }

        private static void ToggleWindow()
        {
            if (instance) instance.Close();
            else ShowWindow();
        }

        private void Toolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("UEEBookmarkSearchTextField");
            _filter = GUILayoutUtils.ToolbarSearchField(_filter);

            if (focusOnSearch && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("UEEBookmarkSearchTextField");
                focusOnSearch = false;
            }

            if (EditorGUI.EndChangeCheck()) UpdateFilteredItems();

            DrawFilterByType();
            DrawFilterByLabel();
            DrawClearButton();

            if (GUILayoutUtils.ToolbarButton(TempContent.Get("?", "Help"))) Links.OpenDocumentation("bookmarks");

            EditorGUILayout.EndHorizontal();
        }

        private void UpdateFilteredItems()
        {
            if (string.IsNullOrEmpty(_filter) && string.IsNullOrEmpty(activeLabel))
            {
                filteredItems = null;
                return;
            }

            string assetType;
            string pattern = SearchableItem.GetPattern(_filter, out assetType);

            IEnumerable<BookmarkItem> query;
            if (selectedFolderItems == null)
            {
                query = projectItems.Select(i => i as BookmarkItem);
                query = query.Concat(folders.SelectMany(f => f.items));
                if (string.IsNullOrEmpty(activeLabel)) query = query.Concat(sceneItems.SelectMany(sr => sr.Value));
            }
            else query = selectedFolderItems;

            if (string.IsNullOrEmpty(activeLabel))
            {
                query = query.Where(i => i.Update(pattern, assetType));
            }
            else
            {
                query = query.Where(i => i.HasLabel(activeLabel) && i.Update(pattern, assetType));
            }

            filteredItems = query.ToList();
        }
    }
}