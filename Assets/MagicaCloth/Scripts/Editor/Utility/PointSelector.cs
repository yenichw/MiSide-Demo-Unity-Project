// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#pragma warning disable 0436

namespace MagicaCloth
{
    /// <summary>
    /// ????????????????????
    /// ???? PointSelectorTest / PointSelectorTestInspector ???
    /// </summary>
    public class PointSelector
    {
        /// <summary>
        /// ????
        /// </summary>
        public static bool EditEnable { get; private set; }
        private static int EditInstanceId = 0;
        private static UnityEngine.Object EditObject = null;

        //=========================================================================================
        /// <summary>
        /// Reload Domain ??
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            EditEnable = false;
            EditInstanceId = 0;
            EditObject = null;
        }

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// </summary>
        private class PointType
        {
            /// <summary>
            /// ???
            /// </summary>
            public string label;

            /// <summary>
            /// ?????
            /// </summary>
            public Color col;

            /// <summary>
            /// ???
            /// </summary>
            public int value;
        }

        /// <summary>
        /// ??????????
        /// </summary>
        List<PointType> pointTypeList = new List<PointType>();

        /// <summary>
        /// ?????????
        /// </summary>
        Dictionary<int, PointType> value2typeDict = new Dictionary<int, PointType>(); // ???????????

        /// <summary>
        /// ???????
        /// </summary>
        public class PointData
        {
            /// <summary>
            /// ??????(????)
            /// </summary>
            public Vector3 pos;

            /// <summary>
            /// ?????????
            /// </summary>
            public int index;

            /// <summary>
            /// ????
            /// </summary>
            public int value;

            /// <summary>
            /// Z??(????)
            /// </summary>
            public float distance;
        }

        /// <summary>
        /// ??????????
        /// </summary>
        List<PointData> pointList = new List<PointData>();


        /// <summary>
        /// ???????
        /// </summary>
        float pointSize = 0.01f;

        /// <summary>
        /// ???????
        /// </summary>
        bool selectNearest = false;

        /// <summary>
        /// ??????
        /// </summary>
        int selectPointType = 0;

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// </summary>
        public void EnableEdit()
        {
            // ???????????
            //Clear();
            //            SceneView.duringSceneGui += OnSceneView;
            //            SceneView.RepaintAll();
        }

        /// <summary>
        /// ?????????
        /// </summary>
        public void DisableEdit(UnityEngine.Object obj)
        {
            EndEdit(obj);
            //Clear();
            //            SceneView.duringSceneGui -= OnSceneView;
            //            SceneView.RepaintAll();
        }

        /// <summary>
        /// ????
        /// </summary>
        void StartEdit(UnityEngine.Object obj)
        {
            if (EditEnable)
                return;
            Clear();
            EditEnable = true;
            EditInstanceId = obj.GetInstanceID();
            EditObject = obj;

            pointSize = EditorPrefs.GetFloat("PointSelector_PointSize", 0.01f);
            selectNearest = EditorPrefs.GetBool("PointSelector_SelectNearest", false);

            SceneView.duringSceneGui += OnSceneView;
            SceneView.RepaintAll();
        }

        /// <summary>
        /// ????
        /// </summary>
        void EndEdit(UnityEngine.Object obj)
        {
            if (EditEnable == false)
                return;
            if (IsEdit(obj) == false)
                return;
            EditEnable = false;
            EditInstanceId = 0;
            EditObject = null;
            Clear();

            EditorPrefs.SetFloat("PointSelector_PointSize", pointSize);
            EditorPrefs.SetBool("PointSelector_SelectNearest", selectNearest);

            SceneView.duringSceneGui -= OnSceneView;
            SceneView.RepaintAll();
        }

        public bool IsEdit(UnityEngine.Object obj)
        {
            return EditEnable && EditInstanceId == obj.GetInstanceID();
        }

        //=========================================================================================
        /// <summary>
        /// ?????
        /// </summary>
        void Clear()
        {
            // ?????????
            pointTypeList.Clear();
            value2typeDict.Clear();
            selectPointType = 0;

            // ??????
            pointList.Clear();
        }

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="label"></param>
        /// <param name="col"></param>
        /// <param name="value"></param>
        public void AddPointType(string label, Color col, int value)
        {
            if (value2typeDict.ContainsKey(value))
                return;

            PointType pt = new PointType();
            pt.label = label;
            pt.col = col;
            pt.value = value;
            pointTypeList.Add(pt);

            // ???????
            value2typeDict[value] = pt;
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="normal"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void AddPoint(Vector3 pos, int index, int value)
        {
            PointData p = new PointData();
            p.pos = pos;
            p.index = index;
            p.value = value;
            pointList.Add(p);
        }

        /// <summary>
        /// ?????????????
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        public List<PointData> GetPointList()
        {
            return pointList;
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Color GetPointColor(int value)
        {
            PointType pt;
            if (value2typeDict.TryGetValue(value, out pt))
            {
                return pt.col;
            }
            return Color.black;
        }

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="sceneView"></param>
        void OnSceneView(SceneView sceneView)
        {
            if (EditorApplication.isPlaying)
                return;

            if (EditEnable == false)
                return;

            if (EditObject == null)
            {
                return;
            }

            // ??????·???
            Camera cam = SceneView.currentDrawingSceneView.camera;
            Vector3 campos = cam.transform.position;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            Vector3 spos = ray.origin;
            Vector3 epos = spos + ray.direction * 1000.0f;

            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            // ?????
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt)
            {
                hitTest(spos, epos, pointSize * 0.5f);

                // ?????????????????????????????????
                GUIUtility.hotControl = controlId;
                Event.current.Use(); // ?
            }
            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0 && !Event.current.alt)
            {
                hitTest(spos, epos, pointSize * 0.5f);

                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Event.current.alt)
            {
                // ??????UP??????????????????
                GUIUtility.hotControl = 0;
                Event.current.Use(); // ?
            }

            if (Event.current.type == EventType.Repaint)
            {
                // Z???
                ZSort(campos, cam.transform.forward);

                if (selectNearest == false)
                {
                    // Z test off
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                }
                else
                {
                    // Z test on
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                }

                // ??????
                int pcnt = pointList.Count;
                for (int i = 0; i < pcnt; i++)
                {
                    var p = pointList[i];
                    Handles.color = GetPointColor(p.value);
                    Handles.SphereHandleCap(0, p.pos, Quaternion.identity, pointSize, EventType.Repaint);
                }
            }

            // ?????????????????
            if (GUI.changed)
            {
                // ????????????
                HandleUtility.Repaint();
            }
        }

        /// <summary>
        /// ???????????????Z?????
        /// </summary>
        void ZSort(Vector3 campos, Vector3 camdir)
        {
            // ???????
            int pcnt = pointList.Count;
            for (int i = 0; i < pcnt; i++)
            {
                var p = pointList[i];
                p.distance = Vector3.Distance(p.pos, campos);
            }

            // ???(??????????!)
            pointList.Sort((a, b) => a.distance > b.distance ? -1 : 1);
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="spos"></param>
        /// <param name="epos"></param>
        /// <param name="hitRadius"></param>
        /// <returns></returns>
        bool hitTest(Vector3 spos, Vector3 epos, float hitRadius)
        {
            // ????????
            bool change = false;
            int pcnt = pointList.Count;
            for (int i = pcnt - 1; i >= 0; i--)
            {
                var p = pointList[i];

                // ??????????????
                float sqlen = SqDistPointSegment(spos, epos, p.pos);

                if (sqlen <= hitRadius * hitRadius)
                {
                    // ???!
                    // ????
                    p.value = pointTypeList[selectPointType].value;
                    change = true;

                    // ??????????????
                    if (selectNearest)
                        break;
                }
            }

            return change;
        }

        /// <summary>
        /// ¦?C???ab???????????
        /// ???????????????????????? P.130
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        float SqDistPointSegment(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 bc = c - b;
            float e = Vector3.Dot(ac, ab);

            // C?ab??????????????
            if (e <= 0)
                return Vector3.Dot(ac, ac);
            float f = Vector3.Dot(ab, ab);
            if (e >= f)
                return Vector3.Dot(bc, bc);

            // C?ab????????????
            return Vector3.Dot(ac, ac) - e * e / f;
        }

        //=========================================================================================
        /// <summary>
        /// ????GUI??
        /// </summary>
        public void DrawInspectorGUI(
            UnityEngine.Object obj,
            System.Action<PointSelector> startAction,
            System.Action<PointSelector> endAction
            )
        {
            if (EditorApplication.isPlaying)
                return;

            EditorGUILayout.Space();

            if (EditEnable && IsEdit(obj) == false)
                return;

            bool change = false;

            using (var verticalScope = new GUILayout.VerticalScope(GUI.skin.box))
            {
                GUI.backgroundColor = Color.cyan;
                if (EditEnable == false && GUILayout.Button("Start Point Selection"))
                {
                    GUI.backgroundColor = Color.white;
                    StartEdit(obj);
                    if (startAction != null)
                        startAction(this);
                    change = true;
                }
                else if (EditEnable && GUILayout.Button("End Point Selection"))
                {
                    GUI.backgroundColor = Color.white;
                    if (endAction != null)
                        endAction(this);
                    EndEdit(obj);
                    change = true;
                }

                GUI.backgroundColor = Color.white;
                if (EditEnable && GUILayout.Button("Cancel Point Selection"))
                {
                    // ??????????
                    EndEdit(obj);
                    change = true;
                }

                if (EditEnable)
                {
                    EditorGUILayout.Space();

                    // ????????????
                    float psize = EditorGUILayout.Slider("Point Size", pointSize, 0.001f, 0.1f);
                    if (psize != pointSize)
                    {
                        pointSize = psize;
                        change = true;
                    }
                    EditorGUILayout.Space();

                    // ???????
                    EditorGUILayout.Space();
                    var oldSelectNearest = selectNearest;
                    selectNearest = EditorGUILayout.ToggleLeft("Z Test On & Select Near Point Only", selectNearest);
                    if (oldSelectNearest != selectNearest)
                        change = true;
                    EditorGUILayout.Space();

                    // ?????????????GUILayout.Toolbar()?????
                    EditorGUILayout.LabelField("Point Type");
                    int tcnt = pointTypeList.Count;
                    Color bcol = GUI.backgroundColor;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        int nowtype = selectPointType;
                        for (int i = 0; i < tcnt; i++)
                        {
                            // ???
                            GUI.backgroundColor = pointTypeList[i].col;
                            bool ret = GUILayout.Toggle(i == nowtype, pointTypeList[i].label, EditorStyles.miniButtonLeft);
                            if (ret)
                            {
                                nowtype = i;
                            }
                        }
                        if (nowtype != selectPointType)
                        {
                            selectPointType = nowtype;
                        }
                    }
                    GUI.backgroundColor = bcol;

                    // ??????
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Fill"))
                    {
                        foreach (var p in pointList)
                        {
                            p.value = pointTypeList[selectPointType].value;
                        }
                        change = true;
                    }
                }
            }

            // ?????
            if (change)
            {
                SceneView.RepaintAll();
            }
        }
    }
}
