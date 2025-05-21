using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Utilities.AssetsCache
{
    public class AssetsCache : EditorWindow
    {
        private const string StrSaveName = "AssetsCacheSave";
        private const string StrShowTab = "Tabs";
        private const string StrShowOptions = "Options";
        private const string StrDone = "Done";
        private const string StrEdit = "Edit";
        private const string StrReset = "Reset";
        private const string StrDrag = "Drag and drop assets here";
        private const string StrOpen = "Ping";
        private const string StrRemove = "-";
        private const string StrShowPath = "Show path";
        private const string StrShowNumber = "Show number";
        private const string StrTip = "Tip";
        private const string StrList = "List";
        private const string StrGrid = "Grid";
        private bool isDrag;
        private bool _showPath;
        private float contentWidth;

        private int _tipIndex;

        private List<string> _tipList = new List<string>()
        {
            "Hold control & click to ping",
            "Hold alt & click to remove",
            "Click to icon, drag and drop to create object or reference",
        };

        private bool ShowPath
        {
            get => _showPath;
            set
            {
                _showPath = value;
                EditorPrefs.SetBool("Assets_Cache_ShowPath", value);
            }
        }

        private bool _showNumber;

        private bool ShowNumber
        {
            get => _showNumber;
            set
            {
                _showNumber = value;
                EditorPrefs.SetBool("Assets_Cache_ShowNumber", value);
            }
        }

        public List<CacheTabInfo> tabs = new List<CacheTabInfo>() { new CacheTabInfo { id = 0, tabName = "Common" } };
        public List<CacheObjectInfo> objects = new List<CacheObjectInfo>();
        public List<CacheObjectInfo> filterList = new List<CacheObjectInfo>();
        public string progress;

        public string searchText = string.Empty;
        private string _oldSearchText;
        private Vector2 _objectsScrollPosition;
        private Vector2 _tabsScrollPosition;
        private EditorCacheStyle _s;
        private int _oldTabIndex = -1;
        private int _tabIndex;
        private bool _isEditMode;
        private bool _isShowTabs;
        private bool _isShowTip = true;
        private bool _showOptions;
        private ReorderableList _objectsReorderableList;
        private ReorderableList _tabsReorderableList;

        private float _currentTabViewWidth;
        private bool _isResize;
        private Rect _cursorChangeRect;
        private GUILayoutOption expandWidthFalse = GUILayout.ExpandWidth(false);
        private double _time;

        private void OnEnable()
        {
            _time = EditorApplication.timeSinceStartup;
            _oldSearchText = "old";
            InitReOrderTabList();

            _currentTabViewWidth = 110;
            _cursorChangeRect = new Rect(_currentTabViewWidth, 0, 5, position.size.y);
            ShowPath = EditorPrefs.GetBool("Assets_Cache_ShowPath", false);
            ShowNumber = EditorPrefs.GetBool("Assets_Cache_ShowNumber", false);
            _isShowTip = EditorPrefs.GetBool("Assets_Cache_ShowTip", true);

            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        [MenuItem("Tools/Assets Cache")]
        public static void Init()
        {
            var w = GetWindow<AssetsCache>();
            w.titleContent.text = "Assets Cache";
            w.Read();
            w.Show();
        }

        private void Update()
        {
            if (!_isShowTip)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup - _time >= 3)
            {
                _time = EditorApplication.timeSinceStartup;
                _tipIndex++;
                if (_tipIndex >= _tipList.Count)
                {
                    _tipIndex = 0;
                }
            }
        }

        private void InitReOrderTabList()
        {
            _tabsReorderableList = new ReorderableList(tabs, typeof(CacheTabInfo), true, false, true, true);
            _tabsReorderableList.drawElementCallback += DrawTabElementCallback;
            _tabsReorderableList.onChangedCallback += _ => { Save(); };
            _tabsReorderableList.onAddCallback += _ => { AddNewTab(); };
        }

        private void FilterWhenTabChanged()
        {
            if (_oldTabIndex != _tabIndex)
            {
                if (tabs.Count > 0)
                {
                    _oldTabIndex = _tabIndex;
                    if (tabs[_tabIndex].list == null) tabs[_tabIndex].list = new List<CacheObjectInfo>();
                    objects = tabs[_tabIndex].list;
                    Filter();
                }
                else
                    tabs = new List<CacheTabInfo> { new CacheTabInfo() { id = 0, tabName = "Common" } };

                EditorGUI.FocusTextInControl("");
                UpdatePreviewSize();
            }
        }

        private void UpdatePreviewSize()
        {
            _s.previewTexture.fixedWidth = _s.previewTexture.fixedHeight = tabs[_tabIndex].iconSize;
        }

        private void OnGUI()
        {
            if (_s == null)
            {
                _s = new EditorCacheStyle();
                UpdatePreviewSize();
            }

            FilterWhenTabChanged();
            DisplayTopBar();
            GUILayout.BeginHorizontal();
            if (_isShowTabs)
            {
                DisplayTabs();
                contentWidth = position.width - _currentTabViewWidth - 80;
            }
            else
            {
                contentWidth = position.width - 80;
            }

            DisplayObjectGroup();

            GUILayout.EndHorizontal();
            if (_isShowTip) GUILayout.Label(_tipList[_tipIndex], EditorStyles.helpBox);
        }

        private void ShowOptions()
        {
            ShowPath = GUILayout.Toggle(ShowPath, StrShowPath, _s.expandWidthFalse);
            ShowNumber = GUILayout.Toggle(ShowNumber, StrShowNumber, _s.expandWidthFalse);
            EditorGUIUtility.labelWidth = 60;
            var iconSize = EditorGUILayout.Slider("Icon Size", tabs[_tabIndex].iconSize, 20, 100);
            if (Math.Abs(iconSize - tabs[_tabIndex].iconSize) > .01f)
            {
                tabs[_tabIndex].iconSize = iconSize;
                UpdatePreviewSize();
                Save();
            }
        }

        private void DisplayObjects()
        {
            GUILayout.BeginVertical();
            if (_showOptions)
            {
                ShowOptions();
            }

            _objectsScrollPosition = GUILayout.BeginScrollView(_objectsScrollPosition);
            if (_isEditMode) DisplayReorderList();
            else
            {
                if (tabs[_tabIndex].gridView)
                {
                    DisplayGridObject();
                }
                else
                {
                    DisplayListObjects();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DisplayObjectGroup()
        {
            if (objects == null) return;
            DisplayObjects();
            UpdateDragAndDrop();
        }

        private void DisplayTopBar()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(StrShowTab, _isShowTabs ? _s.buttonStyle : EditorStyles.toolbarButton, _s.expandWidthFalse))
            {
                _isShowTabs = !_isShowTabs;
            }

            if (GUILayout.Button(StrShowOptions, _showOptions ? _s.buttonStyle : EditorStyles.toolbarButton, _s.expandWidthFalse))
            {
                _showOptions = !_showOptions;
            }

            if (GUILayout.Button(tabs[_tabIndex].gridView ? StrList : StrGrid, EditorStyles.toolbarButton, _s.expandWidthFalse))
            {
                tabs[_tabIndex].gridView = !tabs[_tabIndex].gridView;
            }

            if (GUILayout.Button(StrTip, _isShowTip ? _s.buttonStyle : EditorStyles.toolbarButton, _s.expandWidthFalse))
            {
                _isShowTip = !_isShowTip;
                EditorPrefs.SetBool("Assets_Cache_ShowTip", _isShowTip);
            }

            searchText = EditorGUILayout.TextField(searchText, _s.toolbarSeachTextField);
            GUILayout.Label(progress, _s.expandWidthFalse);
            if (GUILayout.Button(StrReset, EditorStyles.toolbarButton, _s.expandWidthFalse))
            {
                objects.Clear();
                filterList.Clear();
                progress = $"{filterList.Count.ToString()}/{objects.Count.ToString()}";
                Focus();
                Save();
            }

            if (_oldSearchText != searchText)
            {
                _oldSearchText = searchText;
                Filter();
            }

            if (!_isEditMode)
            {
                if (GUILayout.Button(StrEdit, _s.buttonStyle, _s.expandWidthFalse))
                {
                    _isEditMode = true;
                }
            }
            else
            {
                if (GUILayout.Button(StrDone, _s.buttonStyle, _s.expandWidthFalse))
                {
                    _isEditMode = false;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DisplayDetail()
        {
        }

        private void DisplayReorderList()
        {
            GUILayout.BeginVertical();
            _objectsReorderableList.DoLayoutList();
            GUILayout.EndVertical();
        }

        private void DisplayGridObject()
        {
            var size = tabs[_tabIndex].iconSize;
            int itemsPerRow = (int)(contentWidth / size);
            if (itemsPerRow <= 0)
            {
                itemsPerRow = 1;
            }

            GUILayout.BeginVertical();
            int count = 0;
            int page = 0;
            int countItems = 0;
            GUILayout.BeginHorizontal();

            bool isHoleControl = Event.current.control;
            bool isHoleAlt = Event.current.alt;
            bool isMouseDown = Event.current.type == EventType.MouseDown;

            CacheObjectInfo hoverObject = null;
            foreach (var o in filterList)
            {
                bool ignore = false;
                if (o.previewTexture == null)
                {
                    o.previewTexture = AssetPreview.GetAssetPreview(o.obj);
                    if (o.previewTexture == null && !o.isPrefab)
                    {
                        o.previewTexture = AssetPreview.GetMiniThumbnail(o.obj);
                    }
                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(o.previewTexture, GUILayout.Width(size), GUILayout.Height(size));
                GUILayout.EndVertical();
                var lastPreviewRect = GUILayoutUtility.GetLastRect();
                if (isMouseDown && lastPreviewRect.Contains(Event.current.mousePosition))
                {
                    if (isHoleControl)
                    {
                        o.Ping();
                    }
                    else if (isHoleAlt)
                    {
                        UpdateRemove(o);
                        GUILayout.EndHorizontal();
                        break;
                    }
                    else if (lastPreviewRect.Contains(Event.current.mousePosition))
                    {
                        isDrag = true;
                        GUIUtility.hotControl = 0;
                        DragAndDrop.PrepareStartDrag();
                        DragAndDrop.objectReferences = new[] { o.obj };
                        DragAndDrop.SetGenericData("DRAG_ID", o.obj);
                        DragAndDrop.StartDrag("A");
                    }
                }

                page++;
                countItems++;
                if (countItems >= itemsPerRow)
                {
                    countItems = 0;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DisplayListObjects()
        {
            GUILayout.BeginVertical();
            if (filterList.Count == 0)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Label(StrDrag, _s.textEmpty);
                EditorGUI.EndDisabledGroup();
            }

            int count = 1;
            bool isHoleControl = Event.current.control;
            bool isHoleAlt = Event.current.alt;
            bool isMouseDown = Event.current.type == EventType.MouseDown;
            foreach (var o in filterList)
            {
                bool ignore = false;
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    if (o.previewTexture == null)
                    {
                        o.previewTexture = AssetPreview.GetAssetPreview(o.obj);
                        if (o.previewTexture == null && !o.isPrefab)
                        {
                            o.previewTexture = AssetPreview.GetMiniThumbnail(o.obj);
                        }
                    }

                    if (_showNumber)
                    {
                        GUILayout.Label(count.ToString(), _s.expandWidthFalse);
                    }

                    GUILayout.Label(o.previewTexture, _s.previewTexture);
                    var lastPreviewRect = GUILayoutUtility.GetLastRect();
                    if (lastPreviewRect.Contains(Event.current.mousePosition))
                    {
                        if (isMouseDown)
                        {
                            isDrag = true;
                            ignore = true;
                            GUIUtility.hotControl = 0;
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new[] { o.obj };
                            DragAndDrop.SetGenericData("DRAG_ID", o.obj);
                            DragAndDrop.StartDrag("A");
                        }
                    }

                    GUILayout.BeginVertical();
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(o.GetDisplayName(), GUILayout.MinWidth(30));
                        if (o.location == CacheObjectLocation.Assets)
                        {
                            if (GUILayout.Button(StrOpen, _s.expandWidthFalse))
                            {
                                ignore = true;
                                o.Ping();
                            }
                        }

                        if (GUILayout.Button(StrRemove, _s.expandWidthFalse))
                        {
                            ignore = true;
                            UpdateRemove(o);
                            GUILayout.EndHorizontal();
                            break;
                        }

                        GUILayout.EndHorizontal();
                    }
                    if (_showPath)
                    {
                        if (!string.IsNullOrEmpty(o.path))
                        {
                            GUILayout.Label(o.path, _s.text1);
                        }
                        else if (!string.IsNullOrEmpty(o.prefabPath))
                        {
                            GUILayout.Label(o.prefabPath, _s.text1);
                        }
                    }

                    // ViewParents(o);
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                var itemRect = GUILayoutUtility.GetLastRect();
                if (!ignore && Event.current.type == EventType.MouseDown &&
                    itemRect.Contains(Event.current.mousePosition))
                {
                    if (isHoleControl)
                    {
                        o.Ping();
                    }
                    else if (isHoleAlt)
                    {
                        ignore = true;
                        UpdateRemove(o);
                        GUILayout.EndHorizontal();
                        break;
                    }
                    else if (o.obj == null)
                    {
                        o.Ping();
                    }
                    else if (o.obj is DefaultAsset)
                    {
                        AssetDatabase.OpenAsset(o.obj);
                    }
                    else if (!string.IsNullOrEmpty(o.prefabPath))
                    {
                        AssetDatabase.OpenAsset(o.obj);
                    }
                    else
                    {
                        AssetDatabase.OpenAsset(o.obj);
                    }
                }

                count++;
            }

            GUILayout.EndVertical();
        }

        public void UpdateRemove(CacheObjectInfo o)
        {
            objects.Remove(o);
            filterList.Remove(o);
            progress = $"{filterList.Count.ToString()}/{objects.Count.ToString()}";
            Save();
            Focus();
        }

        private void ResizeScrollView()
        {
            EditorGUIUtility.AddCursorRect(_cursorChangeRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && _cursorChangeRect.Contains(Event.current.mousePosition))
            {
                _isResize = true;
            }

            if (_isResize)
            {
                _currentTabViewWidth = Event.current.mousePosition.x;
                _currentTabViewWidth = Mathf.Clamp(_currentTabViewWidth, 100, 200);
                _cursorChangeRect.Set(_currentTabViewWidth, _cursorChangeRect.y, _cursorChangeRect.width,
                    _cursorChangeRect.height);
                Repaint();
            }

            if (Event.current.type == EventType.MouseUp)
                _isResize = false;
        }

        private void DisplayTabs()
        {
            GUILayout.BeginVertical(_s.ProjectBrowserIconAreaBg, GUILayout.Width(_currentTabViewWidth));
            _tabsScrollPosition = GUILayout.BeginScrollView(_tabsScrollPosition);
            if (!_isEditMode)
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    var buttonStyle = i == _tabIndex ? _s.SelectionRect : _s.RectangleToolSelection;

                    if (_isEditMode && i == _tabIndex)
                    {
                        GUILayout.BeginVertical();
                        tabs[i].tabName = EditorGUILayout.TextField(tabs[i].tabName, _s.expandWidth200);
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        if (GUILayout.Button(tabs[i].tabName, buttonStyle, GUILayout.Width(_currentTabViewWidth - 5)))
                        {
                            _tabIndex = i;
                        }
                    }
                }
            }
            else
            {
                _tabsReorderableList.DoLayoutList();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            ResizeScrollView();
        }

        private void AddNewTab()
        {
            if (tabs.Count == 0)
            {
                tabs.Add(new CacheTabInfo { id = 0, tabName = "New tab", list = new List<CacheObjectInfo>() });
            }
            else
            {
                tabs.Add(new CacheTabInfo
                    { id = tabs.Max(s => s.id) + 1, tabName = "New tab", list = new List<CacheObjectInfo>() });
            }

            Save();
        }

        private void Filter()
        {
            if (string.IsNullOrEmpty(searchText))
            {
                filterList = objects;
            }
            else
            {
                var temp = searchText.ToLower();
                filterList = objects.Where(s => s.Name.ToLower().Contains(temp)).ToList();
            }

            progress = $"{filterList.Count.ToString()}/{objects.Count.ToString()}";
            _objectsReorderableList =
                new ReorderableList(filterList, typeof(CacheObjectInfo), true, false, false, false);
            _objectsReorderableList.drawElementCallback += DrawObjectElementCallback;
            _objectsReorderableList.onChangedCallback += _ => { Save(); };
        }

        private void DrawObjectElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            var data = filterList[index];
            var avatarRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            GUI.Label(avatarRect, data.previewTexture);
            var nameRect = new Rect(rect.x + rect.height + 3, rect.y, rect.width - rect.height - 3, rect.height);
            GUI.Label(nameRect, data.GetDisplayName());
        }

        private void DrawTabElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            var data = tabs[index];
            var nameRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            data.tabName = GUI.TextField(nameRect, data.tabName);
        }

        void UpdateDragAndDrop()
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                if (isDrag)
                {
                    isDrag = false;
                    return;
                }

                if (DragAndDrop.paths.Length == 0 && DragAndDrop.objectReferences.Length > 0)
                {
                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        var currentPrefab = PrefabStageUtility.GetCurrentPrefabStage();
                        string assetPath = obj.name;
                        var transform = (obj as GameObject).transform;
                        GetScenePath(transform.transform, ref assetPath);
                        var item = new CacheObjectInfo
                        {
                            Name = obj.name,
                            path = assetPath,
                            parents = GetPrefabParents(new List<string>(), transform)
                        };
                        item.SetDisplayName();

                        CacheObjectLocation location = CacheObjectLocation.Assets;
                        if (currentPrefab != null)
                        {
                            item.prefabPath = currentPrefab.prefabAssetPath;
                            item.location = CacheObjectLocation.Prefab;
                        }
                        else
                        {
                            item.location = CacheObjectLocation.Scene;
                        }

                        if (objects.Find(s => s.obj == item.obj) == null)
                        {
                            objects.Add(item);
                            Save();
                        }
                        else if (objects.Find(s => s.path == item.path) == null)
                        {
                            objects.Add(item);
                            Save();
                        }
                    }

                    Filter();
                }
                else if (DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences.Length == 0)
                {
                    foreach (string path in DragAndDrop.paths)
                    {
                        Debug.Log("- " + path);
                    }
                }
                else if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        Object obj = DragAndDrop.objectReferences[i];
                        if (objects.Find(s => s.obj == obj) == null)
                        {
                            var item = new CacheObjectInfo
                            {
                                Name = obj.name,
                                location = CacheObjectLocation.Assets,
                                obj = obj,
                                path = AssetDatabase.GetAssetPath(obj),
                            };
                            item.SetDisplayName();
                            objects.Add(item);
                            Save();
                        }
                    }

                    Filter();
                }
            }
        }

        private List<string> GetPrefabParents(List<string> list, Transform target)
        {
            if (target.parent != null)
            {
                list.Insert(0, target.parent.name);
                GetPrefabParents(list, target.parent);
            }

            return list;
        }

        private void ViewParents(CacheObjectInfo cpInfo)
        {
            if (!_showPath || cpInfo.parents.Count <= 0) return;
            GUILayout.BeginHorizontal();
            foreach (var parent in cpInfo.parents)
            {
                if (parent == null)
                {
                    continue;
                }

                if (GUILayout.Button(parent, _s.parent, _s.expandWidthFalse))
                {
                    // Ping(parent.transform);
                }
            }

            GUILayout.EndHorizontal();
        }

        private void Ping(Component component)
        {
//             if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(component)))
//             {
// #if UNITY_2021_OR_NEWER
//             var stage = PrefabStageUtility.OpenPrefab(AssetDatabase.GetAssetPath(component));
//             currentObject = stage.prefabContentsRoot;
// #else
//                 AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(component)));
//                 currentObject = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
// #endif
//
//                 Find(currentObject);
//             }

            SceneView.lastActiveSceneView.LookAt(component.gameObject.transform.position);
            Selection.activeObject = component;
            EditorGUIUtility.PingObject(component);
        }

        private void GetScenePath(Transform obj, ref string path)
        {
            var parent = obj.parent;
            if (parent != null)
            {
                path = parent.name + "/" + path;
                GetScenePath(parent, ref path);
            }
        }

        [Serializable]
        public class CacheObjectInfo
        {
            public Object obj;
            public string path;
            public string prefabPath;
            public bool isPrefab;
            public string Name;
            public CacheObjectLocation location;
            public Texture2D previewTexture;
            public string displayName;
            public List<string> parents = new List<string>();

            public void SetDisplayName()
            {
                isPrefab = path.EndsWith(".prefab");
                displayName = Name;
                // displayName = $"{GetPrefix()} {Name}";
            }

            public string GetDisplayName()
            {
                return displayName;
            }

            public string GetPrefix()
            {
                if (location == CacheObjectLocation.Assets)
                    return "A:";
                if (location == CacheObjectLocation.Scene)
                    return "S:";
                return "P:";
            }

            public void OpenPrefab()
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)));
                var rootGameObjects = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;

                var arrayPath = path.Split('/').ToList();
                arrayPath.RemoveAt(0);
                arrayPath.RemoveAt(0);
                var newPath = string.Join("/", arrayPath);
                var obj = rootGameObjects.transform.Find(newPath);
                if (obj != null)
                {
                    SceneView.lastActiveSceneView.LookAt(obj.gameObject.transform.position);
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
            }

            public void PingLocation(int locationIndex)
            {
                if (location == CacheObjectLocation.Prefab)
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)));
                    var rootGameObjects = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;

                    var arrayPath = path.Split('/').ToList();
                    arrayPath.RemoveAt(0);
                    arrayPath.RemoveAt(0);

                    var newPath = string.Join("/", arrayPath);
                    var obj = rootGameObjects.transform.Find(newPath);
                    if (obj != null)
                    {
                        SceneView.lastActiveSceneView.LookAt(obj.gameObject.transform.position);
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }
            }

            public void Ping()
            {
                if (location == CacheObjectLocation.Assets)
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
                else if (location == CacheObjectLocation.Prefab)
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)));
                    var rootGameObjects = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;

                    var arrayPath = path.Split('/').ToList();
                    arrayPath.RemoveAt(0);
                    arrayPath.RemoveAt(0);
                    var newPath = string.Join("/", arrayPath);
                    var obj = rootGameObjects.transform.Find(newPath);
                    if (obj != null)
                    {
                        SceneView.lastActiveSceneView.LookAt(obj.gameObject.transform.position);
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }
                else if (location == CacheObjectLocation.Scene)
                {
                    if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                    {
                        PrefabUtility.UnloadPrefabContents(
                            PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot);
                    }

                    var arrayPath = path.Split('/').ToList();
                    arrayPath.RemoveAt(0);
                    var newPath = string.Join("/", arrayPath);

                    if (arrayPath.Count == 0)
                        newPath = path;

                    Transform obj = null;
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        bool check = false;
                        var scene = SceneManager.GetSceneAt(i);
                        var rootGameObjects = scene.GetRootGameObjects().ToList();
                        foreach (var gameObject in rootGameObjects)
                        {
                            obj = gameObject.transform.Find(newPath);
                            if (obj != null)
                            {
                                check = true;
                                break;
                            }
                        }

                        if (check)
                            break;

                        var tempObj = rootGameObjects.Find(s => s.name == newPath);
                        if (tempObj != null)
                        {
                            obj = tempObj.transform;
                            break;
                        }
                    }

                    if (obj != null)
                    {
                        SceneView.lastActiveSceneView.LookAt(obj.gameObject.transform.position);
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }
            }
        }

        private void Save()
        {
            string data = JsonUtility.ToJson(new EditorCacheSave { tabs = tabs });
            EditorPrefs.SetString(Application.dataPath + StrSaveName, data);
        }

        private void Read()
        {
            var data = EditorPrefs.GetString(Application.dataPath + StrSaveName);
            EditorCacheSave editorCacheSave;
            if (string.IsNullOrEmpty(data))
            {
                editorCacheSave = new EditorCacheSave
                    { tabs = new List<CacheTabInfo> { new CacheTabInfo() { id = 0, tabName = "Common" } } };
            }
            else
            {
                editorCacheSave = JsonUtility.FromJson<EditorCacheSave>(data);
            }

            tabs.Clear();
            tabs.AddRange(editorCacheSave.tabs);
            foreach (var tabInfo in tabs)
            {
                foreach (var item in tabInfo.list)
                {
                    if (string.IsNullOrEmpty(item.displayName))
                    {
                        item.SetDisplayName();
                    }
                }
            }

            InitReOrderTabList();
        }

        [Serializable]
        public class EditorCacheSave
        {
            public List<CacheTabInfo> tabs = new List<CacheTabInfo>();
        }

        public enum CacheObjectLocation
        {
            Assets,
            Scene,
            Prefab,
        }

        [Serializable]
        public class CacheTabInfo
        {
            public int id;
            public string tabName;
            public List<CacheObjectInfo> list = new List<CacheObjectInfo>();
            public CacheObjectInfo selected;
            public Editor editor;
            public float iconSize = 20;
            public bool gridView;
        }
    }

    public class EditorCacheStyle
    {
        public readonly GUILayoutOption expandWidthFalse = GUILayout.ExpandWidth(false);
        public readonly GUILayoutOption expandWidth200 = GUILayout.Width(100);
        public readonly GUIStyle previewTexture;
        public readonly GUIStyle buttonStyle;
        public readonly GUIStyle textEmpty;
        public readonly GUIStyle toolBar;

        public readonly GUIStyle buttonLeftSelected;
        public readonly GUIStyle buttonMidSelected;
        public readonly GUIStyle buttonRightSelected;

        public readonly GUIStyle toolbarSeachTextField;
        public readonly GUIStyle RectangleToolSelection;
        public readonly GUIStyle ProjectBrowserIconAreaBg;
        public readonly GUIStyle SelectionRect;
        public readonly GUIStyle parent;
        public readonly GUIStyle text1;

        public EditorCacheStyle()
        {
            toolbarSeachTextField = GUI.skin.GetStyle(GUI.skin.textField.name);
            ProjectBrowserIconAreaBg = GUI.skin.GetStyle("ProjectBrowserIconAreaBg");
            RectangleToolSelection = GUI.skin.GetStyle("RectangleToolSelection");
            SelectionRect = GUI.skin.GetStyle("SelectionRect");
            previewTexture = new GUIStyle(GUI.skin.box)
                { fixedWidth = 20, fixedHeight = 20, alignment = TextAnchor.MiddleCenter };
            buttonLeftSelected = new GUIStyle(EditorStyles.miniButtonLeft);
            buttonMidSelected = new GUIStyle(EditorStyles.miniButtonMid);
            buttonRightSelected = new GUIStyle(EditorStyles.miniButtonRight);
            buttonLeftSelected.normal.textColor = buttonMidSelected.normal.textColor =
                buttonRightSelected.normal.textColor = Color.yellow;
            buttonLeftSelected.onHover.textColor = buttonMidSelected.onHover.textColor =
                buttonRightSelected.onHover.textColor = Color.yellow;

            buttonLeftSelected.focused.textColor = buttonMidSelected.focused.textColor =
                buttonRightSelected.focused.textColor = Color.yellow;

            buttonLeftSelected.fontStyle = buttonMidSelected.fontStyle =
                buttonRightSelected.fontStyle = FontStyle.Bold;

            ColorUtility.TryParseHtmlString("#363636", out var bgColor);
            buttonStyle = new GUIStyle("Button");
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            textEmpty = new GUIStyle(EditorStyles.label);
            textEmpty.alignment = TextAnchor.MiddleCenter;
            toolBar = new GUIStyle(EditorStyles.toolbar);
            toolBar.fixedHeight = 60;

            parent = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    textColor = new Color(.6f, .6f, .6f, .8f)
                },
                fontSize = 10,
                padding = new RectOffset(5, 5, 5, 5)
            };
            text1 = new GUIStyle()
            {
                normal =
                {
                    textColor = new Color(.6f, .6f, .6f, .8f)
                },
                fontSize = 10,
                padding = new RectOffset(5, 5, 5, 5)
            };
        }
    }

    public static class EditorCacheHelper
    {
        public static void ShowFolderContents(int folderInstanceID)
        {
            Assembly editorAssembly = typeof(Editor).Assembly;
            Type projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
            MethodInfo showFolderContents = projectBrowserType.GetMethod(
                "ShowFolderContents", BindingFlags.Instance | BindingFlags.NonPublic);
            Object[] projectBrowserInstances = Resources.FindObjectsOfTypeAll(projectBrowserType);
            if (projectBrowserInstances.Length > 0)
            {
                for (int i = 0; i < projectBrowserInstances.Length; i++)
                    ShowFolderContentsInternal(projectBrowserInstances[i], showFolderContents, folderInstanceID);
            }
            else
            {
                EditorWindow projectBrowser = OpenNewProjectBrowser(projectBrowserType);
                ShowFolderContentsInternal(projectBrowser, showFolderContents, folderInstanceID);
            }
        }

        private static void ShowFolderContentsInternal(Object projectBrowser, MethodInfo showFolderContents,
            int folderInstanceID)
        {
            SerializedObject serializedObject = new SerializedObject(projectBrowser);
            bool inTwoColumnMode = serializedObject.FindProperty("m_ViewMode").enumValueIndex == 1;

            if (!inTwoColumnMode)
            {
                MethodInfo setTwoColumns = projectBrowser.GetType().GetMethod(
                    "SetTwoColumns", BindingFlags.Instance | BindingFlags.NonPublic);
                setTwoColumns.Invoke(projectBrowser, null);
            }

            bool revealAndFrameInFolderTree = true;
            showFolderContents.Invoke(projectBrowser, new object[] { folderInstanceID, revealAndFrameInFolderTree });
        }

        private static EditorWindow OpenNewProjectBrowser(Type projectBrowserType)
        {
            EditorWindow projectBrowser = EditorWindow.GetWindow(projectBrowserType);
            projectBrowser.Show();
            MethodInfo init = projectBrowserType.GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
            init.Invoke(projectBrowser, null);
            return projectBrowser;
        }
    }
}