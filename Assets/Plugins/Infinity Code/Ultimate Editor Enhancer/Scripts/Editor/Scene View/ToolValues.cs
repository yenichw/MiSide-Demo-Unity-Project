/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.Tools;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class ToolValues
    {
        public const string StyleID = "sv_label_0";

        public static Action<Rect> OnDrawLate;

        public static Vector3 lastScreenPosition;
        public static bool isBelowHandle;
        public static GUIStyle style;

        private static Transform firstTransform;
        private static bool samePositionX;
        private static bool samePositionY;
        private static bool samePositionZ;
        private static bool sameRotationX;
        private static bool sameRotationY;
        private static bool sameRotationZ;
        private static bool sameScaleX;
        private static bool sameScaleY;
        private static bool sameScaleZ;
        private static string label;

        static ToolValues()
        {
            SceneViewManager.AddListener(OnSceneViewGUI, SceneViewOrder.Normal, true);
            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged();
        }

        private static void AppendPosition(StringBuilder builder)
        {
            Vector3 pos;

            RectTransform rt = firstTransform as RectTransform;

            if (rt != null)
            {
                builder.Append("Anchored Position (");
                pos = rt.anchoredPosition3D;
            }
            else
            {
                builder.Append("Position (");
                pos = firstTransform.localPosition;
            }

            builder.Append(samePositionX ? pos.x.ToString("F2", Culture.numberFormat) : "---");
            builder.Append(", ");
            builder.Append(samePositionY ? pos.y.ToString("F2", Culture.numberFormat) : "---");
            builder.Append(", ");
            builder.Append(samePositionZ ? pos.z.ToString("F2", Culture.numberFormat) : "---");
            builder.Append(")");
        }

        private static void AppendRotation(StringBuilder builder)
        {
            builder.Append("Rotation (");
            builder.Append(sameRotationX ? firstTransform.eulerAngles.x.ToString("F2", Culture.numberFormat) : "---");
            builder.Append(", ");
            builder.Append(sameRotationY ? firstTransform.eulerAngles.y.ToString("F2", Culture.numberFormat) : "---");
            builder.Append(", ");
            builder.Append(sameRotationZ ? firstTransform.eulerAngles.z.ToString("F2", Culture.numberFormat) : "---");
            builder.Append(")");
        }

        private static void AppendScale(StringBuilder builder)
        {
            builder.Append("Scale (");
            builder.Append(sameScaleX ? firstTransform.localScale.x.ToString("F2", Culture.numberFormat) : "---");
            builder.Append(", ");
            builder.Append(sameScaleY ? firstTransform.localScale.y.ToString("F2", Culture.numberFormat) : "---");
            builder.Append(", ");
            builder.Append(sameScaleZ ? firstTransform.localScale.z.ToString("F2", Culture.numberFormat) : "---");
            builder.Append(")");
        }

        private static void AppendSize(RectTransform rt, StringBuilder builder)
        {
            builder.Append("Size (");
            builder.Append(rt.sizeDelta.x.ToString("F2", Culture.numberFormat));
            builder.Append(", ");
            builder.Append(rt.sizeDelta.y.ToString("F2", Culture.numberFormat));
            builder.Append(")");
        }

        private static void DrawLabel(SceneView sceneView, string text)
        {
            if (style == null)
            {
                style = new GUIStyle(StyleID)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                    fixedHeight = 0,
                    border = new RectOffset(8, 8, 8, 8)
                };
            }

            GUIContent content = TempContent.Get(text);
            float pixelPerPoint = EditorGUIUtility.pixelsPerPoint;
            Vector2 size = style.CalcSize(content);

            Handles.BeginGUI();
            Vector3 screenPoint = sceneView.camera.WorldToScreenPoint(UnityEditor.Tools.handlePosition) / pixelPerPoint;
            if (screenPoint.y > size.y + 150 / pixelPerPoint)
            {
                screenPoint.y -= size.y + 50 / pixelPerPoint;
                isBelowHandle = true;
            }
            else
            {
                screenPoint.y += size.y + 150 / pixelPerPoint;
                isBelowHandle = false;
            }

            lastScreenPosition = screenPoint;

            Rect rect = new Rect(screenPoint.x - size.x / 2, Screen.height / pixelPerPoint - screenPoint.y - size.y / 2, size.x, size.y);
            Event e = Event.current;

            if (e.type == EventType.Repaint) GUI.Label(rect, content, style);
            else if (e.type == EventType.MouseDown)
            {
                if (rect.Contains(e.mousePosition))
                {
                    Transform[] transforms = Selection.gameObjects.Select(g => g.GetComponent<Transform>()).ToArray();
                    TransformEditorWindow.ShowPopup(transforms);

                    e.Use();
                    SceneViewManager.BlockMouseUp();
                }
            }

            if (OnDrawLate != null) OnDrawLate(rect);

            Handles.EndGUI();
        }

        private static void InitLabel()
        {
            Event e = Event.current;
            label = null;

            if (!Prefs.showToolValues) return;
            if (firstTransform == null) return;
            if (TransformEditorWindow.instance != null) return;
            if (UnityEditor.Tools.hidden || UnityEditor.Tools.current == Tool.View) return;
            if (e.modifiers != Prefs.toolValuesModifiers) return;

            StringBuilder builder = StaticStringBuilder.Start();

            Tool tool = UnityEditor.Tools.current;
#if UNITY_2020_2_OR_NEWER
            if (tool == Tool.Move || ToolManager.activeToolType == typeof(DuplicateTool))
#else
            if (tool == Tool.Move || EditorTools.activeToolType == typeof(DuplicateTool))
#endif
            {
                AppendPosition(builder);
                label = builder.ToString();
            }
            else if (tool == Tool.Rotate)
            {
                AppendRotation(builder);
                label = builder.ToString();
            }
            else if (tool == Tool.Scale)
            {
                AppendScale(builder);
                label = builder.ToString();
            }
            else if (tool == Tool.Rect)
            {
                AppendPosition(builder);
                builder.Append("\n");

                RectTransform rt = firstTransform as RectTransform;
                if (rt != null) AppendSize(rt, builder);
                else AppendScale(builder);

                label = builder.ToString();
            }
            else if (tool == Tool.Transform)
            {
                AppendPosition(builder);
                builder.Append("\n");
                AppendRotation(builder);
                builder.Append("\n");
                AppendScale(builder);
                label = builder.ToString();
            }
        }

        private static void OnSceneViewGUI(SceneView sceneView)
        {
            try
            {
                if (Event.current.type == EventType.Layout) InitLabel();
                if (!string.IsNullOrEmpty(label)) DrawLabel(sceneView, label);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void OnSelectionChanged()
        {
            firstTransform = null;
            if (Selection.gameObjects.Length == 0) return;

            samePositionX = samePositionY = samePositionZ = sameRotationX = sameRotationY = sameRotationZ = sameScaleX = sameScaleY = sameScaleZ = true;

            int[] instanceIDs = Selection.instanceIDs;

            for (int i = 0; i < instanceIDs.Length; i++)
            {
                GameObject go = EditorUtility.InstanceIDToObject(instanceIDs[i]) as GameObject;
                if (go == null || go.scene.name == null) continue;

                if (firstTransform == null)
                {
                    firstTransform = go.transform;
                    continue;
                }

                Transform t = go.transform;
                Vector3 p1 = t.localPosition;
                Vector3 p2 = firstTransform.localPosition;
                Vector3 r1 = t.eulerAngles;
                Vector3 r2 = firstTransform.eulerAngles;
                Vector3 s1 = t.localScale;
                Vector3 s2 = firstTransform.localScale;
                if (samePositionX && Math.Abs(p1.x - p2.x) > float.Epsilon) samePositionX = false;
                if (samePositionY && Math.Abs(p1.y - p2.y) > float.Epsilon) samePositionY = false;
                if (samePositionZ && Math.Abs(p1.z - p2.z) > float.Epsilon) samePositionZ = false;
                if (sameRotationX && Math.Abs(r1.x - r2.x) > float.Epsilon) sameRotationX = false;
                if (sameRotationY && Math.Abs(r1.y - r2.y) > float.Epsilon) sameRotationY = false;
                if (sameRotationZ && Math.Abs(r1.z - r2.z) > float.Epsilon) sameRotationZ = false;
                if (sameScaleX && Math.Abs(s1.x - s2.x) > float.Epsilon) sameScaleX = false;
                if (sameScaleY && Math.Abs(s1.y - s2.y) > float.Epsilon) sameScaleY = false;
                if (sameScaleZ && Math.Abs(s1.z - s2.z) > float.Epsilon) sameScaleZ = false;
            }
        }
    }
}