// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// Deformer??????
    /// </summary>
    public class DeformerGizmoDrawer
    {
        //=========================================================================================
        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="editorMesh"></param>
        /// <param name="meshData">null?????????????</param>
        /// <param name="clothSelection"></param>
        /// <param name="size"></param>
        public static void DrawDeformerGizmo(
            IEditorMesh editorMesh,
            IEditorCloth editorCloth,
            float size = 0.005f
            )
        {
            if (ClothMonitorMenu.Monitor == null)
                return;
            if (ClothMonitorMenu.Monitor.UI.DrawDeformer == false)
                return;

            if (ClothMonitorMenu.Monitor.UI.DrawDeformerVertexPosition == false
                && ClothMonitorMenu.Monitor.UI.DrawDeformerTriangle == false
                && ClothMonitorMenu.Monitor.UI.DrawDeformerLine == false
                && ClothMonitorMenu.Monitor.UI.DrawDeformerVertexAxis == false
#if MAGICACLOTH_DEBUG
                && ClothMonitorMenu.Monitor.UI.DrawDeformerVertexNumber == false
                && ClothMonitorMenu.Monitor.UI.DrawDeformerTriangleNormal == false
#endif
                )
                return;

            // ??????????
            List<Vector3> posList;
            List<Vector3> norList;
            List<Vector3> tanList;
            int vcnt = editorMesh.GetEditorPositionNormalTangent(out posList, out norList, out tanList);
            if (posList.Count == 0)
                return;

            // ??????
            List<int> useList = null;
            List<int> selList = null;
            if (editorCloth != null)
            {
                useList = editorCloth.GetUseList();
                selList = editorCloth.GetSelectionList();
            }

            // ??
            DrawVertex(vcnt, posList, norList, tanList, useList, selList, size);

            // ???????
            DrawTriangle(editorMesh, posList, norList, tanList, useList);

            // ???
            DrawLine(editorMesh, posList, norList, tanList, useList);
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="editorMesh"></param>
        /// <param name="meshData"></param>
        /// <param name="clothSelection"></param>
        /// <param name="size"></param>
        static void DrawVertex(
            int vcnt,
            List<Vector3> posList,
            List<Vector3> norList,
            List<Vector3> tanList,
            List<int> useList,
            List<int> selList,
            float size
            )
        {
            bool position = ClothMonitorMenu.Monitor.UI.DrawDeformerVertexPosition;
            bool axis = ClothMonitorMenu.Monitor.UI.DrawDeformerVertexAxis;
#if MAGICACLOTH_DEBUG
            bool number = ClothMonitorMenu.Monitor.UI.DrawDeformerVertexNumber;
            if (position == false && axis == false && number == false)
                return;
#else
            if (position == false && axis == false)
                return;
#endif
#if MAGICACLOTH_DEBUG
            float radius = -1;
            Vector3 center = Vector3.zero;
            if (ClothMonitorMenu.Monitor.UI.DebugDrawDeformerVertexNumber >= 0)
            {
                int vindex = ClothMonitorMenu.Monitor.UI.DebugDrawDeformerVertexNumber;
                if (vindex >= 0 && vindex < vcnt)
                {
                    center = posList[vindex];
                    radius = 0.05f;
                }
            }
#endif
            bool hasNormal = norList.Count > 0;
            bool hasTangent = tanList.Count > 0;

            for (int i = 0; i < vcnt; i++)
            {
                // ??????
                if (useList != null && vcnt <= useList.Count && useList[i] == 0)
                    continue;

                Vector3 pos = posList[i];

                // ????
#if MAGICACLOTH_DEBUG
                // ??????
                if (radius > 0 && Vector3.Distance(pos, center) > radius)
                    continue;
#endif

                if (position)
                {
                    Gizmos.color = GetVertexColor(selList, i);
                    Gizmos.DrawSphere(pos, size);
                }

                if (axis && (hasNormal || hasTangent))
                {
                    const float axisSize = 0.03f;
                    if (hasNormal)
                    {
                        Vector3 nor = norList[i];
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(pos, pos + nor * axisSize);
                    }
                    if (hasTangent)
                    {
                        Vector3 tan = tanList[i];
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(pos, pos + tan * axisSize);
                    }
                    if (hasNormal && hasTangent)
                    {
                        Vector3 nor = norList[i];
                        Vector3 tan = tanList[i];
                        Vector3 bin = Vector3.Cross(tan, nor).normalized;
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(pos, pos + bin * axisSize);
                    }
                }

#if MAGICACLOTH_DEBUG
                if (number)
                {
                    Handles.Label(pos, i.ToString());
                }
#endif
            }
        }

        private static Color GetVertexColor(List<int> selList, int vindex)
        {
            if (selList == null || vindex >= selList.Count)
                return GizmoUtility.ColorDeformerPoint;
            if (selList[vindex] == SelectionData.Move)
                return Color.green;
            if (selList[vindex] == SelectionData.Fixed)
                return Color.red;

            return GizmoUtility.ColorDeformerPoint;
        }

        //=========================================================================================
        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="editorMesh"></param>
        static void DrawTriangle(
            IEditorMesh editorMesh,
            List<Vector3> posList,
            List<Vector3> norList,
            List<Vector3> tanList,
            List<int> useList
            )
        {
            bool drawTriangle = ClothMonitorMenu.Monitor.UI.DrawDeformerTriangle;
#if MAGICACLOTH_DEBUG
            bool drawNormal = ClothMonitorMenu.Monitor.UI.DrawDeformerTriangleNormal;
            bool drawNumber = ClothMonitorMenu.Monitor.UI.DrawDeformerTriangleNumber;
#else
            bool drawNormal = false;
            bool drawNumber = false;
#endif
            if (!drawTriangle && !drawNormal && !drawNumber)
                return;

            var triangles = editorMesh.GetEditorTriangleList();

            if (triangles == null || triangles.Count == 0)
                return;

            //Gizmos.color = GizmoUtility.ColorTriangle;
            int tcnt = triangles.Count / 3;

            // ????
            float radius = -1;
            Vector3 center = Vector3.zero;
#if MAGICACLOTH_DEBUG
            if (ClothMonitorMenu.Monitor.UI.DebugDrawDeformerTriangleNumber >= 0)
            {
                int tindex = ClothMonitorMenu.Monitor.UI.DebugDrawDeformerTriangleNumber;
                if (tindex >= 0 && tindex < tcnt)
                {
                    int index = tindex * 3;

                    int i0 = triangles[index];
                    int i1 = triangles[index + 1];
                    int i2 = triangles[index + 2];

                    Vector3 pos0 = posList[i0];
                    Vector3 pos1 = posList[i1];
                    Vector3 pos2 = posList[i2];
                    center = (pos0 + pos1 + pos2) / 3.0f;
                    radius = 0.05f;
                }
            }
#endif

            // ??
            for (int tindex = 0; tindex < tcnt; tindex++)
            {
                DrawTriangle1(tindex, triangles, posList, useList, center, radius, drawTriangle, drawNormal, drawNumber);
            }
        }

        /// <summary>
        /// ???????1???
        /// </summary>
        /// <param name="tindex"></param>
        /// <param name="triangles"></param>
        /// <param name="posList"></param>
        /// <param name="useList"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="drawTriangle"></param>
        /// <param name="drawNormal"></param>
        /// <param name="drawNumber"></param>
        static void DrawTriangle1(
            int tindex, List<int> triangles, List<Vector3> posList, List<int> useList,
            Vector3 center, float radius,
            bool drawTriangle, bool drawNormal, bool drawNumber
            )
        {
            int index = tindex * 3;

            int i0 = triangles[index];
            int i1 = triangles[index + 1];
            int i2 = triangles[index + 2];

            // ??????
            if (useList != null)
            {
                if (useList[i0] == 0 || useList[i1] == 0 || useList[i2] == 0)
                    return;
            }

            Vector3 pos0 = posList[i0];
            Vector3 pos1 = posList[i1];
            Vector3 pos2 = posList[i2];
            var pos = (pos0 + pos1 + pos2) / 3.0f;

            // ??????
            if (radius > 0 && Vector3.Distance(pos, center) > radius)
                return;

            if (drawTriangle)
            {
                Gizmos.color = GizmoUtility.ColorTriangle;
                Gizmos.DrawLine(pos0, pos1);
                Gizmos.DrawLine(pos0, pos2);
                Gizmos.DrawLine(pos1, pos2);
            }
            if (drawNormal)
            {
                var vn1 = pos1 - pos0;
                var vn2 = pos2 - pos0;
                var nor = Vector3.Cross(vn1, vn2).normalized;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(pos, pos + nor * 0.02f);
            }
            if (drawNumber)
            {
                Handles.Label(pos, tindex.ToString());
            }
        }

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="editorMesh"></param>
        static void DrawLine(
            IEditorMesh editorMesh,
            List<Vector3> posList,
            List<Vector3> norList,
            List<Vector3> tanList,
            List<int> useList
            )
        {
            if (ClothMonitorMenu.Monitor.UI.DrawDeformerLine == false)
                return;

            var lines = editorMesh.GetEditorLineList();
            if (lines == null || lines.Count == 0)
                return;

            Gizmos.color = GizmoUtility.ColorStructLine;
            int lcnt = lines.Count / 2;
            for (int i = 0; i < lcnt; i++)
            {
                int index = i * 2;

                int i0 = lines[index];
                int i1 = lines[index + 1];

                // ??????
                if (useList != null)
                {
                    if (i0 >= useList.Count || i1 >= useList.Count)
                        continue;

                    if (useList[i0] == 0 || useList[i1] == 0)
                        continue;
                }

                if (i0 >= posList.Count || i1 >= posList.Count)
                    continue;

                Vector3 pos0 = posList[i0];
                Vector3 pos1 = posList[i1];

                Gizmos.DrawLine(pos0, pos1);
            }
        }
    }
}
