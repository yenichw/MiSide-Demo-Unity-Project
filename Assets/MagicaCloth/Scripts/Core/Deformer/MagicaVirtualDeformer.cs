// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????????????????????
    /// </summary>
    [HelpURL("https://magicasoft.jp/magica-cloth-virtual-deformer/")]
    [AddComponentMenu("MagicaCloth/MagicaVirtualDeformer")]
    public class MagicaVirtualDeformer : CoreComponent
    {
        /// <summary>
        /// ????????
        /// </summary>
        private const int DATA_VERSION = 2;

        /// <summary>
        /// ???????????
        /// </summary>
        private const int ERR_DATA_VERSION = 0;

        /// <summary>
        /// ?????????????
        /// </summary>
        [SerializeField]
        private VirtualMeshDeformer deformer = new VirtualMeshDeformer();

        [SerializeField]
        private int deformerHash;
        [SerializeField]
        private int deformerVersion;

        /// <summary>
        /// ?????????????(-1=???????)
        /// </summary>
        internal PhysicsTeam.TeamCullingMode cullModeCash { get; private set; } = (PhysicsTeam.TeamCullingMode)(-1);

        //=========================================================================================
        public override ComponentType GetComponentType()
        {
            return ComponentType.VirtualDeformer;
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public override int GetDataHash()
        {
            int hash = 0;
            hash += Deformer.GetDataHash();
            return hash;
        }

        //=========================================================================================
        public VirtualMeshDeformer Deformer
        {
            get
            {
                deformer.Parent = this;
                return deformer;
            }
        }

        //=========================================================================================
        void OnValidate()
        {
            //Deformer.OnValidate();
        }

        protected override void OnInit()
        {
            LinkRenderDeformerStatus(true);
            Deformer.Init();
        }

        protected override void OnDispose()
        {
            Deformer.Dispose();
            LinkRenderDeformerStatus(false);
        }

        protected override void OnUpdate()
        {
            Deformer.Update();
        }

        protected override void OnActive()
        {
            Deformer.OnEnable();
        }

        protected override void OnInactive()
        {
            Deformer.OnDisable();
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <param name="sw"></param>
        private void LinkRenderDeformerStatus(bool sw)
        {
            int cnt = Deformer.RenderDeformerCount;
            for (int i = 0; i < cnt; i++)
            {
                var rd = Deformer.GetRenderDeformer(i);
                if (rd != null)
                {
                    // ???MagicaVirtualDeformer <-> MagicaRenderDeformer?????
                    if (sw)
                    {
                        rd.Status.LinkParentStatus(status);
                    }
                    else
                    {
                        rd.Status.UnlinkParentStatus(status);
                    }
                }
            }
        }

        /// <summary>
        /// UnityPhyiscs???????
        /// ????????????????????????????
        /// </summary>
        /// <param name="sw"></param>
        protected override void ChangeUseUnityPhysics(bool sw)
        {
            Deformer.ChangeUseUnityPhysics(sw);

            // ????????????
            for (int i = 0; i < Deformer.RenderDeformerCount; i++)
            {
                Deformer.GetRenderDeformer(i)?.SetUseUnityPhysics(sw);
            }
        }

        //=========================================================================================
        internal override void UpdateCullingMode(CoreComponent caller)
        {
            // ???????(????????????????)
            cullModeCash = 0;
            foreach (var status in Status.childStatusSet)
            {
                if (status != null)
                {
                    var owner = status.OwnerFunc() as BaseCloth;
                    if (owner != null)
                    {
                        var ownerCull = owner.CullingMode;
                        if (ownerCull > cullModeCash)
                            cullModeCash = ownerCull;
                    }
                }
            }

            // ????(???????????????)
            bool visible = false;
            if (cullModeCash == PhysicsTeam.TeamCullingMode.Off)
            {
                visible = true;
            }
            else
            {
                for (int i = 0; i < Deformer.RenderDeformerCount; i++)
                {
                    var rd = Deformer.GetRenderDeformer(i);
                    if (rd && rd.IsVisible)
                    {
                        visible = true;
                        break;
                    }
                }
            }
            IsVisible = visible;

            // ????
            bool stopInvisible = cullModeCash != PhysicsTeam.TeamCullingMode.Off;
            bool calc = true;
            if (stopInvisible)
            {
                calc = visible;
            }
            var val = calc ? 1 : 0;
            if (calculateValue != val)
            {
                calculateValue = val;
                OnChangeCalculation();
            }

            // ???????????????????????????
            foreach (var status in Status.childStatusSet)
            {
                var core = status?.OwnerFunc() as CoreComponent;
                if (core && core != caller)
                    core.UpdateCullingMode(this);
            }
        }

        protected override void OnChangeCalculation()
        {
            //Debug.Log($"VD [{this.name}] Visible:{IsVisible} Calc:{IsCalculate} F:{Time.frameCount}");
            Deformer.ChangeCalculation(IsCalculate);
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
            deformerHash = Deformer.SaveDataHash;
            deformerVersion = Deformer.SaveDataVersion;
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

            var d = Deformer;
            if (d == null)
                return Define.Error.DeformerNull;

            var deformerError = d.VerifyData();
            if (deformerError != Define.Error.None)
                return deformerError;

            if (deformerHash != d.SaveDataHash)
                return Define.Error.DeformerHashMismatch;
            if (deformerVersion != d.SaveDataVersion)
                return Define.Error.DeformerVersionMismatch;

            return Define.Error.None;
        }

        public override string GetInformation()
        {
            if (Deformer != null)
                return Deformer.GetInformation();
            else
                return base.GetInformation();
        }

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="boneReplaceDict"></param>
        public override void ReplaceBone<T>(Dictionary<T, Transform> boneReplaceDict)
        {
            base.ReplaceBone(boneReplaceDict);

            Deformer.ReplaceBone(boneReplaceDict);
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        public override HashSet<Transform> GetUsedBones()
        {
            var bones = base.GetUsedBones();
            bones.UnionWith(Deformer.GetUsedBones());
            return bones;
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
        /// ???1????????????
        /// ??????????null???
        /// </summary>
        /// <returns></returns>
        public override List<int> GetUseList()
        {
            if (Application.isPlaying)
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
            else
                return null;
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        public override List<ShareDataObject> GetAllShareDataObject()
        {
            var slist = base.GetAllShareDataObject();
            slist.Add(Deformer.MeshData);
            return slist;
        }

        /// <summary>
        /// source?????????????????
        /// ??????????????
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
        {
            if (Deformer.MeshData == source)
            {
                //Deformer.MeshData = Instantiate(Deformer.MeshData);
                Deformer.MeshData = ShareDataObject.Clone(Deformer.MeshData);
                return Deformer.MeshData;
            }

            return null;
        }
    }
}
