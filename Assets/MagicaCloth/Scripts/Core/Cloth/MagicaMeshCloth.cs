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
    /// ???????
    /// </summary>
    [HelpURL("https://magicasoft.jp/magica-cloth-mesh-cloth/")]
    [AddComponentMenu("MagicaCloth/MagicaMeshCloth", 100)]
    public class MagicaMeshCloth : BaseCloth
    {
        /// <summary>
        /// ????????
        /// </summary>
        private const int DATA_VERSION = 7;

        /// <summary>
        /// ???????????
        /// </summary>
        private const int ERR_DATA_VERSION = 3;

        /// <summary>
        /// ????????????
        /// </summary>
        [SerializeField]
        private MagicaVirtualDeformer virtualDeformer = null;

        [SerializeField]
        private int virtualDeformerHash;
        [SerializeField]
        private int virtualDeformerVersion;

        //=========================================================================================
        public override ComponentType GetComponentType()
        {
            return ComponentType.MeshCloth;
        }

        //=========================================================================================
        /// <summary>
        /// ???????????
        /// </summary>
        /// <returns></returns>
        public override int GetDataHash()
        {
            int hash = base.GetDataHash();
            hash += virtualDeformer.GetDataHash();
            return hash;
        }

        //=========================================================================================
        public VirtualMeshDeformer Deformer
        {
            get
            {
                if (virtualDeformer != null)
                    return virtualDeformer.Deformer;
                return null;
            }
        }

        //=========================================================================================
        protected override void Reset()
        {
            base.Reset();
            ResetParams();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
        }

        protected override void OnInit()
        {
            base.OnInit();
        }

        protected override void OnActive()
        {
            base.OnActive();
        }

        protected override void OnInactive()
        {
            base.OnInactive();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
        }

        //=========================================================================================
        /// <summary>
        /// ????????????????(??????0)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected override uint UserFlag(int index)
        {
            // ???????????
            return 0;
        }

        /// <summary>
        /// ?????????????????(??????null)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected override Transform UserTransform(int index)
        {
            // ???????????
            return null;
        }

        /// <summary>
        /// ????????????????LocalPosition???(??????0)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected override float3 UserTransformLocalPosition(int vindex)
        {
            // ???????????
            return 0;
        }

        /// <summary>
        /// ????????????????LocalRotation???(??????quaternion.identity)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected override quaternion UserTransformLocalRotation(int vindex)
        {
            // ???????????
            return quaternion.identity;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <returns></returns>
        public override bool IsRequiresDeformer()
        {
            return true;
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override BaseMeshDeformer GetDeformer()
        {
            return Deformer;
        }

        /// <summary>
        /// ???????????MeshData???(????null)
        /// </summary>
        /// <returns></returns>
        protected override MeshData GetMeshData()
        {
            return Deformer.MeshData;
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        protected override void WorkerInit()
        {
            // ????????????????????
            var meshParticleWorker = MagicaPhysicsManager.Instance.Compute.MeshParticleWorker;
            var minfo = MagicaPhysicsManager.Instance.Mesh.GetVirtualMeshInfo(Deformer.MeshIndex);
            var cdata = ClothData;
            for (int i = 0; i < cdata.VertexUseCount; i++)
            {
                int pindex = particleChunk.startIndex + i;
                int vindex = minfo.vertexChunk.startIndex + cdata.useVertexList[i];

                if (pindex >= 0)
                    meshParticleWorker.Add(TeamId, vindex, pindex);
            }
        }

        /// <summary>
        /// ???????????????
        /// ???????? AddUseVertex() / RemoveUseVertex() ?????
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="deformer"></param>
        protected override void SetDeformerUseVertex(bool sw, BaseMeshDeformer deformer)
        {
            var cdata = ClothData;
            for (int i = 0; i < cdata.VertexUseCount; i++)
            {
                // ????????
                if (ClothData.IsInvalidVertex(i))
                    continue;

                int vindex = cdata.useVertexList[i];
                bool fix = !ClothData.IsMoveVertex(i);

                if (sw)
                    deformer.AddUseVertex(vindex, fix);
                else
                    deformer.RemoveUseVertex(vindex, fix);
            }
        }

        /// <summary>
        /// UnityPhyiscs???????
        /// ????????????????????????????
        /// </summary>
        /// <param name="sw"></param>
        protected override void ChangeUseUnityPhysics(bool sw)
        {
            base.ChangeUseUnityPhysics(sw);

            // ????????
            virtualDeformer?.SetUseUnityPhysics(sw);
        }

        //=========================================================================================
        public override int GetVersion()
        {
            return DATA_VERSION;
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <returns></returns>
        public override int GetErrorVersion()
        {
            return ERR_DATA_VERSION;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns></returns>
        public override void CreateVerifyData()
        {
            base.CreateVerifyData();
            virtualDeformerHash = virtualDeformer.SaveDataHash;
            virtualDeformerVersion = virtualDeformer.SaveDataVersion;
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

            if (ClothData == null)
                return Define.Error.ClothDataNull;
            if (virtualDeformer == null)
                return Define.Error.DeformerNull;
            var vdeformerError = virtualDeformer.VerifyData();
            if (vdeformerError != Define.Error.None)
                return vdeformerError;
            if (virtualDeformerHash != virtualDeformer.SaveDataHash)
                return Define.Error.DeformerHashMismatch;
            if (virtualDeformerVersion != virtualDeformer.SaveDataVersion)
                return Define.Error.DeformerVersionMismatch;

            return Define.Error.None;
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        /// <returns></returns>
        public override string GetInformation()
        {
            // ??????????
            StaticStringBuilder.Clear();

            var err = VerifyData();
            if (err == Define.Error.None)
            {
                // OK
                var cdata = ClothData;
                StaticStringBuilder.AppendLine("Active: ", Status.IsActive);
                StaticStringBuilder.AppendLine($"Visible: {IsVisible}");
                StaticStringBuilder.AppendLine($"Calculation:{IsCalculate}");
                StaticStringBuilder.AppendLine("Vertex: ", cdata.VertexUseCount);
                StaticStringBuilder.AppendLine("Clamp Distance: ", cdata.ClampDistanceConstraintCount);
                StaticStringBuilder.AppendLine("Clamp Position: ", clothParams.UseClampPositionLength ? cdata.VertexUseCount : 0);
                StaticStringBuilder.AppendLine("Clamp Rotation [", cdata.clampRotationAlgorithm, "] : ", cdata.GetClampRotationCount());
                StaticStringBuilder.AppendLine("Struct Distance: ", cdata.StructDistanceConstraintCount / 2);
                StaticStringBuilder.AppendLine("Bend Distance: ", cdata.BendDistanceConstraintCount / 2);
                StaticStringBuilder.AppendLine("Near Distance: ", cdata.NearDistanceConstraintCount / 2);
                StaticStringBuilder.AppendLine("Restore Rotation [", cdata.restoreRotationAlgorithm, "] : ", cdata.GetRestoreRotationCount());
                StaticStringBuilder.AppendLine("Triangle Bend [", cdata.triangleBendAlgorithm, "] : ", cdata.TriangleBendConstraintCount);
                StaticStringBuilder.AppendLine("Collider: ", teamData.ColliderCount);
                StaticStringBuilder.Append("Line Rotation: ", cdata.LineRotationWorkerCount);
            }
            else if (err == Define.Error.EmptyData)
            {
                StaticStringBuilder.Append(Define.GetErrorMessage(err));
            }
            else
            {
                // ???
                StaticStringBuilder.AppendLine("This mesh cloth is in a state error!");
                if (Application.isPlaying)
                {
                    StaticStringBuilder.AppendLine("Execution stopped.");
                }
                else
                {
                    StaticStringBuilder.AppendLine("Please recreate the cloth data.");
                }
                StaticStringBuilder.Append(Define.GetErrorMessage(err));
            }

            return StaticStringBuilder.ToString();
        }

        public bool IsValidPointSelect()
        {
            if (ClothSelection == null)
                return false;

            if (Deformer.MeshData.ChildCount != ClothSelection.DeformerCount)
                return false;

            return true;
        }

        //=========================================================================================
        /// <summary>
        /// ???????????/??/?????(?????)
        /// </summary>
        /// <param name="wposList"></param>
        /// <param name="wnorList"></param>
        /// <param name="wtanList"></param>
        /// <returns>???</returns>
        public override int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
        {
            return Deformer.GetEditorPositionNormalTangent(out wposList, out wnorList, out wtanList);
        }

        /// <summary>
        /// ??????????????????(?????)
        /// </summary>
        /// <returns></returns>
        public override List<int> GetEditorTriangleList()
        {
            return Deformer.GetEditorTriangleList();
        }

        /// <summary>
        /// ??????????????(?????)
        /// </summary>
        /// <returns></returns>
        public override List<int> GetEditorLineList()
        {
            return Deformer.GetEditorLineList();
        }

        //=========================================================================================
        /// <summary>
        /// ????????????????(?????)
        /// ????? ClothSelection.Invalid / ClothSelection.Fixed / ClothSelection.Move
        /// ????Invalid???null???
        /// </summary>
        /// <returns></returns>
        public override List<int> GetSelectionList()
        {
            if (ClothSelection != null && virtualDeformer != null && Deformer.MeshData != null)
                return ClothSelection.GetSelectionData(Deformer.MeshData, Deformer.GetRenderDeformerMeshList());
            else
                return null;
        }

        /// <summary>
        /// ????????????????(?????)
        /// ???1????????????
        /// ??????????null???
        /// </summary>
        /// <returns></returns>
        public override List<int> GetUseList()
        {
            if (Application.isPlaying && virtualDeformer != null)
            {
                if (Deformer != null)
                {
                    var minfo = MagicaPhysicsManager.Instance.Mesh.GetVirtualMeshInfo(Deformer.MeshIndex);
                    //var infoList = MagicaPhysicsManager.Instance.Mesh.virtualVertexInfoList;
                    var vertexUseList = MagicaPhysicsManager.Instance.Mesh.virtualVertexUseList;

                    var useList = new List<int>();
                    for (int i = 0; i < minfo.vertexChunk.dataLength; i++)
                    {
                        //uint data = infoList[minfo.vertexChunk.startIndex + i];
                        //useList.Add((int)(data & 0xffff));

                        useList.Add(vertexUseList[minfo.vertexChunk.startIndex + i]);
                    }
                    return useList;
                }
            }

            return null;
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        public override List<ShareDataObject> GetAllShareDataObject()
        {
            var sdata = base.GetAllShareDataObject();
            if (Deformer != null)
                sdata.Add(Deformer.MeshData);
            return sdata;
        }

        /// <summary>
        /// source?????????????????
        /// ??????????????
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
        {
            var sdata = base.DuplicateShareDataObject(source);
            if (sdata != null)
                return sdata;

            if (Deformer.MeshData == source)
            {
                //Deformer.MeshData = Instantiate(Deformer.MeshData);
                Deformer.MeshData = ShareDataObject.Clone(Deformer.MeshData);
                return Deformer.MeshData;
            }

            return null;
        }

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        void ResetParams()
        {
            clothParams.AlgorithmType = ClothParams.Algorithm.Algorithm_2;
            clothParams.SetRadius(0.02f, 0.02f);
            clothParams.SetMass(10.0f, 1.0f, true, -0.5f, true);
            clothParams.SetGravity(true, -5.0f, -5.0f);
            clothParams.SetDrag(true, 0.01f, 0.01f);
            clothParams.SetMaxVelocity(true, 3.0f, 3.0f);
            clothParams.SetWorldInfluence(3.0f, 0.5f, 1.0f);
            clothParams.SetTeleport(false);
            clothParams.SetClampDistanceRatio(true, 0.5f, 1.05f, 0.1f);
            clothParams.SetClampPositionLength(false, 0.0f, 0.4f);
            clothParams.SetClampRotationAngle(false, 0.0f, 180.0f, 0.2f);
            clothParams.SetRestoreDistance(1.0f);
            clothParams.SetRestoreRotation(false, 0.03f, 0.005f, 0.3f);
            clothParams.SetSpring(false);
            clothParams.SetAdjustRotation();
            clothParams.SetTriangleBend(true, 1.0f, 1.0f);
            clothParams.SetVolume(false);
            clothParams.SetCollision(false, 0.1f, 0.03f);
            clothParams.SetExternalForce(0.3f, 1.0f, 0.7f, 0.6f);
        }
    }
}
