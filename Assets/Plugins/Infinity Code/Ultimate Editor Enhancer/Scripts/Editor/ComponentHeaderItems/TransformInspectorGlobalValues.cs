/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using InfinityCode.UltimateEditorEnhancer.Interceptors;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.ComponentHeader
{
    public static class TransformInspectorGlobalValues
    {
        private static GUIStyle activeStyle;
        private static bool canUseSize;
        private static GUIContent content;
        private static bool enabled;
        private static bool inited;
        private static GUIContent linkedContent;
        private static Vector3 originalScale;
        private static Vector3 originalSize;
        private static Vector3 position;
        private static bool proportional;
        private static Vector3 rotation;
        private static Vector3 scale;
        private static int scaleType;
        private static string[] sizeTypeTexts = { "World Scale", "World Size" };
        private static GUIStyle style;
        private static Transform target;
        private static GUIContent unlinkedContent;

        private static void Disable()
        {
            scaleType = 0;
            proportional = false;
            enabled = false;
            target = null;
            inited = false;
            TransformInspectorInterceptor.OnInspector3DPrefix -= DrawInspector3D;
        }

        [ComponentHeaderButton(ComponentHeaderButtonOrder.TransformGlobalValues)]
        public static bool DrawHeaderButton(Rect rect, Object[] targets)
        {
            if (targets.Length != 1)
            {
                if (enabled) Disable();
                return false;
            }
            Object target = targets[0];
            if (!Validate(target))
            {
                return false;
            }

            if (!inited) Init();

            EditorGUI.BeginChangeCheck();
            enabled = GUI.Toggle(rect, enabled, content, enabled? activeStyle: style);
            if (EditorGUI.EndChangeCheck())
            {
                if (enabled) Enable(target as Transform);
                else Disable();
            }

            return true;
        }

        private static bool DrawInspector3D(Editor editor)
        {
            EditorGUI.BeginChangeCheck();
            position = EditorGUILayout.Vector3Field("World Position", position);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change Position");

                target.position = position;
                EditorUtility.SetDirty(target);
            }

            EditorGUI.BeginChangeCheck();
            rotation = EditorGUILayout.Vector3Field("World Rotation", rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change Rotation");

                target.rotation = Quaternion.Euler(rotation);
                EditorUtility.SetDirty(target);
            }

            DrawScale();

            return false;
        }

        private static void DrawScale()
        {
            if (scaleType == 0)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newScale = EditorGUILayout.Vector3Field("World Scale", scale);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Change Scale");

                    if (!proportional) scale = newScale;
                    else
                    {
                        if (Math.Abs(scale.x - newScale.x) > float.Epsilon)
                        {
                            if (Math.Abs(originalScale.x) > float.Epsilon)
                            {
                                scale = originalScale * newScale.x / originalScale.x;
                            }
                            else scale = new Vector3(newScale.x, newScale.x, newScale.x);
                        }
                        else if (Math.Abs(scale.x - newScale.x) > float.Epsilon)
                        {
                            if (Math.Abs(originalScale.y) > float.Epsilon)
                            {
                                scale = originalScale * newScale.y / originalScale.y;
                            }
                            else scale = new Vector3(newScale.y, newScale.y, newScale.y);
                        }
                        else if (Math.Abs(scale.z - newScale.z) > float.Epsilon)
                        {
                            if (Math.Abs(originalScale.z) > float.Epsilon)
                            {
                                scale = originalScale * newScale.z / originalScale.z;
                            }
                            else scale = new Vector3(newScale.z, newScale.z, newScale.z);
                        }
                    }

                    GameObjectUtils.SetLossyScale(target, scale);
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                Vector3 size = originalSize;
                size.Scale(scale);
                Vector3 newSize = EditorGUILayout.Vector3Field("World Size", size);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!proportional)
                    {
                        size = newSize;
                    }
                    else
                    {
                        if (Math.Abs(size.x - newSize.x) > float.Epsilon)
                        {
                            if (Math.Abs(originalSize.x) > float.Epsilon) size = originalSize * newSize.x / originalSize.x;
                            else size.x = 1;
                        }
                        else if (Math.Abs(size.y - newSize.y) > float.Epsilon)
                        {
                            if (Math.Abs(originalSize.y) > float.Epsilon) size = originalSize * newSize.y / originalSize.y;
                            else size.y = 1;
                        }
                        else if (Math.Abs(size.z - newSize.z) > float.Epsilon)
                        {
                            if (Math.Abs(originalSize.z) > float.Epsilon) size = originalSize * newSize.z / originalSize.z;
                            else size.z = 1;
                        }
                    }

                    if (Math.Abs(originalSize.x) > float.Epsilon) size.x /= originalSize.x;
                    else size.x = 1;

                    if (Math.Abs(originalSize.y) > float.Epsilon) size.y /= originalSize.y;
                    else size.y = 1;

                    if (Math.Abs(originalSize.z) > float.Epsilon) size.z /= originalSize.z;
                    else size.z = 1;

                    Undo.RecordObject(target, "Change Scale");

                    target.localScale = scale = size;
                }
            }

            Rect rect = GUILayoutUtility.GetLastRect();


            const int proportionalWidth = 20;
            if (GUI.Button(new Rect(rect.x + EditorGUIUtility.labelWidth - proportionalWidth, rect.y, proportionalWidth, rect.height), proportional? linkedContent: unlinkedContent, GUIStyle.none))
            {
                proportional = !proportional;
                originalScale = target.lossyScale;
            }

            EditorGUI.BeginDisabledGroup(!canUseSize);
            scaleType = GUILayout.Toolbar(scaleType, sizeTypeTexts);
            if (scaleType == 1)
            {
                EditorGUILayout.HelpBox("The world size is calculated based on the contained Renderers.", MessageType.None);
            }
            EditorGUI.EndDisabledGroup();

            if (!canUseSize)
            {
                EditorGUILayout.HelpBox("The world size is not available because the current and child GameObjects do not contain a Renderer.", MessageType.None);
            }
        }

        private static void Enable(Transform transform)
        {
            InitTarget(transform);

            TransformInspectorInterceptor.OnInspector3DPrefix += DrawInspector3D;
            Selection.selectionChanged += Disable;
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            canUseSize = renderers.Length > 0;
            if (canUseSize)
            {
                originalSize = GameObjectUtils.GetOriginalBounds(target.gameObject).size;
            }
        }

        private static void Init()
        {
            inited = true;

            content = new GUIContent(EditorIconContents.toolHandleGlobal.image, "Display transform values in world space.");
            linkedContent = new GUIContent(EditorIconContents.linked.image, "Disable constrained proportions");
            unlinkedContent = new GUIContent(EditorIconContents.unlinked.image, "Enable constrained proportions");

            style = new GUIStyle(Styles.iconButton)
            {
                name = null,
                alignment = TextAnchor.MiddleCenter,
            };

            activeStyle = new GUIStyle(style)
            {
                normal =
                {
                    background = Resources.CreateSinglePixelTexture( 110, 204, 204, 77)
                }
            };
        }

        private static void InitTarget(Transform transform)
        {
            target = transform;
            rotation = target.rotation.eulerAngles;
            position = target.position;
            scale =  target.lossyScale;
            
            inited = true;
        }

        private static bool Validate(Object target)
        {
            if (!Prefs.componentExtraHeaderButtons || !Prefs.transformInspectorGlobalValues) return false;
            if (!(target is Transform)) return false;
            if (target is RectTransform) return false;
            return true;
        }
    }
}