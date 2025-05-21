/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    [EditorTool("Pivot Tool")]
    public class PivotTool : EditorTool
    {
        public const string StyleID = "sv_label_4";
        
        private static GUIContent activeContent;
        private static Vector3 firstPoint;
        private static Texture handleIcon;
        private static Mode mode = Mode.Move;
        private static PRS[] oldValues;

        private static GUIContent passiveContent;
        private static int pointIndex;
        
        private static bool hasRenderers;
        private static GUIStyle style;

        private static bool initialized;

        private static Vector3 handlePosition;

        private static Quaternion handleRotation;

        public override GUIContent toolbarIcon
        {
            get
            {
#if UNITY_2020_2_OR_NEWER
                if (ToolManager.IsActiveTool(this))
#else
                if (EditorTools.IsActiveTool(this))
#endif
                {
                    if (activeContent == null) activeContent = new GUIContent(Icons.pivotToolActive, "Pivot Tool");
                    return activeContent;
                }

                if (passiveContent == null) passiveContent = new GUIContent(Icons.pivotTool, "Pivot Tool");
                return passiveContent;
            }
        }

        private void ChangePivot(Vector3 position, Quaternion rotation)
        {
            handlePosition = position;
            handleRotation = rotation;
            
            int childCount = Selection.gameObjects.Max(t => t.transform.childCount);
            if (oldValues == null)
            {
                oldValues = new PRS[Mathf.Max(8, Mathf.NextPowerOfTwo(childCount + 1))];
            }
            else if (oldValues.Length <= childCount)
            {
                oldValues = new PRS[Mathf.NextPowerOfTwo(childCount + 1)];
            }

            Undo.SetCurrentGroupName("Change Pivot");
            int group = Undo.GetCurrentGroup();

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                GameObject go = Selection.gameObjects[i];
                Transform t = go.transform;

                for (int j = 0; j < t.childCount; j++)
                {
                    oldValues[j] = PRS.Save(t.GetChild(j));
                }

                Undo.RecordObject(t, "Change Pivot");

                t.position = position;
                t.rotation = rotation;

                for (int j = 0; j < t.childCount; j++)
                {
                    Transform child = t.GetChild(j);
                    Undo.RecordObject(child, "Change Pivot");
                    oldValues[j].Restore(child);
                }
            }

            Undo.CollapseUndoOperations(@group);
        }

        private static void DoMove(ref Vector3 position, ref Quaternion rotation)
        {
            float handleSize;
            position = Handles.PositionHandle(position, rotation);
            rotation = Handles.RotationHandle(rotation, position);
            handleSize = HandleUtility.GetHandleSize(position);
            Handles.Label(position + new Vector3(1, -1, 0) * handleSize * 0.125f, handleIcon);
        }

        private Vector3 DoSetOrientation(EditorWindow window, EventType eventType, Vector3 position)
        {
            Event e = Event.current;
            float handleSize;
            if (pointIndex == 1)
            {
                handleSize = HandleUtility.GetHandleSize(firstPoint);
                Handles.RectangleHandleCap(-1, firstPoint, (window as SceneView).camera.transform.rotation, handleSize * 0.125f, eventType);
            }

            handleSize = HandleUtility.GetHandleSize(position);
            HandleUtilityRef.FindNearestVertex(e.mousePosition, out position);
            Handles.RectangleHandleCap(-1, position, (window as SceneView).camera.transform.rotation, handleSize * 0.125f, eventType);
            Handles.Label(position + new Vector3(1, -1, 0) * handleSize, handleIcon);

            if (pointIndex == 1)
            {
                Color color = Handles.color;
                Handles.color = Color.green;
                Handles.DrawLine(firstPoint, position);
                Handles.color = color;
            }

            if (eventType == EventType.MouseDown && e.button == 0)
            {
                if (pointIndex == 0)
                {
                    firstPoint = position;
                    pointIndex = 1;
                    e.Use();
                }
                else
                {
                    pointIndex = 0;
                    mode = Mode.Move;
                    e.Use();

                    GenericMenuEx menu = GenericMenuEx.Start();
                    menu.Add("X", () => SetOrientation(firstPoint, position, Vector3.right));
                    menu.Add("Y", () => SetOrientation(firstPoint, position, Vector3.up));
                    menu.Add("Z", () => SetOrientation(firstPoint, position, Vector3.forward));
                    menu.Show();
                }
            }

            return position;
        }

        private static void DoSetPivot(EditorWindow window, ref Vector3 position, EventType eventType, ref bool changed)
        {
            Event e = Event.current;
            
            float handleSize;
            handleSize = HandleUtility.GetHandleSize(position);
            HandleUtilityRef.FindNearestVertex(e.mousePosition, out position);
            Handles.RectangleHandleCap(-1, position, (window as SceneView).camera.transform.rotation, handleSize * 0.125f, eventType);
            Handles.Label(position + new Vector3(1, -1, 0) * handleSize * 0.125f, handleIcon);
            if (eventType == EventType.MouseDown && e.button == 0)
            {
                changed = true;
                mode = Mode.Move;
                e.Use();
            }
        }

        private static void DrawLines(Vector3 position, Quaternion rotation)
        {
            Color clr = Handles.color;

            Handles.color = Color.blue;
            Handles.DrawLine(position - rotation * Vector3.forward * 1000, position + rotation * Vector3.forward * 1000);

            Handles.color = Color.red;
            Handles.DrawLine(position - rotation * Vector3.left * 1000, position + rotation * Vector3.left * 1000);

            Handles.color = Color.green;
            Handles.DrawLine(position - rotation * Vector3.up * 1000, position + rotation * Vector3.up * 1000);

            Handles.color = clr;
        }

        private static void DrawMeshRendererWarning(EditorWindow window, Vector3 position, Quaternion rotation)
        {
            if (!hasRenderers) return;
            
            Handles.BeginGUI();
            
            if (style == null)
            {
                style = new GUIStyle(Styles.normalRow)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                    fixedHeight = 0,
                    border = new RectOffset(8, 8, 8, 8)
                };
            }

            SceneView sceneView = window as SceneView;
            const string message = "The selected GameObjects contain MeshRenderer components on themselves.\nWrap into a new empty GameObject to change the pivot correctly?";
            GUIContent content = TempContent.Get(message);
            Texture iconTexture = EditorIconContents.consoleErrorIconSmall.image;

            Vector2 size = EditorStyles.label.CalcSize(content);
            size.x += 50;
            size.y += 35;

            float pixelPerPoint = EditorGUIUtility.pixelsPerPoint;
            Vector3 screenPoint = sceneView.camera.WorldToScreenPoint(UnityEditor.Tools.handlePosition) / pixelPerPoint;
            if (screenPoint.y > size.y + 150 / pixelPerPoint)
            {
                screenPoint.y -= size.y + 50 / pixelPerPoint;
            }
            else
            {
                screenPoint.y += size.y + 150 / pixelPerPoint;
            }

            Rect rect = new Rect(screenPoint.x - size.x / 2, Screen.height / pixelPerPoint - screenPoint.y - size.y / 2, size.x, size.y);

            GUI.Box(rect, GUIContent.none, style);
            
            Rect iconRect = new Rect(rect.x + 5, rect.y + 5, 32, 32);
            GUI.DrawTexture(iconRect, iconTexture, ScaleMode.ScaleToFit);

            Rect labelRect = new Rect(rect.x + 40, rect.y + 5, rect.width - 45, rect.height - 35);
            GUI.Label(labelRect, content);

            Rect buttonRect = new Rect(rect.x + 5, labelRect.yMax + 5, rect.width - 10, 20);
            if (GUI.Button(buttonRect, "Wrap", EditorStyles.toolbarButton)) WrapSelection(position, rotation);

            Handles.EndGUI();
        }

        private static void Initialize()
        {
            if (Selection.gameObjects.Length == 1)
            {
                GameObject go = Selection.gameObjects[0];
                handlePosition = go.transform.position;
                handleRotation = go.transform.rotation;
            }
            else
            {
                handlePosition = UnityEditor.Tools.handlePosition;
                handleRotation = UnityEditor.Tools.handleRotation;
            }

            initialized = true;
        }

#if UNITY_2020_3_OR_NEWER

        public override void OnActivated()
#else
        private void OnEnable()
#endif
        {
            mode = 0;
            pointIndex = 0;

            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }

        private void OnSelectionChanged()
        {
            if (Selection.gameObjects.Length == 0)
            {
                hasRenderers = false;
                return;
            }
            hasRenderers = Selection.gameObjects.Any(g => g.GetComponent<MeshRenderer>() != null);

            Initialize();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (Selection.gameObjects.Length == 0) return;
            if (handleIcon == null) handleIcon = EditorIconContents.avatarPivot.image;

            if (!initialized)
            {
                Initialize();
            }

            Vector3 position = handlePosition;
            Quaternion rotation = handleRotation;

            Event e = Event.current;
            if (e.modifiers == EventModifiers.Alt) DrawLines(position, rotation);

            ProcessEvents();

            bool changed = false;
            EventType eventType = e.type;
            
            EditorGUI.BeginChangeCheck();
            if (mode == Mode.Move)
            {
                DoMove(ref position, ref rotation);
            }
            else if (mode == Mode.SetPivot)
            {
                DoSetPivot(window, ref position, eventType, ref changed);
            }
            else if (mode == Mode.SetOrientation)
            {
                position = DoSetOrientation(window, eventType, position);
            }

            if (EditorGUI.EndChangeCheck() || changed) ChangePivot(position, rotation);
            
            DrawMeshRendererWarning(window, position, rotation);
        }

#if UNITY_2020_3_OR_NEWER
        public override void OnWillBeDeactivated()
#else
	    private void OnDisable()
#endif
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void ProcessEvents()
        {
            Event e = Event.current;
            
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.V)
                {
                    mode = Mode.SetPivot;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.LeftShift && mode != Mode.SetOrientation)
                {
                    mode = Mode.SetOrientation;
                    pointIndex = 0;
                    e.Use();
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == KeyCode.V || e.keyCode == KeyCode.LeftShift)
                {
                    mode = Mode.Move;
                }
                if (e.keyCode == KeyCode.C && e.modifiers == EventModifiers.Alt)
                {
                    if (TryCenterPivot()) e.Use();
                }
            }
        }

        private bool TryCenterPivot()
        {
            if (Selection.gameObjects.Length == 0) return false;

            Bounds bounds = new Bounds(Selection.gameObjects[0].transform.position, Vector3.zero);
            foreach (GameObject go in Selection.gameObjects)
            {
                Collider[] colliders = go.GetComponentsInChildren<Collider>();
                if (colliders.Length > 0)
                {
                    foreach (Collider collider in colliders)
                    {
                        bounds.Encapsulate(collider.bounds);
                    }
                }
                else
                {
                    Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                    if (renderers.Length > 0)
                    {
                        foreach (Renderer renderer in renderers)
                        {
                            bounds.Encapsulate(renderer.bounds);
                        }
                    }
                    else bounds.Encapsulate(go.transform.position);
                }
            }

            ChangePivot(bounds.center, handleRotation);
            return true;
        }

        private void SetOrientation(Vector3 v1, Vector3 v2, Vector3 axis)
        {
            Quaternion rotation = Quaternion.FromToRotation(axis, v2 - v1);
            ChangePivot(UnityEditor.Tools.handlePosition, rotation);
        }

        private static void WrapSelection(Vector3 position, Quaternion rotation)
        {
            GameObject go = new GameObject("GameObject Wrapper");
            Undo.RegisterCreatedObjectUndo(go, "Wrap");
            go.transform.position = position;
            go.transform.rotation = rotation;
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                Undo.SetTransformParent(gameObject.transform, go.transform, "Wrap");
            }

            Selection.activeGameObject = go;
        }

        private struct PRS
        {
            public Vector3 position;
            public Quaternion rotation;

            public static PRS Save(Transform transform)
            {
                return new PRS
                {
                    position = transform.position,
                    rotation = transform.rotation
                };
            }

            public void Restore(Transform transform)
            {
                transform.position = position;
                transform.rotation = rotation;
            }
        }

        public enum Mode
        {
            Move,
            SetPivot,
            SetOrientation
        }
    }
}