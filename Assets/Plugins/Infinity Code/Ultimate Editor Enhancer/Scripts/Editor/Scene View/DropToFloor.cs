/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class DropToFloor
    {
        public static Dictionary<Transform, float> movedObjects;
        public static List<Vector3> points;

        static DropToFloor()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnPress += OnInvoke;
            binding.OnValidate += OnValidate;
        }

        private static void DropGameObject(GameObject gameObject)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer) DropTarget(renderer.transform, renderer.bounds);
            else if (!TryDropByChildren(gameObject.transform)) DropTransform(gameObject.transform);
        }

        private static void DropTarget(Transform transform, Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 size = bounds.size;

            points.Clear();

            RaycastRendererPoints(min, size);

            if (points.Count == 0)
            {
                points = new List<Vector3>
                {
                    new Vector3(min.x, 0, min.z)
                };
            }

            Undo.RecordObject(transform, "Drop To Floor");

            float shift = points.Average(v => v.y) - min.y;

            transform.Translate(0, shift, 0, Space.World);
            movedObjects.Add(transform, shift);
        }

        private static void DropTransform(Transform transform)
        {
            Vector3 p;
            if (RaycastDown(transform.position + new Vector3(0, 0.1f, 0), out p) == -1)
            {
                p = new Vector3(transform.position.x, 0, transform.position.z);
            }

            Undo.RecordObject(transform, "Drop To Floor");

            float shift = p.y - transform.position.y;

            transform.Translate(0, shift, 0, Space.World);
            movedObjects.Add(transform, shift);
        }

        private static void OnInvoke()
        {
            GameObject[] targets = Selection.gameObjects.Where(g => g.scene.name != null).OrderBy(g => g.transform.position.y).ToArray();

            Undo.SetCurrentGroupName("Drop To Floor");
            int group = Undo.GetCurrentGroup();

            if (movedObjects == null) movedObjects = new Dictionary<Transform, float>();
            if (points == null) points = new List<Vector3>(5);

            for (int i = 0; i < targets.Length; i++)
            {
                DropGameObject(targets[i]);
            }

            movedObjects.Clear();
            Undo.CollapseUndoOperations(group);
        }

        private static bool OnValidate()
        {
            Event e = Event.current;
            if (!Prefs.dropToFloor || e.keyCode != Prefs.dropToFloorKeyCode || e.modifiers != (Prefs.dropToFloorModifiers | EventModifiers.FunctionKey)) return false;

            if (Selection.gameObjects.Length == 0) return false;
            if (Selection.gameObjects.All(g => g.scene.name == null)) return false;
            return true;
        }

        public static int RaycastDown(Vector3 point, out Vector3 hitPoint)
        {
            hitPoint = Vector3.zero;
            if (!Physics.Raycast(point, Vector3.down, out RaycastHit hit)) return -1;
            
            hitPoint = hit.point;
            if (!movedObjects.TryGetValue(hit.transform, out float shift)) return 0;
            
            hitPoint.y += shift;
            return 1;
        }

        public static void RaycastRendererPoints(Vector3 min, Vector3 size, CountRays countRays = CountRays.five)
        {
            Vector3 p;
            float y = min.y + 0.1f;
            int r;
            if (countRays == CountRays.five)
            {
                r = RaycastDown(new Vector3(min.x, y, min.z), out p);
                if (r == 0) points.Add(p);
                else if (r == 1)
                {
                    points.Clear();
                    points.Add(p);
                    return;
                }

                r = RaycastDown(new Vector3(min.x + size.x, y, min.z), out p);
                if (r == 0) points.Add(p);
                else if (r == 1)
                {
                    points.Clear();
                    points.Add(p);
                    return;
                }

                r = RaycastDown(new Vector3(min.x + size.x, y, min.z + size.z), out p);
                if (r == 0) points.Add(p);
                else if (r == 1)
                {
                    points.Clear();
                    points.Add(p);
                    return;
                }

                r = RaycastDown(new Vector3(min.x, y, min.z + size.z), out p);
                if (r == 0) points.Add(p);
                else if (r == 1)
                {
                    points.Clear();
                    points.Add(p);
                    return;
                }
            }

            r = RaycastDown(new Vector3(min.x + size.x / 2, y, min.z + size.z / 2), out p);
            if (r == 0) points.Add(p);
            else if (r == 1)
            {
                points.Clear();
                points.Add(p);
            }
        }

        private static bool TryDropByChildren(Transform transform)
        {
            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return false;
            
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            
            DropTarget(transform, bounds);
            return true;
        }

        public enum CountRays
        {
            five = 0,
            one = 1
        }
    }
}