/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class SelectionSize
    {
        public const string StyleID = "sv_label_6";
        private static GUIStyle style;
        private static bool forcedShow;
        private static Point[] points;

        static SelectionSize()
        {
            SceneViewManager.AddListener(OnSceneViewGUI, SceneViewOrder.Normal, true);
        }

        private static void OnSceneViewGUI(SceneView sceneView)
        {
            if (!Event.current.capsLock && !forcedShow) return;
            if (!SelectionBoundsManager.hasBounds) return;
            GameObject first = SelectionBoundsManager.firstGameObject;
            if (first == null || first.scene.name == null) return;

            if (style == null)
            {
                style = new GUIStyle(StyleID)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = false,
                    fixedHeight = 16,
                    normal =
                    {
                        textColor = Color.white
                    },
                    padding = new RectOffset(),
                    stretchWidth = true
                };
            }

            Color color = Handles.color;
            Handles.color = Color.red;

            Bounds bounds = SelectionBoundsManager.bounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            Vector3 p1 = min;
            Vector3 p2 = new Vector3(max.x, min.y, min.z);
            Vector3 p3 = new Vector3(min.x, max.y, min.z);
            Vector3 p4 = new Vector3(min.x, min.y, max.z);
            Vector3 p5 = new Vector3(max.x, max.y, min.z);
            Vector3 p6 = new Vector3(max.x, min.y, max.z);
            Vector3 p7 = new Vector3(min.x, max.y, max.z);
            Vector3 p8 = max;

            Handles.DrawLine(p1, p2);
            Handles.DrawLine(p1, p3);
            Handles.DrawLine(p2, p5);
            Handles.DrawLine(p3, p5);

            Handles.DrawLine(p4, p6);
            Handles.DrawLine(p4, p7);
            Handles.DrawLine(p6, p8);
            Handles.DrawLine(p7, p8);

            Handles.DrawLine(p1, p4);
            Handles.DrawLine(p2, p6);
            Handles.DrawLine(p3, p7);
            Handles.DrawLine(p5, p8);

            if (points == null)
            {
                points = new Point[12];
                for (int i = 0; i < 12; i++) points[i] = new Point();
            }

            if (Event.current.type == EventType.Layout)
            {
                Vector3 d1 = (p1 + p2) / 2;
                Vector3 d2 = (p3 + p5) / 2;
                Vector3 d3 = (p7 + p8) / 2;
                Vector3 d4 = (p4 + p6) / 2;

                Vector3 d5 = (p1 + p3) / 2;
                Vector3 d6 = (p2 + p5) / 2;
                Vector3 d7 = (p4 + p7) / 2;
                Vector3 d8 = (p6 + p8) / 2;

                Vector3 d9 = (p1 + p4) / 2;
                Vector3 d10 = (p2 + p6) / 2;
                Vector3 d11 = (p3 + p7) / 2;
                Vector3 d12 = (p5 + p8) / 2;

                string m1 = (p1 - p2).magnitude.ToString("F2");
                string m2 = (p1 - p3).magnitude.ToString("F2");
                string m3 = (p1 - p4).magnitude.ToString("F2");

                points[0].label = points[1].label = points[2].label = points[3].label = m1;
                points[4].label = points[5].label = points[6].label = points[7].label = m2;
                points[8].label = points[9].label = points[10].label = points[11].label = m3;

                points[0].pos = d1;
                points[1].pos = d2;
                points[2].pos = d3;
                points[3].pos = d4;
                points[4].pos = d5;
                points[5].pos = d6;
                points[6].pos = d7;
                points[7].pos = d8;
                points[8].pos = d9;
                points[9].pos = d10;
                points[10].pos = d11;
                points[11].pos = d12;

                Vector3 camPos = sceneView.camera.transform.position;

                points[0].distance = (d1 - camPos).sqrMagnitude;
                points[1].distance = (d2 - camPos).sqrMagnitude;
                points[2].distance = (d3 - camPos).sqrMagnitude;
                points[3].distance = (d4 - camPos).sqrMagnitude;
                points[4].distance = (d5 - camPos).sqrMagnitude;
                points[5].distance = (d6 - camPos).sqrMagnitude;
                points[6].distance = (d7 - camPos).sqrMagnitude;
                points[7].distance = (d8 - camPos).sqrMagnitude;
                points[8].distance = (d9 - camPos).sqrMagnitude;
                points[9].distance = (d10 - camPos).sqrMagnitude;
                points[10].distance = (d11 - camPos).sqrMagnitude;
                points[11].distance = (d12 - camPos).sqrMagnitude;
            }

            if (SelectionBoundsManager.isRectTransform)
            {
                Handles.Label(points[0].pos, points[0].label, style);
                Handles.Label(points[1].pos, points[1].label, style);
                Handles.Label(points[4].pos, points[4].label, style);
                Handles.Label(points[5].pos, points[5].label, style);
            }
            else
            {
                foreach (Point p in points.OrderByDescending(p => p.distance))
                {
                    Handles.Label(p.pos, p.label, style);
                }
            }

            Handles.color = color;
        }

        public static void SetState(bool value)
        {
            forcedShow = value;
        }

        internal class Point
        {
            public Vector3 pos;
            public string label;
            public float distance;
        }
    }
}