/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.EditorMenus.Layouts;
using InfinityCode.UltimateEditorEnhancer.SceneTools;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus
{
    [InitializeOnLoad]
    public static class EditorMenu
    {
        public static bool allowCloseWindow = true;
        public static Func<bool> OnValidateOpen;

        private static MainLayoutItem[] items;
        private static bool _isOpened;
        private static Vector2 _lastPosition;
        private static EditorWindow _lastWindow;

        public static EditorWindow lastWindow
        {
            get { return _lastWindow; }
        }

        public static bool isOpened
        {
            get { return _isOpened; }
        }

        static EditorMenu()
        {
            AssetPreview.SetPreviewTextureCacheSize(32000);

            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += ValidateShortcut;
            binding.OnPress += OnShortcutPress;

            EventManager.AddBinding(EventManager.ClosePopupEvent).OnInvoke += b => Close();

            EditorApplication.quitting += CloseAll;
            EditorApplication.update += OnEditorApplicationUpdate;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            CloseAllFloatingWindows();
        }

        private static void CloseAllFloatingWindows()
        {
            try
            {
                Windows.PopupWindow[] windows = UnityEngine.Resources.FindObjectsOfTypeAll<Windows.PopupWindow>();
                foreach (Windows.PopupWindow wnd in windows)
                {
                    if (wnd == null) continue;
                    if (wnd is AutoSizePopupWindow pw && !pw.closeOnLossFocus && !pw.closeOnCompileOrPlay) continue; 
                    try
                    {
                        wnd.Close();
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Log.Add(e);
            }

            ObjectToolbar.CloseActiveWindow(); 
        }

        private static void CheckOpened()
        {
            if (!_isOpened || items == null) return;

            if (WindowsHelper.IsFocusOnContextMenu()) return;

            bool lastOpened = _isOpened;

            _isOpened = false;
            if (items.Any(item => EditorWindow.focusedWindow == item.window))
            {
                _isOpened = true;
                return;
            }

            if (lastOpened && !_isOpened) EventManager.BroadcastClosePopup();
        }

        public static void Close()
        {
            _isOpened = false;
            _lastWindow = null;
            if (items != null)
            {
                foreach (MainLayoutItem wnd in items) wnd.Close();
            }
        }

        private static void CloseAll()
        {
            Close();
            CloseAllFloatingWindows();
            EventManager.BroadcastClosePopup();
        }

        private static void InitLayoutItems()
        {
            if (items != null) return;

            List<MainLayoutItem> temp = new List<MainLayoutItem>();
            Type[] types = typeof(EditorMenu).Assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsAbstract || !type.IsSubclassOf(typeof(MainLayoutItem))) continue;
                temp.Add(Activator.CreateInstance(type, true) as MainLayoutItem);
            }

            items = temp.OrderBy(w => w.order).ToArray();
        }

        private static void OnCompilationStarted(object obj)
        {
            CloseAll();
        }

        private static void OnEditorApplicationUpdate()
        {
            if (EditorApplication.isCompiling) CloseAll();
            CheckOpened();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode) allowCloseWindow = false;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                allowCloseWindow = true;
                EditorApplication.delayCall += CloseAllFloatingWindows;
            }
        }

        private static void OnShortcutPress()
        {
            if (!Prefs.contextMenuOnHotKey) return;

            if (EditorWindow.focusedWindow != null)
            {
                Rect rect = EditorWindow.focusedWindow.position;
                rect.position += Event.current.mousePosition;
                Show(rect.position);
            }
            else Show(Event.current.mousePosition);
        }

        private static void Prepare(Vector2 position)
        {
            Vector2 offset = Vector2.zero;
            bool flipHorizontal = false;
            bool flipVertical = false;
            GameObject[] targets = Selection.gameObjects;

            foreach (MainLayoutItem item in items)
            {
                try
                {
                    item.Prepare(targets, position, ref offset, ref flipHorizontal, ref flipVertical);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }

            foreach (MainLayoutItem item in items)
            {
                try
                {
                    if (item.isActive) item.SetPosition(position, offset, flipHorizontal, flipVertical);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }

        private static void Show()
        {
            if (!ValidateOpen()) return;
            
            EventManager.BroadcastClosePopup();

            if (Prefs.contextMenuPauseInPlayMode && EditorApplication.isPlaying) EditorApplication.isPaused = true;

            _lastWindow = EditorWindow.focusedWindow;

            foreach (MainLayoutItem item in items)
            {
                if (item.isActive) item.Show();
            }

            _isOpened = true;
        }

        public static void Show(Vector2 position)
        {
            if (!ValidateOpen()) return;
            
#if !UNITY_2021_1_OR_NEWER || UNITY_2021_2_OR_NEWER
            EditorWindow focusedWindow = EditorWindow.focusedWindow;
            if (focusedWindow != null) position -= focusedWindow.position.position;
#endif
            _lastPosition = position = GUIUtility.GUIToScreenPoint(position);

            InitLayoutItems();
            Prepare(position);
            Show();
        }

        public static void ShowInLastPosition()
        {
            if (!ValidateOpen()) return;
            
            EventManager.BroadcastClosePopup();

            InitLayoutItems();
            Prepare(_lastPosition);
            Show();
        }

        private static bool ValidateOpen()
        {
            if (OnValidateOpen == null) return true;

            Delegate[] invocationList = OnValidateOpen.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    if (!(bool)invocationList[i].DynamicInvoke()) return false;
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }

            return true;
        }

        private static bool ValidateShortcut()
        {
            if (!Prefs.contextMenuOnHotKey) return false;
            if (EditorApplication.isPlaying && Prefs.contextMenuDisableInPlayMode && EditorWindow.focusedWindow.GetType() == GameViewRef.type) return false;
            if (Event.current.keyCode != Prefs.contextMenuHotKey) return false;
            if (Event.current.modifiers != Prefs.contextMenuHotKeyModifiers) return false;
            return true;
        }
    }
}