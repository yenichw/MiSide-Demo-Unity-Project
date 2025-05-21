/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        public static bool viewGalleryHotKey = true;
        public static bool viewGalleryUIState = true;
        public static bool viewGallerySceneViewState = true;
        public static bool createViewStateFromSelection = true;
        public static bool restoreViewStateFromSelection = true;
        public static bool showViewStateInScene = true;

        public static KeyCode viewGalleryKeyCode = KeyCode.V;
        public static KeyCode createViewStateFromSelectionKeyCode = KeyCode.Slash;
        public static KeyCode restoreViewStateFromSelectionKeyCode = KeyCode.Slash;

        public static EventModifiers viewGalleryModifiers = EventModifiers.Alt | EventModifiers.Shift;

#if !UNITY_EDITOR_OSX
        public static EventModifiers createViewStateFromSelectionModifiers = EventModifiers.Control;
        public static EventModifiers restoreViewStateFromSelectionModifiers = EventModifiers.Control | EventModifiers.Shift;
#else
        public static EventModifiers createViewStateFromSelectionModifiers = EventModifiers.Command;
        public static EventModifiers restoreViewStateFromSelectionModifiers = EventModifiers.Command | EventModifiers.Shift;
#endif

        public class ViewGalleryManager : StandalonePrefManager<ViewGalleryManager>, IHasShortcutPref, IStateablePref
        {
            public override IEnumerable<string> keywords
            {
                get
                {
                    return new[]
                    {
                        "Create View State For Selection",
                        "Restore View State For Selection",
                        "View Gallery",
                    };
                }
            }

            public override float order
            {
                get { return -40; }
            }

            public override void Draw()
            {
                DrawFieldWithHotKey("View Gallery", ref viewGalleryHotKey, ref viewGalleryKeyCode, ref viewGalleryModifiers, EditorStyles.label, 17);
                EditorGUI.indentLevel++;

                viewGallerySceneViewState = EditorGUILayout.ToggleLeft("Scene View State", viewGallerySceneViewState);
                viewGalleryUIState = EditorGUILayout.ToggleLeft("UI State", viewGalleryUIState);

                EditorGUI.indentLevel--;

                DrawFieldWithHotKey("Create View State For Selection", ref createViewStateFromSelection, ref createViewStateFromSelectionKeyCode, ref createViewStateFromSelectionModifiers, EditorStyles.label, 17);
                DrawFieldWithHotKey("Restore View State For Selection", ref restoreViewStateFromSelection, ref restoreViewStateFromSelectionKeyCode, ref restoreViewStateFromSelectionModifiers, EditorStyles.label, 17);
                showViewStateInScene = EditorGUILayout.ToggleLeft("Show View State In SceneView (Hot Key - ALT)", showViewStateInScene);
            }

            public string GetMenuName()
            {
                return "View Gallery";
            }

            public IEnumerable<Shortcut> GetShortcuts()
            {
                if (!viewGalleryHotKey) return new Shortcut[0];

                return new[]
                {
                    new Shortcut("Open View Gallery", "Everywhere", viewGalleryModifiers, viewGalleryKeyCode),
                    new Shortcut("Create View State For Selection", "Everywhere", createViewStateFromSelectionModifiers, createViewStateFromSelectionKeyCode),
                    new Shortcut("Restore View State For Selection", "Everywhere", restoreViewStateFromSelectionModifiers, restoreViewStateFromSelectionKeyCode),
                    new Shortcut("Show ViewState In SceneView", "Scene View", EventModifiers.Alt),
                };
            }

            public override void SetState(bool state)
            {
                base.SetState(state);
                
                viewGalleryHotKey = state;
                createViewStateFromSelection = state;
                restoreViewStateFromSelection = state;
                showViewStateInScene = state;
            }
        }
    }
}