/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.Integration;
using InfinityCode.UltimateEditorEnhancer.SceneTools;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class Switcher
    {
        static Switcher()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += () => Prefs.switcher;
            binding.OnPress += OnInvoke;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void GameViewToSceneView()
        {
            if (FullscreenEditor.isPresent)
            {
                EditorWindow[] activeWindows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();
                EditorWindow fullScreenWindow = activeWindows.FirstOrDefault(FullscreenEditor.IsFullscreen);
                if (fullScreenWindow != null && !(fullScreenWindow is SceneView))
                {
                    FullscreenEditor.OpenFullscreenSceneView();
                    return;
                }
            }

            IList list = Compatibility.GetGameViews();
            if (list == null || list.Count == 0) return;
            
            EditorWindow gameView = list[0] as EditorWindow;
            if (gameView == null || !gameView.maximized) return;

            gameView.maximized = false;
            gameView.Repaint();

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            sceneView.Focus();
            sceneView.maximized = true;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            if (Prefs.switcherWindows && e.keyCode == Prefs.switcherWindowsKeyCode && e.modifiers == Prefs.switcherWindowsModifiers) OnSwitch();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange mode)
        {
            if (!Prefs.switchToGameViewOnPlay) return;

            try
            {
                if (mode == PlayModeStateChange.EnteredPlayMode) EditorApplication.delayCall += SceneViewToGameView;
                else if (mode == PlayModeStateChange.EnteredEditMode) GameViewToSceneView();
            }
            catch
            {
            }
        }

        private static void OnSwitch()
        {
            EditorWindow window = EditorWindow.focusedWindow;
            if (window == null)
            {
                window = WindowsHelper.mouseOverWindow;
                if (window == null) return;
            }

            EditorWindow[] activeWindows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();

            if (ToggleFullscreenEditorWindows(activeWindows)) return;

            EditorWindow maximizedWindow = activeWindows.FirstOrDefault(w => w.maximized);

            bool maximized = maximizedWindow != null;

            if (maximized)
            {
                maximizedWindow.maximized = false;
                window.Repaint();
                window = maximizedWindow;
            }

            if (window is SceneView)
            {
                SwitchToGameView(maximized);
            }
            else if (window is PinAndClose || window == QuickAccess.activeWindow || window == ObjectToolbar.activeWindow)
            {
                SwitchToGameView(maximized);
            }
            else
            {
                SwitchToSceneView(maximized);
            }

            Event.current.Use();
        }

        private static void SceneViewToGameView()
        {
            if (FullscreenEditor.isPresent)
            {
                EditorWindow[] activeWindows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();
                EditorWindow fullScreenWindow = activeWindows.FirstOrDefault(FullscreenEditor.IsFullscreen);
                if (fullScreenWindow != null && fullScreenWindow.GetType() != GameViewRef.type)
                {
                    FullscreenEditor.OpenFullscreenGameView();
                    return;
                }
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || !sceneView.maximized) return;

            sceneView.maximized = false;
            sceneView.Repaint();

            IList list = Compatibility.GetGameViews();
            if (list == null || list.Count == 0) return;

            EditorWindow gameView = list[0] as EditorWindow;
            if (gameView != null)
            {
                gameView.maximized = true;
                gameView.Focus();
            }
        }

        private static void SwitchToGameView(bool maximized)
        {
            IList list = Compatibility.GetGameViews();
            if (list == null || list.Count == 0) return;

            EditorWindow gameView = list[0] as EditorWindow;
            if (gameView == null) return;

            if (maximized) gameView.maximized = true;
            gameView.Focus();
            ObjectToolbar.CloseActiveWindow();
            QuickAccess.CloseActiveWindow();
        }

        private static void SwitchToSceneView(bool maximized)
        {
            if (SceneView.sceneViews == null || SceneView.sceneViews.Count == 0) return;
            
            EditorWindow sceneView = SceneView.sceneViews[0] as EditorWindow;
            if (sceneView == null) return;
            
            if (maximized) sceneView.maximized = true;
            sceneView.Focus();
            if (EditorApplication.isPlaying) EditorApplication.isPaused = true;
        }

        private static bool ToggleFullscreenEditorWindows(EditorWindow[] activeWindows)
        {
            if (!FullscreenEditor.isPresent) return false;

            EditorWindow fullScreenWindow = activeWindows.FirstOrDefault(FullscreenEditor.IsFullscreen);
            if (fullScreenWindow == null) return false;

            if (fullScreenWindow is SceneView)
            {
                FullscreenEditor.OpenFullscreenGameView();
            }
            else FullscreenEditor.OpenFullscreenSceneView();

            return true;
        }
    }
}