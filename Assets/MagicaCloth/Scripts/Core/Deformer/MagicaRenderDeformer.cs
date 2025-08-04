// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace MagicaCloth
{
    /// <summary>
    /// ??????????????????????
    /// </summary>
    [HelpURL("https://magicasoft.jp/magica-cloth-render-deformer/")]
    [AddComponentMenu("MagicaCloth/MagicaRenderDeformer")]
    public class MagicaRenderDeformer : CoreComponent
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
        /// ???????????????
        /// </summary>
        [SerializeField]
        private RenderMeshDeformer deformer = new RenderMeshDeformer();

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
            return ComponentType.RenderDeformer;
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
        public RenderMeshDeformer Deformer
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
            Deformer.OnValidate();
        }

        protected override void OnInit()
        {
            Deformer.Init();
        }

        protected override void OnDispose()
        {
            Deformer.Dispose();
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

        protected void OnBecameVisible()
        {
            //Debug.Log("RD Visible");
            if (MagicaPhysicsManager.IsInstance())
                UpdateCullingMode(this);
        }

        protected void OnBecameInvisible()
        {
            //Debug.Log("RD Invisible");
            if (MagicaPhysicsManager.IsInstance())
                UpdateCullingMode(this);
        }


        //=========================================================================================
        internal override void UpdateCullingMode(CoreComponent caller)
        {
            // ???????(VirtualDeformer??????)
            cullModeCash = 0;
            foreach (var status in Status.parentStatusSet)
            {
                if (status != null)
                {
                    var owner = status.OwnerFunc() as MagicaVirtualDeformer;
                    if (owner != null)
                    {
                        var ownerCull = owner.cullModeCash;
                        if (ownerCull > cullModeCash)
                            cullModeCash = ownerCull;
                    }
                }
            }

            // ????
            // ???????????????????????????????????IsVisible????????
            // ????????????????????
            var visible = true;
            if (cullModeCash != PhysicsTeam.TeamCullingMode.Off)
                visible = Deformer.IsRendererVisible;
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

            // VirtualDeformer???
            foreach (var status in Status.parentStatusSet)
            {
                var core = status?.OwnerFunc() as CoreComponent;
                if (core && core != caller)
                    core.UpdateCullingMode(this);
            }
        }

        protected override void OnChangeCalculation()
        {
            //Debug.Log($"RD [{this.name}] Visible:{IsVisible} Calc:{IsCalculate} F:{Time.frameCount}");
            Deformer.ChangeCalculation(IsCalculate);

            if (IsCalculate && MagicaPhysicsManager.Instance.IsDelay && cullModeCash == PhysicsTeam.TeamCullingMode.Reset)
            {
                // ?????????1????????
                Deformer.IsWriteSkip = true;
            }
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

            if (Deformer == null)
                return Define.Error.DeformerNull;

            var deformerError = Deformer.VerifyData();
            if (deformerError != Define.Error.None)
                return deformerError;

            if (deformerHash != Deformer.SaveDataHash)
                return Define.Error.DeformerHashMismatch;
            if (deformerVersion != Deformer.SaveDataVersion)
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
        /// UnityPhyiscs???????
        /// ????????????????????????????
        /// </summary>
        /// <param name="sw"></param>
        protected override void ChangeUseUnityPhysics(bool sw)
        {
            Deformer.ChangeUseUnityPhysics(sw);
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
            return Deformer.GetEditorUseList();
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
