/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class JumpToPoint
    {
        static JumpToPoint()
        {
            SceneViewManager.AddListener(OnSceneGUI);
        }

        public static bool GetTargetPoint(Event e, out Vector3 targetPosition)
        {
            List<GameObject> targets = new List<GameObject>();
            Vector2 mousePosition = e.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            targetPosition = Vector3.zero;

            int count = 0;
            while (count < 20)
            {
                GameObject go = HandleUtility.PickGameObject(mousePosition, false, targets.ToArray());
                if (go == null) break;

                targets.Add(go);
                Collider collider = go.GetComponentInParent<Collider>();

                if (collider != null)
                {
                    RaycastHit hit;
                    if (collider.Raycast(ray, out hit, float.MaxValue))
                    {
                        targetPosition = hit.point;
                        return true;
                    }
                }

                count++;
            }

            return false;
        }

        private static void OnSceneGUI(SceneView view)
        {
            if (!Prefs.jumpToPoint || view.orthographic) return;

            Event e = Event.current;
            bool isJump = e.type == EventType.MouseUp && e.button == 2 && e.modifiers == EventModifiers.Shift;

            if (!isJump && Prefs.alternativeJumpShortcut && WindowsHelper.mouseOverWindow is SceneView)
            {
                bool isAlternativeShortcut = e.type == EventType.MouseUp && e.button == 1 && e.alt && !e.shift && !e.control && !e.command;
                isJump = isAlternativeShortcut;
            }

            if (!isJump) return;
            if (!GetTargetPoint(e, out Vector3 targetPosition)) return;

            view.LookAt(targetPosition + new Vector3(0, 1.5f, 0), view.rotation, 1.5f);

            UnityEditor.Tools.viewTool = ViewTool.None;
            GUIUtility.hotControl = 0;

            e.Use();
        }
    }
}