// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// MonoBehaviour???????????????????????????????
    /// �???????
    /// �?????
    /// �????????
    /// �???????
    /// �?????????
    /// �??????
    /// </summary>
    public abstract partial class CoreComponent : BaseComponent, IShareDataObject, IDataVerify, IEditorMesh, IEditorCloth, IDataHash, IBoneReplace
    {
        [SerializeField]
        protected int dataHash;
        [SerializeField]
        protected int dataVersion;

        /// <summary>
        /// ????
        /// </summary>
        protected RuntimeStatus status = new RuntimeStatus();

        public RuntimeStatus Status
        {
            get
            {
                return status;
            }
        }

        /// <summary>
        /// ???????????
        /// </summary>
        //protected int ActiveCount { get; private set; }

        /// <summary>
        /// UnityPhysics?????????(v1.9.4)
        /// ???????1????????????????UnityPhysics??????????????
        /// </summary>
        private int useUnityPhysicsCount = 0;

        /// <summary>
        /// ???UnityPhysics????
        /// </summary>
        private bool nowUseUnityPhysics = false;

        /// <summary>
        /// ???????
        /// </summary>
        public bool IsVisible { get; protected set; } = false;

        /// <summary>
        /// ???????(0:OFF, 1:ON, -1:invalid)
        /// </summary>
        protected int calculateValue = -1;
        public bool IsCalculate => calculateValue == 1;

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public abstract int GetDataHash();

        public int SaveDataHash
        {
            get
            {
                return dataHash;
            }
        }

        public int SaveDataVersion
        {
            get
            {
                return dataVersion;
            }
        }

        //=========================================================================================
        protected virtual void Start()
        {
            Init();
        }

        public virtual void OnEnable()
        {
            status.SetEnable(true);
            status.UpdateStatus();
        }

        public virtual void OnDisable()
        {
            //Debug.Log("Core.OnDisable():" + gameObject.name);
            status.SetEnable(false);
            status.UpdateStatus();
        }

        protected virtual void OnDestroy()
        {
            if (Status.IsDispose)
                return;

            //Debug.Log("Core.OnDestroy():" + gameObject.name);
            status.SetDispose();
            OnDispose();

            // ????
            if (MagicaPhysicsManager.IsInstance())
                MagicaPhysicsManager.Instance.Component.RemoveComponent(this);
        }

        // ????????????????????????????????
        // ??????????????????????????
        //protected virtual void ManagedUpdate()
        //{
        //    //Debug.Log("ManagedUpdate.");
        //    if (status.IsInitSuccess)
        //    {
        //        var error = VerifyData() != Define.Error.None;
        //        status.SetRuntimeError(error);
        //        UpdateStatus();

        //        if (status.IsActive)
        //            OnUpdate();
        //    }
        //}

        //protected virtual void Update()
        //{
        //    if (status.IsInitSuccess)
        //    {
        //        var error = VerifyData() != Define.Error.None;
        //        status.SetRuntimeError(error);
        //        UpdateStatus();

        //        if (status.IsActive)
        //            OnUpdate();
        //    }
        //}

        //=========================================================================================
        /// <summary>
        /// ???
        /// ???Start()???
        /// ?????????????????????????
        /// </summary>
        /// <param name="vcnt"></param>
        public void Init()
        {
            //Develop.Log($"Core.Init():{gameObject.name}");

            status.UpdateStatusAction = OnUpdateStatus;
            status.DisconnectedAction = OnDisconnectedStatus;
            status.OwnerFunc = () => this;
            if (status.IsInitComplete || status.IsInitStart)
                return;
            status.SetInitStart();

            // ??
            MagicaPhysicsManager.Instance.Component.AddComponent(this);

            if (VerifyData() != Define.Error.None)
            {
                status.SetInitError();
                return;
            }

            OnInit();
            if (status.IsInitError)
                return;

            status.SetInitComplete();

            status.UpdateStatus();
        }

        //=========================================================================================
        /// <summary>
        /// ???
        /// </summary>
        protected abstract void OnInit();

        /// <summary>
        /// ??
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// ??
        /// </summary>
        protected abstract void OnUpdate();

        /// <summary>
        /// ????????????????
        /// </summary>
        protected abstract void OnActive();

        /// <summary>
        /// ?????????????????
        /// </summary>
        protected abstract void OnInactive();

        /// <summary>
        /// ????????????????????
        /// </summary>
        protected virtual void OnUpdateStatus()
        {
            if (status.IsActive)
            {
                // ????????
                //Debug.Log($"[{this.name}] OnActive() F:{Time.frameCount}");
                //ActiveCount++; // ???????
                OnActive();
                ActiveUseUnityPhysics();
                UpdateCullingMode(this); // ?????????
            }
            else
            {
                // ?????????
                //Debug.Log($"[{this.name}] OnInactive() F:{Time.frameCount}");
                InactiveUseUnityPhysics();
                calculateValue = 0; // ?????OFF
                OnInactive();
            }
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        protected virtual void OnDisconnectedStatus()
        {
            //Debug.Log("DisconnectStatus:" + gameObject.name);
            // ????
            OnDestroy();
        }


        //=========================================================================================
        /// <summary>
        /// UnityPhysics????????
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="immediate"></param>
        public void SetUseUnityPhysics(bool sw)
        {
            useUnityPhysicsCount += sw ? 1 : -1;
            Debug.Assert(useUnityPhysicsCount >= 0);

            //Debug.Log($"[{this.name}] SetUseUnityPhysics():{sw} count:{useUnityPhysicsCount}");

            // ??????
            if (Status.IsActive)
            {
                if (IsUseUnityPhysics() != nowUseUnityPhysics)
                {
                    nowUseUnityPhysics = IsUseUnityPhysics();
                    ChangeUseUnityPhysics(nowUseUnityPhysics);
                }
            }
        }

        private void ActiveUseUnityPhysics()
        {
            if (nowUseUnityPhysics == false && useUnityPhysicsCount > 0)
            {
                //Debug.Log($"[{this.name}] ActiveUseUnityPhysics()");
                nowUseUnityPhysics = true;
                ChangeUseUnityPhysics(nowUseUnityPhysics);
            }
        }

        private void InactiveUseUnityPhysics()
        {
            if (nowUseUnityPhysics == true)
            {
                //Debug.Log($"[{this.name}] InactiveUseUnityPhysics()");
                nowUseUnityPhysics = false;
                ChangeUseUnityPhysics(nowUseUnityPhysics);
            }
        }

        /// <summary>
        /// UnityPhyiscs???????
        /// ????????????????????UnityPhysics?????????????
        /// </summary>
        /// <param name="sw"></param>
        protected virtual void ChangeUseUnityPhysics(bool sw)
        {
            //Debug.Log($"[{this.name}] ChangeUseUnityPhysics():{sw}");
        }

        protected bool IsUseUnityPhysics()
        {
            return useUnityPhysicsCount > 0;
        }

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        /// <returns></returns>
        public virtual List<ShareDataObject> GetAllShareDataObject()
        {
            return new List<ShareDataObject>();
        }

        /// <summary>
        /// source?????????????????
        /// ??????????????
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public abstract ShareDataObject DuplicateShareDataObject(ShareDataObject source);

        //=========================================================================================
        /// <summary>
        /// ???????????????????(v1.2)
        /// </summary>
        /// <param name="sw"></param>
        protected void SetUserEnable(bool sw)
        {
            if (status.SetUserEnable(sw))
            {
                status.UpdateStatus();
            }
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="Caller"></param>
        internal virtual void UpdateCullingMode(CoreComponent Caller)
        {
            // (1)?????????
            // (2)??????
            // (3)??????
            // (4)??
        }

        /// <summary>
        /// ????????????
        /// </summary>
        protected virtual void OnChangeCalculation() { }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        public abstract int GetVersion();

        /// <summary>
        /// ???????????????????
        /// ??????????????????????????
        /// </summary>
        /// <returns></returns>
        public abstract int GetErrorVersion();

        /// <summary>
        /// ?????????(???????)???
        /// </summary>
        /// <returns></returns>
        public virtual Define.Error VerifyData()
        {
            if (dataVersion == 0)
                return Define.Error.EmptyData;
            if (dataHash == 0)
                return Define.Error.InvalidDataHash;
            if (dataVersion > 0 && GetErrorVersion() > 0 && dataVersion <= GetErrorVersion())
                return Define.Error.TooOldDataVersion; // ?????????????(????)
            if (dataVersion > GetVersion())
                return Define.Error.HigherDataVersion; // ??????????????(?????)
            //if (dataVersion != GetVersion())
            //    return Define.Error.DataVersionMismatch;

            return Define.Error.None;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <returns></returns>
        public Define.Error VerifyDataVersion()
        {
            if (dataVersion == 0)
                return Define.Error.None;

            return dataVersion == GetVersion() ? Define.Error.None : Define.Error.OldDataVersion;
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        public bool IsOldDataVertion()
        {
            return dataVersion != GetVersion();
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns></returns>
        public virtual void CreateVerifyData()
        {
            dataHash = GetDataHash();
            dataVersion = GetVersion();
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        /// <returns></returns>
        public virtual string GetInformation()
        {
            return "No information.";
        }

        /// <summary>
        /// ?????????????????
        /// ???????????????????????
        /// </summary>
        /// <returns>true=????, false=????</returns>
        public virtual bool UpgradeFormat()
        {
            return false;
        }

        //=========================================================================================
        /// <summary>
        /// ??????(????)
        /// </summary>
        /// <param name="boneReplaceDict"></param>
        public void ChangeAvatar<T>(Dictionary<T, Transform> boneReplaceDict) where T : class
        {
            // ?????????????
            bool active = status.IsActive;
            if (active)
            {
                status.SetEnable(false);
                status.UpdateStatus();
            }

            // ?????
            ReplaceBone(boneReplaceDict);

            // ????????????????
            if (active)
            {
                status.SetEnable(true);
                status.UpdateStatus();
            }
        }

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="boneReplaceDict"></param>
        public virtual void ReplaceBone<T>(Dictionary<T, Transform> boneReplaceDict) where T : class
        {
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        public virtual HashSet<Transform> GetUsedBones()
        {
            //return new HashSet<Transform>();
            var bones = new HashSet<Transform>();
            bones.Add(transform); // ??????????Transform
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
        public virtual int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// ??????????????????(?????)
        /// </summary>
        /// <returns></returns>
        public virtual List<int> GetEditorTriangleList()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// ??????????????(?????)
        /// </summary>
        /// <returns></returns>
        public virtual List<int> GetEditorLineList()
        {
            throw new System.NotImplementedException();
        }

        //=========================================================================================
        /// <summary>
        /// ????????????????(?????)
        /// ????? ClothSelection.Invalid / ClothSelection.Fixed / ClothSelection.Move
        /// ????Invalid???null???
        /// </summary>
        /// <returns></returns>
        public virtual List<int> GetSelectionList()
        {
            return null;
        }

        /// <summary>
        /// ????????????????(?????)
        /// ???1????????????
        /// ??????????null???
        /// </summary>
        /// <returns></returns>
        public virtual List<int> GetUseList()
        {
            return null;
        }
    }
}
