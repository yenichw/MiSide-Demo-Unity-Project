// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicaCloth
{
    /// <summary>
    /// ????????
    /// </summary>
    public abstract partial class BaseCloth : PhysicsTeam
    {
        /// <summary>
        /// ???????
        /// </summary>
        [SerializeField]
        protected ClothParams clothParams = new ClothParams();

        [SerializeField]
        protected List<int> clothParamDataHashList = new List<int>();

        /// <summary>
        /// ??????
        /// </summary>
        [SerializeField]
        private ClothData clothData = null;

        [SerializeField]
        protected int clothDataHash;
        [SerializeField]
        protected int clothDataVersion;

        /// <summary>
        /// ???????
        /// </summary>
        [SerializeField]
        private SelectionData clothSelection = null;

        [SerializeField]
        private int clothSelectionHash;
        [SerializeField]
        private int clothSelectionVersion;

        /// <summary>
        /// ?????????????
        /// BoneCloth / BoneSpring ???
        /// </summary>
        [SerializeField]
        private List<Renderer> cullRendererList = new List<Renderer>();

        /// <summary>
        /// ??????????
        /// </summary>
        protected ClothSetup setup = new ClothSetup();


        //=========================================================================================
        private float oldBlendRatio = -1.0f;
        private TeamUpdateMode oldUpdateMode = 0;
        private TeamCullingMode oldCullingMode = 0;
        private bool oldUseAnimatedDistance = false;

        //=========================================================================================
        /// <summary>
        /// ???????????
        /// </summary>
        /// <returns></returns>
        public override int GetDataHash()
        {
            int hash = base.GetDataHash();
            if (ClothData != null)
                hash += ClothData.GetDataHash();
            if (ClothSelection != null)
                hash += ClothSelection.GetDataHash();

            return hash;
        }

        //=========================================================================================
        public ClothParams Params
        {
            get
            {
                return clothParams;
            }
        }

        public ClothData ClothData
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    return clothData;
                else
                {
                    // unity2019.3????null?????????(??)
                    var so = new SerializedObject(this);
                    return so.FindProperty("clothData").objectReferenceValue as ClothData;
                }
#else
                return clothData;
#endif
            }
            set
            {
                clothData = value;
            }
        }

        public SelectionData ClothSelection
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    return clothSelection;
                else
                {
                    // unity2019.3????null?????????(??)
                    var so = new SerializedObject(this);
                    return so.FindProperty("clothSelection").objectReferenceValue as SelectionData;
                }
#else
                return clothSelection;
#endif
            }
        }

        public ClothSetup Setup
        {
            get
            {
                return setup;
            }
        }

        //=========================================================================================
        protected virtual void Reset()
        {
        }

        protected virtual void OnValidate()
        {
            if (Application.isPlaying == false)
                return;

            // ?????????????????
            setup.ChangeData(this, clothParams, clothData);
        }

        //=========================================================================================
        protected override void OnInit()
        {
            base.OnInit();
            BaseClothInit();
        }

        protected override void OnActive()
        {
            base.OnActive();
            // ?????????
            EnableParticle(UserTransform, UserTransformLocalPosition, UserTransformLocalRotation);
            // ?????????
            TeamData.UpdateStatus();
            SetUseMesh(true);
            ClothActive();
        }

        protected override void OnInactive()
        {
            base.OnInactive();
            // ?????????
            DisableParticle(UserTransform, UserTransformLocalPosition, UserTransformLocalRotation);
            SetUseMesh(false);
            ClothInactive();
        }

        protected override void OnDispose()
        {
            BaseClothDispose();
            base.OnDispose();
        }

        //=========================================================================================
        internal override void UpdateCullingMode(CoreComponent caller)
        {
            //Debug.Log($"UpdateCullingMode [{this.name}]");

            // ???????
            bool isBoneCloth = GetComponentType() == ComponentType.BoneCloth || GetComponentType() == ComponentType.BoneSpring;
            if (CullingMode != TeamCullingMode.Off && isBoneCloth && cullRendererList.Count == 0)
            {
                // BoneCloth/BoneSpring?????????????????????????????OFF?????
                CullingMode = TeamCullingMode.Off;
            }

            // deformer
            CoreComponent vd = GetDeformer()?.Parent;

            // ????
            bool visible = false;
            if (CullingMode == TeamCullingMode.Off)
            {
                visible = true;
            }
            else if (IsActive()) // ?????
            {
                if (isBoneCloth)
                {
                    // ???????????????????
                    if (cullRendererList.Count > 0)
                    {
                        foreach (var ren in cullRendererList)
                        {
                            if (ren && ren.isVisible)
                            {
                                visible = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        // ??????????????
                        visible = true;
                    }
                }
                else
                {
                    // ?????????????????
                    visible = vd ? vd.IsVisible : false;
                }
            }
            IsVisible = visible;

            // ????
            bool stopInvisible = (CullingMode != TeamCullingMode.Off);
            bool calc = true;
            if (stopInvisible)
            {
                calc = visible;
            }
            int val = calc ? 1 : 0;

            // ??????????????
            val = Status.IsActive ? val : 0;

            // ????
            if (calculateValue != val)
            {
                calculateValue = val;
                OnChangeCalculation();
            }

            // ?????????
            if (vd && vd != caller)
                GetDeformer()?.Parent?.UpdateCullingMode(this);
        }

        protected override void OnChangeCalculation()
        {
            //Debug.Log($"Cloth [{this.name}] Visible:{IsVisible} Calc:{IsCalculate} F:{Time.frameCount}");
            MagicaPhysicsManager.Instance.Team.SetFlag(teamId, PhysicsManagerTeamData.Flag_Pause, !IsCalculate);

            if (IsCalculate)
            {
                // ?????????????
                if (CullingMode == TeamCullingMode.Reset)
                {
                    //Debug.Log($"Reset cloth! [{this.name}] F:{Time.frameCount}");
                    ResetCloth(ClothParams.TeleportMode.Reset);
                }

                // ?????????????????
                // ????+?????????
                //if (MagicaPhysicsManager.Instance.IsDelay && ActiveCount > 1)
                if (MagicaPhysicsManager.Instance.IsDelay)
                {
                    GetDeformer()?.ResetFuturePrediction();
                }

                // ????????????????????
                // ????+?????????
                //if (MagicaPhysicsManager.Instance.IsDelay && ActiveCount > 1)
                if (MagicaPhysicsManager.Instance.IsDelay)
                {
                    MagicaPhysicsManager.Instance.Team.ResetFuturePredictionCollidere(TeamId);
                }
            }
        }

        public int GetCullRenderListCount()
        {
            if (cullRendererList == null)
                return 0;
            return cullRendererList.Count(x => x != null);
        }

        //=========================================================================================
        void BaseClothInit()
        {
            // ?????????
            if (IsRequiresDeformer())
            {
                var deformer = GetDeformer();
                if (deformer == null)
                {
                    Status.SetInitError();
                    return;
                }

                // ????????????
                var component = deformer.Parent;
                Status.LinkParentStatus(component.Status); // ?????????????????????

                component.Init();
                if (component.Status.IsInitError)
                {
                    Status.SetInitError();
                    return;
                }
            }

            if (VerifyData() != Define.Error.None)
            {
                Status.SetInitError();
                return;
            }

            // ??????
            ClothInit();

            // ??????????????????
            WorkerInit();

            // ?????
            SetUseVertex(true);

            // ???????
            oldUpdateMode = UpdateMode;
            oldCullingMode = CullingMode;
            oldUseAnimatedDistance = UseAnimatedPose;

            // UnityPhysics????????????
            if (UpdateMode == TeamUpdateMode.UnityPhysics)
                SetUseUnityPhysics(true);
        }

        void BaseClothDispose()
        {
            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            // ???????????????
            var deformer = GetDeformer();
            if (deformer != null)
            {
                var component = deformer.Parent;
                Status.UnlinkParentStatus(component.Status);
            }

            if (Status.IsInitSuccess)
            {
                // ?????
                SetUseVertex(false);

                // ?????
                // ??????????????????????????????????????
                setup.ClothDispose(this);

                ClothDispose();
            }
        }

        /// <summary>
        /// ??????
        /// </summary>
        protected virtual void ClothInit()
        {
            setup.ClothInit(this, GetMeshData(), ClothData, clothParams, UserFlag);
        }

        protected virtual void ClothActive()
        {
            setup.ClothActive(this, clothParams, ClothData);

            // ?????????????????
            MagicaPhysicsManager.Instance.Team.SetFlag(TeamId, PhysicsManagerTeamData.Flag_AnimatedPose, UseAnimatedPose);
        }

        protected virtual void ClothInactive()
        {
            setup.ClothInactive(this);
        }

        protected virtual void ClothDispose()
        {
        }

        /// <summary>
        /// ????????????????(??????0)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected abstract uint UserFlag(int vindex);

        /// <summary>
        /// ?????????????????(??????null)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected abstract Transform UserTransform(int vindex);

        /// <summary>
        /// ????????????????LocalPosition???(??????0)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected abstract float3 UserTransformLocalPosition(int vindex);

        /// <summary>
        /// ????????????????LocalRotation???(??????quaternion.identity)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected abstract quaternion UserTransformLocalRotation(int vindex);

        /// <summary>
        /// ????????????
        /// </summary>
        /// <returns></returns>
        public abstract bool IsRequiresDeformer();

        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract BaseMeshDeformer GetDeformer();

        /// <summary>
        /// ???????????MeshData???(????null)
        /// </summary>
        /// <returns></returns>
        protected abstract MeshData GetMeshData();

        /// <summary>
        /// ??????????????????
        /// </summary>
        protected abstract void WorkerInit();


        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="sw"></param>
        void SetUseMesh(bool sw)
        {
            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            if (Status.IsInitSuccess == false)
                return;

            var deformer = GetDeformer();
            if (deformer != null)
            {
                if (sw)
                    deformer.AddUseMesh(this);
                else
                    deformer.RemoveUseMesh(this);
            }
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="sw"></param>
        void SetUseVertex(bool sw)
        {
            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            var deformer = GetDeformer();
            if (deformer != null)
            {
                SetDeformerUseVertex(sw, deformer);
            }
        }

        /// <summary>
        /// ?????????????
        /// ???????? AddUseVertex() / RemoveUseVertex() ?????
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="deformer"></param>
        protected abstract void SetDeformerUseVertex(bool sw, BaseMeshDeformer deformer);

        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <param name="act"></param>
        internal void DeformerForEach(System.Action<BaseMeshDeformer> act)
        {
            var deformer = GetDeformer();
            if (deformer != null)
            {
                act(deformer);
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        public void UpdateBlend()
        {
            if (teamId <= 0)
                return;

            // ?????????
            float blend = UserBlendWeight;

            // ???????
            blend *= setup.DistanceBlendRatio;

            // ??????
            blend = Mathf.Clamp01(blend);
            if (blend != oldBlendRatio)
            {
                // ?????????
                MagicaPhysicsManager.Instance.Team.SetBlendRatio(teamId, blend);

                // ????????????
                SetUserEnable(blend >= 1e-03f);

                oldBlendRatio = blend;
            }

            // ?????????
            if (CullingMode != oldCullingMode)
            {
                // ??
                UpdateCullingMode(this);
                oldCullingMode = CullingMode;
            }

            // ???????
            if (UpdateMode != oldUpdateMode)
            {
                // ?????????
                //Debug.Log($"Change Update Mode:{UpdateMode}");
                MagicaPhysicsManager.Instance.Team.SetUpdateMode(TeamId, UpdateMode);

                // ???????????????????
                SetUseUnityPhysics(UpdateMode == TeamUpdateMode.UnityPhysics);

                oldUpdateMode = UpdateMode;
            }

            // ???????????????
            if (UseAnimatedPose != oldUseAnimatedDistance)
            {
                // ?????????
                MagicaPhysicsManager.Instance.Team.SetFlag(TeamId, PhysicsManagerTeamData.Flag_AnimatedPose, UseAnimatedPose);

                oldUseAnimatedDistance = UseAnimatedPose;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="boneReplaceDict"></param>
        public override void ReplaceBone<T>(Dictionary<T, Transform> boneReplaceDict)
        {
            base.ReplaceBone(boneReplaceDict);

            // ???????????????
            setup.ReplaceBone(this, clothParams, boneReplaceDict);
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        public override HashSet<Transform> GetUsedBones()
        {
            var bones = base.GetUsedBones();

            // ???????????????
            bones.UnionWith(setup.GetUsedBones(this, clothParams));

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
            if (teamId <= 0)
                return;

            setup.ChangeUseUnityPhysics(sw);
            MagicaPhysicsManager.Instance.Team.ChangeUseUnityPhysics(TeamId, sw);
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns></returns>
        public override void CreateVerifyData()
        {
            base.CreateVerifyData();
            clothDataHash = ClothData != null ? ClothData.SaveDataHash : 0;
            clothDataVersion = ClothData != null ? ClothData.SaveDataVersion : 0;
            clothSelectionHash = ClothSelection != null ? ClothSelection.SaveDataHash : 0;
            clothSelectionVersion = ClothSelection != null ? ClothSelection.SaveDataVersion : 0;

            // ?????????
            clothParamDataHashList.Clear();
            for (int i = 0; i < (int)ClothParams.ParamType.Max; i++)
            {
                int paramHash = clothParams.GetParamHash(this, (ClothParams.ParamType)i);
                clothParamDataHashList.Add(paramHash);
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

            // clothData??????
            if (ClothData != null)
            {
                var clothDataError = ClothData.VerifyData();
                if (clothDataError != Define.Error.None)
                    return clothDataError;
                if (clothDataHash != ClothData.SaveDataHash)
                    return Define.Error.ClothDataHashMismatch;
                if (clothDataVersion != ClothData.SaveDataVersion)
                    return Define.Error.ClothDataVersionMismatch;
            }

            // clothSelection??????
            if (ClothSelection != null)
            {
                var clothSelectionError = ClothSelection.VerifyData();
                if (clothSelectionError != Define.Error.None)
                    return clothSelectionError;
                if (clothSelectionHash != ClothSelection.SaveDataHash)
                    return Define.Error.ClothSelectionHashMismatch;
                if (clothSelectionVersion != ClothSelection.SaveDataVersion)
                    return Define.Error.ClothSelectionVersionMismatch;
            }

            return Define.Error.None;
        }

        /// <summary>
        /// ????????????????????
        /// ??????????????????????
        /// </summary>
        /// <param name="ptype"></param>
        /// <returns></returns>
        public bool HasChangedParam(ClothParams.ParamType ptype)
        {
            int index = (int)ptype;
            if (clothParamDataHashList.Count == 0)
                return false;
            if (index >= clothParamDataHashList.Count)
            {
                return true;
            }
            int hash = clothParams.GetParamHash(this, ptype);
            if (hash == 0)
                return false;

            return clothParamDataHashList[index] != hash;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns></returns>
        public Define.Error VerifyAlgorithmVersion()
        {
            if (clothData == null)
                return Define.Error.None;

            if (clothData.clampRotationAlgorithm != ClothParams.Algorithm.Algorithm_2)
                return Define.Error.OldAlgorithm;
            if (clothData.restoreRotationAlgorithm != ClothParams.Algorithm.Algorithm_2)
                return Define.Error.OldAlgorithm;
            if (clothData.triangleBendAlgorithm != ClothParams.Algorithm.Algorithm_2)
                return Define.Error.OldAlgorithm;

            return Define.Error.None;
        }

        /// <summary>
        /// ?????????????????
        /// ???????????????????????
        /// </summary>
        /// <returns>true=????, false=????</returns>
        public override bool UpgradeFormat()
        {
            bool change = false;

            // ??????
            if (clothParams.AlgorithmType == ClothParams.Algorithm.Algorithm_1)
            {
                // ??????[2]????????
                clothParams.AlgorithmType = ClothParams.Algorithm.Algorithm_2;
                clothParams.ConvertToLatestAlgorithmParameter();
                change = true;
            }

            return change;
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        public override List<ShareDataObject> GetAllShareDataObject()
        {
            var sdata = base.GetAllShareDataObject();
            sdata.Add(ClothData);
            sdata.Add(ClothSelection);
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
            if (ClothData == source)
            {
                //clothData = Instantiate(ClothData);
                clothData = ShareDataObject.Clone(ClothData);
                return clothData;
            }

            if (ClothSelection == source)
            {
                //clothSelection = Instantiate(ClothSelection);
                clothSelection = ShareDataObject.Clone(ClothSelection);
                return clothSelection;
            }

            return null;
        }

        //=========================================================================================
        /// <summary>
        /// ????????????API?????
        /// </summary>
        /// <param name="teleportMode"></param>
        /// <param name="resetStabilizationTime"></param>
        private void ResetClothInternal(ClothParams.TeleportMode teleportMode, float resetStabilizationTime)
        {
            if (IsValid())
            {
                switch (teleportMode)
                {
                    case ClothParams.TeleportMode.Reset:
                        MagicaPhysicsManager.Instance.Team.SetFlag(teamId, PhysicsManagerTeamData.Flag_Reset_WorldInfluence, true);
                        MagicaPhysicsManager.Instance.Team.SetFlag(teamId, PhysicsManagerTeamData.Flag_Reset_Position, true);
                        MagicaPhysicsManager.Instance.Team.SetFlag(teamId, PhysicsManagerTeamData.Flag_Reset_Keep, false);
                        MagicaPhysicsManager.Instance.Team.ResetStabilizationTime(teamId, resetStabilizationTime);
                        break;
                    case ClothParams.TeleportMode.Keep:
                        MagicaPhysicsManager.Instance.Team.SetFlag(teamId, PhysicsManagerTeamData.Flag_Reset_WorldInfluence, false);
                        MagicaPhysicsManager.Instance.Team.SetFlag(teamId, PhysicsManagerTeamData.Flag_Reset_Position, false);
                        MagicaPhysicsManager.Instance.Team.SetFlag(teamId, PhysicsManagerTeamData.Flag_Reset_Keep, true);
                        break;
                }
            }
        }
    }
}
