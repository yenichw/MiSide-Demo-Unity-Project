// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
    /// <summary>
    /// ??????????
    /// ·????????1?????
    /// ·????????
    /// ·?????????????
    /// </summary>
    public class ReductionMesh
    {
        /// <summary>
        /// ????????????
        /// </summary>
        public enum ReductionWeightMode
        {
            /// <summary>
            /// ?????????????????(????)
            /// </summary>
            Distance = 0,

            /// <summary>
            /// ??????????????????????
            /// </summary>
            Average = 1,

            /// <summary>
            /// ????????????????(???)
            /// </summary>
            DistanceAverage = 2,
        }
        public ReductionWeightMode WeightMode { get; set; } = ReductionWeightMode.Distance;


        //=========================================================================================
        private MeshData meshData = new MeshData();

        private ReductionData reductionData = new ReductionData();

        private DebugData debugData = new DebugData();

        //=========================================================================================
        public MeshData MeshData
        {
            get
            {
                meshData.SetParent(this);
                return meshData;
            }
        }

        public ReductionData ReductionData
        {
            get
            {
                reductionData.SetParent(this);
                return reductionData;
            }
        }

        public DebugData DebugData
        {
            get
            {
                debugData.SetParent(this);
                return debugData;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// ?????????????????
        /// </summary>
        /// <param name="isSkinning"></param>
        /// <param name="mesh"></param>
        /// <param name="bones"></param>
        public int AddMesh(bool isSkinning, Mesh mesh, List<Transform> bones, Matrix4x4[] bindPoseList, BoneWeight[] boneWeightList)
        {
            return MeshData.AddMesh(isSkinning, mesh, bones, bindPoseList, boneWeightList);
        }

        /// <summary>
        /// ?????????
        /// ?????????????????(-1=???)
        /// </summary>
        /// <param name="ren"></param>
        public int AddMesh(Renderer ren)
        {
            if (ren == null)
            {
                Debug.LogError("Renderer is NUll!");
                return -1;
            }

            if (ren is SkinnedMeshRenderer)
            {
                var sren = ren as SkinnedMeshRenderer;
                return MeshData.AddMesh(true, sren.sharedMesh, new List<Transform>(sren.bones), sren.sharedMesh.bindposes, sren.sharedMesh.boneWeights);
            }
            else
            {
                var mfilter = ren.GetComponent<MeshFilter>();
                var bones = new List<Transform>();
                bones.Add(ren.transform);
                return MeshData.AddMesh(false, mfilter.sharedMesh, bones, null, null);
            }
        }

        /// <summary>
        /// ?????????
        /// ?????????????????
        /// </summary>
        /// <param name="root"></param>
        /// <param name="posList"></param>
        /// <param name="norList"></param>
        /// <param name="tanList"></param>
        /// <param name="uvList"></param>
        /// <returns></returns>
        public int AddMesh(Transform root, List<Vector3> posList, List<Vector3> norList = null, List<Vector4> tanList = null, List<Vector2> uvList = null, List<int> triangleList = null)
        {
            return MeshData.AddMesh(root, posList, norList, tanList, uvList, triangleList);
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="zeroRadius">??????????(0.0f=?????)</param>
        /// <param name="radius">??????????(0.0f=?????)</param>
        /// <param name="polygonLength">????????????(0.0f=?????)</param>
        public void Reduction(float zeroRadius, float radius, float polygonLength, bool createTetra)
        {
            // ????????????
            if (zeroRadius > 0.0f)
                ReductionData.ReductionZeroDistance(zeroRadius);

            // ????????????
            if (radius > 0.0f)
                ReductionData.ReductionRadius(radius);

            // ????????????????????
            if (polygonLength > 0.0f)
                ReductionData.ReductionPolygonLink(polygonLength);

            // ?????????
            MeshData.UpdateMeshData(createTetra);

            // ?????????
            ReductionData.ReductionBone();
        }

        /// <summary>
        /// ????????????????
        /// ???????????????????????(weightLength=0)???????
        /// </summary>
        /// <param name="root">???????????????(??????????????????)</param>
        public FinalData GetFinalData(Transform root)
        {
            return MeshData.GetFinalData(root);
        }

    }
}
