// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
    /// <summary>
    /// ?????????????????
    /// </summary>
    public class PolygonLinkReduction
    {
        protected MeshData meshData;

        private float reductionLength;

        /// <summary>
        /// ?????
        /// </summary>
        public class Point
        {
            public MeshData.ShareVertex shareVertex;

            /// <summary>
            /// ???????????(null=??)
            /// </summary>
            public Point nearPoint;

            /// <summary>
            /// ???????????????
            /// </summary>
            public float nearDist;

        }

        /// <summary>
        /// ????????
        /// </summary>
        List<Point> pointList = new List<Point>();

        /// <summary>
        /// ????????????
        /// </summary>
        Dictionary<MeshData.ShareVertex, Point> pointDict = new Dictionary<MeshData.ShareVertex, Point>();

        //=========================================================================================
        public PolygonLinkReduction(float length)
        {
            reductionLength = length;
        }

        public int PointCount
        {
            get
            {
                return pointList.Count;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????????????
        /// </summary>
        /// <param name="meshData"></param>
        public void Create(MeshData meshData)
        {
            this.meshData = meshData;

            foreach (var sv in meshData.shareVertexList)
            {
                AddPoint(sv);
            }

            // ????????????
            SearchNearPointAll();
        }

        /// <summary>
        /// ????????
        /// </summary>
        public void Reduction()
        {
            Point p0 = null;
            var nlist = new List<Point>();
            while ((p0 = GetNearPointPair()) != null)
            {
                // p0?p1??????
                var p1 = p0.nearPoint;
                Debug.Assert(p1 != null);

                var sv0 = p0.shareVertex;
                var sv1 = p1.shareVertex;

                // ??2????????????????????
                nlist.Clear();
                foreach (var sv in sv0.linkShareVertexSet)
                    nlist.Add(pointDict[sv]);
                foreach (var sv in sv1.linkShareVertexSet)
                    nlist.Add(pointDict[sv]);
                nlist.Add(p0); // p0?????

                // ?????????
                foreach (var np in nlist)
                {
                    np.nearPoint = null;
                    np.nearDist = 100000;
                }

                // p1???
                Remove(p1);
                p1 = null;

                // sv1?sv2????
                meshData.CombineVertex(sv0, sv1);

                // p0/p1???????????????????????
                foreach (var np in nlist)
                {
                    SearchNearPoint(np);
                }
            }
        }

        //=========================================================================================
        void AddPoint(MeshData.ShareVertex sv)
        {
            var p = new Point();
            p.shareVertex = sv;
            pointList.Add(p);
            pointDict.Add(sv, p);
        }

        Point GetPoint(MeshData.ShareVertex sv)
        {
            if (pointDict.ContainsKey(sv))
                return pointDict[sv];
            return null;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="p"></param>
        void Remove(Point p)
        {
            // ?????
            pointDict.Remove(p.shareVertex);
            pointList.Remove(p);
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????????
        /// </summary>
        void SearchNearPointAll()
        {
            foreach (var p in pointList)
            {
                SearchNearPoint(p);
            }
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="p"></param>
        void SearchNearPoint(Point p)
        {
            p.nearPoint = null;
            p.nearDist = 100000;

            var wpos = p.shareVertex.wpos;

            foreach (var sv in p.shareVertex.linkShareVertexSet)
            {
                var dist = Vector3.Distance(wpos, sv.wpos);
                if (dist < p.nearDist && dist <= reductionLength)
                {
                    p.nearDist = dist;
                    p.nearPoint = pointDict[sv];
                }
            }
        }

        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <returns></returns>
        Point GetNearPointPair()
        {
            float nearDist = 10000;
            Point nearPoint = null;

            // ????
            foreach (var p in pointList)
            {
                if (p.nearPoint != null && p.nearDist < nearDist)
                {
                    nearDist = p.nearDist;
                    nearPoint = p;
                }
            }

            return nearPoint;
        }
    }
}
