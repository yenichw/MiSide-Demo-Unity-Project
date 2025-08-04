// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????????
    /// </summary>
    public abstract partial class WindComponent : BaseComponent
    {
        [SerializeField]
        [Range(0.0f, Define.Compute.MaxWindMain)]
        protected float main = 5.0f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        protected float turbulence = 1.0f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        protected float frequency = 1.0f;

        [SerializeField]
        protected Vector3 areaSize = new Vector3(5.0f, 5.0f, 5.0f);

        [SerializeField]
        protected float areaRadius = 5.0f;

        //[SerializeField]
        //protected Vector3 anchor;

        [SerializeField]
        [Range(-180, 180)]
        protected float directionAngleX = 0;

        [SerializeField]
        [Range(-180, 180)]
        protected float directionAngleY = 0;

        [SerializeField]
        protected PhysicsManagerWindData.DirectionType directionType;

        [SerializeField]
        protected BezierParam attenuation = new BezierParam(1f, 1f, false, 0.0f, false);

        //=========================================================================================
        /// <summary>
        /// ????ID
        /// </summary>
        protected int windId = -1;

        /// <summary>
        /// ????
        /// </summary>
        protected RuntimeStatus status = new RuntimeStatus();

        internal RuntimeStatus Status
        {
            get
            {
                return status;
            }
        }

        //=========================================================================================
        protected virtual void Reset()
        {
            ResetParams();
        }

        protected virtual void OnValidate()
        {
            //anchor = math.clamp(anchor, -1, 1);
            areaSize = math.max(areaSize, 0.1f);
            areaRadius = math.max(areaRadius, 0.1f);

            if (Application.isPlaying)
                status.SetDirty();
        }

        // Animator/Animation?????????????????
        void OnDidApplyAnimationProperties()
        {
            if (Application.isPlaying)
            {
                status.SetDirty();
            }
        }

        protected virtual void Start()
        {
            Init();
        }

        internal virtual void OnEnable()
        {
            status.SetEnable(true);
            status.UpdateStatus();
        }

        internal virtual void OnDisable()
        {
            status.SetEnable(false);
            status.UpdateStatus();
        }

        protected virtual void OnDestroy()
        {
            OnDispose();
            status.SetDispose();
        }

        protected virtual void Update()
        {
            if (status.IsInitSuccess)
            {
                var error = !VerifyData();
                status.SetRuntimeError(error);
                status.UpdateStatus();

                if (status.IsActive)
                    OnUpdate();
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???
        /// ???Start()???
        /// </summary>
        /// <param name="vcnt"></param>
        void Init()
        {
            status.UpdateStatusAction = OnUpdateStatus;
            status.OwnerFunc = () => this;
            if (status.IsInitComplete || status.IsInitStart)
                return;
            status.SetInitStart();

            if (VerifyData() == false)
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
        /// ?????????(???????)???
        /// </summary>
        /// <returns></returns>
        internal virtual bool VerifyData()
        {
            return true;
        }

        //=========================================================================================
        /// <summary>
        /// ???
        /// </summary>
        protected virtual void OnInit()
        {
            // ???
            CreateWind();

            // ??????????????
            if (Status.IsActive)
                EnableWind();
        }

        /// <summary>
        /// ??
        /// </summary>
        protected virtual void OnDispose()
        {
            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            // ??????
            RemoveWind();
        }

        /// <summary>
        /// ??
        /// </summary>
        protected virtual void OnUpdate()
        {
            // ??????????
            if (status.IsDirty)
            {
                status.ClearDirty();
                ChangeParameter();
            }
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        protected virtual void OnActive()
        {
            // ????
            EnableWind();
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        protected virtual void OnInactive()
        {
            // ????
            DisableWind();
        }

        //=========================================================================================
        /// <summary>
        /// ????
        /// </summary>
        protected void EnableWind()
        {
            if (windId >= 0)
                MagicaPhysicsManager.Instance.Wind.SetEnable(windId, true, transform);
        }

        /// <summary>
        /// ????
        /// </summary>
        protected void DisableWind()
        {
            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            if (windId >= 0)
                MagicaPhysicsManager.Instance.Wind.SetEnable(windId, false, transform);
        }

        //=========================================================================================
        /// <summary>
        /// ???
        /// </summary>
        private void RemoveWind()
        {
            if (MagicaPhysicsManager.IsInstance())
            {
                if (windId >= 0)
                {
                    MagicaPhysicsManager.Instance.Wind.RemoveWind(windId);
                }
            }
            windId = -1;
        }

        /// <summary>
        /// ???
        /// </summary>
        private void CreateWind()
        {
            windId = MagicaPhysicsManager.Instance.Wind.CreateWind(
                GetWindType(), GetShapeType(), GetAreaSize(), IsAddition(), main, turbulence, frequency,
                GetLocalDirection(), GetDirectionType(), GetAreaVolume(), GetAreaLength(),
                attenuation
                );
            status.ClearDirty();
        }

        /// <summary>
        /// ??????(????)
        /// </summary>
        /// <returns></returns>
        internal Vector3 GetLocalDirection()
        {
            var q = Quaternion.Euler(directionAngleX, directionAngleY, 0.0f);
            return q * Vector3.forward;
            //var rot = transform.rotation * q;
            //return rot * Vector3.forward;
        }

        /// <summary>
        /// ???????????
        /// </summary>
        private void ChangeParameter()
        {
            if (windId >= 0)
            {
                MagicaPhysicsManager.Instance.Wind.SetParameter(
                    windId, GetAreaSize(), IsAddition(), main, turbulence, frequency,
                    GetLocalDirection(), GetAreaVolume(), GetAreaLength(),
                    attenuation
                    );
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <returns></returns>
        public abstract PhysicsManagerWindData.WindType GetWindType();

        /// <summary>
        /// ????????
        /// </summary>
        /// <returns></returns>
        public abstract PhysicsManagerWindData.ShapeType GetShapeType();

        /// <summary>
        /// ?????????
        /// </summary>
        /// <returns></returns>
        public abstract PhysicsManagerWindData.DirectionType GetDirectionType();

        /// <summary>
        /// ??????????
        /// </summary>
        /// <returns></returns>
        public abstract bool IsAddition();

        /// <summary>
        /// ?????????
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 GetAreaSize();

        /// <summary>
        /// ?????????
        /// </summary>
        /// <returns></returns>
        //public abstract Vector3 GetAnchor();

        /// <summary>
        /// ??????????
        /// </summary>
        /// <returns></returns>
        public abstract float GetAreaVolume();

        /// <summary>
        /// ????????????
        /// </summary>
        /// <returns></returns>
        public abstract float GetAreaLength();

        /// <summary>
        /// ????????
        /// </summary>
        protected abstract void ResetParams();
    }
}
