/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.InspectorTools
{
    public class InspectorBar: InspectorInjector
    {
        private const string ElementName = "InspectorBar";
        private const int LastLineOffset = 70;
        private const int LineHeight = 20;

        private static InspectorBar instance;

        public static Vector2 lastPosition = Vector2.zero;
        public static Func<Component, Component[]> OnInitRelatedComponents;
        public static Action<EditorWindow, List<Component>> OnDrawRelatedComponents;
        
        private static GUIContent _collapseContent;
        private static GUIContent _expandContent;
        private static GUIStyle _selectedContentStyle;
        private static Dictionary<int, ContentWrapper> contentCache;
        private static Dictionary<EditorWindow, VisualElementWrapper> visualElements;
        private static GUIContent openPrefabContent;

        //private static List<Component> relatedComponents;

        private static GUIContent collapseContent
        {
            get
            {
                if (_collapseContent == null) _collapseContent = new GUIContent(Icons.collapse, "Collapse all components");
                return _collapseContent;
            }
        }

        private static GUIContent expandContent
        {
            get
            {
                if (_expandContent == null) _expandContent = new GUIContent(Icons.expand, "Expand all components");
                return _expandContent;
            }
        }

        private static GUIStyle selectedContentStyle
        {
            get
            {
                if (_selectedContentStyle == null || _selectedContentStyle.normal.background == null)
                {
                    GUIStyle s = EditorStyles.toolbarButton;
                    _selectedContentStyle = new GUIStyle
                    {
                        normal =
                        {
                            background = Resources.CreateSinglePixelTexture(1, 0.1f),
                            textColor = s.normal.textColor
                        },
                        margin = s.margin,
                        padding = s.padding,
                        fixedHeight = s.fixedHeight,
                        alignment = s.alignment
                    };
                }

                return _selectedContentStyle;
            }
        }

        InspectorBar()
        {
            EditorApplication.delayCall += InitInspector;
            WindowManager.OnWindowFocused += OnWindowFocused;
            WindowManager.OnMaximizedChanged += OnMaximizedChanged;
            Selection.selectionChanged += OnSelectionChanged;
            contentCache = new Dictionary<int, ContentWrapper>();
            //relatedComponents = new List<Component>();
            visualElements = new Dictionary<EditorWindow, VisualElementWrapper>();
        }

        private static void DrawBar(EditorWindow wnd, VisualElement editorsList)
        {
            if (editorsList == null) return;
            if (editorsList.childCount < 1) return;
            VisualElement elements = editorsList[0];

            Editor[] editors = EditorElementRef.GetEditors(elements);
            if (editors == null || editors.Length < 2) return;

            lastPosition = Vector2.zero;
            //relatedComponents.Clear();

            DrawExpand(wnd, editors);
            DrawOpenPrefab(wnd, editors);

            int editorIndex = 0;

            for (int i = 0; i < editorsList.childCount; i++)
            {
                DrawIcon(wnd, editorsList, i, ref editorIndex, editors);
            }

            //if (OnDrawRelatedComponents != null && relatedComponents.Count > 0) OnDrawRelatedComponents(wnd, relatedComponents);

            GUIContent addContentContent = TempContent.Get("+", "Add Component");

            GUIStyle style = EditorStyles.toolbarButton;
            Rect rect = GetRect(style.CalcSize(addContentContent).x, wnd.position.width);

            if (GUI.Button(rect, addContentContent, style))
            {
                Vector2 s = Prefs.defaultWindowSize;
                Rect wp = wnd.position;
                Vector2 p = GUIUtility.GUIToScreenPoint(rect.position);
                p.x = wp.x + (wp.width - s.x) / 2;
                p.y += 45;
                rect = new Rect(p, s);

                AddComponent.ShowAddComponent(rect);
            }

            DrawLateButtons(wnd, style);

            int rowCount = Mathf.RoundToInt(lastPosition.y / LineHeight + 1);
            VisualElementWrapper visualElement = visualElements[wnd];
            if (rowCount != visualElement.rowCount)
            {
                visualElement.style.height = rowCount * LineHeight;
                visualElement.rowCount = rowCount;
            }
        }

        private static void DrawExpand(EditorWindow wnd, Editor[] editors)
        {
            ActiveEditorTracker tracker = InspectorWindowRef.GetTracker(wnd);
            bool isExpanded = false;
            for (int i = 1; i < editors.Length; i++)
            {
                Editor editor = editors[i];
                if (editor is MaterialEditor)
                {
                    if (InternalEditorUtility.GetIsInspectorExpanded(editor.target))
                    {
                        isExpanded = true;
                        break;
                    }
                }
                else if (tracker.GetVisible(i) == 1)
                {
                    isExpanded = true;
                    break;
                }
            }

            GUIContent content = isExpanded ? collapseContent : expandContent;

            Rect rect = GetRect(25, wnd.position.width);

            if (SafeButton(rect, content, EditorStyles.toolbarButton))
            {
                int v = isExpanded ? 0 : 1;
                for (int i = 1; i < editors.Length; i++)
                {
                    Editor editor = editors[i];
                    if (editor is MaterialEditor)
                    {
                        InternalEditorUtility.SetIsInspectorExpanded(editor.target, !isExpanded);
                    }
                    else tracker.SetVisible(i, v);
                }
            }
        }

        private static void DrawIcon(EditorWindow wnd, VisualElement editorsList, int elementIndex, ref int editorIndex, Editor[] editors)
        {
            VisualElement el = editorsList[elementIndex];
            if (el.GetType().Name != "EditorElement") return;
            if (el.childCount < 2)
            {
                editorIndex++;
                return;
            }
            
            if (elementIndex >= editors.Length) return;

            Editor editor = editors[editorIndex];
            if (editor == null || editor.target == null) return;
            if (!Prefs.inspectorBarShowMaterials && editor.target is Material) return;

            int id = editor.target.GetInstanceID();
            ContentWrapper wrapper;

            GUIStyle normalStyle = EditorStyles.toolbarButton;

            if (!contentCache.TryGetValue(id, out wrapper)) wrapper = InitContent(editor, normalStyle);
            //if (wrapper.relatedComponents != null) relatedComponents.AddRange(wrapper.relatedComponents);

            StyleEnum<DisplayStyle> display = el.style.display;
            bool isActive = display.keyword == StyleKeyword.Null || display == DisplayStyle.Flex;
            GUIStyle style = isActive ? normalStyle : selectedContentStyle;

            float maxWidth = wnd.position.width;
            Rect rect = GetRect(wrapper.width, maxWidth - (elementIndex == editorsList.childCount - 2 ? LastLineOffset : 0));

            Component component = editor.target as Component;
            bool isDisabled = component != null && !ComponentUtils.GetEnabled(component);
            
            bool state = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;

            GUIContent content = TempContent.Get(wrapper.content);
            if (isDisabled) content.tooltip += " (Disabled)";
            
            GUILayoutUtils.BeginDisabledStyle(isDisabled);
            EventType eventType = Event.current.type;
            GUILayoutUtils.Button(rect, content, style);
            GUILayoutUtils.EndDisabledStyle();

            Debug.unityLogger.logEnabled = state;
            ProcessIconEvents(wnd, editorsList, elementIndex, editorIndex, isActive, editor, eventType, rect);

            editorIndex++;
        }

        private static void DrawLateButtons(EditorWindow wnd, GUIStyle style)
        {
            Rect rect;

            GUIContent helpContent = TempContent.Get("?", "Help");
            float helpContentWidth = style.CalcSize(helpContent).x;

            if (Updater.hasNewVersion)
            {
                GUIContent updateContent = TempContent.Get(Icons.updateAvailable, "Update Available.\nClick to open the built-in update system.");
                float updateContentWidth = style.CalcSize(updateContent).x;
                rect = new Rect(wnd.position.width - updateContentWidth - helpContentWidth, lastPosition.y, updateContentWidth, LineHeight);

                if (GUI.Button(rect, updateContent, style)) Updater.OpenWindow();
            }

            rect = new Rect(wnd.position.width - helpContentWidth, lastPosition.y, helpContentWidth, LineHeight);

            if (GUI.Button(rect, helpContent, style)) Links.OpenDocumentation("inspector-bar");
        }

        private static void DrawOpenPrefab(EditorWindow wnd, Editor[] editors)
        {
            if (EditorApplication.isPlaying) return;

            Object target = editors[0].target;
            if (target == null) return;

            string assetPath;
            if (target.GetType() == PrefabImporterRef.type)
            {
                assetPath = AssetDatabase.GetAssetPath(target);
                target = null;
            }
            else if (PrefabUtility.IsPartOfAnyPrefab(target))
            {
                assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target);
            }
            else return;

            Rect rect = GetRect(25, wnd.position.width);

            if (openPrefabContent == null)
            {
                openPrefabContent = new GUIContent(EditorIconContents.folderOpened.image, "Open Prefab");
            }

            if (SafeButton(rect, openPrefabContent, EditorStyles.toolbarButton))
            {
                if (!string.IsNullOrEmpty(assetPath))
                {
                    PrefabStageUtilityRef.OpenPrefab(assetPath, target as GameObject);
                }
            }
        }

        public static Rect GetRect(float width, float maxWidth)
        {
            Rect rect = new Rect(lastPosition, new Vector2(width, LineHeight));
            if (rect.xMax >= maxWidth)
            {
                rect.x = 0;
                rect.y += LineHeight;
                lastPosition.x = width;
                lastPosition.y = rect.y;
            }
            else lastPosition.x += width;

            return rect;
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Updater.CheckNewVersionAvailable();
            instance = new InspectorBar();
        }

        private static ContentWrapper InitContent(Editor editor, GUIStyle normalStyle)
        {
            Texture thumbnail = AssetPreview.GetMiniThumbnail(editor.target);
            string tooltip = ObjectNames.NicifyVariableName(editor.target.GetType().Name);

            ContentWrapper wrapper = new ContentWrapper
            {
                content = new GUIContent(thumbnail, tooltip),
            };

            bool useIcon = true;
            if (thumbnail.name == "cs Script Icon" || thumbnail.name == "d_cs Script Icon")
            {
                GameObjectUtils.GetPsIconContent(wrapper.content);
                useIcon = false;
            }

            Vector2 s = new Vector2();

            if (!useIcon)
            {
                s = normalStyle.CalcSize(wrapper.content);
                if (s.x < 25) s.x = 25;
            }
            else s.x = 25;

            wrapper.width = s.x;
            contentCache.Add(editor.target.GetInstanceID(), wrapper);
            return wrapper;
        }

        private static bool IsTransform(VisualElement el)
        {
            return el.name == "Transform" || el.name == "Rect Transform";
        }

        protected override bool OnInject(EditorWindow wnd, VisualElement mainContainer, VisualElement editorsList)
        {
            if (mainContainer[0].name == ElementName) mainContainer.RemoveAt(0);

            if (!Prefs.inspectorBar) return true;

            if (editorsList.childCount < 2) return false; 
            VisualElement elements = editorsList[0];

            Editor[] editors = EditorElementRef.GetEditors(elements);
            if (editors == null || editors.Length < 2) return false;
            Object target = editors[0].target;

            if (!(target is GameObject) && target.GetType() != PrefabImporterRef.type) return false;

            VisualElement visualElement = new IMGUIContainer(() => DrawBar(wnd, editorsList));
            visualElement.name = ElementName;
            visualElement.style.height = LineHeight;
            visualElement.style.position = Position.Relative;
            mainContainer.Insert(0, visualElement);

            visualElements[wnd] = new VisualElementWrapper(visualElement, 1);
            return true;
        }

        private void OnSelectionChanged()
        {
            contentCache.Clear();
            InitInspector();
        }

        private void OnWindowFocused(EditorWindow wnd)
        {
            if (wnd == null) return;
            if (wnd.GetType() != InspectorWindowRef.type) return;
            InjectBar(wnd);
        }

        private static void ProcessIconClick(EditorWindow wnd, VisualElement editorsList, int elementIndex, int editorIndex, bool isActive, Editor editor)
        {
            Event e = Event.current;
            if (e.button == 0) ProcessIconLeftClick(wnd, editorsList, elementIndex, editorIndex, isActive);
            else if (e.button == 1) ComponentUtils.ShowContextMenu(editor.target);
            else if (e.button == 2)
            {
                Component component = editor.target as Component;
                if (component != null && ComponentUtils.CanBeDisabled(component))
                {
                    Undo.RecordObject(component, "Modified Property in " + component.gameObject.name);
                    ComponentUtils.SetEnabled(component, !ComponentUtils.GetEnabled(component));
                    EditorUtility.SetDirty(component);
                }
                        
            }

            e.Use();
        }

        private static void ProcessIconEvents(EditorWindow wnd, VisualElement editorsList, int elementIndex, int editorIndex, bool isActive, Editor editor, EventType eventType, Rect rect)
        {
            Event e = Event.current;
            if (!rect.Contains(e.mousePosition)) return;
            if (eventType == EventType.MouseUp)
            {
                ProcessIconClick(wnd, editorsList, elementIndex, editorIndex, isActive, editor);
            }
            else if (eventType == EventType.MouseDrag)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = editor.targets;
                DragAndDrop.StartDrag("Drag " + editor.target.name);
                e.Use();
            }
        }

        private static void ProcessIconLeftClick(EditorWindow wnd, VisualElement editorsList, int elementIndex, int editorIndex, bool isActive)
        {
            Event e = Event.current;

            if (e.command || e.control || e.shift)
            {
                ToggleVisible(wnd, editorsList, elementIndex, editorIndex, !isActive);
                return;
            }

            if (!isActive) SetSoloVisible(wnd, editorsList, elementIndex, editorIndex, false);
            else
            {
                int countActive = 0;
                for (int j = 0; j < editorsList.childCount; j++)
                {
                    VisualElement el2 = editorsList[j];
                    if (el2.childCount < 2) continue;
                    StyleEnum<DisplayStyle> display = el2.style.display;
                    if (display.keyword == StyleKeyword.Null || display == DisplayStyle.Flex) countActive++;
                }

                SetSoloVisible(wnd, editorsList, elementIndex, editorIndex, countActive == 1);
            }
        }

        public static bool SafeButton(Rect rect, GUIContent content, GUIStyle style)
        {
            bool state = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            bool ret = GUI.Button(rect, content, style);
            Debug.unityLogger.logEnabled = state;
            return ret;
        }

        private static void SetSoloVisible(EditorWindow wnd, VisualElement element, int index, int editorIndex, bool show)
        {
            if (show)
            {
                for (int i = 0; i < element.childCount; i++)
                {
                    VisualElement el = element[i];
                    el.style.display = DisplayStyle.Flex;
                    if (IsTransform(el)) el.style.marginTop = 0;
                }
            }
            else
            {
                for (int i = 0; i < element.childCount; i++)
                {
                    VisualElement el = element[i];
                    if (i == index)
                    {
                        el.style.display = DisplayStyle.Flex;
                        if (IsTransform(el)) el.style.marginTop = 7;
                        object inspectorElement = EditorElementRef.GetInspectorElement(el);
                        EditorElementRef.SetElementVisible(inspectorElement, false);

                    }
                    else el.style.display = DisplayStyle.None;
                }

                ActiveEditorTracker tracker = InspectorWindowRef.GetTracker(wnd);
                tracker.SetVisible(editorIndex, 1);
            }
        }

        private static void ToggleVisible(EditorWindow wnd, VisualElement element, int index, int editorIndex, bool show)
        {
            VisualElement el = element[index];
            if (show)
            {
                el.style.display = DisplayStyle.Flex;
                if (IsTransform(el)) el.style.marginTop = 0;
            }
            else
            {
                el.style.display = DisplayStyle.None;
                for (int i = 0; i < element.childCount; i++)
                {
                    el = element[i];
                    if (el.childCount < 2) continue;

                    if (IsTransform(el))
                    {
                        el.style.marginTop = 7;
                        break;
                    }
                    if (el.style.display == DisplayStyle.Flex) break;
                }
                ActiveEditorTracker tracker = InspectorWindowRef.GetTracker(wnd);
                tracker.SetVisible(editorIndex, 1);
            }
        }

        internal class ContentWrapper
        {
            public GUIContent content;
            //public Component[] relatedComponents;
            public float width;
        }

        internal class VisualElementWrapper
        {
            public VisualElement visualElement;
            public int rowCount;

            public IStyle style
            {
                get { return visualElement.style; }
            }

            public VisualElementWrapper(VisualElement visualElement, int rowCount)
            {
                this.visualElement = visualElement;
                this.rowCount = rowCount;
            }
        }
    }
}