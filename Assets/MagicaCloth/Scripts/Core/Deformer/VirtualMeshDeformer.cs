// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????????????
    /// ????????????????????????
    /// </summary>
    [System.Serializable]
    public class VirtualMeshDeformer : BaseMeshDeformer, IBoneReplace
    {
        /// <summary>
        /// ????????
        /// </summary>
        private const int DATA_VERSION = 1;

        /// <summary>
        /// ????RenderDeformer
        /// ?RenderMeshDeformer??????????????????????????
        /// </summary>
        [SerializeField]
        private List<MagicaRenderDeformer> renderDeformerList = new List<MagicaRenderDeformer>();

        [SerializeField]
        private List<int> renderDeformerHashList = new List<int>();
        [SerializeField]
        private int renderDeformerVersion;

        /// <summary>
        /// ????????(?????????????1????????)
        /// </summary>
        [SerializeField]
        [Range(0.0f, 0.3f)]
        private float mergeVertexDistance = 0.001f;

        /// <summary>
        /// ???????????????(?????????????1????????)
        /// </summary>
        [SerializeField]
        [Range(0.0f, 0.3f)]
        private float mergeTriangleDistance = 0.0f;

        /// <summary>
        /// ?????????????????
        /// </summary>
        [SerializeField]
        [Range(10.0f, 90.0f)]
        private float sameSurfaceAngle = 80.0f;

        /// <summary>
        /// ???????????(false????1????????????)
        /// </summary>
        [SerializeField]
        private bool useSkinning = true;

        /// <summary>
        /// ?????????????????
        /// </summary>
        [SerializeField]
        [Range(1, 4)]
        private int maxWeightCount = 4;

        [SerializeField]
        [Range(1.0f, 5.0f)]
        private float weightPow = 3.0f;

        /// <summary>
        /// ????????????
        /// </summary>
        [SerializeField]
        private List<Transform> boneList = new List<Transform>();

        //=========================================================================================
        /// <summary>
        /// ???????????????????
        /// renderDeformerList?1:1???
        /// </summary>
        private List<int> sharedChildMeshIndexList = new List<int>();

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public override int GetDataHash()
        {
            int hash = base.GetDataHash();

            hash += RenderDeformerCount.GetDataHash();
            hash += renderDeformerList.GetDataHash();

            hash += BoneCount.GetDataHash();
            hash += boneList.GetDataHash();

            return hash;
        }

        //=========================================================================================
        /// <summary>
        /// ???
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            if (status.IsInitError)
                return;

            // ????????????????????
            if (MeshData == null || MeshData.VerifyData() != Define.Error.None)
            {
                status.SetInitError();
                return;
            }
            for (int i = 0; i < MeshData.ChildCount; i++)
            {
                if (renderDeformerList[i] == null)
                {
                    status.SetInitError();
                    return;
                }
                var renderDeformer = renderDeformerList[i];
                if (renderDeformer == null)
                {
                    status.SetInitError();
                    return;
                }

                // ????MagicaRenderDeformer????????(v1.5.1)
                renderDeformer.Init();

                if (renderDeformer.Deformer.Status.IsInitError)
                {
                    status.SetInitError();
                    return;
                }
            }

            VertexCount = MeshData.VertexCount;
            TriangleCount = MeshData.TriangleCount;
            //LineCount = meshData.LineCount;
            int triangleIndexCount = MeshData.vertexToTriangleIndexList != null ? MeshData.vertexToTriangleIndexList.Length : 0;

            // ???????????ID
            int uid = MeshData.SaveDataHash; // ????????????ID???
            bool first = MagicaPhysicsManager.Instance.Mesh.IsEmptySharedVirtualMesh(uid);

            //Develop.Log($"???????:{MeshData.name} uid:{uid} first:{first}");

            // ??????
            MeshIndex = MagicaPhysicsManager.Instance.Mesh.AddVirtualMesh(
                uid,
                MeshData.VertexCount,
                MeshData.WeightCount,
                MeshData.BoneCount,
                MeshData.TriangleCount,
                triangleIndexCount,
                TargetObject.transform
                );

            // ????????
            SkinningVertexCount = MeshData.VertexCount;

            // ????????????????
            //MagicaPhysicsManager.Instance.Mesh.SetVirtualMeshBone(MeshIndex, boneList);

            // ??????????
            if (first)
            {
                MagicaPhysicsManager.Instance.Mesh.SetSharedVirtualMeshData(
                    MeshIndex,
                    MeshData.vertexInfoList,
                    MeshData.vertexWeightList,
                    MeshData.uvList,
                    MeshData.triangleList,
                    MeshData.vertexToTriangleInfoList,
                    MeshData.vertexToTriangleIndexList
                    );
            }

            // ???????????????
            for (int i = 0; i < MeshData.ChildCount; i++)
            {
                var cdata = MeshData.childDataList[i];

                // ???????
                long cuid = (long)uid << 16 + i;
                bool cfirst = MagicaPhysicsManager.Instance.Mesh.IsEmptySharedChildMesh(cuid);
                int sharedChildMeshIndex = MagicaPhysicsManager.Instance.Mesh.AddSharedChildMesh(
                    cuid,
                    MeshIndex,
                    cdata.VertexCount,
                    cdata.vertexWeightList.Length
                    );

                // ????????
                if (cfirst)
                {
                    MagicaPhysicsManager.Instance.Mesh.SetSharedChildMeshData(
                        sharedChildMeshIndex,
                        cdata.vertexInfoList,
                        cdata.vertexWeightList
                        );
                }

                sharedChildMeshIndexList.Add(sharedChildMeshIndex);
            }
        }

        /// <summary>
        /// ??
        /// </summary>
        public override void Dispose()
        {
            if (MagicaPhysicsManager.IsInstance())
            {
                // ??????
                if (MagicaPhysicsManager.Instance.Mesh.ExistsVirtualMesh(MeshIndex))
                {
                    foreach (var sharedChildMeshIndex in sharedChildMeshIndexList)
                    {
                        MagicaPhysicsManager.Instance.Mesh.RemoveSharedChildMesh(sharedChildMeshIndex);
                    }

                    // ???????
                    //MagicaPhysicsManager.Instance.Mesh.ResetVirtualMeshBone(MeshIndex);

                    MagicaPhysicsManager.Instance.Mesh.RemoveVirtualMesh(MeshIndex);
                }
            }

            base.Dispose();
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        protected override void OnActive()
        {
            base.OnActive();
            if (status.IsInitSuccess)
            {
                // ????????????????
                MagicaPhysicsManager.Instance.Mesh.AddVirtualMeshBone(MeshIndex, boneList);

                MagicaPhysicsManager.Instance.Mesh.SetVirtualMeshActive(MeshIndex, true);
            }
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        protected override void OnInactive()
        {
            base.OnInactive();
            if (status.IsInitSuccess)
            {
                if (MagicaPhysicsManager.IsInstance())
                {
                    MagicaPhysicsManager.Instance.Mesh.SetVirtualMeshActive(MeshIndex, false);

                    // ???????
                    MagicaPhysicsManager.Instance.Mesh.RemoveVirtualMeshBone(MeshIndex);
                }
            }
        }

        internal override void MeshCalculation(int bufferIndex) { }

        internal override void NormalWriting(int bufferIndex) { }

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        public void ReplaceBone<T>(Dictionary<T, Transform> boneReplaceDict) where T : class
        {
            for (int i = 0; i < boneList.Count; i++)
            {
                boneList[i] = MeshUtility.GetReplaceBone(boneList[i], boneReplaceDict);
            }
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        public HashSet<Transform> GetUsedBones()
        {
            return new HashSet<Transform>(boneList);
        }

        //=========================================================================================
        public BaseMeshDeformer GetDeformer()
        {
            return this;
        }

        public float MergeVertexDistance
        {
            get
            {
                return mergeVertexDistance;
            }
        }

        public float MergeTriangleDistance
        {
            get
            {
                return mergeTriangleDistance;
            }
        }

        public float SameSurfaceAngle
        {
            get
            {
                return sameSurfaceAngle;
            }
        }

        public int MaxWeightCount
        {
            get
            {
                if (useSkinning)
                {
                    // ????????????????????????1?????
                    if (mergeVertexDistance <= 0.001f && mergeTriangleDistance <= 0.001f)
                        return 1;
                    else
                        return maxWeightCount;
                }
                else
                    return 1;
            }
        }

        public float WeightPow
        {
            get
            {
                return weightPow;
            }
        }

        public int RenderDeformerCount
        {
            get
            {
                return renderDeformerList.Count;
            }
        }

        public MagicaRenderDeformer GetRenderDeformer(int index)
        {
            return renderDeformerList[index];
        }

        public int GetRenderMeshDeformerIndex(RenderMeshDeformer deformer)
        {
            return renderDeformerList.FindIndex(d => d.Deformer == deformer);
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        /// <returns></returns>
        public List<MeshData> GetRenderDeformerMeshList()
        {
            List<MeshData> mdataList = new List<MeshData>();

            for (int i = 0; i < renderDeformerList.Count; i++)
            {
                MeshData mdata = null;

                if (renderDeformerList[i] != null)
                    mdata = renderDeformerList[i].Deformer.MeshData;

                mdataList.Add(mdata);
            }

            return mdataList;
        }

        //=========================================================================================
        public override bool IsMeshUse()
        {
            if (status.IsInitSuccess)
            {
                return MagicaPhysicsManager.Instance.Mesh.IsUseVirtualMesh(MeshIndex);
            }

            return false;
        }

        public override bool IsActiveMesh()
        {
            if (status.IsInitSuccess)
            {
                return MagicaPhysicsManager.Instance.Mesh.IsActiveVirtualMesh(MeshIndex);
            }

            return false;
        }

        public override void AddUseMesh(System.Object parent)
        {
            if (status.IsInitSuccess)
            {
                MagicaPhysicsManager.Instance.Mesh.AddUseVirtualMesh(MeshIndex);

                // ???????????
                for (int i = 0; i < renderDeformerList.Count; i++)
                {
                    var deformer = renderDeformerList[i].Deformer;

                    deformer.AddUseMesh(this);
                }
            }
        }

        public override void RemoveUseMesh(System.Object parent)
        {
            if (status.IsInitSuccess)
            {
                MagicaPhysicsManager.Instance.Mesh.RemoveUseVirtualMesh(MeshIndex);

                // ???????????
                for (int i = 0; i < renderDeformerList.Count; i++)
                {
                    var deformer = renderDeformerList[i].Deformer;

                    deformer.RemoveUseMesh(this);
                }
            }
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        public override bool AddUseVertex(int vindex, bool fix)
        {
            if (status.IsInitSuccess == false)
                return false;

            bool change = MagicaPhysicsManager.Instance.Mesh.AddUseVirtualVertex(MeshIndex, vindex, fix);

            return change;
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        public override bool RemoveUseVertex(int vindex, bool fix)
        {
            if (status.IsInitSuccess == false)
                return false;

            bool change = MagicaPhysicsManager.Instance.Mesh.RemoveUseVirtualVertex(MeshIndex, vindex, fix);

            return change;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public override void ResetFuturePrediction()
        {
            base.ResetFuturePrediction();
            MagicaPhysicsManager.Instance.Mesh.ResetFuturePredictionVirtualMeshBone(MeshIndex);
        }

        /// <summary>
        /// UnityPhysics?????????
        /// </summary>
        /// <param name="sw"></param>
        public override void ChangeUseUnityPhysics(bool sw)
        {
            if (status.IsInitSuccess)
            {
                MagicaPhysicsManager.Instance.Mesh.ChangeVirtualMeshUseUnityPhysics(MeshIndex, sw);
            }
        }

        public void ChangeCalculation(bool sw)
        {
            if (status.IsInitSuccess)
            {
                MagicaPhysicsManager.Instance.Mesh.SetVirtualMeshFlag(MeshIndex, PhysicsManagerMeshData.Meshflag_Pause, !sw);
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????????/??/?????(???????)
        /// </summary>
        /// <param name="wposList"></param>
        /// <param name="wnorList"></param>
        /// <param name="wtanList"></param>
        /// <returns>???</returns>
        public override int GetEditorPositionNormalTangent(
            out List<Vector3> wposList,
            out List<Vector3> wnorList,
            out List<Vector3> wtanList
            )
        {
            wposList = new List<Vector3>();
            wnorList = new List<Vector3>();
            wtanList = new List<Vector3>();

            if (Application.isPlaying)
            {
                if (IsMeshUse() == false)
                    return 0;

                Vector3[] posArray = new Vector3[VertexCount];
                Vector3[] norArray = new Vector3[VertexCount];
                Vector3[] tanArray = new Vector3[VertexCount];
                MagicaPhysicsManager.Instance.Mesh.CopyToVirtualMeshWorldData(MeshIndex, posArray, norArray, tanArray);

                wposList = new List<Vector3>(posArray);
                wnorList = new List<Vector3>(norArray);
                wtanList = new List<Vector3>(tanArray);

                return VertexCount;
            }
            else
            {
                if (MeshData == null || TargetObject == null || boneList.Count == 0)
                    return 0;

                MeshUtility.CalcMeshWorldPositionNormalTangent(MeshData, boneList, out wposList, out wnorList, out wtanList);

                return MeshData.VertexCount;
            }
        }

        /// <summary>
        /// ??????????????????(???????)
        /// </summary>
        /// <returns></returns>
        public override List<int> GetEditorTriangleList()
        {
            if (MeshData != null && MeshData.triangleList != null)
            {
                return new List<int>(MeshData.triangleList);
            }

            return null;
        }

        /// <summary>
        /// ??????????????(?????)
        /// </summary>
        /// <returns></returns>
        public override List<int> GetEditorLineList()
        {
            if (MeshData != null && MeshData.lineList != null)
            {
                return new List<int>(MeshData.lineList);
            }

            return null;
        }

        //=========================================================================================
        public override int GetVersion()
        {
            return DATA_VERSION;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns></returns>
        public override void CreateVerifyData()
        {
            base.CreateVerifyData();

            renderDeformerHashList.Clear();
            renderDeformerVersion = 0;
            foreach (var rd in renderDeformerList)
            {
                renderDeformerHashList.Add(rd.SaveDataHash);
                renderDeformerVersion = rd.SaveDataVersion;
            }
        }

        /// <summary>
        /// ?????????(???????)???
        /// </summary>
        /// <returns></returns>
        public override Define.Error VerifyData()
        {
            var baseError = base.VerifyData();
            if (baseError != Define.Error.None)
                return baseError;

            if (renderDeformerList.Count == 0)
                return Define.Error.DeformerCountZero;
            foreach (var rd in renderDeformerList)
            {
                if (rd == null)
                    return Define.Error.DeformerNull;
                var rdError = rd.VerifyData();
                if (rdError != Define.Error.None)
                    return rdError;
            }

            if (renderDeformerHashList.Count != renderDeformerList.Count)
                return Define.Error.DeformerCountMismatch;
            for (int i = 0; i < renderDeformerHashList.Count; i++)
            {
                var rd = renderDeformerList[i];
                if (rd.SaveDataHash != renderDeformerHashList[i])
                    return Define.Error.DeformerHashMismatch;
                if (rd.SaveDataVersion != renderDeformerVersion)
                    return Define.Error.DeformerVersionMismatch;
            }

            if (boneList.Count == 0)
                return Define.Error.BoneListZero;
            foreach (var bone in boneList)
                if (bone == null)
                    return Define.Error.BoneListNull;

            if (renderDeformerList.Count != MeshData.ChildCount)
                return Define.Error.DeformerCountMismatch;

            return Define.Error.None;
        }

        /// <summary>
        /// ?????
        /// </summary>
        /// <returns></returns>
        public override string GetInformation()
        {
            StaticStringBuilder.Clear();

            var err = VerifyData();
            if (err == Define.Error.None)
            {
                // OK
                StaticStringBuilder.AppendLine("Active: ", Status.IsActive);
                StaticStringBuilder.AppendLine($"Visible: {Parent.IsVisible}");
                StaticStringBuilder.AppendLine($"Calculation:{Parent.IsCalculate}");
                StaticStringBuilder.AppendLine("Vertex: ", MeshData.VertexCount);
                StaticStringBuilder.AppendLine("Line: ", MeshData.LineCount);
                StaticStringBuilder.AppendLine("Triangle: ", MeshData.TriangleCount);
                StaticStringBuilder.Append("Bone: ", MeshData.BoneCount);
            }
            else if (err == Define.Error.EmptyData)
            {
                StaticStringBuilder.Append(Define.GetErrorMessage(err));
            }
            else
            {
                // ???
                StaticStringBuilder.AppendLine("This mesh data is Invalid!");
                if (Application.isPlaying)
                {
                    StaticStringBuilder.AppendLine("Execution stopped.");
                }
                else
                {
                    StaticStringBuilder.AppendLine("Please recreate the mesh data.");
                }
                StaticStringBuilder.Append(Define.GetErrorMessage(err));
            }

            return StaticStringBuilder.ToString();
        }
    }
}
