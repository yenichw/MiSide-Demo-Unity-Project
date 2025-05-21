/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.EditorMenus;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public static class SceneViewManager
    {
        public static Action OnNextGUI;
        public static Func<bool> OnValidateOpenContextMenu;

        private static Vector2 _lastMousePosition;

        private static bool beforeInvoked = false;
        private static double lastUpdateLate = 0;
        private static List<Listener> lateListeners;
        private static List<Listener> listeners;
        private static Vector2 pressPoint;
        private static Dictionary<int, VisualElement> rectElements;

        private static bool waitOpenMenu;
        private static Plane plane2D;
        private static Plane plane3D;

        static SceneViewManager()
        {
            rectElements = new Dictionary<int, VisualElement>();

            SceneView.beforeSceneGui += SceneGUI;

            plane3D = new Plane(Vector3.up, Vector3.zero);
            plane2D = new Plane(Vector3.back, Vector3.zero);
        }

        public static void AddListener(Action<SceneView> invoke, float weight = 0, bool late = false)
        {
            if (!late)
            {
                if (listeners == null) listeners = new List<Listener>();
                listeners.Add(new Listener(invoke, weight));
                listeners = listeners.OrderByDescending(l => l.weight).ToList();
            }
            else
            {
                if (lateListeners == null) lateListeners = new List<Listener>();
                lateListeners.Add(new Listener(invoke, weight));
                lateListeners = lateListeners.OrderByDescending(l => l.weight).ToList();
            }
        }

        public static void BlockMouseUp()
        {
            AddListener(BlockMouseUpMethod);
            GUIUtility.hotControl = 1000;
        }

        private static void BlockMouseUpMethod(SceneView view)
        {
            Event e = Event.current;
            if (e.type != EventType.MouseUp) return;

            RemoveListener(BlockMouseUpMethod);
            GUIUtility.hotControl = 0;
        }

        public static Rect GetRect(SceneView view)
        {
#if !UNITY_2021_2_OR_NEWER
            Rect rect = view.position;
            rect.yMin += 20;
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) rect.yMax -= 25;
            return rect;
#else

            int id = view.GetInstanceID();
            VisualElement el;
            if (rectElements.TryGetValue(id, out el)) return el.contentRect;

            el = view.rootVisualElement.Q("unity-scene-view-camera-rect");
            rectElements[id] = el;
            return el.contentRect;
#endif
        }

        public static void GetWorldPosition(out Vector3 worldPosition, GameObject[] ignore = null)
        {
            GetWorldPositionAndNormal(SceneView.lastActiveSceneView, out worldPosition, out Vector3 _, ignore);
        }

        public static void GetWorldPosition(SceneView view, out Vector3 worldPosition, GameObject[] ignore = null)
        {
            Vector3 normal;
            GetWorldPositionAndNormal(view, out worldPosition, out normal, ignore);
        }

        public static void GetWorldPositionAndNormal(out Vector3 worldPosition, out Vector3 normal, GameObject[] ignore = null)
        {
            GetWorldPositionAndNormal(SceneView.lastActiveSceneView, out worldPosition, out normal, ignore);
        }

        public static void GetWorldPositionAndNormal(SceneView view, out Vector3 worldPosition, out Vector3 normal, GameObject[] ignore = null)
        {
            worldPosition = Vector3.zero;
            normal = Vector3.up;

            if (view == null) view = SceneView.lastActiveSceneView;
            if (view == null) return;

            Camera camera = view.camera;
            if (camera == null || camera.pixelWidth == 0 || camera.pixelHeight == 0) return;

            GameObject go = HandleUtility.PickGameObject(_lastMousePosition, false, ignore);

            Vector2 pixelCoordinate = HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition);
            Ray screenRay = camera.ScreenPointToRay(pixelCoordinate);

            if (go != null && !view.in2DMode)
            {
                MeshFilter meshFilter = go.GetComponent<MeshFilter>();
                RaycastHit hit;

                if (meshFilter != null && meshFilter.sharedMesh != null && HandleUtilityRef.IntersectRayMesh(screenRay, meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, out hit))
                {
                    worldPosition = hit.point;
                    normal = hit.normal;
                }
                else
                {
                    Collider collider = go.GetComponentInParent<Collider>();
                    if (collider != null)
                    {
                        if (collider.Raycast(screenRay, out hit, float.MaxValue))
                        {
                            worldPosition = hit.point;
                            normal = hit.normal;
                        }
                    }
                    else
                    {
                        RectTransform rectTransform = go.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, pixelCoordinate, view.camera, out worldPosition);
                            normal = Vector3.forward;
                        }
                    }
                }
            }
            else
            {
                float distance;
                Plane plane = view.in2DMode ? plane2D : plane3D;
                if (plane.Raycast(screenRay, out distance)) worldPosition = screenRay.GetPoint(distance);
                else worldPosition = Vector3.zero;
                normal = Vector3.up;
            }
        }

        private static void InvokeSceneGUI(SceneView sceneview)
        {
            if (listeners == null) return;

            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                try
                {
                    if (i < listeners.Count) listeners[i].Invoke(sceneview);
                }
                catch (Exception exception)
                {
                    Log.Add(exception);
                }
            }
        }

        private static void InvokeSceneGUILate(SceneView sceneview)
        {
            if (lateListeners == null) return;

            for (int i = lateListeners.Count - 1; i >= 0; i--)
            {
                try
                {
                    lateListeners[i].Invoke(sceneview);
                }
                catch (Exception exception)
                {
                    Log.Add(exception);
                }
            }
        }

        private static void OnMouseDown(Event e)
        {
            if (e.button != 1) return;

            waitOpenMenu = true;
            pressPoint = e.mousePosition;
        }

        private static void OnMouseDrag(Event e)
        {
            if (!waitOpenMenu) return;

            if ((e.mousePosition - pressPoint).sqrMagnitude > 100) waitOpenMenu = false;
        }

        private static void OnMouseUp(Event e)
        {
            if (e.button != 1 || !waitOpenMenu) return;

            waitOpenMenu = false;

            if (OnValidateOpenContextMenu != null)
            {
                Delegate[] invocationList = OnValidateOpenContextMenu.GetInvocationList();
                if (invocationList.Any(d => !(bool)d.DynamicInvoke())) return;
            }

            if (Prefs.pickGameObject && e.modifiers == Prefs.pickGameObjectModifiers)
            {
                Selection.activeGameObject = HandleUtility.PickGameObject(e.mousePosition, false);
            }

            if (Prefs.contextMenuOnRightClick && (e.modifiers == Prefs.rightClickModifiers || e.modifiers == Prefs.pickGameObjectModifiers))
            {
#if !UNITY_2021_1
                Vector2 position = e.mousePosition;
                if (EditorWindow.focusedWindow != null) position += EditorWindow.focusedWindow.position.position;
                EditorMenu.Show(position);
#else
                EditorMenu.Show(e.mousePosition);
#endif
            }
        }

        public static void RemoveListener(Action<SceneView> invoke)
        {
            if (listeners != null)
            {
                for (int i = listeners.Count - 1; i >= 0; i--)
                {
                    if (listeners[i].Invoke == invoke) listeners.RemoveAt(i);
                }
            }
            if (lateListeners != null)
            {
                for (int i = lateListeners.Count - 1; i >= 0; i--)
                {
                    if (lateListeners[i].Invoke == invoke) lateListeners.RemoveAt(i);
                }
            }
        }

        private static void SceneGUI(SceneView view)
        {
            beforeInvoked = true;
            if (OnNextGUI != null)
            {
                try
                {
                    OnNextGUI();
                }
                catch (Exception exception)
                {
                    Log.Add(exception);
                }
                OnNextGUI = null;
            }

            Event e = Event.current;

            if (EditorApplication.timeSinceStartup - lastUpdateLate > 1) UpdateSceneGUILate();

            if (e.type == EventType.MouseMove || e.type == EventType.DragUpdated)
            {
                _lastMousePosition = e.mousePosition;
            }

            InvokeSceneGUI(view);

            if (e.type == EventType.MouseDown)
            {
                if (GUILayoutUtils.hoveredButtonID != 0) GUIUtility.hotControl = GUILayoutUtils.hoveredButtonID;
                OnMouseDown(e);
            }
            else if (e.type == EventType.MouseUp) OnMouseUp(e);
            else if (e.type == EventType.MouseDrag) OnMouseDrag(e);
            else if (e.type == EventType.MouseMove) GUILayoutUtils.hoveredButtonID = 0;
        }

        private static void SceneGUILate(SceneView view)
        {
            if (!beforeInvoked) SceneGUI(view);
            InvokeSceneGUILate(view);
            beforeInvoked = false;
        }

        private static void UpdateSceneGUILate()
        {
            SceneView.duringSceneGui -= SceneGUILate;
            SceneView.duringSceneGui += SceneGUILate;
            lastUpdateLate = EditorApplication.timeSinceStartup;
        }

        internal class Listener
        {
            public Action<SceneView> Invoke;
            public float weight;

            public Listener(Action<SceneView> invoke, float weight)
            {
                Invoke = invoke;
                this.weight = weight;
            }
        }
    }
}
