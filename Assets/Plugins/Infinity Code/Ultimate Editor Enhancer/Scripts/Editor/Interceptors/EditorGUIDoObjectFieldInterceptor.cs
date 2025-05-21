/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class EditorGUIDoObjectFieldInterceptor : StatedInterceptor<EditorGUIDoObjectFieldInterceptor>
    {
        private enum ObjectFieldValidatorOptions { None = 0x0, ExactObjectTypeValidation = 0x1 }
        private delegate Object ObjectFieldValidator(Object[] references, Type objType, SerializedProperty property, ObjectFieldValidatorOptions options);
        
        private static Object dragTarget;
        protected override MethodInfo originalMethod => EditorGUIRef.doObjectFieldMethod;
        public override bool state => true;
        
        protected override string prefixMethodName => nameof(DoObjectFieldPrefix);

        private static void DoObjectFieldPrefix(Rect position, Rect dropRect, int id, Object obj, Object objBeingEdited, Type objType, SerializedProperty property, ObjectFieldValidator validator, bool allowSceneObjects, GUIStyle style)
        {
            Event e = Event.current;
            if (!position.Contains(e.mousePosition)) return;
            if (e.button != 0 && e.button != 1) return;
            Object target = obj != null ? obj : property != null ? property.objectReferenceValue : null;
            if (target is Transform t) target = t.gameObject;
            
            if (e.type == EventType.MouseDown) dragTarget = target;
            else if (e.type == EventType.MouseDrag) 
            {
                if (target == dragTarget && target != null) 
                {
                    StartDrag(target);
                    dragTarget = null;
                }
            }
        }

        private static void StartDrag(Object target) 
        {
            if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0) return;
            if (target == null) return;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new []{ target };
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(target));
        }
    }
}