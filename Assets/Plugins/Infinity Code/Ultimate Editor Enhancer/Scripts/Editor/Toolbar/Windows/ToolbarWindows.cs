/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using PopupWindow = UnityEditor.PopupWindow;

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    [InitializeOnLoad]
    public static partial class ToolbarWindows
    {
        public static Func<IEnumerable<Provider>> OnInitProviders;
        public static List<WindowRecord> recent;

        private static GUIContent buttonContent;
        private static Type lastFocusedWindowType;
        private static Type lastWindowType;
        private static List<Provider> providers;
        private static List<int> removeKeys;
        private static List<EditorWindow> tempWindows;
        private static Dictionary<int, WindowRecord> windows;

        static ToolbarWindows()
        {
            Reinit();
        }

        private static void CacheWindows()
        {
            Type focusedWindowType = EditorWindow.focusedWindow != null ? EditorWindow.focusedWindow.GetType() : null;
            if (lastWindowType == focusedWindowType && windows.Count != 0) return;
            
            if (!IterateActiveWindows(focusedWindowType)) return;

            foreach (var pair in windows) pair.Value.used = false;

            foreach (EditorWindow w in tempWindows)
            {
                int key = w.GetType().GetHashCode();
                WindowRecord record;
                if (windows.TryGetValue(key, out record)) record.used = true;
                else if (!IgnoreWindow(w))
                {
                    record = new WindowRecord(w);
                    windows.Add(key, record);
                }
            }

            UpdateRecentWindows();

            foreach (int key in removeKeys) windows.Remove(key);

            lastWindowType = focusedWindowType;
            if (lastWindowType != null) lastFocusedWindowType = lastWindowType;
        }

        private static void DrawToolbar()
        {
            if (buttonContent == null) buttonContent = new GUIContent(Icons.windows, "Windows");

            GUIStyle style = new GUIStyle(Styles.dropdown)
            {
                margin = new RectOffset(0, 0, 0, 0)
            };

            if (!GUILayout.Button(buttonContent, style, GUILayout.Width(40)))
            {
                return;
            }

            if (providers == null) InitProviders();

            GenericMenuEx menu = GenericMenuEx.Start();
            bool hasItems = false;

            foreach (Provider provider in providers) provider.GenerateMenu(menu, ref hasItems);

            menu.Show();
        }

        private static void FocusWindow(object userdata)
        {
            bool maximized = false;
            WindowRecord record = userdata as WindowRecord;
            Type type = record.type;
            EditorWindow wnd = null;

            EditorWindow[] activeWindows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (EditorWindow window in activeWindows)
            {
                if (window == null) continue;
                if (window.maximized)
                {
                    maximized = true;
                    if (window is SceneView || window.GetType() == GameViewRef.type)
                    {
                        if (type != typeof(SceneView) && type != GameViewRef.type)
                        {
                            EditorWindow newWindow = EditorWindow.GetWindow(type, false, record.title);

                            if (type == SceneHierarchyWindowRef.type)
                            {
                                if (Selection.activeGameObject != null)
                                {
                                    SceneHierarchyWindowRef.SetExpandedRecursive(newWindow, Selection.activeGameObject.GetInstanceID(), true);
                                }
                                else
                                {
                                    object sceneHierarchy = SceneHierarchyWindowRef.GetSceneHierarchy(newWindow);
                                    SceneHierarchyRef.SetScenesExpanded(sceneHierarchy, new List<string> { SceneManager.GetActiveScene().name });
                                }
                            }

                            return;
                        }
                    }
                    window.maximized = false;
                    window.Repaint();
                    break;
                }
            }

            if (maximized) activeWindows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (EditorWindow window in activeWindows)
            {
                if (window == null) continue;
                if (window.GetType() == type)
                {
                    wnd = window;
                    break;
                }
            }

            if (wnd == null) return;

            wnd.Focus();
            if (maximized && (wnd is SceneView || wnd.GetType() == GameViewRef.type)) wnd.maximized = true;

            lastFocusedWindowType = type;
        }

        private static bool IgnoreWindow(EditorWindow window)
        {
            if (window is ComponentWindow) return true;
            if (window is CreateBrowser) return true;
            if (window is FlatSmartSelectionWindow) return true;
            if (window is InputDialog) return true;
            if (window is LayoutWindow) return true;
            if (window is PinAndClose) return true;
            if (window is PopupWindow) return true;
            if (window is Search) return true;

#if UNITY_2023_2_OR_NEWER
            if (window.GetType().Name == "ContextMenu") return true;
#endif
            
            return false;
        }

        private static void InitProviders()
        {
            providers = new List<Provider>
            {
                new OpenedProvider(),
                new RecentProvider(),
                new MiniLayouts(),
                new FavoriteProvider()
            };

            if (OnInitProviders != null)
            {
                foreach (Func<IEnumerable<Provider>> d in OnInitProviders.GetInvocationList())
                {
                    providers.AddRange(d());
                }
            }

            providers = providers.OrderBy(p => p.order).ToList();
        }

        private static bool IterateActiveWindows(Type focusedWindowType)
        {
            EditorWindow[] activeWindows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();
            if (activeWindows.Length == 0) return false;
            
            tempWindows.Clear();

            foreach (EditorWindow window in activeWindows)
            {
                if (window == null) continue;
                if (window.maximized)
                {
                    lastFocusedWindowType = window.GetType();
                    lastWindowType = focusedWindowType;
                    if (lastWindowType != null) lastFocusedWindowType = lastWindowType;

                    return true;
                }

                tempWindows.Add(window);
            }

            return true;
        }

        public static void Reinit()
        {
            windows = new Dictionary<int, WindowRecord>();
            tempWindows = new List<EditorWindow>();
            recent = new List<WindowRecord>();
            removeKeys = new List<int>();
            EditorApplication.update -= CacheWindows;
            ToolbarManager.RemoveRightToolbar("ToolbarWindows");

            if (Prefs.windowsToolbarIcon)
            {
                ToolbarManager.AddRightToolbar("ToolbarWindows", DrawToolbar, 0);
                EditorApplication.update += CacheWindows;
            }
        }

        public static void RestoreRecentWindow(object userdata)
        {
            WindowRecord record = userdata as WindowRecord;
            if (record == null) return;

            EditorWindow.GetWindow(record.type, false, record.title);
            recent.Remove(record);
        }

        private static void UpdateRecentWindows()
        {
            removeKeys.Clear();

            foreach (var pair in windows)
            {
                if (pair.Value.used) continue;

                removeKeys.Add(pair.Key);
                recent.Insert(0, pair.Value);
            }

            while (recent.Count > Prefs.maxRecentWindows) recent.RemoveAt(recent.Count - 1);
        }
    }
}