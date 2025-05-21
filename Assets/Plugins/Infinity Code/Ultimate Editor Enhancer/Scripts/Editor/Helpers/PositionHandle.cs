/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class PositionHandle
    {
        internal static int xAxisMoveHandleHash = "xAxisFreeMoveHandleHash".GetHashCode();
        internal static int yAxisMoveHandleHash = "yAxisFreeMoveHandleHash".GetHashCode();
        internal static int zAxisMoveHandleHash = "zAxisFreeMoveHandleHash".GetHashCode();
        internal static int xzAxisMoveHandleHash = "xzAxisFreeMoveHandleHash".GetHashCode();
        internal static int xyAxisMoveHandleHash = "xyAxisFreeMoveHandleHash".GetHashCode();
        internal static int yzAxisMoveHandleHash = "yzAxisFreeMoveHandleHash".GetHashCode();

        private static float[] cameraViewLerp = new float[6];
        private static readonly float cameraViewLerpStart1 = Mathf.Cos(5f * (float)Math.PI / 36f);
        private static readonly float cameraViewLerpEnd1 = Mathf.Cos(0.2617994f);
        private static readonly float cameraViewLerpStart2 = Mathf.Cos(2.96706f);
        private static readonly float cameraViewLerpEnd2 = Mathf.Cos(3.054326f);
        private static Vector3[] axisVector = {
            Vector3.right,
            Vector3.up,
            Vector3.forward
        };

        private static int[] nextIndex = {
            1,
            2,
            0
        };

        private static Color[] axisColor = {
            new Color(0.8588235f, 0.2431373f, 0.1137255f, 0.93f), 
            new Color(0.6039216f, 0.9529412f, 0.282353f, 0.93f), 
            new Color(0.227451f, 0.4784314f, 0.972549f, 0.93f)
        };

        internal static Color staticColor = new Color(0.5f, 0.5f, 0.5f, 0.0f);

        private static Vector3 planarHandlesOctant = Vector3.one;

        private static Vector3[] verts = {
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero
        };

        private static int[] axisDrawOrder = {
            0,
            1,
            2
        };

        private static Vector3 axisHandlesOctant = Vector3.one;
        internal static float staticBlend = 0.6f;

        private static string[] axisNames = {
            "xAxis",
            "yAxis",
            "zAxis"
        };

        internal static Color disabledHandleColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        private static int[] prevPlaneIndex = {
            5,
            3,
            4
        };

        internal static void ArrowHandleCap(
            int controlID,
            Vector3 position,
            Quaternion rotation,
            float size,
            EventType eventType,
            Vector3 coneOffset)
        {
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    Vector3 vector3 = rotation * Vector3.forward;
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToLine(position, position + (vector3 + coneOffset) * (size * 0.9f)));
#if UNITY_2020_3_OR_NEWER
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToCone(position + (vector3 + coneOffset) * size, rotation, size * 0.2f));
#else
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToCircle(position + (vector3 + coneOffset) * size, size * 0.2f));
#endif
                    break;
                case EventType.Repaint:
#if UNITY_2020_3_OR_NEWER
                    Vector3 rhs = rotation * Vector3.forward;
                    float lineThickness = Handles.lineThickness;
                    float size1 = size * 0.2f;
                    if (IsHovering(controlID))
                    {
                        lineThickness += 1;
                        size1 *= 1.05f;
                    }
                    Camera current = Camera.current;
                    bool flag = Vector3.Dot(current != null ? current.transform.forward : -rhs, rhs) < 0.0;
                    Vector3 position1 = position + (rhs + coneOffset) * size;
                    Vector3 p2 = position + (rhs + coneOffset) * (size * 0.9f);
                    if (flag)
                    {
                        Handles.DrawLine(position, p2, lineThickness);
                        Handles.ConeHandleCap(controlID, position1, rotation, size1, eventType);
                        break;
                    }
                    Handles.ConeHandleCap(controlID, position1, rotation, size1, eventType);
                    Handles.DrawLine(position, p2, lineThickness);
#else
                    Vector3 forward = rotation * Vector3.forward;
                    Handles.ConeHandleCap(controlID, position + (forward + coneOffset) * size, Quaternion.LookRotation(forward), size * 0.2f, eventType);
                    Handles.DrawLine(position, position + (forward + coneOffset) * size * 0.9f);
#endif
                    break;
            }
        }

        private static void CalcDrawOrder(Vector3 viewDir, int[] ordering)
        {
            ordering[0] = 0;
            ordering[1] = 1;
            ordering[2] = 2;
            if (viewDir.y > (double)viewDir.x) Swap(ref viewDir, ordering, 1, 0);
            if (viewDir.z > (double)viewDir.y) Swap(ref viewDir, ordering, 2, 1);
            if (viewDir.y <= (double)viewDir.x) return;
            Swap(ref viewDir, ordering, 1, 0);
        }

        private static Vector3 DoPlanarHandle(
            int id,
            int planePrimaryAxis,
            Vector3 position,
            Quaternion rotation,
            float handleSize,
            float cameraLerp,
            Vector3 viewVectorDrawSpace)
        {
            int index2 = (planePrimaryAxis + 1) % 3;
            int axis = (planePrimaryAxis + 2) % 3;
            Color color = Handles.color;
            
            float alpha = 0.8f;
            if (GUIUtility.hotControl == id) Handles.color = Handles.selectedColor;
            else
            {
                Handles.color = GetFadedAxisColor(!GUI.enabled ? staticColor : axisColor[axis], cameraLerp, id);
                alpha = !IsHovering(id) ? 0.1f : 0.4f;
            }

            Handles.color = ToActiveColorSpace(Handles.color);
            if (GUIUtility.hotControl == 0)
            {
                planarHandlesOctant[planePrimaryAxis] = viewVectorDrawSpace[planePrimaryAxis] > 0.00999999977648258 ? -1f : 1f;
                planarHandlesOctant[index2] = viewVectorDrawSpace[index2] > 0.00999999977648258 ? -1f : 1f;
            }
            Vector3 b = planarHandlesOctant;
            b[axis] = 0.0f;
            b = rotation * (b * handleSize * 0.5f);
            Vector3 slideDir1 = Vector3.zero;
            Vector3 zero1 = Vector3.zero;
            Vector3 zero2 = Vector3.zero;
            slideDir1[planePrimaryAxis] = 1f;
            zero1[index2] = 1f;
            zero2[axis] = 1f;
            slideDir1 = rotation * slideDir1;
            Vector3 slideDir2 = rotation * zero1;
            Vector3 handleDir = rotation * zero2;
            verts[0] = position + b + (slideDir1 + slideDir2) * handleSize * 0.5f;
            verts[1] = position + b + (-slideDir1 + slideDir2) * handleSize * 0.5f;
            verts[2] = position + b + (-slideDir1 - slideDir2) * handleSize * 0.5f;
            verts[3] = position + b + (slideDir1 - slideDir2) * handleSize * 0.5f;
            Color faceColor = new Color(Handles.color.r, Handles.color.g, Handles.color.b, Handles.color.a * alpha);
            Handles.DrawSolidRectangleWithOutline(verts, faceColor, Color.clear);
            Handles.CapFunction capFunction = Handles.RectangleHandleCap;
            Vector2 snap;
            if (!SnapHelper.enabled)
            {
                Vector3 move = EditorSnapSettings.move;
                double num3 = move[planePrimaryAxis];
                move = EditorSnapSettings.move;
                double num4 = move[index2];
                snap = new Vector2((float)num3, (float)num4);
            }
            else snap = Vector2.zero;
            position = Handles.Slider2D(id, position, b, handleDir, slideDir1, slideDir2, handleSize * 0.5f, capFunction, snap, false);
            Handles.color = color;
            return position;
        }

        private static void DoPositionHandle_ArrowCap(
            int controlId,
            Vector3 position,
            Quaternion rotation,
            float size,
            EventType eventType)
        {
            ArrowHandleCap(controlId, position, rotation, size, eventType, Vector3.zero);
        }

        private static bool ShouldShow(int axis)
        {
            return (127 & (1 << axis)) > 0;
        }

        public static Vector3 DoPositionHandle(Ids ids, Vector3 position, Quaternion rotation)
        {
            Color color = Handles.color;

            Camera current = Camera.current;
            Matrix4x4 matrix = (Handles.matrix * Matrix4x4.TRS(position, rotation, Vector3.one)).inverse;
            Vector3 cameraViewFrom = current.orthographic ? matrix.MultiplyVector(current.transform.forward).normalized : matrix.MultiplyVector(position - current.transform.position).normalized;

            float handleSize = HandleUtility.GetHandleSize(position);
            for (int axis = 0; axis < 3; ++axis)
            {
                float v = 0;
                if (ids[axis] == GUIUtility.hotControl)
                {
                    float num = Vector3.Dot(cameraViewFrom, axisVector[axis]);
                    v = Mathf.Max(Mathf.InverseLerp(cameraViewLerpStart1, cameraViewLerpEnd1, num), Mathf.InverseLerp(cameraViewLerpStart2, cameraViewLerpEnd2, num));
                }
                cameraViewLerp[axis] = v;
            }
            for (int index = 0; index < 3; ++index) cameraViewLerp[3 + index] = Mathf.Max(cameraViewLerp[index], cameraViewLerp[(index + 1) % 3]);
            bool isAnyAxis = ids.Has(GUIUtility.hotControl);
            Vector3 vector3_3 = Vector3.one * 0.25f;
            for (int planePrimaryAxis = 0; planePrimaryAxis < 3; ++planePrimaryAxis)
            {
                if (ShouldShow(3 + planePrimaryAxis) && (!isAnyAxis || ids[3 + planePrimaryAxis] == GUIUtility.hotControl))
                {
                    float cameraLerp = isAnyAxis ? 0.0f : cameraViewLerp[3 + planePrimaryAxis];
                    if (cameraLerp <= 0.600000023841858)
                    {
                        float num = Mathf.Max(vector3_3[planePrimaryAxis], vector3_3[nextIndex[planePrimaryAxis]]);
                        position = DoPlanarHandle(ids[3 + planePrimaryAxis], planePrimaryAxis, position, rotation, handleSize * num, cameraLerp, cameraViewFrom);
                    }
                }
            }
            CalcDrawOrder(cameraViewFrom, axisDrawOrder);

            bool guiEnabled = !GUI.enabled;

            for (int index = 0; index < 3; ++index)
            {
                int axis = axisDrawOrder[index];
                if (!ShouldShow(axis)) continue;

                if (GUIUtility.hotControl == 0) axisHandlesOctant[axis] = 1f;
                bool isCurrentAxis = isAnyAxis && ids[axis] == GUIUtility.hotControl;
                Color colorByAxis = axisColor[axis];
                Handles.color = guiEnabled ? Color.Lerp(colorByAxis, staticColor, staticBlend) : colorByAxis;
                GUI.SetNextControlName(axisNames[axis]);
                float fade = isCurrentAxis ? 0.0f : cameraViewLerp[axis];
                if (fade > 0.600000023841858) continue;

                Handles.color = GetFadedAxisColor(Handles.color, fade, ids[axis]);
                Vector3 aVector = axisVector[axis];
                Vector3 vector3_4 = rotation * aVector;
                Vector3 direction = vector3_4 * axisHandlesOctant[axis];
                if (isAnyAxis)
                {
                    if (!isCurrentAxis) Handles.color = disabledHandleColor;
                    if ((ids[prevPlaneIndex[axis]] == GUIUtility.hotControl || ids[axis + 3] == GUIUtility.hotControl))
                    {
                        Handles.color = Handles.selectedColor;
                    }
                }
                Handles.color = ToActiveColorSpace(Handles.color);
                int id = ids[axis];
                double size = handleSize;
                Handles.CapFunction capFunction = DoPositionHandle_ArrowCap;
                double snap = 0;
                if (!SnapHelper.enabled) snap = EditorSnapSettings.move[axis];
                position = Handles.Slider(id, position, Vector3.zero, direction, (float)size, capFunction, (float)snap);
            }
            if (SnapHelper.enabled) position = SnapHelper.Snap(position);
            Handles.color = color;
            return position;
        }

        internal static Color GetFadedAxisColor(Color col, float fade, int id)
        {
            if (id != 0 && id == GUIUtility.hotControl || id == HandleUtility.nearestControl) fade = 0.0f;
            col = Color.Lerp(col, Color.clear, fade);
            return col;
        }

        internal static bool IsHovering(int cid)
        {
#if UNITY_2020_3_OR_NEWER
            return cid == HandleUtility.nearestControl && GUIUtility.hotControl == 0 && !UnityEditor.Tools.viewToolActive;
#else
            return cid == HandleUtility.nearestControl && GUIUtility.hotControl == 0;
#endif
        }

        internal static Color ToActiveColorSpace(Color color)
        {
            return QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
        }

        private static void Swap(ref Vector3 v, int[] indices, int a, int b)
        {
            float num = v[a];
            v[a] = v[b];
            v[b] = num;
            int index = indices[a];
            indices[a] = indices[b];
            indices[b] = index;
        }


        public struct Ids
        {
            public readonly int x;
            public readonly int y;
            public readonly int z;
            public readonly int xy;
            public readonly int yz;
            public readonly int xz;

            public static Ids @default
            {
                get
                {
                    return new Ids(
                        GUIUtility.GetControlID(xAxisMoveHandleHash, FocusType.Passive),
                        GUIUtility.GetControlID(yAxisMoveHandleHash, FocusType.Passive),
                        GUIUtility.GetControlID(zAxisMoveHandleHash, FocusType.Passive),
                        GUIUtility.GetControlID(xyAxisMoveHandleHash, FocusType.Passive),
                        GUIUtility.GetControlID(xzAxisMoveHandleHash, FocusType.Passive),
                        GUIUtility.GetControlID(yzAxisMoveHandleHash, FocusType.Passive));
                }
            }

            public int this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0:
                            return x;
                        case 1:
                            return y;
                        case 2:
                            return z;
                        case 3:
                            return xy;
                        case 4:
                            return yz;
                        case 5:
                            return xz;
                        default:
                            return -1;
                    }
                }
            }

            public bool Has(int id)
            {
                return x == id || y == id || z == id || xy == id || yz == id || xz == id;
            }

            private Ids(int x, int y, int z, int xy, int xz, int yz)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.xy = xy;
                this.yz = yz;
                this.xz = xz;
            }
        }
    }
}