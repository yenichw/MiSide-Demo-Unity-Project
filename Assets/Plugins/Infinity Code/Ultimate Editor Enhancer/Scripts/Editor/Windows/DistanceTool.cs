/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class DistanceTool : EditorWindow
    {
        public const int LineHeight = 24;
        private const string DistanceStyleID = "sv_label_3";
        private const string IndexStyleID = "sv_label_1";

        private static GUIStyle _distanceStyle;
        private static GUIStyle _indexStyle;
        private static Vector3 axisMul;
        private static bool isDirty;
        private static Target pickTarget;
        private static bool lastPointUnderCursor;

        private bool hasPrev = false;

        private Vector3 prevPosition;
        private ReorderableList reorderableList;
        private Vector2 scrollPosition;

        [SerializeField]
        private List<Target> targets;

        private float totalDistance;
        private bool useX = true;
        private bool useY = true;
        private bool useZ = true;
        private Vector3 pointUnderCursor;

        public static GUIStyle distanceStyle
        {
            get
            {
                if (_distanceStyle == null)
                {
                    _distanceStyle = new GUIStyle(DistanceStyleID)
                    {
                        fontSize = 10,
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = false,
                        fixedHeight = 16,
                        normal =
                        {
                            textColor = Color.white
                        },
                        padding = new RectOffset(2, 2, 0, 0),
                    };
                }

                return _distanceStyle;
            }
        }

        public static GUIStyle indexStyle
        {
            get
            {
                if (_indexStyle == null)
                {
                    _indexStyle = new GUIStyle(IndexStyleID)
                    {
                        fontSize = 10,
                        alignment = TextAnchor.MiddleCenter,
                        wordWrap = false,
                        fixedHeight = 16,
                        normal =
                        {
                            textColor = Color.white
                        },
                        padding = new RectOffset(2, 2, 0, 0),
                    };
                }

                return _indexStyle;
            }
        }

        public void AddPoint(Vector3 point)
        {
            Target target = new Target(TargetType.point)
            {
                point = point
            };
            targets.Add(target);
        }

        private void AddItem(ReorderableList list)
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add("Transform", () => targets.Add(new Target(TargetType.transform)));
            menu.Add("Point", () => targets.Add(new Target(TargetType.point)));
            menu.Show();
        }

        private void BottomToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Total Distance: " + totalDistance.ToString("F1") + "m");
            EditorGUI.BeginChangeCheck();
            useX = GUILayout.Toggle(useX, "X", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
            useY = GUILayout.Toggle(useY, "Y", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
            useZ = GUILayout.Toggle(useZ, "Z", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck()) isDirty = true;
            if (GUILayoutUtils.ToolbarButton("?")) Links.OpenDocumentation("distance-tool");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Target t = targets[index];

            Rect r = new Rect(rect);
            r.y += 2;
            r.height = LineHeight - 4;

            if (t.type == TargetType.transform)
            {
                t.transform = EditorGUI.ObjectField(r, t.transform, typeof(Transform), true) as Transform;
            }
            else
            {
                Rect r2 = new Rect(r);

                r2.width -= 100;
                t.point = EditorGUI.Vector3Field(r2, string.Empty, t.point);
                r2.x = r2.xMax + 4;
                r2.y -= 1;
                r2.width = 92;

                if (pickTarget != t)
                {
                    if (GUI.Button(r2, "Pick"))
                    {
                        pickTarget = t;

                        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.Focus();
                        SceneViewManager.AddListener(OnPickSceneView);
                    }
                }
                else
                {
                    if (GUI.Button(r2, "Stop")) pickTarget = null;
                }
            }

            string message = "";

            if (t.type == TargetType.transform)
            {
                message = "Ignored";
                if (t.transform != null)
                {
                    message = t.position.ToString("F1");
                    if (hasPrev)
                    {
                        float d = GetDistance(t.position, prevPosition);
                        message += ", Distance: " + d.ToString("F1") + "m";
                        totalDistance += d;
                    }
                }
            }
            else
            {
                if (hasPrev)
                {
                    float d = GetDistance(t.point, prevPosition);
                    message += "Distance: " + d.ToString("F1") + "m";
                    totalDistance += d;
                }
            }


            r.y += 20;
            EditorGUI.LabelField(r, message);

            if (t.isValid)
            {
                prevPosition = t.position;
                hasPrev = true;
            }
        }

        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Targets");
        }

        private static void DrawLine(Vector3 prev, Vector3 p)
        {
            Handles.DrawLine(p, prev);
            Handles.Label((p + prev) / 2, DistanceTool.GetDistance(p, prev).ToString("F1"), DistanceTool.distanceStyle);
        }

        private void DrawUseCursorPosition()
        {
            lastPointUnderCursor = EditorGUILayout.ToggleLeft("Last point is the cursor in Scene View", lastPointUnderCursor);

            if (!lastPointUnderCursor) return;

            Event e = Event.current;

            if (hasPrev && e.type == EventType.MouseMove)
            {
                Vector3 worldPosition;
                SceneViewManager.GetWorldPosition(out worldPosition);
                totalDistance += GetDistance(worldPosition, prevPosition);
            }

            isDirty = true;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    lastPointUnderCursor = false;
                    e.Use();
                }
            }
        }

        private void DrawUseCursorSceneGUI(SceneView sceneView)
        {
            if (!lastPointUnderCursor) return;

            Event e = Event.current;

            if (e.type == EventType.MouseMove) pointUnderCursor = GetCurrentPoint();
            if (hasPrev)
            {
                DrawLine(prevPosition, pointUnderCursor);
            }
            
            if (!SceneView.lastActiveSceneView.in2DMode && KeyManager.IsKeyDown(KeyCode.V))
            {
                float handleSize = HandleUtility.GetHandleSize(pointUnderCursor);
                Handles.RectangleHandleCap(-1, pointUnderCursor, sceneView.camera.transform.rotation, handleSize * 0.125f, Event.current.type);
            }

            if (e.type == EventType.MouseDown && e.modifiers == EventModifiers.None)
            {
                if (e.button == 0)
                {
                    AddPoint(pointUnderCursor);
                    SceneViewManager.BlockMouseUp();
                    e.Use();
                }
                else if (e.button == 1)
                {
                    lastPointUnderCursor = false;
                    SceneViewManager.BlockMouseUp();
                    e.Use();
                }
            }
            else if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    lastPointUnderCursor = false;
                    e.Use();
                }
            }
        }

        private Vector3 GetCurrentPoint()
        {
            Event e = Event.current;

            SceneView view = SceneView.lastActiveSceneView;
            if (view.in2DMode)
            {
                Vector3 p = view.camera.ScreenToWorldPoint(new Vector3(e.mousePosition.x, Screen.height - e.mousePosition.y - 40));
                p.z = 0;
                return p;
            }

            GameObject go = HandleUtility.PickGameObject(e.mousePosition, false);

            if (go != null)
            {
                if (KeyManager.IsKeyDown(KeyCode.V))
                {
                    Vector3 p;
                    HandleUtilityRef.FindNearestVertex(e.mousePosition, out p);
                    return p;
                }
                Vector3 worldPosition;
                SceneViewManager.GetWorldPosition(out worldPosition);
                return worldPosition;
            }

            return Vector3.zero;
        }

        public static float GetDistance(Vector3 p, Vector3 prev)
        {
            Vector3 d = p - prev;
            d.Scale(axisMul);
            return d.magnitude;
        }

        private float GetElementHeight(int index)
        {
            Target t = targets[index];
            if (index == 0 && t.type == TargetType.point) return LineHeight;
            return LineHeight * 2 - 4;
        }

        private void OnDestroy()
        {
            pickTarget = null;
            SceneViewManager.RemoveListener(OnSceneView);
        }

        private void OnEnable()
        {
            SceneViewManager.AddListener(OnSceneView, SceneViewOrder.Normal, true);
        }

        private void OnGUI()
        {
            if (targets == null) targets = new List<Target>();

            if (reorderableList == null)
            {
                reorderableList = new ReorderableList(targets, typeof(Transform), true, true, true, true);
                reorderableList.drawHeaderCallback += DrawHeader;
                reorderableList.drawElementCallback += DrawElement;
                reorderableList.onAddCallback += AddItem;
                reorderableList.onRemoveCallback += RemoveItem;
                reorderableList.elementHeightCallback += GetElementHeight;
                reorderableList.elementHeight = 48;
            }

            axisMul = new Vector3(useX ? 1 : 0, useY ? 1 : 0, useZ ? 1 : 0);

            totalDistance = 0;
            hasPrev = false;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            reorderableList.DoLayoutList();
            EditorGUILayout.EndScrollView();

            DrawUseCursorPosition();

            if (pickTarget != null)
            {
                EditorGUILayout.HelpBox("Press Enter to finish pick.", MessageType.Info);
            }

            BottomToolbar();

            ProcessEvents();

            if (isDirty)
            {
                isDirty = false;
                SceneView.RepaintAll();
                Repaint();
            }
        }

        private void OnPickSceneView(SceneView sceneView)
        {
            Event e = Event.current;
            if (pickTarget == null)
            {
                pickTarget = null;
                SceneViewManager.RemoveListener(OnSceneView);
                return;
            }

            if (e.type == EventType.MouseMove)
            {
                GameObject go = HandleUtility.PickGameObject(e.mousePosition, false);
                if (go != null)
                {
                    Vector3 worldPosition;
                    SceneViewManager.GetWorldPosition(out worldPosition);
                    float d = (pickTarget.point - worldPosition).sqrMagnitude;
                    pickTarget.point = worldPosition;
                    if (d > 0) Repaint();
                }
                else if (sceneView.in2DMode)
                {
                    Vector3 point = sceneView.camera.ScreenToWorldPoint(new Vector3(e.mousePosition.x, Screen.height - e.mousePosition.y - 40));
                    point.z = 0;
                    pickTarget.point = point;
                    Repaint();
                }
            }
            else if (e.type == EventType.KeyDown &&
                     (e.keyCode == KeyCode.Return ||
                      e.keyCode == KeyCode.KeypadEnter ||
                      e.keyCode == KeyCode.Escape))
            {
                StopPick();
                e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 0)
            {
                StopPick();
                e.Use();
                SceneViewManager.BlockMouseUp();
            }
        }

        private void OnSceneView(SceneView sceneView)
        {
            if (!Prefs.showDistanceInScene) return;

            if (targets == null) targets = new List<Target>();

            Color color = Handles.color;

            Handles.color = Color.green;

            Vector3 prev = Vector3.zero;
            hasPrev = false;

            foreach (Target t in targets)
            {
                if (t == null) continue;
                if (!t.isValid) continue;

                Vector3 p = t.position;

                if (hasPrev)
                {
                    Handles.DrawLine(p, prev);
                    Handles.Label((p + prev) / 2, GetDistance(p, prev).ToString("F1"), distanceStyle);
                }
                else hasPrev = true;

                prev = p;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                Target t = targets[i];
                if (t == null) continue;
                if (!t.isValid) continue;

                Vector3 p = t.position;

                if (t.type == TargetType.point)
                {
                    Vector3 newPoint = Handles.PositionHandle(p, Quaternion.identity);
                    if (p != newPoint)
                    {
                        t.point = p = newPoint;
                        Repaint();
                    }
                }

                Handles.Label(p, (i + 1).ToString(), indexStyle);
            }

            DrawUseCursorSceneGUI(sceneView);

            Handles.color = color;
        }

        [MenuItem(WindowsHelper.MenuPath + "Distance Tool", false, MenuItemOrder.DistanceTool)]
        public static void OpenWindow()
        {
            GetWindow<DistanceTool>(false, "Distance Tool").autoRepaintOnSceneChange = true;
        }

        private void ProcessEvents()
        {
            Event e = Event.current;

            if (e.type == EventType.DragUpdated)
            {
                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    Object obj = DragAndDrop.objectReferences[i];
                    if (!(obj is GameObject || obj is Component)) return;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                e.Use();
            }
            else if (e.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    Object obj = DragAndDrop.objectReferences[i];
                    if (obj is GameObject) targets.Add(new Target((obj as GameObject).transform));
                    else if (obj is Component) targets.Add(new Target((obj as Component).transform));
                }

                e.Use();
            }
        }

        private void RemoveItem(ReorderableList list)
        {
            targets.RemoveAt(list.index);
        }

        private void StopPick()
        {
            pickTarget = null;
            SceneViewManager.RemoveListener(OnPickSceneView);
        }

        [Serializable]
        public class Target
        {
            public TargetType type;
            public Transform transform;
            public Vector3 point;

            public Vector3 position
            {
                get
                {
                    if (type == TargetType.transform) return transform != null ? transform.position : Vector3.zero;
                    return point;
                }
            }

            public bool isValid
            {
                get { return type != TargetType.transform || transform != null; }
            }

            public Target()
            {

            }

            public Target(TargetType type)
            {
                this.type = type;
            }

            public Target(Transform transform)
            {
                type = TargetType.transform;
                this.transform = transform;
            }
        }

        public enum TargetType
        {
            transform,
            point
        }
    }
}