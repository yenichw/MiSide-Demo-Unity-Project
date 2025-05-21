/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.JSON;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfinityCode.UltimateEditorEnhancer.InspectorTools
{
    public class EmptyInspector: InspectorInjector
    {
        private const string ElementName = "EmptyInspector";
        private const string SearchFieldName = "UEEEmptyInspectorSearchField";

        private static EmptyInspector instance;
        private static VisualElement visualElement;
        private static string filterText;

        public EmptyInspector()
        {
            EditorApplication.delayCall += InitInspector;
            WindowManager.OnMaximizedChanged += OnMaximizedChanged;
            Selection.selectionChanged += InitInspector;
        }

        private static void CreateButton(VisualElement parent, string submenu, string text)
        {
            ToolbarButton button = new ToolbarButton(() => EditorApplication.ExecuteMenuItem(submenu));
            button.text = text;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.left = 0;
            button.style.borderLeftWidth = button.style.borderRightWidth = 0;
            parent.Add(button);
        }

        private VisualElement CreateContainer(VisualElement parent)
        {
            VisualElement el = new VisualElement();
            el.style.borderBottomWidth = el.style.borderTopWidth = el.style.borderLeftWidth = el.style.borderRightWidth = 1;
            el.style.borderBottomColor = el.style.borderTopColor = el.style.borderLeftColor = el.style.borderRightColor = 
                Styles.isProSkin? new Color(.33f, .33f, .33f) : new Color(.66f, .66f, .66f); 
            el.style.marginLeft = 3;
            el.style.marginRight = 5;
            parent.Add(el);
            return el;
        }

        private static void CreateLabel(VisualElement parent, string text)
        {
            Label label = new Label(text);
            label.style.marginTop = 10;
            label.style.marginLeft = label.style.marginRight = 3;
            label.style.paddingLeft = 5;
            parent.Add(label);
        }

        private void DrawFilterTextField()
        {
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName(SearchFieldName);
            EditorGUI.BeginChangeCheck();
            filterText = GUILayoutUtils.ToolbarSearchField(filterText);
            if (EditorGUI.EndChangeCheck()) UpdateFilteredItems();

            if (GUILayout.Button(TempContent.Get(EditorIconContents.settings.image, "Settings"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                Settings.OpenEmptyInspectorSettings();
            }

            if (GUILayout.Button(TempContent.Get("?", "Help"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                Links.OpenDocumentation("empty-inspector");
            }

            GUILayout.EndHorizontal();
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            instance = new EmptyInspector();
        }

        private void InitItems(VisualElement parent)
        {
            InitTopContent(parent);

            List<Group> groups = ReferenceManager.emptyInspectorItems;

            for (int i = 0; i < groups.Count; i++)
            {
                Group g = groups[i];
                if (!g.enabled || g.count == 0) continue;

                CreateLabel(parent, g.title);
                VisualElement container = CreateContainer(parent);
                for (int j = 0; j < g.items.Count; j++)
                {
                    Item item = g.items[j];
                    if (!item.enabled) continue;

                    CreateButton(container, item.menuPath, item.title);
                }
            }
        }

        private void InitTopContent(VisualElement parent)
        {
            VisualElement topContent = new VisualElement();
            parent.Add(topContent);

            Updater.CheckNewVersionAvailable();

            if (Updater.hasNewVersion && Prefs.emptyInspectorShowUpdates) InitUpdateAvailableContent(topContent);

            Label helpbox = new Label("Nothing selected")
            {
                style =
                {
                    backgroundColor = Color.gray,
                    height = 30,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            topContent.Add(helpbox);

            IMGUIContainer search = new IMGUIContainer(DrawFilterTextField)
            {
                style =
                {
                    marginTop = 5,
                    marginLeft = 5,
                    marginRight = 5
                }
            };
            
            parent.Add(search);
        }

        private static void InitUpdateAvailableContent(VisualElement topContent)
        {
            ToolbarButton updateAvailable = new ToolbarButton(Updater.OpenWindow)
            {
                style =
                {
                    backgroundColor = (Color)new Color32(173, 216, 230, 128),
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    left = 0,
                    flexDirection = FlexDirection.Row
                }
            };

            const string text = "A new version of Ultimate Editor Enhancer is available.\nClick to open the built-in update system.";
            Label textContainer = new Label
            {
                text = text.Replace('\n', ' '),
                tooltip = text,
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    color = Color.white,
                    left = 0,
                    paddingTop = updateAvailable.style.paddingBottom = 5,
                    paddingLeft = 10,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    whiteSpace = WhiteSpace.Normal,
                    flexShrink = 1
                }
            };

            VisualElement imageContainer = new VisualElement
            {
                style =
                {
                    width = 20,
                    height = 20,
                    marginLeft = 5,
                    marginRight = 5,
                    backgroundImage = Icons.updateAvailable2 as Texture2D,
                    alignSelf = Align.Center
                }
            };

            updateAvailable.Add(imageContainer);
            updateAvailable.Add(textContainer);

            topContent.Add(updateAvailable);
        }

        protected override bool OnInject(EditorWindow wnd, VisualElement mainContainer, VisualElement editorsList)
        {
            if (editorsList.parent[0].name == ElementName) editorsList.parent.RemoveAt(0);
            if (!Prefs.emptyInspector || ReferenceManager.emptyInspectorItems.Count == 0) return false;
            if (editorsList.childCount != 0 || float.IsNaN(editorsList.layout.width)) return false;

            if (visualElement == null)
            {
                visualElement = new VisualElement();
                visualElement.name = ElementName;
                InitItems(visualElement);
            }
            editorsList.parent.Insert(0, visualElement);
            filterText = "";
            UpdateFilteredItems();

            return true;
        }

        public static void ResetCachedItems()
        {
            if (visualElement != null)
            {
                if (instance != null)
                {
                    visualElement.Clear();
                    instance.InitItems(visualElement);
                }
                else visualElement = null;
            }
        }

        private static void UpdateElementVisibility(int i, string pattern)
        {
            VisualElement el = visualElement[i];

            bool hasVisible = false;
            VisualElement container = el;
            for (int j = 0; j < container.childCount; j++)
            {
                ToolbarButton b = container[j] as ToolbarButton;
                bool visible = SearchableItem.Match(pattern, b.text);
                if (visible)
                {
                    b.style.display = DisplayStyle.Flex;
                    hasVisible = true;
                }
                else b.style.display = DisplayStyle.None;
            }

            if (hasVisible)
            {
                visualElement[i - 1].style.display = DisplayStyle.Flex;
                el.style.display = DisplayStyle.Flex;
            }

            else
            {
                visualElement[i - 1].style.display = DisplayStyle.None;
                el.style.display = DisplayStyle.None;
            }
        }

        private void UpdateFilteredItems()
        {
            string t = filterText.Trim();
            if (string.IsNullOrEmpty(t))
            {
                for (int i = 2; i < visualElement.childCount; i += 2)
                {
                    visualElement[i].style.display = DisplayStyle.Flex;
                    VisualElement container = visualElement[i + 1];
                    container.style.display = DisplayStyle.Flex;
                    for (int j = 0; j < container.childCount; j++)
                    {
                        container[j].style.display = DisplayStyle.Flex;
                    }

                }
                return;
            }

            string pattern = SearchableItem.GetPattern(t);

            for (int i = 3; i < visualElement.childCount; i += 2)
            {
                UpdateElementVisibility(i, pattern);
            }
        }

        [Serializable]
        public class Group
        {
            public string title;
            public bool enabled = true;
            public List<Item> items;

            public int count
            {
                get => items.Count;
            }

            public JsonObject json
            {
                get
                {
                    return Json.Serialize(this) as JsonObject;
                }
            }

            public Group()
            {

            }

            public Group(string title)
            {
                this.title = title;
                items = new List<Item>();
            }

            public Group(string title, List<Item> items)
            {
                this.title = title;
                this.items = items;
            }

            public void Add(Item item)
            {
                items.Add(item);
            }
        }

        [Serializable]
        public class Item
        {
            public string title;
            public string menuPath;
            public bool enabled = true;
        }
    }
}