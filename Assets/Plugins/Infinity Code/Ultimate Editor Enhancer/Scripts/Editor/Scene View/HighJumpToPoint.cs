/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class HighJumpToPoint
    {
        private static double lastShiftPressed;

#if UNITY_EDITOR_OSX
        private const EventModifiers Modifiers = EventModifiers.Command | EventModifiers.Shift;
#else
        private const EventModifiers Modifiers = EventModifiers.Control | EventModifiers.Shift;
#endif

        static HighJumpToPoint()
        {
            SceneViewManager.AddListener(OnSceneGUI);
        }

        private static void OnSceneGUI(SceneView view)
        {
            if (!Prefs.highJumpToPoint || view.orthographic) return;

            Event e = Event.current;
            bool isJump = e.type == EventType.MouseUp && e.button == 2 && e.modifiers == Modifiers;

            if (!isJump && Prefs.alternativeJumpShortcut && WindowsHelper.mouseOverWindow is SceneView)
            {
                bool isAlternativeShortcut = e.type == EventType.MouseUp && e.button == 1 && e.alt && e.shift && !e.control && !e.command;
                isJump = isAlternativeShortcut;
            }

            if (!isJump) return;

            if (!JumpToPoint.GetTargetPoint(e, out Vector3 targetPosition)) return;

            view.LookAt(targetPosition + new Vector3(0, 20, 0), view.rotation, 20);

            UnityEditor.Tools.viewTool = ViewTool.None;
            GUIUtility.hotControl = 0;

            e.Use();
        }
    }
}