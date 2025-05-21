/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [Serializable]
    public class ComponentWindow : AutoSizePopupWindow
    {
        #region Fields

        #region Static

        private static ComponentWindow autoPopupWindow;
        private static GUIContent bookmarkContent;
        private static GUIContent debugContent;
        private static GUIContent debugOnContent;
        private static GUIStyle inspectorBigStyle;
        private static GUIContent removeBookmarkContent;
        private static GUIContent selectContent;
        private static GUIContent titleSettingsIcon;

        #endregion


        public bool allowInitEditor = true;

        [SerializeField]
        public bool displayGameObject = true;
        
        [SerializeField]
        public bool isPopup;

        [SerializeField]
        private Component _component;

        [SerializeField]
        private string componentID;

        private bool destroyAnyway;

        [NonSerialized]
        private Editor editor;

        [NonSerialized]
        private bool isDebug;

        [NonSerialized]
        private bool isMissed = false;
        
        [NonSerialized]
        private SerializedObject serializedObject;

        [NonSerialized]
        private List<SearchableProperty> searchableProperties;

        [NonSerialized]
        private string filter;
        
        [NonSerialized]
        private List<SearchableProperty> filteredItems;

        #endregion

        public Component component
        {
            get { return _component; }
            set
            {
                _component = value;
                if (_component != null)
                {
                    componentID = GlobalObjectIdHelper.GetGlobalObjectIdSlow(value).ToString();
                    if (editor != null) DestroyImmediate(editor);
                    InitEditor();


                }
            }
        }

        private void CacheSerializedObject()
        {
            if (component == null) return;
            serializedObject = new SerializedObject(component);
            serializedObject.Update();
            searchableProperties = new List<SearchableProperty>();

            SerializedProperty p = serializedObject.GetIterator();
            if (!p.Next(true)) return;

            do
            {
                searchableProperties.Add(new SearchableProperty(p));
            }
            while (p.NextVisible(true));

            if (!string.IsNullOrEmpty(filter)) UpdateFilteredItems();
        }

        private void DrawHeader()
        {
            if (_component == null) return;

            if (inspectorBigStyle == null)
            {
                inspectorBigStyle = new GUIStyle(Reflection.GetStaticPropertyValue<GUIStyle>(typeof(EditorStyles), "inspectorBig"));
                inspectorBigStyle.margin = new RectOffset(1, 1, 0, 0);
            }

            titleSettingsIcon = EditorIconContents.popup;

            GUILayout.BeginHorizontal(inspectorBigStyle);
            GUILayout.Space(38f);
            GUILayout.BeginVertical();
            GUILayout.Space(19f);
            GUILayout.BeginHorizontal();
            EditorGUILayout.GetControlRect();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect r = new Rect(lastRect.x, lastRect.y, lastRect.width, lastRect.height);
            
            DrawHeaderPreview(r);
            DrawHeaderComponent(r);
            DrawHeaderIcons(r);
            DrawHeaderGameObject(r);
            DrawHeaderExtraButtons(r);
        }

        private void DrawHeaderComponent(Rect r)
        {
            Event e = Event.current;

            int verticalOffset = 4;
            if (!displayGameObject) verticalOffset += 8;

            if (ComponentUtils.CanBeDisabled(_component))
            {
                Rect tr1 = new Rect(r.x + 44, r.y + verticalOffset + 1, 16, 18);
                EditorGUI.BeginChangeCheck();
                bool v1 = GUI.Toggle(tr1, ComponentUtils.GetEnabled(_component), GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_component, "Modified Property in " + _component.gameObject.name);
                    ComponentUtils.SetEnabled(_component, v1);
                    EditorUtility.SetDirty(_component);
                }
            }

            Rect r2 = new Rect(r.x + 60, r.y + verticalOffset, r.width - 100, 18);

            GUI.Label(r2, _component.GetType().Name, EditorStyles.largeLabel);
            if (e.type == EventType.MouseDown && r2.Contains(e.mousePosition))
            {
                if (e.button == 1)
                {
                    ComponentUtils.ShowContextMenu(_component);
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && r2.Contains(e.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] {_component};

                DragAndDrop.StartDrag("Drag " + _component.name);
                e.Use();
            }
        }

        private void DrawHeaderExtraButtons(Rect r)
        {
            Event e = Event.current;
            r.y += 27;
            int offset = 18;

            if (!Prefs.headerBookmarks)
            {
                bool containBookmark = Bookmarks.Contain(component);
                if (GUI.Button(new Rect(r.xMax - 18, r.y, 16, 16), containBookmark ? removeBookmarkContent : bookmarkContent, Styles.transparentButton))
                {
                    if (e.modifiers == EventModifiers.Control)
                    {
                        Bookmarks.ShowWindow();
                    }
                    else
                    {
                        if (containBookmark) Bookmarks.Remove(component);
                        else Bookmarks.Add(component);
                        Bookmarks.Redraw();
                    }
                }
                
                offset += 18;
            }

            if (debugContent == null) debugContent = new GUIContent(Icons.debug, "Debug");
            if (debugOnContent == null) debugOnContent = new GUIContent(Icons.debugOn, "Debug");

            if (GUI.Button(new Rect(r.width - offset, r.y, 16, 16), isDebug ? debugOnContent : debugContent, Styles.transparentButton))
            {
                ToggleDebugMode();
            }
        }

        private void DrawHeaderGameObject(Rect r)
        {
            if (!displayGameObject) return;

            Event e = Event.current;
            GameObject gameObject = _component.gameObject;

            Rect tr2 = new Rect(r.x + 44, r.y + 25, 16, 18);
            EditorGUI.BeginChangeCheck();
            bool v2 = GUI.Toggle(tr2, gameObject.activeSelf, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_component, "Modified Property in " + gameObject.name);
                gameObject.SetActive(v2);
                EditorUtility.SetDirty(gameObject);
            }

            r = new Rect(r.x + 60, r.y + 25, r.width - 100, 18);
            GUI.Label(r, gameObject.name, EditorStyles.boldLabel);
            if (e.type == EventType.MouseDown && r.Contains(e.mousePosition))
            {
                if (e.button == 0)
                {
                    Selection.activeGameObject = gameObject;
                    EditorGUIUtility.PingObject(Selection.activeGameObject);
                    e.Use();
                }
                else if (e.button == 1)
                {
                    GameObjectUtils.ShowContextMenu(false, gameObject);
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && r.Contains(e.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] { gameObject };

                DragAndDrop.StartDrag("Drag " + gameObject.name);
                e.Use();
            }
        }

        private void DrawHeaderIcons(Rect r)
        {
            Vector2 settingsSize = Styles.iconButton.CalcSize(titleSettingsIcon);
            Rect settingsRect = new Rect(r.xMax - settingsSize.x, r.y + 5, settingsSize.x, settingsSize.y);
            if (EditorGUI.DropdownButton(settingsRect, titleSettingsIcon, FocusType.Passive, Styles.iconButton))
            {
                ComponentUtils.ShowContextMenu(_component);
            }

            float offset = settingsSize.x * 2;

            EditorGUIUtilityRef.DrawEditorHeaderItems(new Rect(r.xMax - offset, r.y + 5f, settingsSize.x, settingsSize.y), new Object[] { component }, 0);
        }

        private void DrawHeaderPreview(Rect r)
        {
            Rect r1 = new Rect(r.x + 6, r.y + 6, 32, 32);

            bool isLoadingAssetPreview = AssetPreview.IsLoadingAssetPreview(component.GetInstanceID());
            Texture2D texture2D = AssetPreview.GetAssetPreview(component);
            if (texture2D == null)
            {
                if (isLoadingAssetPreview) Repaint();
                texture2D = AssetPreview.GetMiniThumbnail(component);
            }

            GUI.Label(r1, texture2D, Styles.centeredLabel);
        }

        private void DrawSerializedObject()
        {
            if (serializedObject == null)
            {
                CacheSerializedObject();
                if (serializedObject == null) return;
            }
            serializedObject.Update();

            bool wideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;

            if (filteredItems == null)
            {
                SerializedProperty p = serializedObject.GetIterator();

                if (!p.Next(true)) return;

                do
                {
                    EditorGUILayout.PropertyField(p);
                } while (p.NextVisible(false));
            }
            else
            {
                foreach (SearchableProperty item in filteredItems) EditorGUILayout.PropertyField(serializedObject.FindProperty(item.path));
            }

            EditorGUIUtility.wideMode = wideMode;

            serializedObject.ApplyModifiedProperties();
        }

        public void FreeEditor()
        {
            if (editor == null) return;

            if (destroyAnyway || editor.GetType().ToString() != "UnityEditor.TerrainInspector") DestroyImmediate(editor);
            editor = null;
        }

        private void FreeReferences()
        {
            OnClose = null;

            FreeEditor();
        }

        public void InitEditor()
        {
            FreeEditor();

            if (component is Terrain)
            {
                editor = TerrainInspectorRef.GetActiveTerrainInspectorInstance();
                if (editor == null)
                {
                    destroyAnyway = true;
                    editor = Editor.CreateEditor(component);
                    TerrainInspectorRef.SetActiveTerrainInspectorInstance(editor);
                    TerrainInspectorRef.SetActiveTerrainInspector(editor.GetInstanceID());
                }
            }
            else
            {
                editor = Editor.CreateEditor(component);
            }
        }

        private void OnCompilationStarted(object obj)
        {
            FreeEditor();
        }

        protected override void OnContentGUI()
        {
            DrawHeader();

            if (isDebug)
            {
                EditorGUI.BeginChangeCheck();
                filter = GUILayoutUtils.ToolbarSearchField(filter);
                if (EditorGUI.EndChangeCheck()) UpdateFilteredItems();

                DrawSerializedObject();
            }
            else if (editor != null)
            {
                EditorGUILayout.BeginVertical(contentAreaStyle);
                try
                {
                    editor.OnInspectorGUI();
                }
                catch (ExitGUIException e)
                {
                    throw e;
                }
                catch
                {

                }
                EditorGUILayout.EndVertical();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            searchableProperties = null;
            serializedObject = null;

            FreeReferences();
            _component = null; 

            if (autoPopupWindow == this) autoPopupWindow = null;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnEnable()
        {
            FreeReferences();
            isDebug = false;
            allowInitEditor = true;

            selectContent = EditorIconContents.rectTransformBlueprint;
            selectContent.tooltip = "Select GameObject";

            bookmarkContent = new GUIContent(Styles.isProSkin ? Icons.starWhite: Icons.starBlack, "Bookmark");
            removeBookmarkContent = new GUIContent(Icons.starYellow, "Remove Bookmark");

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
        }

        protected override void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                FreeEditor();
                return;
            }

            if (_component == null)
            {
                if (!isMissed)
                {
                    GlobalObjectId gid;
                    if (!string.IsNullOrEmpty(componentID) && GlobalObjectIdHelper.TryParse(componentID, out gid))
                    {
                        _component = GlobalObjectIdHelper.GlobalObjectIdentifierToObjectSlow(gid) as Component;
                        isMissed = _component == null;
                    }
                    else isMissed = true;
                }

                if (isMissed)
                {
                    if (GUILayout.Button("Try to restore"))
                    {
                        GlobalObjectId gid;
                        if (!string.IsNullOrEmpty(componentID) && GlobalObjectIdHelper.TryParse(componentID, out gid))
                        {
                            _component = GlobalObjectIdHelper.GlobalObjectIdentifierToObjectSlow(gid) as Component;
                            isMissed = _component == null;
                        }
                    }

                    EditorGUILayout.LabelField("Component is missed.");
                    return;
                }
            }

            if (editor == null && allowInitEditor) 
            {
                if (_component is Terrain) TryRestoreTerrainEditor();
                else InitEditor();
            }

            if (editor == null && !isDebug) return;

            if (isPopup)
            {
                GUIStyle style = GUI.skin.box;
                style.normal.textColor = Color.blue;
                GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, style);
            }

            base.OnGUI();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode) allowInitEditor = false;
            else
            {
                allowInitEditor = true;
                Repaint();
            }

            FreeEditor();
        }

        public static ComponentWindow Show(Component component, bool autosize = true)
        {
            if (component == null) return null;

            ComponentWindow wnd = CreateInstance<ComponentWindow>();

            Texture2D texture2D = AssetPreview.GetAssetPreview(component);
            if (texture2D == null) texture2D = AssetPreview.GetMiniThumbnail(component);

            wnd.titleContent = new GUIContent(component.GetType().Name + " (" + component.gameObject.name + ")", texture2D);
            wnd.component = component;
            wnd.minSize = Vector2.one;
            wnd.closeOnLossFocus = false;
            wnd.closeOnCompileOrPlay = false;
            wnd.isDebug = false;
            if (autosize) wnd.adjustHeight = AutoSize.center;
            wnd.Show();
            wnd.Focus();
            if (Event.current != null)
            {
                Vector2 screenPoint = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                Vector2 size = Prefs.defaultWindowSize;
                wnd.position = new Rect(screenPoint - size / 2, size);
            }
            return wnd;
        }

        public static ComponentWindow ShowPopup(Component component, Rect? rect = null, string title = null)
        {
            if (component == null) return null;
            if (autoPopupWindow != null)
            {
                autoPopupWindow.Close();
                autoPopupWindow = null;
            }

            ComponentWindow wnd = CreateInstance<ComponentWindow>();
            wnd.component = component;
            wnd.minSize = Vector2.one;

            Event e = Event.current;

            if (!rect.HasValue)
            {
                Vector2 mousePosition = e != null? e.mousePosition: new Vector2(Screen.width / 2, Screen.height / 2);
                Vector2 position = GUIUtility.GUIToScreenPoint(mousePosition);
                Vector2 size = Prefs.defaultWindowSize;
                rect = new Rect(position - size / 2, size);
            }

            Rect r = rect.Value;
            if (r.y < 30) r.y = 30;
            wnd.position = r
;
            wnd.ShowPopup();
            wnd.Focus();
            wnd.adjustHeight = AutoSize.top;
            wnd.isPopup = true;

            if (title == null) title = component.GetType().Name;
            wnd.titleContent = new GUIContent(title);
            wnd.drawTitle = true;
            wnd.OnPin += () =>
            {
                Rect wRect = wnd.position;
                ComponentWindow w = Show(component, false);
                w.position = wRect;
                w.closeOnLossFocus = false;
                w.closeOnCompileOrPlay = false;
                wnd.Close();
            };

            return wnd;
        }

        public static ComponentWindow ShowUtility(Component component, bool autosize = true)
        {
            if (component == null) return null;

            ComponentWindow wnd = CreateInstance<ComponentWindow>();
            wnd.minSize = Vector2.one;
            wnd.titleContent = new GUIContent(component.GetType().Name + " (" + component.gameObject.name + ")");
            wnd.component = component;
            wnd.closeOnLossFocus = false;
            wnd.closeOnCompileOrPlay = false;
            if (autosize) wnd.adjustHeight = AutoSize.center;
            wnd.ShowUtility();
            wnd.Focus();
            Vector2 size = Prefs.defaultWindowSize;
            if (Event.current != null) wnd.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - size / 2, size);
            return wnd;
        }

        private void ToggleDebugMode()
        {
            isDebug = !isDebug;
            if (isDebug)
            {
                FreeEditor();
                CacheSerializedObject();
            }
            else
            {
                serializedObject = null;

                if (component is Terrain) TryRestoreTerrainEditor();
                else InitEditor();
            }
            allowInitEditor = !isDebug;
        }

        public bool TryRestoreTerrainEditor()
        {
            if (Selection.activeGameObject == _component.gameObject)
            {
                InitEditor();
                if (editor != null) return true;
            }
            
            EditorGUILayout.HelpBox("Select Terrain GameObject", MessageType.Info);
            if (GUILayout.Button("Select"))
            {
                Selection.activeGameObject = _component.gameObject;
            }

            return false;
        }

        private void UpdateFilteredItems()
        {
            if (string.IsNullOrEmpty(filter))
            {
                filteredItems = null;
                return;
            }

            string pattern = SearchableItem.GetPattern(filter);
            filteredItems = searchableProperties.Where(p => p.Match(pattern)).ToList();
        }

        public class SearchableProperty : SearchableItem
        {
            private string displayName;
            public string path;

            public SearchableProperty(SerializedProperty prop)
            {
                path = prop.propertyPath;
                displayName = prop.displayName;
            }

            protected override int GetSearchCount()
            {
                return 1;
            }

            protected override string GetSearchString(int index)
            {
                return displayName;
            }
        }
    }
}