/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class DropScriptBehaviour
    {
        static DropScriptBehaviour()
        {
            DragAndDrop.AddDropHandler(OnDrop);
        }

        private static Type GetMonoBehaviour()
        {
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                MonoScript ms = obj as MonoScript;
                if (!ms) continue;

                Type type = ms.GetClass();
                if (type != null && type.IsSubclassOf(typeof(MonoBehaviour))) return type;
            }

            return null;
        }

        private static DragAndDropVisualMode OnDrop(int instanceId, HierarchyDropFlags dropMode, Transform parentForDraggedObjects, bool perform)
        {
            if (!Prefs.hierarchyDropScriptToCreateGameObject) return DragAndDropVisualMode.None;
            if (dropMode != HierarchyDropFlags.DropUpon) return DragAndDropVisualMode.None;
            
            Type type = GetMonoBehaviour();
            if (type == null) return DragAndDropVisualMode.None;
            
            Object obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj && obj is GameObject) return DragAndDropVisualMode.None;
            
            if (!perform) return DragAndDropVisualMode.Copy;
            
            string name = ObjectNames.NicifyVariableName(type.Name);
            GameObject go = new GameObject(name);
            go.AddComponent(type);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            Selection.activeGameObject = go;

            EditorApplication.delayCall += () =>
            {
                Event e = EditorGUIUtility.CommandEvent("Rename");
                SceneHierarchyWindowRef.GetLastInteractedHierarchy().SendEvent(e);
            };

            return DragAndDropVisualMode.Copy;
        }
    }
}