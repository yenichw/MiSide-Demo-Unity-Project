using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EPOOutline
{
    [CustomEditor(typeof(Outlinable))]
    public class OutlinableEditor : Editor
    {
        private UnityEditorInternal.ReorderableList targetsList;

        private void CheckList(SerializedProperty targets)
        {
            if (targetsList != null) 
                return;
            
            targetsList = new UnityEditorInternal.ReorderableList(serializedObject, targets);

            targetsList.drawHeaderCallback = position => EditorGUI.LabelField(position, "Renderers. All renderers that has to be outlined must be included here.");

            targetsList.drawElementCallback = (position, item, isActive, isFocused) =>
                {
                    var renderPosition = position;
                    var element = targets.GetArrayElementAtIndex(item);
                    var rendererItem = element.FindPropertyRelative("renderer");
                    var reference = rendererItem.objectReferenceValue;

                    EditorGUI.PropertyField(renderPosition, element, new GUIContent(reference == null ? "Null" : reference.name), true);
                };

            targetsList.elementHeightCallback = (index) => EditorGUI.GetPropertyHeight(targets.GetArrayElementAtIndex(index));

            targetsList.onRemoveCallback = (list) =>
                {
                    var index = list.index;
                    targets.DeleteArrayElementAtIndex(index);
                    targets.serializedObject.ApplyModifiedProperties();
                };

            targetsList.onAddDropdownCallback = (buttonRect, targetList) =>
                {
                    var outlinable = target as Outlinable;
                    var items = outlinable.gameObject.GetComponentsInChildren<Renderer>(true);
                    var menu = new GenericMenu();

                    if (!Application.isPlaying)
                    {
                        menu.AddItem(new GUIContent("Add all renderers"), false, () =>
                        {
                            (target as Outlinable).AddAllChildRenderersToRenderingList(RenderersAddingMode.All);

                            EditorUtility.SetDirty(target);
                        });

                        menu.AddItem(new GUIContent("Add all basic renderers"), false, () =>
                        {
                            (target as Outlinable).AddAllChildRenderersToRenderingList(RenderersAddingMode.MeshRenderer | RenderersAddingMode.SkinnedMeshRenderer);

                            EditorUtility.SetDirty(target);
                        });
                        
                        menu.AddItem(new GUIContent("Add all skinned mesh renderers"), false, () =>
                        {
                            (target as Outlinable).AddAllChildRenderersToRenderingList(RenderersAddingMode.SkinnedMeshRenderer);

                            EditorUtility.SetDirty(target);
                        });
                        
                        menu.AddItem(new GUIContent("Add all mesh renderers"), false, () =>
                        {
                            (target as Outlinable).AddAllChildRenderersToRenderingList(RenderersAddingMode.MeshRenderer);

                            EditorUtility.SetDirty(target);
                        });
                        
                        menu.AddItem(new GUIContent("Add all sprite renderers"), false, () =>
                        {
                            (target as Outlinable).AddAllChildRenderersToRenderingList(RenderersAddingMode.SpriteRenderer);

                            EditorUtility.SetDirty(target);
                        });
                    }

                    var entryIndex = -1;
                    foreach (var item in items)
                    {
                        entryIndex++;
                        var submeshCount = RendererUtility.GetSubmeshCount(item);
                        
                        var path = string.Empty;
                        if (item.transform != outlinable.transform)
                        {
                            var parent = item.transform;
                            do
                            {
                                path = $"{parent}/{path}";
                                parent = parent.transform.parent;
                            } while (parent != outlinable.transform);

                            path = $"{parent}/{path}";

                            path = path.Substring(0, path.Length - 1);
                        }
                        else
                            path = item.ToString();

                        var foundSubmeshes = new HashSet<int>();
                        for (var submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                        {
                            var found = false;
                            for (var index = 0; index < targets.arraySize; index++)
                            {
                                var element = targets.GetArrayElementAtIndex(index);
                                var elementRenderer = element.FindPropertyRelative("renderer");
                                if (elementRenderer.objectReferenceValue != item)
                                    continue;

                                var submesh = element.FindPropertyRelative("SubmeshIndex");
                                if (submesh.intValue != submeshIndex)
                                    continue;

                                found = true;
                                foundSubmeshes.Add(submeshIndex);
                                break;
                            }

                            var submeshChunk = submeshCount > 1 ? $"{path}/sub mesh {submeshIndex}" : path;
                            var finalPath = $"{entryIndex}: {submeshChunk}";

                            var capturedSubmeshIndex = submeshIndex;
                            GenericMenu.MenuFunction function = () =>
                            {
                                var index = targets.arraySize;
                                targets.InsertArrayElementAtIndex(index);
                                var arrayItem = targets.GetArrayElementAtIndex(index);
                                var renderer = arrayItem.FindPropertyRelative("renderer");
                                arrayItem.FindPropertyRelative("CutoutThreshold").floatValue = 0.5f;
                                arrayItem.FindPropertyRelative("SubmeshIndex").intValue = capturedSubmeshIndex;
                                renderer.objectReferenceValue = item;

                                serializedObject.ApplyModifiedProperties();
                            };

                            if (found)
                                function = null;

                            menu.AddItem(new GUIContent(finalPath), found, function);
                        }

                        if (submeshCount <= 1)
                            continue;

                        GenericMenu.MenuFunction allFunction = () =>
                            {
                                for (var submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                                {
                                    if (foundSubmeshes.Contains(submeshIndex))
                                        continue;
                                    
                                    var index = targets.arraySize;
                                    targets.InsertArrayElementAtIndex(index);
                                    var arrayItem = targets.GetArrayElementAtIndex(index);
                                    var renderer = arrayItem.FindPropertyRelative("renderer");
                                    arrayItem.FindPropertyRelative("CutoutThreshold").floatValue = 0.5f;
                                    arrayItem.FindPropertyRelative("SubmeshIndex").intValue = submeshIndex;
                                    renderer.objectReferenceValue = item;

                                    serializedObject.ApplyModifiedProperties();
                                }
                            };

                        if (foundSubmeshes.Count == submeshCount)
                            allFunction = null;
                        
                        menu.AddItem(new GUIContent($"{entryIndex}: {path}/All sub meshes"), foundSubmeshes.Count == submeshCount, allFunction);
                    }

                    menu.ShowAsContext();
                };
        }

        public override void OnInspectorGUI()
        {
            if ((serializedObject.FindProperty("drawingMode").intValue & (int)OutlinableDrawingMode.Normal) != 0)
            {
                if (serializedObject.FindProperty("renderStyle").intValue == 1)
                {
                    DrawPropertiesExcluding(serializedObject,
                        "frontParameters",
                        "backParameters",
                        "outlineTargets",
                        "outlineTargets",
                        "m_Script");
                }
                else
                {
                    DrawPropertiesExcluding(serializedObject,
                        "outlineParameters",
                        "outlineTargets",
                        "outlineTargets",
                        "m_Script");
                }
            }
            else
            {
                DrawPropertiesExcluding(serializedObject,
                    "outlineParameters",
                    "frontParameters",
                    "backParameters",
                    "outlineTargets",
                    "m_Script");
            }

            serializedObject.ApplyModifiedProperties();

            var renderers = serializedObject.FindProperty("outlineTargets");

            CheckList(renderers);

            if (serializedObject.targetObjects.Count() == 1)
                targetsList.DoLayoutList();
        }
    }
}
