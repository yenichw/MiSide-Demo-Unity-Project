// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
    /// <summary>
    /// 3???????????????????????
    /// </summary>
    public class NearPointReduction
    {
        protected MeshData meshData;

        /// <summary>
        /// ?????
        /// </summary>
        public class Point
        {
            public MeshData.ShareVertex shareVertex;
            public Vector3 pos;
            public Vector3Int grid;

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
        /// 3?????????
        /// </summary>
        protected Dictionary<Vector3Int, List<Point>> gridMap = new Dictionary<Vector3Int, List<Point>>();

        /// <summary>
        /// ???????
        /// </summary>
        protected float gridSize = 0.05f;

        /// <summary>
        /// ????
        /// </summary>
        float searchRadius;

        /// <summary>
        /// ???????????(??:???????????:?????????????)
        /// </summary>
        Dictionary<Point, List<Point>> nearPointDict = new Dictionary<Point, List<Point>>();

        //=========================================================================================
        public NearPointReduction(float radius = 0.05f)
        {
            searchRadius = radius;
            gridSize = radius * 2;
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
                AddPoint(sv, sv.wpos);
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
                if (nearPointDict.ContainsKey(p0))
                {
                    nlist.AddRange(nearPointDict[p0]);
                    nearPointDict.Remove(p0);
                }
                if (nearPointDict.ContainsKey(p1))
                {
                    nlist.AddRange(nearPointDict[p1]);
                    nearPointDict.Remove(p1);
                }
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

                // p0?????????????
                Move(p0, sv0.wpos);

                // p0/p1???????????????????????
                foreach (var np in nlist)
                {
                    SearchNearPoint(np, searchRadius, null);
                }
            }
        }

        //=========================================================================================
        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="pos"></param>
        Point AddPoint(MeshData.ShareVertex sv, Vector3 pos)
        {
            var p = new Point()
            {
                shareVertex = sv,
                pos = pos
            };
            pointList.Add(p);

            AddGrid(p);

            return p;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="p"></param>
        void AddGrid(Point p)
        {
            var grid = GetGridPos(p.pos);
            p.grid = grid;
            if (gridMap.ContainsKey(grid))
                gridMap[grid].Add(p);
            else
            {
                var plist = new List<Point>();
                plist.Add(p);
                gridMap.Add(grid, plist);
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="p"></param>
        void RemoveGrid(Point p)
        {
            var grid = p.grid;
            if (gridMap.ContainsKey(grid))
            {
                var plist = gridMap[grid];
                plist.Remove(p);

                if (plist.Count == 0)
                    gridMap.Remove(grid);
            }
            else
                Debug.LogError("remove faild!");
            p.grid = Vector3Int.zero;
        }

        void Move(Point p, Vector3 newpos)
        {
            // ????????
            RemoveGrid(p);

            // ????
            p.pos = newpos;

            // ??????
            AddGrid(p);
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="p"></param>
        void Remove(Point p)
        {
            // ?????
            RemoveGrid(p);
            pointList.Remove(p);
        }

        //=========================================================================================
        /// <summary>
        /// ????3?????????????
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        protected Vector3Int GetGridPos(Vector3 pos)
        {
            var v = pos / gridSize;
            return new Vector3Int((int)Mathf.Floor(v.x), (int)Mathf.Floor(v.y), (int)Mathf.Floor(v.z));
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        void SearchNearPointAll()
        {
            nearPointDict.Clear();

            foreach (var plist in gridMap.Values)
            {
                foreach (var p in plist)
                {
                    SearchNearPoint(p, searchRadius, null);
                }
            }
        }

        /// <summary>
        /// ????????1???????????????????????
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pos"></param>
        void SearchNearPoint(Point p, float radius, Point ignorePoint)
        {
            Point nearPoint = null;
            float nearDist = 100000;

            // ??P????????????????????????
            if (p.nearPoint != null)
            {
                if (nearPointDict.ContainsKey(p.nearPoint))
                {
                    nearPointDict[p.nearPoint].Remove(p);
                }
            }

            // ????????????????????????????
            var center = p.grid;
            int size = (int)(radius / gridSize) + 1;
            var s = new Vector3Int(size, size, size);
            var sgrid = center - s;
            var egrid = center + s;

            Vector3Int grid = Vector3Int.zero;
            for (int x = sgrid.x; x <= egrid.x; x++)
            {
                grid.x = x;
                for (int y = sgrid.y; y <= egrid.y; y++)
                {
                    grid.y = y;
                    for (int z = sgrid.z; z <= egrid.z; z++)
                    {
                        grid.z = z;

                        // ???????????
                        if (gridMap.ContainsKey(grid))
                        {
                            var plist = gridMap[grid];
                            foreach (var wp in plist)
                            {
                                // ?????
                                if (wp == p)
                                    continue;

                                // ???????????
                                if (wp == ignorePoint)
                                    continue;

                                // ????
                                float dist = Vector3.Distance(wp.pos, p.pos);
                                if (dist < nearDist && dist <= radius)
                                {
                                    nearPoint = wp;
                                    nearDist = dist;
                                }
                            }
                        }
                    }
                }
            }

            // ????
            if (nearPoint != null)
            {
                p.nearPoint = nearPoint;
                p.nearDist = nearDist;

                // ????????
                if (nearPointDict.ContainsKey(nearPoint) == false)
                    nearPointDict.Add(nearPoint, new List<Point>());
                nearPointDict[nearPoint].Add(p);
            }
            else
            {
                p.nearPoint = null;
                p.nearDist = 100000;
            }
        }

        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <returns></returns>
        Point GetNearPointPair()
        {
#if true
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
#else
            if (pointList.Count == 0)
                return null;

            // ?????
            pointList.Sort((a, b) => a.nearDist < b.nearDist ? -1 : 1);
            return pointList[0];
#endif
        }
    }
}
