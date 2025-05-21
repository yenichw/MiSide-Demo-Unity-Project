/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public static class InputManager
    {
        private static bool[] mouseStates = new bool[3];

        static InputManager()
        {
            SceneViewManager.AddListener(OnSceneView, SceneViewOrder.InputManager);
        }

        private static void OnSceneView(SceneView view)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (e.button < 3) mouseStates[e.button] = true;
            }
            else if (e.type == EventType.MouseUp)
            {
                if (e.button < 3) mouseStates[e.button] = false;
            }
        }

        public static bool GetAnyMouseButton()
        {
            return mouseStates[0] || mouseStates[1] || mouseStates[2];
        }

        public static bool GetMouseButton(int button)
        {
            if (button < 0 || button > 2) return false;
            return mouseStates[button];
        }
    }
}