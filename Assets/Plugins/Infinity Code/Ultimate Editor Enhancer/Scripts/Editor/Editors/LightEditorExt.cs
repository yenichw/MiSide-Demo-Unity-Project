using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Editors
{
    [CustomEditor(typeof(Light))]
    public class LightEditorExt : LightEditor
    {
        private Light light;
        private bool initialized;
        private Vector3 targetPoint;

        private void DrawHandles()
        {
            if (!initialized)
            {
                initialized = true;
                targetPoint = light.transform.position;
                if (light.type == LightType.Spot)
                {
                    targetPoint += light.transform.forward * light.range;
                }
                else if (light.type == LightType.Directional)
                {
                    targetPoint += light.transform.forward * HandleUtility.GetHandleSize(targetPoint);
                }
                
                Undo.undoRedoPerformed -= UndoRedoPerformed;
                Undo.undoRedoPerformed += UndoRedoPerformed;
            }
            
            EditorGUI.BeginChangeCheck();
            
            Vector3 position = Handles.PositionHandle(targetPoint, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.SetCurrentGroupName("Change Light Target");
                int group = Undo.GetCurrentGroup();
                Undo.RecordObject(light.transform, "Change Light Target");
                targetPoint = position;
                light.transform.LookAt(targetPoint);
                Undo.RecordObject(light, "Change Light Target");
                light.range = Vector3.Distance(light.transform.position, targetPoint);
                Undo.CollapseUndoOperations(group);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            light = target as Light;
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (!KeyManager.IsKeyDown(KeyCode.B))
            {
                initialized = false;
                return;
            }

            if (light.type == LightType.Spot || light.type == LightType.Directional) DrawHandles();
        }

        private void UndoRedoPerformed()
        {
            initialized = false;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }
    }
}