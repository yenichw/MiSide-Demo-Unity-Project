// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using Unity.Mathematics;


namespace MagicaCloth
{
    /// <summary>
    /// ???????????????????????????
    /// ???????????????????????????????
    /// </summary>
    public class NearPointSearch : GridHash
    {
        float radius;
        Dictionary<int, int> nearDict = new Dictionary<int, int>();
        Dictionary<int, float> distDict = new Dictionary<int, float>();
        HashSet<uint> lockPairSet = new HashSet<uint>();

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="positionList">????????????</param>
        /// <param name="radius">???????</param>
        public void Create(float3[] positionList, float radius)
        {
            base.Create(radius);

            this.radius = radius;

            // ????????????
            for (int i = 0; i < positionList.Length; i++)
            {
                AddPoint(positionList[i], i);
            }
        }

        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        public void SearchNearPointAll()
        {
            foreach (var plist in gridMap.Values)
            {
                foreach (var p in plist)
                {
                    SearchNearPoint(p.id, p.pos);
                }
            }
        }

        /// <summary>
        /// ????????1???????????????????????
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pos"></param>
        public void SearchNearPoint(int id, float3 pos)
        {
            int nearId = -1;
            float nearDist = 100000.0f;

            // ????????????????????????????
            int3 sgrid = GetGridPos(pos - radius, gridSize);
            int3 egrid = GetGridPos(pos + radius, gridSize);

            for (int x = sgrid.x; x <= egrid.x; x++)
            {
                for (int y = sgrid.y; y <= egrid.y; y++)
                {
                    for (int z = sgrid.z; z <= egrid.z; z++)
                    {
                        uint hash = GetGridHash(new int3(x, y, z));

                        // ???????????
                        if (gridMap.ContainsKey(hash))
                        {
                            var plist = gridMap[hash];
                            foreach (var p in plist)
                            {
                                // ?????
                                if (p.id == id)
                                    continue;

                                // ????
                                float dist = math.length(pos - p.pos);
                                if (dist < nearDist)
                                {
                                    nearId = p.id;
                                    nearDist = dist;
                                }
                            }
                        }
                    }
                }
            }

            // ????
            if (nearId >= 0)
            {
                nearDict[id] = nearId;
                distDict[id] = nearDist;
            }
            else
            {
                if (nearDict.ContainsKey(id))
                {
                    nearDict.Remove(id);
                    distDict.Remove(id);
                }
            }
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="r"></param>
        public void SearchNearPoint(float3 pos, float r)
        {
            int3 sgrid = GetGridPos(pos - r, gridSize);
            int3 egrid = GetGridPos(pos + r, gridSize);

            for (int x = sgrid.x; x <= egrid.x; x++)
            {
                for (int y = sgrid.y; y <= egrid.y; y++)
                {
                    for (int z = sgrid.z; z <= egrid.z; z++)
                    {
                        uint hash = GetGridHash(new int3(x, y, z));
                        if (gridMap.ContainsKey(hash))
                        {
                            var plist = gridMap[hash];
                            foreach (var p in plist)
                            {
                                SearchNearPoint(p.id, p.pos);
                            }
                        }
                    }
                }
            }
        }

        public override void AddPoint(float3 pos, int id)
        {
            base.AddPoint(pos, id);
        }

        public override void Remove(float3 pos, int id)
        {
            base.Remove(pos, id);

            if (nearDict.ContainsKey(id))
            {
                nearDict.Remove(id);
                distDict.Remove(id);
            }
        }

        public void AddLockPair(int id1, int id2)
        {
            uint pair = DataUtility.PackPair(id1, id2);
            lockPairSet.Add(pair);
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        public bool GetNearPointPair(out int id1, out int id2)
        {
            int index = -1;
            int nearIndex = -1;
            float nearDist = 100000.0f;

            foreach (var keyval in nearDict)
            {
                int id = keyval.Key;
                int nearId = keyval.Value;
                if (nearId == -1)
                    continue;

                // ??????????
                uint pair = DataUtility.PackPair(id, nearId);
                if (lockPairSet.Contains(pair))
                    continue;

                float dist = distDict[id];
                if (dist > radius)
                    continue;

                if (dist < nearDist)
                {
                    index = id;
                    nearIndex = nearId;
                    nearDist = dist;
                }
            }

            if (index >= 0 && nearIndex >= 0)
            {
                id1 = index;
                id2 = nearIndex;
                return true;
            }
            else
            {
                id1 = -1;
                id2 = -1;
                return false;
            }
        }

        public override string ToString()
        {
            string str = "";

            foreach (var keyval in nearDict)
            {
                str += string.Format("[{0}] -> {1} {2}\n", keyval.Key, keyval.Value, distDict[keyval.Key]);
            }

            return str;
        }
    }
}
