// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ?????????
    /// ?????????????????
    /// </summary>
    [System.Serializable]
    public class MeshData : ShareDataObject
    {
        /// <summary>
        /// ????????
        /// </summary>
        private const int DATA_VERSION = 2;

        /// <summary>
        /// ????????
        /// </summary>
        [System.Serializable]
        public struct VertexWeight
        {
            public Vector3 localPos;
            public Vector3 localNor;
            public Vector3 localTan;
            public int parentIndex;
            public float weight;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        public bool isSkinning;

        /// <summary>
        /// ???(??)
        /// </summary>
        public int vertexCount;

        /// <summary>
        /// ???????????????????????????
        /// ??4bit = ?????
        /// ??28bit = ??????????
        /// </summary>
        public uint[] vertexInfoList;

        /// <summary>
        /// ?????????
        /// </summary>
        public VertexWeight[] vertexWeightList;

        /// <summary>
        /// ?????????(?????)
        /// </summary>
        public ulong[] vertexHashList;

        /// <summary>
        /// UV???(??????)
        /// </summary>
        public Vector2[] uvList;

        /// <summary>
        /// ????
        /// </summary>
        public int lineCount;

        /// <summary>
        /// ????????(????x2)
        /// </summary>
        public int[] lineList;

        /// <summary>
        /// ????????
        /// </summary>
        public int triangleCount;

        /// <summary>
        /// ????????????(????????x3)
        /// </summary>
        public int[] triangleList;

        /// <summary>
        /// ????
        /// </summary>
        public int boneCount;

        /// <summary>
        /// ?????????????????????
        /// ??8bit = ??????????
        /// ??24bit = ????????????(vertexToTriangleIndexList)?????????
        /// </summary>
        public uint[] vertexToTriangleInfoList;

        /// <summary>
        /// ????????????????????????????
        /// ?????????????
        /// </summary>
        public int[] vertexToTriangleIndexList;

        /// <summary>
        /// ????????
        /// </summary>
        [System.Serializable]
        public class ChildData : IDataHash
        {
            /// <summary>
            /// ?????????????
            /// </summary>
            public int childDataHash;

            /// <summary>
            /// ???
            /// </summary>
            public int vertexCount;

            /// <summary>
            /// ???????????????????????????
            /// ??4bit = ?????
            /// ??28bit = ??????????
            /// </summary>
            public uint[] vertexInfoList;

            /// <summary>
            /// ?????????
            /// </summary>
            public VertexWeight[] vertexWeightList;

            /// <summary>
            /// ??????????????????????(??????)
            /// </summary>
            public int[] parentIndexList;

            public int VertexCount
            {
                get
                {
                    return vertexCount;
                }
            }

            public int GetDataHash()
            {
                int hash = 0;
                hash += childDataHash;
                hash += vertexCount.GetDataHash();
                return hash;
            }
        }
        public List<ChildData> childDataList = new List<ChildData>();

        /// <summary>
        /// ???????
        /// </summary>
        public Vector3 baseScale;

        //=========================================================================================
        /// <summary>
        /// ???
        /// </summary>
        public int VertexCount
        {
            get
            {
                return vertexCount;
            }
        }

        public int VertexHashCount
        {
            get
            {
                if (vertexHashList != null)
                    return vertexHashList.Length;
                return 0;
            }
        }

        public int WeightCount
        {
            get
            {
                if (vertexWeightList != null)
                    return vertexWeightList.Length;
                return 0;
            }
        }

        /// <summary>
        /// ????
        /// </summary>
        public int LineCount
        {
            get
            {
                return lineCount;
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        public int TriangleCount
        {
            get
            {
                return triangleCount;
            }
        }

        /// <summary>
        /// ????
        /// </summary>
        public int BoneCount
        {
            get
            {
                return boneCount;
            }
        }

        /// <summary>
        /// ???
        /// </summary>
        public int ChildCount
        {
            get
            {
                return childDataList.Count;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public override int GetDataHash()
        {
            int hash = 0;
            hash += isSkinning.GetDataHash();
            hash += vertexCount.GetDataHash();
            hash += lineCount.GetDataHash();
            hash += triangleCount.GetDataHash();
            hash += boneCount.GetDataHash();
            hash += ChildCount.GetDataHash();

            hash += vertexInfoList.GetDataCountHash();
            hash += vertexWeightList.GetDataCountHash();
            hash += uvList.GetDataCountHash();
            hash += lineList.GetDataCountHash();
            hash += triangleList.GetDataCountHash();

            hash += childDataList.GetDataHash();

            // option
            if (vertexHashList != null && vertexHashList.Length > 0)
                hash += vertexHashList.GetDataCountHash();

            return hash;
        }

        //=========================================================================================
        public override int GetVersion()
        {
            return DATA_VERSION;
        }

        /// <summary>
        /// ?????????(???????)???
        /// </summary>
        /// <returns></returns>
        public override Define.Error VerifyData()
        {
            if (dataHash == 0)
                return Define.Error.InvalidDataHash;
            //if (dataVersion != GetVersion())
            //    return Define.Error.DataVersionMismatch;
            if (vertexCount == 0)
                return Define.Error.VertexCountZero;

            return Define.Error.None;
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????????????????????
        /// ????????????????????????
        /// ????uint???16bit???????????????16bit?????????????
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, List<uint>> GetVirtualToChildVertexDict()
        {
            var dict = new Dictionary<int, List<uint>>();

            for (int i = 0; i < VertexCount; i++)
                dict.Add(i, new List<uint>());

            for (int i = 0; i < childDataList.Count; i++)
            {
                var cdata = childDataList[i];

                for (int j = 0; j < cdata.VertexCount; j++)
                {
                    if (j < cdata.parentIndexList.Length)
                    {
                        int mvindex = cdata.parentIndexList[j];
                        dict[mvindex].Add(DataUtility.Pack16(i, j));
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// ?????????????????????????
        /// </summary>
        /// <param name="originalSelection"></param>
        /// <param name="extendNext">??????????/?????????????</param>
        /// <param name="extendWeight">??/?????????????????????????????</param>
        /// <returns></returns>
        public List<int> ExtendSelection(List<int> originalSelection, bool extendNext, bool extendWeight)
        {
            var selection = new List<int>(originalSelection);

            // (1)??????????/?????????????
            if (extendNext)
            {
                // ???/?????????????????????????????
                List<HashSet<int>> vlink = MeshUtility.GetTriangleToVertexLinkList(vertexCount, new List<int>(lineList), new List<int>(triangleList));

                // ??????????/?????????????
                List<int> changeIndexList = new List<int>();
                for (int i = 0; i < vertexCount; i++)
                {
                    if (selection[i] == SelectionData.Invalid)
                    {
                        // ??????
                        var vset = vlink[i];
                        foreach (var vindex in vset)
                        {
                            if (selection[vindex] == SelectionData.Move || selection[vindex] == SelectionData.Fixed)
                            {
                                // ???????
                                selection[i] = SelectionData.Extend;
                            }
                        }
                    }
                }
            }

            // (2)??/?????????????????????????????
            if (extendWeight)
            {
                var extendSet = new HashSet<int>();
                foreach (var cdata in childDataList)
                {
                    for (int i = 0; i < cdata.VertexCount; i++)
                    {
                        // ?????????????????????
                        uint pack = cdata.vertexInfoList[i];
                        int wcnt = DataUtility.Unpack4_28Hi(pack);
                        int wstart = DataUtility.Unpack4_28Low(pack);

                        bool link = false;
                        for (int j = 0; j < wcnt; j++)
                        {
                            int sindex = wstart + j;
                            var vw = cdata.vertexWeightList[sindex];

                            // ????????/????????????????
                            if (vw.weight > 0.0f && (selection[vw.parentIndex] == SelectionData.Move || selection[vw.parentIndex] == SelectionData.Fixed))
                                link = true;
                        }

                        if (link)
                        {
                            for (int j = 0; j < wcnt; j++)
                            {
                                int sindex = wstart + j;
                                var vw = cdata.vertexWeightList[sindex];

                                // ?????????????Invalid????Extend?????
                                if (vw.weight > 0.0f && selection[vw.parentIndex] == SelectionData.Invalid)
                                    extendSet.Add(vw.parentIndex);
                            }
                        }
                    }
                }
                foreach (var vindex in extendSet)
                {
                    selection[vindex] = SelectionData.Extend;
                }
            }

            return selection;
        }
    }
}
