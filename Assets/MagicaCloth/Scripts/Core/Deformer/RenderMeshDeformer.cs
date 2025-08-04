// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ???????????????
    /// </summary>
    [System.Serializable]
    public class RenderMeshDeformer : BaseMeshDeformer, IBoneReplace
    {
        /// <summary>
        /// ????????
        /// </summary>
        private const int DATA_VERSION = 2;

        /// <summary>
        /// ??????
        /// </summary>
        public enum RecalculateMode
        {
            // ??
            None = 0,

            // ???????
            UpdateNormalPerFrame = 1,

            // ??·???????
            UpdateNormalAndTangentPerFrame = 2,
        }

        // ??/???????
        [SerializeField]
        private RecalculateMode normalAndTangentUpdateMode = RecalculateMode.UpdateNormalPerFrame;

        /// <summary>
        /// ????????????????
        /// </summary>
        public enum BoundsMode
        {
            None = 0,

            // ???????
            ExpandedAtInitialization = 1,

            // ????????(??)
            //RecalculatedPerFrame = 2,
        }
        [SerializeField]
        private BoundsMode boundsUpdateMode = BoundsMode.None;


        [SerializeField]
        private Mesh sharedMesh = null;

        /// <summary>
        /// ??????????
        /// </summary>
        [SerializeField]
        private int meshOptimize = 0;

        // ???????? //////////////////////////////////////////
        // ?????
        Renderer renderer;
        MeshFilter meshFilter;
        SkinnedMeshRenderer skinMeshRenderer;
        Transform[] originalBones;
        Transform[] boneList;
        Mesh cloneMesh = null;
        GraphicsBuffer vertexBuffer;

        // ???????????
        bool IsChangePosition { get; set; }
        bool IsChangeNormalTangent { get; set; }
        bool IsChangeBoneWeights { get; set; }
        bool oldUse;
        internal bool IsWriteSkip { get; set; }
        internal bool IsWriteMeshPosition { get; private set; }
        internal bool IsWriteMeshBoneWeight { get; private set; }
        bool IsWriteMeshNormal;
        bool IsWriteMeshTangent;

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public override int GetDataHash()
        {
            int hash = base.GetDataHash();
            hash += sharedMesh.GetDataHash();
            if (meshOptimize != 0) // ???????
                hash += meshOptimize.GetDataHash();
            return hash;
        }

        //=========================================================================================
        public Mesh SharedMesh
        {
            get
            {
                return sharedMesh;
            }
        }

        //=========================================================================================
        public void OnValidate()
        {
            if (Application.isPlaying == false)
                return;

            if (status.IsActive)
            {
                // ??/??????????
                SetRecalculateNormalAndTangentMode();
            }
        }

        /// <summary>
        /// ???
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            if (status.IsInitError)
                return;

            // ?????????
            if (TargetObject == null)
            {
                status.SetInitError();
                return;
            }
            renderer = TargetObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                status.SetInitError();
                return;
            }

            if (MeshData.VerifyData() != Define.Error.None)
            {
                status.SetInitError();
                return;
            }

            VertexCount = MeshData.VertexCount;
            TriangleCount = MeshData.TriangleCount;

            // ??????????
            // ???????????????
            cloneMesh = null;
            if (renderer is SkinnedMeshRenderer)
            {
                var sren = renderer as SkinnedMeshRenderer;
                skinMeshRenderer = sren;

                // ????????????
                originalBones = sren.bones;

                // sren???????????????????????????????????????
                var blist = new List<Transform>(originalBones);
                blist.Add(sren.rootBone); // (old)renderer.transform
                boneList = blist.ToArray();

                // ????????
                cloneMesh = GameObject.Instantiate(sharedMesh);

                var bindlist = new List<Matrix4x4>(sharedMesh.bindposes);
                bindlist.Add(Matrix4x4.identity); // ???????????????????
                cloneMesh.bindposes = bindlist.ToArray();
            }
            else
            {
                // ????????
                cloneMesh = GameObject.Instantiate(sharedMesh);

                meshFilter = TargetObject.GetComponent<MeshFilter>();
                Debug.Assert(meshFilter);
            }
            oldUse = false;

            // ??????????????(v1.11.1)
            if (boundsUpdateMode == BoundsMode.ExpandedAtInitialization)
            {
                var bounds = skinMeshRenderer ? skinMeshRenderer.localBounds : sharedMesh.bounds;
                //Debug.Log($"original bounds:{bounds}");

                // XYZ??????x2?????
                float maxSize = Mathf.Max(Mathf.Max(bounds.extents.x, bounds.extents.y), bounds.extents.z);
                maxSize *= 2.0f;
                bounds.extents = Vector3.one * maxSize;
                //Debug.Log($"new bounds:{bounds}");

                if (skinMeshRenderer)
                    skinMeshRenderer.localBounds = bounds;
                else
                    cloneMesh.bounds = bounds;
            }

            // ???????uid
            int uid = sharedMesh.GetInstanceID(); // ???????ID???
            bool first = MagicaPhysicsManager.Instance.Mesh.IsEmptySharedRenderMesh(uid);

            // ??????
            MeshIndex = MagicaPhysicsManager.Instance.Mesh.AddRenderMesh(
                uid,
                MeshData.isSkinning,
                MeshData.baseScale,
                MeshData.VertexCount,
                IsSkinning ? boneList.Length - 1 : 0, // ???????????????
                IsSkinning ? sharedMesh.GetAllBoneWeights().Length : 0
                );

            // ??????????????????????????
            if (first)
            {
                MagicaPhysicsManager.Instance.Mesh.SetRenderSharedMeshData(
                    MeshIndex,
                    IsSkinning,
                    sharedMesh.vertices,
                    sharedMesh.normals,
                    sharedMesh.tangents,
                    sharedMesh.GetBonesPerVertex(),
                    sharedMesh.GetAllBoneWeights()
                    );
            }

            // ????????????
            // ?????????????????????????
            MagicaPhysicsManager.Instance.Mesh.UpdateMeshState(MeshIndex);

            // ??/??????????
            SetRecalculateNormalAndTangentMode();
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        protected override void OnActive()
        {
            base.OnActive();
            if (status.IsInitSuccess)
            {
                MagicaPhysicsManager.Instance.Mesh.SetRenderMeshActive(MeshIndex, true);

                // ???????????????
                // ??????????????????????????????
                var meshRootTransform = skinMeshRenderer ? skinMeshRenderer.rootBone : TargetObject.transform; // (old)TargetObject.transform
                MagicaPhysicsManager.Instance.Mesh.AddRenderMeshBone(MeshIndex, meshRootTransform);
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
                    // ???????????????
                    MagicaPhysicsManager.Instance.Mesh.RemoveRenderMeshBone(MeshIndex);

                    MagicaPhysicsManager.Instance.Mesh.SetRenderMeshActive(MeshIndex, false);
                }
            }

            // ????????
            // ????????????????VertexBuffer???????????
            if (vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
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
                MagicaPhysicsManager.Instance.Mesh.RemoveRenderMesh(MeshIndex);
            }

            // ????????
            if (vertexBuffer != null)
                vertexBuffer.Dispose();

            // ??????????
            if (cloneMesh)
                GameObject.Destroy(cloneMesh);

            base.Dispose();
        }

        /// <summary>
        /// ??/??????????
        /// </summary>
        void SetRecalculateNormalAndTangentMode()
        {
            // ??????????????/???????
            bool normal = false;
            bool tangent = false;
            if (normalAndTangentUpdateMode == RecalculateMode.UpdateNormalPerFrame)
            {
                normal = true;
            }
            else if (normalAndTangentUpdateMode == RecalculateMode.UpdateNormalAndTangentPerFrame)
            {
                normal = tangent = true;
            }
            MagicaPhysicsManager.Instance.Mesh.SetRenderMeshFlag(MeshIndex, PhysicsManagerMeshData.Meshflag_CalcNormal, normal);
            MagicaPhysicsManager.Instance.Mesh.SetRenderMeshFlag(MeshIndex, PhysicsManagerMeshData.Meshflag_CalcTangent, tangent);
        }

        /// <summary>
        /// UnityPhysics?????????
        /// </summary>
        /// <param name="sw"></param>
        public override void ChangeUseUnityPhysics(bool sw)
        {
            if (status.IsInitSuccess)
            {
                MagicaPhysicsManager.Instance.Mesh.ChangeRenderMeshUseUnityPhysics(MeshIndex, sw);
            }
        }

        public void ChangeCalculation(bool sw)
        {
            if (status.IsInitSuccess)
            {
                MagicaPhysicsManager.Instance.Mesh.SetRenderMeshFlag(MeshIndex, PhysicsManagerMeshData.Meshflag_Pause, !sw);
            }
        }

        //=========================================================================================
        public override bool IsMeshUse()
        {
            if (status.IsInitSuccess)
            {
                return MagicaPhysicsManager.Instance.Mesh.IsUseRenderMesh(MeshIndex);
            }

            return false;
        }

        public override bool IsActiveMesh()
        {
            if (status.IsInitSuccess)
            {
                return MagicaPhysicsManager.Instance.Mesh.IsActiveRenderMesh(MeshIndex);
            }

            return false;
        }

        public override void AddUseMesh(System.Object parent)
        {
            var virtualMeshDeformer = parent as VirtualMeshDeformer;
            Debug.Assert(virtualMeshDeformer != null);

            if (status.IsInitSuccess)
            {
                //Develop.Log($"?AddUseMesh:{this.Parent.name} meshIndex:{MeshIndex}");

                MagicaPhysicsManager.Instance.Mesh.AddUseRenderMesh(MeshIndex);
                IsChangePosition = true;
                IsChangeNormalTangent = true;
                IsChangeBoneWeights = true;

                // ?????????????
                int virtualMeshIndex = virtualMeshDeformer.MeshIndex;
                var virtualMeshInfo = MagicaPhysicsManager.Instance.Mesh.virtualMeshInfoList[virtualMeshIndex];
                var sharedVirtualMeshInfo = MagicaPhysicsManager.Instance.Mesh.sharedVirtualMeshInfoList[virtualMeshInfo.sharedVirtualMeshIndex];
                int index = virtualMeshDeformer.GetRenderMeshDeformerIndex(this);
                long cuid = (long)sharedVirtualMeshInfo.uid << 16 + index;
                int sharedChildMeshIndex = MagicaPhysicsManager.Instance.Mesh.sharedChildMeshIdToSharedVirtualMeshIndexDict[cuid];
                var sharedChildMeshInfo = MagicaPhysicsManager.Instance.Mesh.sharedChildMeshInfoList[sharedChildMeshIndex];

                MagicaPhysicsManager.Instance.Mesh.LinkRenderMesh(
                    MeshIndex,
                    sharedChildMeshInfo.vertexChunk.startIndex,
                    sharedChildMeshInfo.weightChunk.startIndex,
                    virtualMeshInfo.vertexChunk.startIndex,
                    sharedVirtualMeshInfo.vertexChunk.startIndex
                    );

                // ??????
                //MagicaPhysicsManager.Instance.Compute.RenderMeshWorker.SetUpdateUseFlag();
            }
        }

        public override void RemoveUseMesh(System.Object parent)
        {
            //base.RemoveUseMesh();

            var virtualMeshDeformer = parent as VirtualMeshDeformer;
            Debug.Assert(virtualMeshDeformer != null);

            if (status.IsInitSuccess)
            {
                // ????????????????
                int virtualMeshIndex = virtualMeshDeformer.MeshIndex;
                var virtualMeshInfo = MagicaPhysicsManager.Instance.Mesh.virtualMeshInfoList[virtualMeshIndex];
                var sharedVirtualMeshInfo = MagicaPhysicsManager.Instance.Mesh.sharedVirtualMeshInfoList[virtualMeshInfo.sharedVirtualMeshIndex];
                int index = virtualMeshDeformer.GetRenderMeshDeformerIndex(this);
                long cuid = (long)sharedVirtualMeshInfo.uid << 16 + index;
                int sharedChildMeshIndex = MagicaPhysicsManager.Instance.Mesh.sharedChildMeshIdToSharedVirtualMeshIndexDict[cuid];
                var sharedChildMeshInfo = MagicaPhysicsManager.Instance.Mesh.sharedChildMeshInfoList[sharedChildMeshIndex];

                MagicaPhysicsManager.Instance.Mesh.UnlinkRenderMesh(
                    MeshIndex,
                    sharedChildMeshInfo.vertexChunk.startIndex,
                    sharedChildMeshInfo.weightChunk.startIndex,
                    virtualMeshInfo.vertexChunk.startIndex,
                    sharedVirtualMeshInfo.vertexChunk.startIndex
                    );


                MagicaPhysicsManager.Instance.Mesh.RemoveUseRenderMesh(MeshIndex);
                IsChangePosition = true;
                IsChangeNormalTangent = true;
                IsChangeBoneWeights = true;

                // ??????
                //MagicaPhysicsManager.Instance.Compute.RenderMeshWorker.SetUpdateUseFlag();
            }
        }

        //=========================================================================================
        public bool IsRendererVisible
        {
            get
            {
                return renderer ? renderer.isVisible : false;
            }
        }

        internal bool HasNormal
        {
            get
            {
                return normalAndTangentUpdateMode == RecalculateMode.UpdateNormalPerFrame || normalAndTangentUpdateMode == RecalculateMode.UpdateNormalAndTangentPerFrame;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="bufferIndex"></param>
        internal override void MeshCalculation(int bufferIndex)
        {
            IsWriteMeshPosition = false;
            IsWriteMeshNormal = false;
            IsWriteMeshTangent = false;
            IsWriteMeshBoneWeight = false;

            bool use = IsMeshUse();

            // ????
            if (Parent.IsCalculate == false && Status.IsActive)
            {
                //Debug.Log($"Finish Skip! :{Parent.name}");
                switch ((Parent as MagicaRenderDeformer)?.cullModeCash)
                {
                    case PhysicsTeam.TeamCullingMode.Pause:
                        // ????
                        return;
                    case PhysicsTeam.TeamCullingMode.Reset:
                        // ?????????
                        use = false;
                        break;
                }
            }

            // ?????/?????????
            bool vertexCalc = false;
            if (use)
            {
                if (bufferIndex == 1)
                {
                    var state = MagicaPhysicsManager.Instance.Mesh.renderMeshStateDict[MeshIndex];
                    vertexCalc = state.IsFlag(PhysicsManagerMeshData.RenderStateFlag_DelayedCalculated);
                }
                else
                    vertexCalc = true;
            }
            if (vertexCalc == false)
            {
                use = false;
            }

            if (use && IsWriteSkip)
            {
                use = false;
                IsWriteSkip = false;
            }

            //Debug.Log($"Write Mesh. MeshUse:{IsMeshUse()} use:{use} vertexCalc:{vertexCalc} Calc:{Parent.IsCalculate} buffIndex:{bufferIndex} F:{Time.frameCount}");

#if true
            // ??????
            // ??????????????????????
            if (use != oldUse)
            {
                if (meshFilter)
                {
                    meshFilter.mesh = use ? cloneMesh : sharedMesh;
                }
                else if (skinMeshRenderer)
                {
                    skinMeshRenderer.sharedMesh = use ? cloneMesh : sharedMesh;
                    skinMeshRenderer.bones = use ? boneList : originalBones;
                }
                oldUse = use;

                if (use)
                {
                    IsChangePosition = true;
                    IsChangeNormalTangent = true;
                    IsChangeBoneWeights = true;
                }
                else
                {
                    // ????????????????
                    if (vertexBuffer != null)
                    {
                        vertexBuffer.Dispose();
                        vertexBuffer = null;
                    }
                }
            }

            // ??????????
            if (vertexCalc == false)
                return;

            // ??/???????
            bool normal = normalAndTangentUpdateMode == RecalculateMode.UpdateNormalPerFrame || normalAndTangentUpdateMode == RecalculateMode.UpdateNormalAndTangentPerFrame;
            bool tangent = normalAndTangentUpdateMode == RecalculateMode.UpdateNormalAndTangentPerFrame;

            // ?????/?????????????
            if (IsChangeNormalTangent && normal == false && tangent == false)
            {
                // ????
                cloneMesh.normals = sharedMesh.normals;
                cloneMesh.tangents = sharedMesh.tangents;
                IsChangeNormalTangent = false;
            }

            {
                // ???????????????
                if ((use || IsChangePosition))
                {
                    IsWriteMeshPosition = true;
                    if (normal)
                        IsWriteMeshNormal = true;
                    if (tangent)
                        IsWriteMeshTangent = true;
                    IsChangePosition = false;
                }
            }
            if (use && IsSkinning && IsChangeBoneWeights)
            {
                // ??????????
                //Debug.Log("Change Mesh Weights:" + mesh.name + " buff:" + bufferIndex + " frame:" + Time.frameCount);
                IsWriteMeshBoneWeight = true;
                IsChangeBoneWeights = false;
            }
#endif
        }


        /// <summary>
        /// ???????????
        /// </summary>
        internal override void NormalWriting(int bufferIndex)
        {
            if (IsWriteMeshPosition)
            {
                // ???????????(??)
                MagicaPhysicsManager.Instance.Mesh.CopyToRenderMeshLocalPositionData(MeshIndex, cloneMesh, bufferIndex);
                if (IsWriteMeshNormal || IsWriteMeshTangent)
                {
                    MagicaPhysicsManager.Instance.Mesh.CopyToRenderMeshLocalNormalTangentData(MeshIndex, cloneMesh, bufferIndex, IsWriteMeshNormal, IsWriteMeshTangent);
                }
            }

            if (IsWriteMeshBoneWeight)
            {
                //Debug.Log($"BoneWeights Write:{renderer.name} F:{Time.frameCount}");
                // ????????
                MagicaPhysicsManager.Instance.Mesh.CopyToRenderMeshBoneWeightData(MeshIndex, cloneMesh, sharedMesh, bufferIndex);

#if UNITY_2021_2_OR_NEWER
                // ????????????????????VertexBuffer????????????????
                vertexBuffer?.Release();
                vertexBuffer?.Dispose();
                vertexBuffer = null;
#endif
            }
        }

        /// <summary>
        /// ???????/???????????
        /// </summary>
        public void ChangeNormalTangentUpdateMode()
        {
            // ??/????????????????????
            IsChangeNormalTangent = true;
        }

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        public void ReplaceBone<T>(Dictionary<T, Transform> boneReplaceDict) where T : class
        {
            if (originalBones != null)
            {
                for (int i = 0; i < originalBones.Length; i++)
                {
                    originalBones[i] = MeshUtility.GetReplaceBone(originalBones[i], boneReplaceDict);
                }
            }

            if (boneList != null)
            {
                for (int i = 0; i < boneList.Length; i++)
                {
                    boneList[i] = MeshUtility.GetReplaceBone(boneList[i], boneReplaceDict);
                }
            }
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        public HashSet<Transform> GetUsedBones()
        {
            var bonesSet = new HashSet<Transform>();
            if (originalBones != null)
                foreach (var t in originalBones)
                    bonesSet.Add(t);
            if (boneList != null)
                foreach (var t in boneList)
                    bonesSet.Add(t);
            return bonesSet;
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
                if (Status.IsDispose)
                    return 0;

                if (IsMeshUse() == false || TargetObject == null)
                    return 0;

                Vector3[] posArray = new Vector3[VertexCount];
                Vector3[] norArray = new Vector3[VertexCount];
                Vector3[] tanArray = new Vector3[VertexCount];
                var meshRootTransform = skinMeshRenderer ? skinMeshRenderer.rootBone : TargetObject.transform; // (old)TargetObject.transform
                MagicaPhysicsManager.Instance.Mesh.CopyToRenderMeshWorldData(MeshIndex, meshRootTransform, posArray, norArray, tanArray);

                wposList = new List<Vector3>(posArray);
                wnorList = new List<Vector3>(norArray);
                wtanList = new List<Vector3>(tanArray);

                return VertexCount;
            }
            else
            {
                if (TargetObject == null)
                {
                    return 0;
                }
                var ren = TargetObject.GetComponent<Renderer>();
                MeshUtility.CalcMeshWorldPositionNormalTangent(ren, sharedMesh, out wposList, out wnorList, out wtanList);

                return wposList.Count;
            }
        }

        /// <summary>
        /// ??????????????????(???????)
        /// </summary>
        /// <returns></returns>
        public override List<int> GetEditorTriangleList()
        {
            if (sharedMesh)
            {
                return new List<int>(sharedMesh.triangles);
            }

            return null;
        }

        /// <summary>
        /// ??????????????(?????)
        /// </summary>
        /// <returns></returns>
        public override List<int> GetEditorLineList()
        {
            // ?????????????????
            return null;
        }

        /// <summary>
        /// ???????????????(?????)
        /// </summary>
        /// <returns></returns>
        public List<int> GetEditorUseList()
        {
            if (Application.isPlaying && IsMeshUse())
            {
                return MagicaPhysicsManager.Instance.Mesh.GetVertexUseList(MeshIndex);
            }
            else
                return null;
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
            var baseError = base.VerifyData();
            if (baseError != Define.Error.None)
                return baseError;

            if (sharedMesh == null)
                return Define.Error.SharedMeshNull;
            if (sharedMesh.isReadable == false)
                return Define.Error.SharedMeshCannotRead;
            var targetMesh = GetTargetSharedMesh();
            if (MeshData != null && targetMesh != null && MeshData.vertexCount != targetMesh.vertexCount)
                return Define.Error.SharedMeshDifferentVertexCount; // ?????????????????????

            // ??????65535(?????????????)
            if (sharedMesh.vertexCount > 65535)
                return Define.Error.MeshVertexCount65535Over;

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                // ???????????????????????????????NG
                // ????????????????????????????
                if (meshOptimize != 0 && meshOptimize != EditUtility.GetOptimizeMesh(sharedMesh))
                    return Define.Error.MeshOptimizeMismatch;

                // KeepQuads???????(v1.11.1)
                if (EditUtility.IsKeepQuadsMesh(sharedMesh))
                    return Define.Error.MeshKeepQuads;
            }
#endif

            return Define.Error.None;
        }

        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        private Mesh GetTargetSharedMesh()
        {
            if (TargetObject == null)
            {
                return null;
            }
            var ren = TargetObject.GetComponent<Renderer>();
            if (ren == null)
            {
                return null;
            }

            if (ren is SkinnedMeshRenderer)
            {
                var sren = ren as SkinnedMeshRenderer;
                return sren.sharedMesh;
            }
            else
            {
                meshFilter = TargetObject.GetComponent<MeshFilter>();
                return meshFilter.sharedMesh;
            }
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
                StaticStringBuilder.AppendLine("Skinning: ", MeshData.isSkinning);
                StaticStringBuilder.AppendLine("Vertex: ", MeshData.VertexCount);
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
                    StaticStringBuilder.AppendLine("Please create the mesh data.");
                }
                StaticStringBuilder.Append(Define.GetErrorMessage(err));
            }

            return StaticStringBuilder.ToString();
        }
    }
}
