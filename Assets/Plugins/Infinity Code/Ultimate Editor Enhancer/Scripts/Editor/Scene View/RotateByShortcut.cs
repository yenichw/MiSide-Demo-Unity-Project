/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class RotateByShortcut
    {
        static RotateByShortcut()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnPress += OnInvoke;
            binding.OnValidate += OnValidate;
        }

        private static Vector3 GetAxis()
        {
            KeyCode k = Event.current.keyCode;

            float ey = SceneView.lastActiveSceneView.rotation.eulerAngles.y;
            Vector3 axis = Vector3.zero;

            if (k == KeyCode.UpArrow)
            {
                if (ey >= 45 && ey <= 135) axis.z = -1;
                else if (ey >= 225 && ey <= 315) axis.z = 1;
                else if (ey >= 135 && ey <= 225) axis.x = -1;
                else axis.x = 1;
            }
            else if (k == KeyCode.DownArrow)
            {
                if (ey >= 45 && ey <= 135) axis.z = 1;
                else if (ey >= 225 && ey <= 315) axis.z = -1;
                else if (ey >= 135 && ey <= 225) axis.x = 1;
                else axis.x = -1;
            }
            else if (k == KeyCode.LeftArrow)
            {
                axis.y = -1;
            }
            else if (k == KeyCode.RightArrow)
            {
                axis.y = 1;
            }
            else if (k == KeyCode.PageDown)
            {
                if (ey >= 45 && ey <= 135) axis.x = -1;
                else if (ey >= 225 && ey <= 315) axis.x = 1;
                else if (ey >= 135 && ey <= 225) axis.z = 1;
                else axis.z = -1;
            }
            else if (k == KeyCode.PageUp)
            {
                if (ey >= 45 && ey <= 135) axis.x = 1;
                else if (ey >= 225 && ey <= 315) axis.x = -1;
                else if (ey >= 135 && ey <= 225) axis.z = -1;
                else axis.z = 1;
            }

            return axis;
        }

        private static void OnInvoke()
        {
            Vector3 axis = GetAxis();
            if (axis == Vector3.zero) return;

            Transform[] transforms = Selection.transforms.Where(t => t.gameObject.scene.name != null).ToArray();
            if (transforms.Length == 0) return;

            Undo.RecordObjects(transforms, "Rotate");

            Vector3 handlePosition = UnityEditor.Tools.handlePosition;
            Quaternion handleRotation = UnityEditor.Tools.handleRotation;

            if (UnityEditor.Tools.pivotMode == PivotMode.Center)
            {
                for (int i = 0; i < transforms.Length; i++)
                {
                    transforms[i].RotateAround(handlePosition, handleRotation * axis, 90);
                }
            }
            else if (UnityEditor.Tools.pivotRotation == PivotRotation.Local)
            {
                for (int i = 0; i < transforms.Length; i++)
                {
                    Transform t = transforms[i];
                    t.Rotate(t.rotation * axis, 90, Space.World);
                }
            }
            else
            {
                for (int i = 0; i < transforms.Length; i++)
                {
                    transforms[i].Rotate(handleRotation * axis, 90, Space.World);
                }
            }

            Event.current.Use();
        }

        private static bool OnValidate()
        {
            Event e = Event.current;
            KeyCode k = e.keyCode;

            if (WindowsHelper.mouseOverWindow is SceneView &&
                (e.control || e.command) && e.shift &&
                (
                    k == KeyCode.UpArrow || k == KeyCode.DownArrow || 
                    k == KeyCode.LeftArrow || k == KeyCode.RightArrow || 
                    k == KeyCode.PageDown || k == KeyCode.PageUp
                ))
            {
                return true;
            }

            return false;
        }
    }
}