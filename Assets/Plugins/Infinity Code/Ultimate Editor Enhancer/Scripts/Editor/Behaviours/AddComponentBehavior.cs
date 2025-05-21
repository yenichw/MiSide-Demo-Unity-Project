/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class AddComponentBehavior
    {
        private static EditorWindow wnd;

        static AddComponentBehavior()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidate;
            binding.OnPress += OnInvoke;
        }

        private static void OnInvoke()
        {
            Event e = Event.current;
            Vector2 position = e.mousePosition;
            if (EditorWindow.focusedWindow != null) position += EditorWindow.focusedWindow.position.position;
            else position = HandleUtility.GUIPointToScreenPixelCoordinate(position);

            Vector2 size = Prefs.defaultWindowSize;
            Rect rect = new Rect(position + new Vector2(-size.x / 2, 20), size);

#if !UNITY_EDITOR_OSX
            if (rect.yMax > Screen.currentResolution.height - 10) rect.y -= rect.height - 50;

            if (rect.x < 5) rect.x = 5;
            else if (rect.xMax > Screen.currentResolution.width - 5) rect.x = Screen.currentResolution.width - 5 - rect.width;
#endif

            if (wnd != null) EventManager.BroadcastClosePopup();

            AddComponentWindowRef.Show(rect, Selection.gameObjects);

            wnd = EditorWindow.GetWindow(AddComponentWindowRef.type);
            wnd.position = rect;

            PinAndClose.Show(wnd, rect, wnd.Close, "Add Component");
            wnd.Focus();
            e.Use();
        }

        private static bool OnValidate()
        {
            if (!Prefs.improveAddComponentBehaviour) return false;
            if (Selection.activeGameObject == null) return false;
            Event e = Event.current;
            if (e.keyCode != KeyCode.A) return false;
#if !UNITY_EDITOR_OSX
            if (e.modifiers != (EventModifiers.Control | EventModifiers.Shift)) return false;
#else
            if (e.modifiers != (EventModifiers.Command | EventModifiers.Shift)) return false;
#endif
            return true;
        }
    }
}