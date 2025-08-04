// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaReductionMesh
{
    /// <summary>
    /// ?????????
    /// ?????????????????????????????????????
    /// </summary>
    [System.Serializable]
    public class FinalData
    {
        //=========================================================================================
        // ?????????
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector4> tangents = new List<Vector4>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<BoneWeight> boneWeights = new List<BoneWeight>();
        public List<Matrix4x4> bindPoses = new List<Matrix4x4>();
        public List<Transform> bones = new List<Transform>();
        public List<int> lines = new List<int>();
        public List<int> triangles = new List<int>();
        public List<int> tetras = new List<int>();
        public List<float> tetraSizes = new List<float>();

        /// <summary>
        /// ???????????????
        /// </summary>
        public List<Matrix4x4> vertexBindPoses = new List<Matrix4x4>();

        /// <summary>
        /// ??????????????????????
        /// ????uint???????????16bit?[???????????]???16bit?[?????????]
        /// </summary>
        [System.Serializable]
        public class MeshIndexData
        {
            public List<uint> meshIndexPackList = new List<uint>();
        }
        public List<MeshIndexData> vertexToMeshIndexList = new List<MeshIndexData>();

        /// <summary>
        /// ??????????????????????
        /// </summary>
        public List<int> vertexToTriangleCountList = new List<int>();   // ???????????
        public List<int> vertexToTriangleStartList = new List<int>();   // vertexToTriangleIndexList?????
        public List<int> vertexToTriangleIndexList = new List<int>();   // ??????????????????(?????????????)

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        [System.Serializable]
        public class MeshInfo
        {
            public int meshIndex;
            public Mesh mesh;

            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector4> tangents = new List<Vector4>();
            public List<BoneWeight> boneWeights = new List<BoneWeight>();

            /// <summary>
            /// ???????????????????
            /// </summary>
            public List<int> parents = new List<int>();

            /// <summary>
            /// ???
            /// </summary>
            public int VertexCount
            {
                get
                {
                    return vertices.Count;
                }
            }
        }
        public List<MeshInfo> meshList = new List<MeshInfo>();

        //=========================================================================================
        /// <summary>
        /// ???????????
        /// </summary>
        public bool IsValid
        {
            get
            {
                return vertices.Count > 0;
            }
        }

        /// <summary>
        /// ???
        /// </summary>
        public int VertexCount
        {
            get
            {
                return vertices.Count;
            }
        }

        /// <summary>
        /// ????
        /// </summary>
        public int LineCount
        {
            get
            {
                return lines.Count / 2;
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        public int TriangleCount
        {
            get
            {
                return triangles.Count / 3;
            }
        }

        /// <summary>
        /// ????
        /// </summary>
        public int TetraCount
        {
            get
            {
                return tetras.Count / 4;
            }
        }

        /// <summary>
        /// ????
        /// </summary>
        public int BoneCount
        {
            get
            {
                return bones.Count;
            }
        }

        /// <summary>
        /// ???????????
        /// </summary>
        public bool IsSkinning
        {
            get
            {
                //return bones.Count > 1;
                return true; // ????????????
            }
        }

        /// <summary>
        /// ??????
        /// </summary>
        public int MeshCount
        {
            get
            {
                return meshList.Count;
            }
        }
    }
}
