// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// 3???????????????
    /// ??????????????????????????????????
    /// </summary>
    public class GridHash
    {
        /// <summary>
        /// ?????
        /// </summary>
        public class Point
        {
            public int id;
            public float3 pos;
        }

        /// <summary>
        /// 3?????????
        /// </summary>
        protected Dictionary<uint, List<Point>> gridMap;

        /// <summary>
        /// ???????
        /// </summary>
        protected float gridSize = 0.1f;

        //=========================================================================================
        /// <summary>
        /// ???
        /// </summary>
        /// <param name="gridSize"></param>
        public virtual void Create(float gridSize = 0.1f)
        {
            gridMap = new Dictionary<uint, List<Point>>();
            this.gridSize = gridSize;
        }

        //=========================================================================================
        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="id"></param>
        public virtual void AddPoint(float3 pos, int id)
        {
            var p = new Point()
            {
                id = id,
                pos = pos
            };
            var grid = GetGridHash(pos, gridSize);
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
        /// ?????????????
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="id"></param>
        public virtual void Remove(float3 pos, int id)
        {
            var grid = GetGridHash(pos, gridSize);
            if (gridMap.ContainsKey(grid))
            {
                var plist = gridMap[grid];
                for (int i = 0; i < plist.Count; i++)
                {
                    if (plist[i].id == id)
                    {
                        plist.RemoveAt(i);
                        break;
                    }
                }
            }
            else
                Debug.LogError("remove faild!");
        }

        /// <summary>
        /// ???
        /// </summary>
        public void Clear()
        {
            gridMap.Clear();
        }

        //=========================================================================================
        /// <summary>
        /// ????3?????????????
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static int3 GetGridPos(float3 pos, float gridSize)
        {
            return math.int3(math.floor(pos / gridSize));
        }

        /// <summary>
        /// 3?????????10??????uint???????????????
        /// 10???????+511~-512???????????????/??????????????
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static uint GetGridHash(int3 pos)
        {
            uint hash = (uint)(pos.x & 0x3ff) | (uint)(pos.y & 0x3ff) << 10 | (uint)(pos.z & 0x3ff) << 20;
            return hash;
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static uint GetGridHash(float3 pos, float gridSize)
        {
            int3 xyz = GetGridPos(pos, gridSize);
            return GetGridHash(xyz);
        }
    }
}
