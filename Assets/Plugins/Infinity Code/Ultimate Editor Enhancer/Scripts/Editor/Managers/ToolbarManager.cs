/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public static class ToolbarManager
    {
        private const float Space = 10;
        private const float LargeSpace = 20;
        private const float ButtonWidth = 32;
        private const float DropdownWidth = 80;
        private const float PlayPauseStopWidth = 140;

        private static int toolCount;
        private static GUIStyle style;

        private static int leftToolbarCount = 0;
        private static Item[] leftToolbarItems;
        
        private static int rightToolbarCount = 0;
        private static Item[] rightToolbarItems;

        private static ScriptableObject currentToolbar;

        static ToolbarManager()
        {
            toolCount = ToolbarRef.GetToolCount();
            EditorApplication.update -= CheckCurrentToolbar;
            EditorApplication.update += CheckCurrentToolbar;
        }

        private static void AddItem(ref Item[] items, ref int count, string key, Action action, int order)
        {
            if (items == null) 
            {
                items = new Item[8];
            }
            else if (count == items.Length)
            {
                Array.Resize(ref items, items.Length * 2);
            }

            int i;
            for (i = 0; i < count; i++)
            {
                if (order < items[i].order) break;
            }

            for (int j = count - 1; j >= i; j--)
            {
                items[j + 1] = items[j];
            }

            items[i] = new Item(key, action, order);
            count++;
        }

        public static void AddLeftToolbar(string key, Action action, int order = 0)
        {
            AddItem(ref leftToolbarItems, ref leftToolbarCount, key, action, order);
        }

        public static void AddRightToolbar(string key, Action action, int order = 0)
        {
            AddItem(ref rightToolbarItems, ref rightToolbarCount, key, action, order);
        }

        private static void CheckCurrentToolbar()
        {
            if (currentToolbar != null) return;

            Object[] toolbars = UnityEngine.Resources.FindObjectsOfTypeAll(ToolbarRef.type);
            if (toolbars.Length == 0)
            {
                currentToolbar = null;
                return;
            }

            currentToolbar = (ScriptableObject) toolbars[0];
            if (currentToolbar == null) return;

#if UNITY_2021_1_OR_NEWER
            VisualElement root = ToolbarRef.GetRoot(currentToolbar);

            CreateArea(root, "ToolbarZoneLeftAlign", Justify.FlexEnd, DrawLeftToolbarItems);
            CreateArea(root, "ToolbarZoneRightAlign", Justify.FlexStart, DrawRightToolbarItems);
#else
            VisualElement visualTree = Compatibility.GetVisualTree(currentToolbar);
            IMGUIContainer container = (IMGUIContainer)visualTree[0];

            Action handler = IMGUIContainerRef.GetGUIHandler(container);
            handler -= OnGUI;
            handler += OnGUI;
            IMGUIContainerRef.SetGUIHandler(container, handler);
#endif
        }

        private static void CreateArea(VisualElement root, string zoneName, Justify justify, Action action)
        {
            if (action == null) return;

            VisualElement toolbar = root.Q(zoneName);
            VisualElement parent = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    justifyContent = justify
                }
            };

            IMGUIContainer container = new IMGUIContainer();
            container.onGUIHandler += action.Invoke;
            parent.Add(container);
            toolbar.Add(parent);
        }

        private static void DrawLeftToolbar(float screenWidth, float playButtonsPosition)
        {
            if (leftToolbarCount == 0) return;

            Rect rect = new Rect(0, 0, screenWidth, Screen.height);
            rect.xMin += Space * 2 + ButtonWidth * toolCount + LargeSpace + 128;
            rect.xMax = playButtonsPosition - Space;
            rect.y = 4;
            rect.height = 24;

            if (rect.width <= 0) return;

            GUILayout.BeginArea(rect);
            DrawLeftToolbarItems();
            GUILayout.EndArea();
        }

        private static void DrawLeftToolbarItems()
        {
            if (!Prefs.toolbar) return;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            for (int i = 0; i < leftToolbarCount; i++)
            {
                try
                {
                    leftToolbarItems[i].action();
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawRightToolbar(float screenWidth, float playButtonsPosition)
        {
            if (rightToolbarCount == 0) return;

            Rect rightRect = new Rect(0, 0, screenWidth, Screen.height);
            rightRect.xMin = playButtonsPosition + style.fixedWidth * 3 + Space;
            rightRect.xMax = screenWidth - Space * 5 - DropdownWidth * 3 - LargeSpace - ButtonWidth - 78;
            rightRect.y = 4;
            rightRect.height = 24;

            if (rightRect.width <= 0) return;

            GUILayout.BeginArea(rightRect);
            DrawRightToolbarItems();
            GUILayout.EndArea();
        }

        private static void DrawRightToolbarItems()
        {
            if (!Prefs.toolbar) return;
            GUILayout.BeginHorizontal();

            for (int i = 0; i < rightToolbarCount; i++)
            {
                try
                {
                    rightToolbarItems[i].action();
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void OnGUI()
        {
            if (!Prefs.toolbar) return;
            if (style == null) style = new GUIStyle("CommandLeft");

            float screenWidth = EditorGUIUtility.currentViewWidth;
            float playButtonsPosition = Mathf.RoundToInt((screenWidth - PlayPauseStopWidth) / 2);

            DrawLeftToolbar(screenWidth, playButtonsPosition);
            DrawRightToolbar(screenWidth, playButtonsPosition);
        }

        private static bool RemoveItem(Item[] items, ref int count, string key)
        {
            if (count == 0) return false;

            int offset = 0;
            for (int i = 0; i < count; i++)
            {
                if (items[i].key == key)
                {
                    items[i].Dispose();
                    offset++;
                }
                else if (offset > 0)
                {
                    items[i - offset] = items[i];
                }
            }

            count -= offset;

            return offset > 0;
        }

        public static bool RemoveLeftToolbar(string key)
        {
            return RemoveItem(leftToolbarItems, ref leftToolbarCount, key);
        }

        public static bool RemoveRightToolbar(string key)
        {
            return RemoveItem(rightToolbarItems, ref rightToolbarCount, key);
        }

        private class Item
        {
            public Action action;
            public string key;
            public int order;

            public Item(string key, Action action, int order)
            {
                this.key = key;
                this.action = action;
                this.order = order;
            }

            public void Dispose()
            {
                action = null;
                key = null;
            }
        }
    }
}