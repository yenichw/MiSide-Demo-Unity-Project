// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicaCloth
{
    /// <summary>
    /// ?????????????
    /// </summary>
    [System.Serializable]
    public abstract class BaseMeshDeformer : IEditorMesh, IDataVerify, IDataHash
    {
        /// <summary>
        /// ?????????
        /// </summary>
        [SerializeField]
        private MeshData meshData = null;

        /// <summary>
        /// ??????????????????(??)
        /// </summary>
        [SerializeField]
        private GameObject targetObject;

        /// <summary>
        /// ?????????
        /// </summary>
        [SerializeField]
        protected int dataHash;
        [SerializeField]
        protected int dataVersion;

        /// <summary>
        /// ????
        /// </summary>
        protected RuntimeStatus status = new RuntimeStatus();

        //=========================================================================================
        /// <summary>
        /// ????????(Unity2019.3???????)
        /// </summary>
        private CoreComponent parent;

        public CoreComponent Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        //=========================================================================================
        public virtual MeshData MeshData
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    return meshData;
                else
                {
                    // unity2019.3????null?????????(??)
                    var so = new SerializedObject(parent);
                    return so.FindProperty("deformer.meshData").objectReferenceValue as MeshData;
                }
#else
                return meshData;
#endif
            }
            set
            {
                meshData = value;
            }
        }

        public GameObject TargetObject
        {
            get
            {
                return targetObject;
            }
            set
            {
                targetObject = value;
            }
        }

        public RuntimeStatus Status
        {
            get
            {
                return status;
            }
        }

        /// <summary>
        /// ????????????
        /// (-1=??)
        /// </summary>
        public int MeshIndex { get; protected set; } = -1;

        /// <summary>
        /// ?????
        /// </summary>
        public int VertexCount { get; protected set; }

        /// <summary>
        /// ??????????
        /// </summary>
        public int SkinningVertexCount { get; protected set; }

        /// <summary>
        /// ??????????
        /// </summary>
        public int TriangleCount { get; protected set; }

        //=========================================================================================
        /// <summary>
        /// ???
        /// ???Start()???
        /// </summary>
        /// <param name="vcnt"></param>
        public virtual void Init()
        {
            status.UpdateStatusAction = OnUpdateStatus;
            status.OwnerFunc = () => Parent;
            if (status.IsInitComplete || status.IsInitStart)
                return;
            status.SetInitStart();

            OnInit();

            // ???????
            if (VerifyData() != Define.Error.None)
            {
                // error
                status.SetInitError();
                return;
            }

            status.SetInitComplete();

            // ????
            status.UpdateStatus();
        }

        protected virtual void OnInit()
        {
            // ???????????
            MeshIndex = -1;

            // ????????
            MagicaPhysicsManager.Instance.Mesh.AddMesh(this);
        }

        /// <summary>
        /// ??
        /// ???OnDestroy()???
        /// </summary>
        public virtual void Dispose()
        {
            // ?????????
            if (MagicaPhysicsManager.IsInstance())
                MagicaPhysicsManager.Instance.Mesh.RemoveMesh(this);

            status.SetDispose();
        }

        public virtual void OnEnable()
        {
            status.SetEnable(true);
            status.UpdateStatus();
        }

        public virtual void OnDisable()
        {
            status.SetEnable(false);
            status.UpdateStatus();
        }

        public virtual void Update()
        {
            // ????????
            var error = VerifyData() != Define.Error.None;
            status.SetRuntimeError(error);
            status.UpdateStatus();
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="bufferIndex"></param>
        internal abstract void MeshCalculation(int bufferIndex);

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="bufferIndex"></param>
        internal abstract void NormalWriting(int bufferIndex);

        // ???????
        protected void OnUpdateStatus()
        {
            if (status.IsActive)
            {
                // ????????
                OnActive();
            }
            else
            {
                // ?????????
                OnInactive();
            }
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        protected virtual void OnActive()
        {
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        protected virtual void OnInactive()
        {
        }

        //=========================================================================================
        public virtual bool IsMeshUse()
        {
            return false;
        }

        public virtual bool IsActiveMesh()
        {
            return false;
        }

        public bool IsSkinning
        {
            get
            {
                if (MeshData != null)
                    return MeshData.isSkinning;
                return false;
            }
        }

        public int BoneCount
        {
            get
            {
                if (MeshData != null)
                {
                    if (MeshData.isSkinning)
                        return MeshData.BoneCount;
                    else
                        return 1;
                }
                else
                    return 0;
            }
        }

        //=========================================================================================
        public virtual void AddUseMesh(System.Object parent)
        {
        }

        public virtual void RemoveUseMesh(System.Object parent)
        {
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns>??????true???</returns>
        public virtual bool AddUseVertex(int vindex, bool fix)
        {
            return false;
        }

        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns>??????true???</returns>
        public virtual bool RemoveUseVertex(int vindex, bool fix)
        {
            return false;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public virtual void ResetFuturePrediction()
        {
        }

        /// <summary>
        /// UnityPhysics?????????
        /// </summary>
        /// <param name="sw"></param>
        public virtual void ChangeUseUnityPhysics(bool sw)
        {
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public virtual int GetDataHash()
        {
            int hash = 0;
            if (MeshData != null)
                hash += MeshData.GetDataHash();
            if (targetObject)
                hash += targetObject.GetDataHash();

            return hash;
        }

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
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        public abstract int GetVersion();

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
            //if (dataVersion != GetVersion())
            //    return Define.Error.DataVersionMismatch;
            if (MeshData == null)
                return Define.Error.MeshDataNull;
            if (targetObject == null)
                return Define.Error.TargetObjectNull;
            var mdataError = MeshData.VerifyData();
            if (mdataError != Define.Error.None)
                return mdataError;

            return Define.Error.None;
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

        //=========================================================================================
        /// <summary>
        /// ???????????/??/?????(???????)
        /// </summary>
        /// <param name="wposList"></param>
        /// <param name="wnorList"></param>
        /// <param name="wtanList"></param>
        /// <returns>???</returns>
        public abstract int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList);

        /// <summary>
        /// ??????????????????(???????)
        /// </summary>
        /// <returns></returns>
        public abstract List<int> GetEditorTriangleList();

        /// <summary>
        /// ??????????????(?????)
        /// </summary>
        /// <returns></returns>
        public abstract List<int> GetEditorLineList();
    }
}
