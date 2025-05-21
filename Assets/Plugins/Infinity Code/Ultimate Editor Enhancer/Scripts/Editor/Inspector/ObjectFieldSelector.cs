/*           INFINITY CODE          */
/*     https://infinity-code.com    */

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif


using System;
using System.Linq;
using System.Reflection;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.Interceptors;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.InspectorTools
{
    [InitializeOnLoad]
    public static class ObjectFieldSelector
    {
        private static bool blockMouseUp = false;

        static ObjectFieldSelector()
        {
            ObjectFieldInterceptor.OnGUIBefore += OnGUIBefore;
        }

        private static void OnGUIBefore(
            Rect position,
            Rect dropRect,
            int id,
            Object obj,
            Object objBeingEdited,
            Type objType,
            Type additionalType,
            SerializedProperty property,
            object validator,
            bool allowSceneObjects,
            GUIStyle style)
        {
            if (!Prefs.objectFieldSelector) return;
            if (property == null) return;

            Event e = Event.current;
            if (e.type == EventType.MouseUp && blockMouseUp)
            {
                blockMouseUp = false;
                e.Use();
                return;
            }

            if (e.type != EventType.MouseDown || e.button != 1) return;

            Rect rect = new Rect(position);
            rect.xMin = rect.xMax - 16;
            if (!rect.Contains(e.mousePosition)) return;

            SerializedObject serializedObject = property.serializedObject;
            if (serializedObject == null) return;

            Object[] targets = serializedObject.targetObjects;
            Object target = targets[0];
            Type type = target.GetType();
            FieldInfo field = Reflection.GetField(type, property.propertyPath, true);
            if (field == null) return;

            Type fieldType = field.FieldType;

            Object[] objects = null;
            GUIContent[] contents = null;

            if (fieldType.IsSubclassOf(typeof(Component)))
            {
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    objects = prefabStage.prefabContentsRoot.GetComponentsInChildren(fieldType, true);
                }
                else
                {
#if UNITY_2020_3_OR_NEWER
                    objects = ObjectHelper.FindObjectsOfType(fieldType, true);
#else
                    objects = ObjectHelper.FindObjectsOfType(field.FieldType);
#endif
                }

                if (objects.Length == 1 && property.objectReferenceValue == null)
                {
                    Undo.RecordObjects(targets, "Modified Property");
                    property.objectReferenceValue = objects[0];
                    serializedObject.SetIsDifferentCacheDirty();
                }
                else
                {
                    objects = new Object[1].Concat(objects.OrderBy(o => o.name)).ToArray();
                    contents = new GUIContent[objects.Length];

                    contents[0] = new GUIContent("None");
                    
                    StringBuilder builder = StaticStringBuilder.Start();

                    for (int i = 1; i < objects.Length; i++)
                    {
                        Component component = objects[i] as Component;
                        builder.Clear();
                        builder.Append(component.name)
                            .Append(" (")
                            .Append(component.GetType().Name)
                            .Append(")");

                        contents[i] = new GUIContent(builder.ToString(), GameObjectUtils.GetGameObjectPath(component.gameObject).ToString());
                    }
                }
            }
            else if (fieldType.IsSubclassOf(typeof(ScriptableObject)))
            {
                objects = UnityEngine.Resources.FindObjectsOfTypeAll(fieldType);
                if (objects.Length == 1 && property.objectReferenceValue == null)
                {
                    Undo.RecordObjects(targets, "Modified Property");
                    property.objectReferenceValue = objects[0];
                    serializedObject.SetIsDifferentCacheDirty();
                }
                else
                {
                    objects = new Object[1].Concat(objects.OrderBy(o => o.name)).ToArray();
                    contents = new GUIContent[objects.Length];
                    contents[0] = new GUIContent("None");

                    for (int i = 1; i < objects.Length; i++)
                    {
                        Object obj2 = objects[i];
                        ScriptableObject so = obj2 as ScriptableObject;
                        contents[i] = new GUIContent(so.name, AssetDatabase.GetAssetPath(so));
                    }
                }
            }
            else if (fieldType == typeof(GameObject))
            {
                objects = ObjectHelper.FindObjectsOfType<GameObject>();
                contents = new GUIContent[objects.Length];
                for (int i = 0; i < objects.Length; i++)
                {
                    contents[i] = new GUIContent(objects[i].name);
                }
            }
            else
            {
                Debug.Log(fieldType);
                objects = UnityEngine.Resources.FindObjectsOfTypeAll(fieldType);
                contents = new GUIContent[objects.Length];
                for (int i = 0; i < objects.Length; i++)
                {
                    contents[i] = new GUIContent(objects[i].name, AssetDatabase.GetAssetPath(objects[i]));
                }
            }

            blockMouseUp = true;
            e.Use();

            if (contents == null || contents.Length == 0) return;

            position.xMin += EditorGUIUtility.labelWidth;

            FlatSelectorWindow.Show(position, contents, -1).OnSelect += index =>
            {
                Undo.SetCurrentGroupName("Modified Property");
                int group = Undo.GetCurrentGroup();
                for (int i = 0; i < targets.Length; i++)
                {
                    Undo.RecordObject(targets[i], "Modified Property");
                    field.SetValue(targets[i], objects[index]);
                    EditorUtility.SetDirty(targets[i]);
                }
                Undo.CollapseUndoOperations(group);
                GUI.changed = true;
            };
        }
    }
}