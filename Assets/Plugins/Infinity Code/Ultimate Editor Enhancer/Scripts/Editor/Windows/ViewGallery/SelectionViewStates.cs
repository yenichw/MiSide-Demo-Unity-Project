/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    [InitializeOnLoad]
    public static class SelectionViewStates
    {
        private static ViewState selectedState;

        static SelectionViewStates()
        {
            ViewGallery.OnPrepareViewStatesMenu += OnPrepareViewStatesMenu;

            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnAddToSelectedValidate;
            binding.OnPress += AddToSelection;

            binding = KeyManager.AddBinding();
            binding.OnValidate += OnRestoreSelectedValidate;
            binding.OnPress += OnRestoreSelectedInvoke;

            ToolbarManager.AddLeftToolbar("SelectionViewStates", ToolbarIcon);
            Selection.selectionChanged += SelectionChanged;
            SelectionChanged();
        }

        public static void AddToSelection()
        {
            ViewState viewState = Selection.activeGameObject.GetComponent<ViewState>();
            if (viewState == null) viewState = Selection.activeGameObject.AddComponent<ViewState>();
            SceneView view = SceneView.lastActiveSceneView;
            viewState.pivot = view.pivot;
            viewState.rotation = view.rotation;
            viewState.size = view.size;
            viewState.is2D = view.in2DMode;
            viewState.title = Selection.activeGameObject.name;

            selectedState = viewState;
        }

        private static bool OnAddToSelectedValidate()
        {
            if (!Prefs.createViewStateFromSelection) return false;
            if (Selection.activeGameObject == null) return false;
            if (Selection.gameObjects.Length > 1) return false;
            Event e = Event.current;
            if (e.keyCode != Prefs.createViewStateFromSelectionKeyCode) return false;
            if (e.modifiers != Prefs.createViewStateFromSelectionModifiers) return false;
            return true;
        }

        private static void OnPrepareViewStatesMenu(GenericMenuEx menu)
        {
            if (!Prefs.createViewStateFromSelection) return;
            if (Selection.activeGameObject == null) return;
            if (Selection.gameObjects.Length > 1) return;

            menu.Add("Create/For Selection", AddToSelection);
        }

        private static void OnRestoreSelectedInvoke()
        {
            ViewState viewState = Selection.activeGameObject.GetComponent<ViewState>();
            if (viewState == null) return;

            SceneView sceneView = SceneView.lastActiveSceneView;
            sceneView.in2DMode = viewState.is2D;
            sceneView.pivot = viewState.pivot;
            sceneView.size = viewState.size;

            if (!viewState.is2D)
            {
                sceneView.rotation = viewState.rotation;
                sceneView.camera.fieldOfView = 60;
            }

            EditorWindow.GetWindow<SceneView>();
        }

        private static bool OnRestoreSelectedValidate()
        {
            if (!Prefs.restoreViewStateFromSelection) return false;
            if (Selection.activeGameObject == null) return false;
            if (Selection.gameObjects.Length > 1) return false;
            Event e = Event.current;
            if (e.keyCode != Prefs.restoreViewStateFromSelectionKeyCode) return false;
            if (e.modifiers != Prefs.restoreViewStateFromSelectionModifiers) return false;
            return true;
        }

        private static void SelectionChanged()
        {
            if (!Prefs.showViewStateToolbarIcon) return;

            selectedState = null;
            if (Selection.gameObjects.Length != 1 || Selection.activeGameObject == null) return;
            selectedState = Selection.activeGameObject.GetComponent<ViewState>();
        }

        private static void ToolbarIcon()
        {
            if (!Prefs.showViewStateToolbarIcon || selectedState == null) return;

            if (GUILayout.Button(new GUIContent(Icons.focusToolbar, "Restore View"), Styles.appToolbarButtonLeft, GUILayout.Width(30)))
            {
                OnRestoreSelectedInvoke();
            }
        }
    }
}