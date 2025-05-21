/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class MoveToPoint
    {
        static MoveToPoint()
        {
            SceneViewManager.AddListener(OnSceneGUI, SceneViewOrder.Early);
        }

        private static void MoveRectTransforms(Vector3 worldPosition, Bounds bounds)
        {
            Vector3 delta = worldPosition - bounds.center;

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                GameObject go = Selection.gameObjects[i];
                Undo.RecordObject(go.GetComponent<RectTransform>(), "Move To Point");
                if (bounds.extents != Vector3.zero) go.transform.Translate(delta, Space.World);
                else go.transform.position = worldPosition;
            }
        }

        private static void MoveTransforms(Vector3 normal, Bounds bounds, Vector3 worldPosition)
        {
            Vector3 cubeSide = MathHelper.NormalToCubeSide(normal);
            Vector3 extents = bounds.extents;
            extents.Scale(cubeSide);
                    
            Vector3 delta = worldPosition - bounds.center + new Vector3(extents.x, extents.y, extents.z);

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                Transform t = Selection.gameObjects[i].transform;
                Undo.RecordObject(t, "Move To Point");
                if (bounds.extents != Vector3.zero) t.Translate(delta, Space.World);
                else t.position = worldPosition;
            }
        }

        private static void OnSceneGUI(SceneView view)
        {
            if (!Validate()) return;

            Event.current.Use();

            Bounds bounds = SelectionBoundsManager.bounds;

            Undo.SetCurrentGroupName("Move To Point");
            int group = Undo.GetCurrentGroup();

            SceneViewManager.GetWorldPositionAndNormal(out Vector3 worldPosition, out Vector3 normal, Selection.gameObjects);

            if (!SelectionBoundsManager.isRectTransform) MoveTransforms(normal, bounds, worldPosition);
            else MoveRectTransforms(worldPosition, bounds);

            Undo.CollapseUndoOperations(group);
        }

        private static bool Validate()
        {
            if (!Prefs.moveToPoint) return false;
            
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return false;
            if (e.keyCode != Prefs.moveToPointKeyCode || e.modifiers != Prefs.moveToPointModifiers) return false;
            if (!(WindowsHelper.mouseOverWindow is SceneView)) return false;
            return !Selection.objects.Any(o =>
            {
                GameObject go = o as GameObject;
                return go == null || go.scene.name == null;
            });
        }
    }
}