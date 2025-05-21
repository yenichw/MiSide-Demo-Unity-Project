/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    [EditorTool("Custom Pivot Rotation Tool")]
    public class CustomPivotRotation: EditorTool
    {
        private static GUIContent activeContent;
        private static GUIContent passiveContent;
        
        private Vector3 pivot;
        private Mode mode = Mode.Move;
        private Quaternion rotation;

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
                    if (activeContent == null) activeContent = new GUIContent(Icons.customPivotRotationToolActive, "Custom Pivot Rotation Tool");
                    return activeContent;
                }

                if (passiveContent == null) passiveContent = new GUIContent(Icons.customPivotRotationTool, "Custom Pivot Rotation Tool");
                return passiveContent;
            }
        }

        private void OnEnable()
        {
            pivot = UnityEditor.Tools.handlePosition;
            rotation = UnityEditor.Tools.handleRotation;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            pivot = UnityEditor.Tools.handlePosition;
            rotation = UnityEditor.Tools.handleRotation;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (Selection.gameObjects.Length == 0) return;

            Event e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.V)
                {
                    mode = Mode.SetPivot;
                    e.Use();
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == KeyCode.V)
                {
                    mode = Mode.Move;
                    e.Use();
                }
            }

            if (mode == Mode.Move)
            {
                EditorGUI.BeginChangeCheck();

                Quaternion prevRotation = rotation;
                pivot = Handles.PositionHandle(pivot, prevRotation);
                rotation = Handles.RotationHandle(prevRotation, pivot);

                if (EditorGUI.EndChangeCheck())
                {
                    Quaternion rotationDifference = rotation * Quaternion.Inverse(prevRotation);
                    
                    Matrix4x4 pivotToOrigin = Matrix4x4.Translate(-pivot);
                    Matrix4x4 originToPivot = Matrix4x4.Translate(pivot);
                    Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rotationDifference);
                    Matrix4x4 transformationMatrix = originToPivot * rotationMatrix * pivotToOrigin;

                    foreach (GameObject go in Selection.gameObjects)
                    {
                        Transform transform = go.transform;
                        Undo.RecordObject(transform, "Custom Pivot Rotation");

                        transform.position = transformationMatrix.MultiplyPoint(transform.position);
                        transform.rotation = rotationDifference * transform.rotation;
                    }
                }
            }
            else
            {
                float handleSize = HandleUtility.GetHandleSize(pivot);
                Vector3 p = pivot;
                HandleUtilityRef.FindNearestVertex(e.mousePosition, out p);
                Handles.RectangleHandleCap(-1, p, (window as SceneView).camera.transform.rotation, handleSize * 0.125f, e.type);
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    pivot = p;
                    mode = Mode.Move;
                    e.Use();
                }
            }

        }

        private enum Mode
        {
            Move,
            SetPivot,
        }
    }
}