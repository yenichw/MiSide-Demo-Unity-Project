/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.SceneTools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [InitializeOnLoad]
    public class AdvancedDropToFloor : EditorWindow
    {
        private int raycastFrom = 0; // 0 - Lower bounds, 1 - Transform
        private int countRays = 0; // 0 - 5, 1 - 1
        private int alignTo = 0; // 0 - Center, 1 - Min, 2 - Average, 3 - Max
        private Vector2 scrollPosition;

        static AdvancedDropToFloor()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnPress += OnInvoke;
            binding.OnValidate += OnValidate;
        }

        private void DropTarget(Transform transform, Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 size = bounds.size;

            DropToFloor.points.Clear();

            DropToFloor.RaycastRendererPoints(min, size, (DropToFloor.CountRays)countRays);

            if (DropToFloor.points.Count == 0)
            {
                DropToFloor.points = new List<Vector3>
                {
                    new Vector3(min.x, 0, min.z)
                };
            }

            float shift = 0;
            if (countRays == 1 || alignTo == 0) shift = DropToFloor.points.Sum(v => v.y) / DropToFloor.points.Count - min.y;
            else if (alignTo == 1) shift = DropToFloor.points.Min(v => v.y) - min.y;
            else if (alignTo == 2) shift = DropToFloor.points.Average(v => v.y) - min.y;
            else if (alignTo == 3) shift = DropToFloor.points.Max(v => v.y) - min.y;

            Undo.RecordObject(transform, "Drop To Floor");

            transform.Translate(0, shift, 0, Space.World);
            DropToFloor.movedObjects.Add(transform, shift);
        }

        private void DropSelection()
        {
            GameObject[] targets = Selection.gameObjects.Where(g => g.scene.name != null).OrderBy(g => g.transform.position.y).ToArray();

            if (targets.Length == 0) return;

            Undo.SetCurrentGroupName("Drop To Floor");
            int group = Undo.GetCurrentGroup();

            if (DropToFloor.movedObjects == null) DropToFloor.movedObjects = new Dictionary<Transform, float>();
            if (DropToFloor.points == null) DropToFloor.points = new List<Vector3>(5);

            for (int i = 0; i < targets.Length; i++)
            {
                GameObject go = targets[i];
                if (raycastFrom == 0)
                {
                    Renderer renderer = go.GetComponent<Renderer>();
                    if (renderer) DropTarget(go.transform, renderer.bounds);
                    else if (!TryDropByChildren(go.transform)) DropTransform(go.transform);
                }
                else if (raycastFrom == 1)
                {
                    DropTransform(go.transform);
                }
            }

            DropToFloor.movedObjects.Clear();
            Undo.CollapseUndoOperations(group);
        }
        
        private bool TryDropByChildren(Transform transform)
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

        private void DropTransform(Transform transform)
        {
            Vector3 p;
            if (DropToFloor.RaycastDown(transform.position + new Vector3(0, 0.1f, 0), out p) == -1)
            {
                p = new Vector3(transform.position.x, 0, transform.position.z);
            }

            Undo.RecordObject(transform, "Drop To Floor");

            float shift = p.y - transform.position.y;

            transform.Translate(0, shift, 0, Space.World);
            DropToFloor.movedObjects.Add(transform, shift);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Raycast from", EditorStyles.boldLabel);

            if (EditorGUILayout.Toggle("Lower Bound", raycastFrom == 0)) raycastFrom = 0;
            if (EditorGUILayout.Toggle("Zero Local Position", raycastFrom == 1)) raycastFrom = 1;
            GUILayout.Space(10);

            if (raycastFrom == 0)
            {
                GUILayout.Label("Count Rays", EditorStyles.boldLabel);

                if (EditorGUILayout.Toggle("Five (Corners And Center)", countRays == 0)) countRays = 0;
                if (EditorGUILayout.Toggle("One (Center)", countRays == 1)) countRays = 1;
                GUILayout.Space(10);
            }

            if (raycastFrom == 0 && countRays == 0)
            {
                GUILayout.Label("Align To Point", EditorStyles.boldLabel);

                if (EditorGUILayout.Toggle("Center", alignTo == 0)) alignTo = 0;
                if (EditorGUILayout.Toggle("Minimum", alignTo == 1)) alignTo = 1;
                if (EditorGUILayout.Toggle("Average", alignTo == 2)) alignTo = 2;
                if (EditorGUILayout.Toggle("Maximum", alignTo == 3)) alignTo = 3;
                GUILayout.Space(10);
            }

            EditorGUILayout.EndScrollView();

            Event e = Event.current;

            if (GUILayout.Button("Drop Selected Objects") || 
                e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                DropSelection();
                Close();
            }
        }

        private static void OnInvoke()
        {
            AdvancedDropToFloor wnd = GetWindow<AdvancedDropToFloor>("Drop To Floor");
        }

        private static bool OnValidate()
        {
            if (!Prefs.advancedDropToFloor) return false;

            Event e = Event.current;
            if (e.keyCode != Prefs.dropToFloorKeyCode || e.modifiers != (Prefs.advancedDropToFloorModifiers | EventModifiers.FunctionKey)) return false;
            return true;
        }
    }
}