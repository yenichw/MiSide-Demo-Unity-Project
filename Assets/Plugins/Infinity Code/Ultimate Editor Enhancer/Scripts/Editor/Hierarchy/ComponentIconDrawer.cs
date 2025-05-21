/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.Behaviors;
using InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions;
using InfinityCode.UltimateEditorEnhancer.Integration;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class ComponentIconDrawer
    {
        private static GameObject prevTarget;
        private static List<Item> prevItems = new List<Item>();
        private static Dictionary<int, List<Item>> cache;
        private static bool ehVisible = true;
        private static int ehRightMargin;
        private static int activeID;

        private static Type[] defaultTypes = new[]
        {
            typeof(Transform),
            typeof(RectTransform),
            typeof(CanvasRenderer)
        };

        static ComponentIconDrawer()
        {
            HierarchyItemDrawer.Register("ComponentIconDrawer", DrawHierarchyItem, HierarchyToolOrder.ComponentIcon);
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void ClearCache()
        {
            if (cache == null) return;

            foreach (KeyValuePair<int, List<Item>> pair in cache)
            {
                foreach (Item item in pair.Value)
                {
                    item.Dispose();
                }
            }

            cache.Clear();
        }

        private static bool ContainsCinemachineBrain(List<Item> items)
        {
            return items.Any(i => i.content.tooltip == "Cinemachine Brain");
        }

        private static Item CreateItem(Component component, Rect rect)
        {
            if (!component) return null;

            if (Prefs.hierarchyIconsHideDefault)
            {
                if (defaultTypes.Contains(component.GetType())) return null;
            }
                
            Texture2D thumbnail = AssetPreview.GetMiniThumbnail(component);
            GUIContent content = new GUIContent(
                thumbnail,
                ObjectNames.NicifyVariableName(component.GetType().Name)
            );
            if (thumbnail.name == "cs Script Icon" || thumbnail.name == "d_cs Script Icon") GameObjectUtils.GetPsIconContent(content);

            Item item = new Item(content, component);
            item.OnClick += () => ShowComponent(component, rect);
            item.OnDrag += () =>
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new UnityEngine.Object[] { component };
                DragAndDrop.StartDrag("Drag Component");
            };
            item.OnRightClick += () => ComponentUtils.ShowContextMenu(component);
            item.OnMiddleClick += () =>
            {
                if (!ComponentUtils.CanBeDisabled(component)) return ;
                Undo.RecordObject(component.gameObject, "Modified Property in " + component.gameObject.name);
                ComponentUtils.SetEnabled(component, !ComponentUtils.GetEnabled(component));
                EditorUtility.SetDirty(component);
            };

            return item;
        }

        private static void DrawHierarchyItem(HierarchyItem item)
        {
            if (!Prefs.hierarchyIcons) return;
            
            if (EnhancedHierarchyIntegration()) return;

            Rect rect = item.rect;

            if (Prefs.hierarchyIconsDisplayRule != HierarchyIconsDisplayRule.always)
            {
                if (Event.current.type == EventType.Layout)
                {
                    if (!item.hovered)
                    {
                        if (item.id == activeID) activeID = -1;
                        return;
                    }

                    activeID = item.id;
                }
                else if (activeID != item.id) return;

                if (item.gameObject != prevTarget)
                {
                    prevTarget = item.gameObject;
                    UpdateItems(rect, item.gameObject, prevItems);
                }

                if (!prevTarget) return;

                DrawItems(prevItems, rect);
            }
            else
            {
                List<Item> items = GetItemsFromCache(item.gameObject, rect);
                DrawItems(items, rect);
            }
        }

        private static void DrawItems(List<Item> items, Rect rect)
        {
            Rect lastRect = new Rect(rect.xMax, rect.y, 0, rect.height);
            
            if (ContainsCinemachineBrain(items)) lastRect.x -= 20;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                Item iconItem = items[i];
                float lastX = iconItem.Draw(lastRect);
                lastRect.x = lastX;
            }
        }

        private static bool EnhancedHierarchyIntegration()
        {
            if (Prefs.hierarchyIconsDisplayRule == HierarchyIconsDisplayRule.onHoverWithModifiers && Event.current.modifiers != Prefs.hierarchyIconsModifiers)
            {
                if (!ehVisible)
                {
                    EnhancedHierarchy.SetRightMargin(ehRightMargin);
                    ehVisible = true;
                }

                return true;
            }

            if (ehVisible)
            {
                ehRightMargin = EnhancedHierarchy.GetRightMargin();
                EnhancedHierarchy.SetRightMargin(-10000);
                ehVisible = false;
            }

            return false;
        }

        private static List<Item> GetItemsFromCache(GameObject target, Rect rect)
        {
            if (!target) return new List<Item>();

            List<Item> cachedItems;
            if (cache == null) cache = new Dictionary<int, List<Item>>();
            else if (cache.TryGetValue(target.GetInstanceID(), out cachedItems)) return cachedItems;
            cachedItems = new List<Item>();
            UpdateItems(rect, target, cachedItems);
            cache.Add(target.GetInstanceID(), cachedItems);
            return cachedItems;
        }

        private static void OnHierarchyChanged()
        {
            ClearCache();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            if (Prefs.hierarchyIconsDisplayRule != HierarchyIconsDisplayRule.always) return;
            if (mode == PlayModeStateChange.ExitingEditMode || mode == PlayModeStateChange.ExitingPlayMode) return;

            ClearCache();
            prevItems.Clear();
        }

        private static void ShowAddComponent(Rect hierarchyRect)
        {
            Event e = Event.current;
            Vector2 position = e.mousePosition;
            position.y = hierarchyRect.yMax;
            position = WindowsHelper.GetMousePositionOnFocusedWindow(position);

            Vector2 size = Prefs.defaultWindowSize;
            Rect rect = new Rect(position + new Vector2(-size.x / 2, 36), size);

#if !UNITY_EDITOR_OSX
            if (rect.yMax > Screen.currentResolution.height - 10) rect.y -= rect.height - 50;

            if (rect.x < 5) rect.x = 5;
            else if (rect.xMax > Screen.currentResolution.width - 5) rect.x = Screen.currentResolution.width - 5 - rect.width;
#endif

            Selection.activeGameObject = prevTarget;
            AddComponent.ShowAddComponent(rect);
        }

        private static void ShowComponent(Component component, Rect hierarchyRect)
        {
            Event e = Event.current;
            Vector2 position = e.mousePosition;
            position.y = hierarchyRect.yMax;
            position = WindowsHelper.GetMousePositionOnFocusedWindow(position);

            Vector2 size = Prefs.defaultWindowSize;
            Rect rect = new Rect(position + new Vector2(-size.x / 2, 36), size);

#if !UNITY_EDITOR_OSX
            if (rect.yMax > Screen.currentResolution.height - 10) rect.y -= rect.height - 50;

            int screenWidth = Screen.currentResolution.width;

            if (rect.x < 5) rect.x = 5;
            else if (rect.center.x > screenWidth)
            {
                if (rect.xMin < screenWidth) rect.x = screenWidth;
            }
            else if (rect.xMax > screenWidth - 5)
            {
                rect.x = screenWidth - 5 - rect.width;
            }
#endif

            ComponentWindow wnd = ComponentWindow.Show(component);
            wnd.position = rect;
        }

        private static void ShowMore(GameObject target, IEnumerable<Component> components, Rect rect)
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            bool useSeparator = false;

            if (Prefs.hierarchyIconsHideDefault)
            {
                foreach (Type type in defaultTypes)
                {
                    Component c = target.GetComponent(type);
                    if (!c || c.GetType() != type) continue;
                        
                    menu.Add(type.Name, () =>
                    {
                        SceneViewManager.OnNextGUI += () => ShowComponent(c, rect);
                        SceneView.RepaintAll();
                    });
                }
            }

            foreach (Component c in components)
            {
                menu.Add(c.GetType().Name, () =>
                {
                    SceneViewManager.OnNextGUI += () => ShowComponent(c, rect);
                    SceneView.RepaintAll();
                });
                useSeparator = true;
            }

            if (useSeparator) menu.AddSeparator();

            ShowMoreAddComponent(rect, menu);
            ShowMoreAddToBookmarks(menu);
            ShowMoreRenameChildren(target, menu);
            ShowMoreSiblingIndex(target, menu);
            ShowMoreSortBy(target, menu);

            menu.Show();
        }

        private static void ShowMoreRenameChildren(GameObject target, GenericMenuEx menu)
        {
            if (target.transform.childCount == 0) return;
            
            menu.Add("Rename Children", () => Rename.Show(GameObjectUtils.GetChildren(target)));
        }

        private static void ShowMoreAddComponent(Rect rect, GenericMenuEx menu)
        {
            menu.Add("Add Component", () =>
            {
                SceneViewManager.OnNextGUI += () => ShowAddComponent(rect);
                SceneView.RepaintAll();
            });
        }

        private static void ShowMoreAddToBookmarks(GenericMenuEx menu)
        {
            if (Prefs.hierarchyBookmarks) return;
            
            menu.Add("Add To Bookmark", () =>
            {
                Bookmarks.Add(prevTarget);
                SceneView.RepaintAll();
            });
        }

        private static void ShowMoreSiblingIndex(GameObject target, GenericMenuEx menu)
        {
            menu.Add("Sibling Index/First", () =>
            {
#if UNITY_2022_3_OR_NEWER
                Undo.SetSiblingIndex(target.transform, 0, "Move First");
#else
                target.transform.SetSiblingIndex(0);
#endif
            });
            menu.Add("Sibling Index/Last", () =>
            {
                Transform parent = target.transform.parent;
                int index = parent ? parent.childCount - 1 : SceneManager.GetActiveScene().rootCount - 1;
#if UNITY_2022_3_OR_NEWER
                Undo.SetSiblingIndex(target.transform, index, "Move Last");
#else
                target.transform.SetSiblingIndex(index);
#endif
            });
        }

        private static void ShowMoreSortBy(GameObject target, GenericMenuEx menu)
        {
            if (target.transform.childCount <= 1) return;
            
            menu.Add("Sort By/Name", () =>
            {
                SortBy(target, new StringWithNumberComparer(), "Sort By Name");
            });
        }
        
        private static void SortBy(GameObject target, IComparer<Transform> comparer, string undoName)
        {
            Undo.RecordObject(target.transform, undoName);
            int group = Undo.GetCurrentGroup();
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < target.transform.childCount; i++)
            {
                children.Add(target.transform.GetChild(i));
            }
            children.Sort(comparer);
            for (int i = 0; i < children.Count; i++)
            {
#if UNITY_2022_3_OR_NEWER
                Undo.SetSiblingIndex(children[i], i, undoName);
#else
                children[i].SetSiblingIndex(i);
#endif
            }
            Undo.CollapseUndoOperations(group);
        }

        private static void UpdateItems(Rect rect, GameObject target, List<Item> items)
        {
            items.Clear();

            if (!target) return;

            Item item;
            Component[] components = target.GetComponents<Component>();

            for (int i = 0; i < Mathf.Min(components.Length, Prefs.hierarchyIconsMaxItems); i++)
            {
                item = CreateItem(components[i], rect);
                if (item != null) items.Add(item);
            }

            int moreItems = components.Length - Prefs.hierarchyIconsMaxItems;

            GUIContent moreContent = new GUIContent(moreItems > 0 ? "+" + moreItems : "...", "More");
            item = new(moreContent, null);
            item.OnClick += () => ShowMore(target, components.Skip(Prefs.hierarchyIconsMaxItems), rect);
            items.Add(item);
        }

        internal class Item
        {
            public Action OnClick;
            public Action OnDrag;
            public Action OnMiddleClick;
            public Action OnRightClick;
            public GUIContent content;
            public Component component;

            public Item(GUIContent content, Component component)
            {
                this.content = content;
                this.component = component;
            }

            public void Dispose()
            {
                OnClick = null;
                OnDrag = null;
                OnRightClick = null;
                content = null;
                component = null;
            }

            public float Draw(Rect rect)
            {
                bool isDisabled = component && !ComponentUtils.GetEnabled(component);
                
                bool useButton = !string.IsNullOrEmpty(content.text);
                rect.xMin -= useButton ? Styles.hierarchyIcon.CalcSize(content).x + 8 : 18;
                GUIContent c = TempContent.Get(content);
                if (isDisabled) c.tooltip += " (Disabled)";
                
                GUILayoutUtils.BeginDisabledStyle(isDisabled);
                GUI.Box(rect, c, useButton ? Styles.hierarchyIcon : Styles.iconButton);
                GUILayoutUtils.EndDisabledStyle();

                if (!rect.Contains(Event.current.mousePosition)) return rect.x;
                ProcessEvents();

                return rect.x;
            }

            private void ProcessEvents()
            {
                Event e = Event.current;
                if (e.type == EventType.MouseUp) ProcessMouseUp();
                else if (e.type == EventType.MouseDown) e.Use();
                else if (e.type == EventType.MouseDrag)
                {
                    if (OnDrag != null) OnDrag();
                    e.Use();
                }
            }

            private void ProcessMouseUp()
            {
                Event e = Event.current;
                
                if (e.button == 0)
                {
                    if (OnClick != null) OnClick();
                    e.Use();
                }
                else if (e.button == 1)
                {
                    if (OnRightClick != null) OnRightClick();
                    e.Use();
                }
                else if (e.button == 2)
                {
                    if (OnMiddleClick != null) OnMiddleClick();
                    e.Use();
                }
            }
        }
    }
}