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
    /// ????????????????????
    /// ?????????????????????????????????????????????????
    /// </summary>
    public abstract class ParticleComponent : BaseComponent, IDataHash
    {
        /// <summary>
        /// ??????ID
        /// ?????(???0)
        /// </summary>
        protected Dictionary<int, ChunkData> particleDict = new Dictionary<int, ChunkData>();

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

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public abstract int GetDataHash();

        //=========================================================================================
        public int CenterParticleIndex
        {
            get
            {
                if (particleDict.ContainsKey(0))
                    return particleDict[0].startIndex;
                return -1;
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
            status.SetEnable(false);
            status.UpdateStatus();
        }

        protected virtual void OnDestroy()
        {
            OnDispose();
            status.SetDispose();
        }

        // ????VerifyData()???true??????????
        //protected virtual void Update()
        //{
        //    if (status.IsInitSuccess)
        //    {
        //        var error = !VerifyData();
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

        // ?????????
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
        public virtual bool VerifyData()
        {
            return true;
        }

        //=========================================================================================
        /// <summary>
        /// ???
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// ??
        /// </summary>
        protected virtual void OnDispose()
        {
            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            // ???????????
            RemoveParticle();
        }

        /// <summary>
        /// ??
        /// </summary>
        protected virtual void OnUpdate() { }

        /// <summary>
        /// ????????????????
        /// </summary>
        protected virtual void OnActive()
        {
            // ?????????
            EnableParticle();
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        protected virtual void OnInactive()
        {
            // ?????????
            DisableParticle();
        }

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// </summary>
        protected void EnableParticle()
        {
            foreach (var teamId in particleDict.Keys)
            {
                EnableTeamParticle(teamId);
            }
        }

        /// <summary>
        /// ?????????
        /// </summary>
        protected void DisableParticle()
        {
            if (MagicaPhysicsManager.IsInstance())
            {
                foreach (var teamId in particleDict.Keys)
                {
                    DisableTeamParticle(teamId);
                }
            }
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="teamId"></param>
        protected void EnableTeamParticle(int teamId)
        {
            var c = particleDict[teamId];
            MagicaPhysicsManager.Instance.Particle.SetEnable(
                c,
                true,
                UserTransform,
                UserTransformLocalPosition,
                UserTransformLocalRotation
                );
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="teamId"></param>
        protected void DisableTeamParticle(int teamId)
        {
            var c = particleDict[teamId];
            MagicaPhysicsManager.Instance.Particle.SetEnable(
                c,
                false,
                UserTransform,
                UserTransformLocalPosition,
                UserTransformLocalRotation
                );
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        protected void ReserveDataUpdate()
        {
            if (MagicaPhysicsManager.IsInstance())
                MagicaPhysicsManager.Instance.Component.ReserveDataUpdateParticleComponent(this);
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        internal virtual void DataUpdate() { }

        /// <summary>
        /// ?????
        /// </summary>
        internal void UpdateStatus()
        {
            status.UpdateStatus();
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????1???
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="mass"></param>
        /// <param name="gravity"></param>
        /// <param name="drag"></param>
        /// <param name="maxVelocity"></param>
        /// <param name="depth"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        protected ChunkData CreateParticle(
            uint flag,
            int teamId,
            float depth,
            float3 radius,
            float3 localPos
            )
        {
            // ????????????(v1.9.3)
            if (particleDict.ContainsKey(teamId))
            {
                return new ChunkData();
            }

            // ???????
            if (MagicaPhysicsManager.Instance.Team.IsFlag(teamId, PhysicsManagerTeamData.Flag_UpdatePhysics))
                flag |= PhysicsManagerParticleData.Flag_Transform_UnityPhysics;

            var t = transform;
            var c = MagicaPhysicsManager.Instance.Particle.CreateParticle(
                flag,
                teamId,
                t.position,
                t.rotation,
                depth,
                radius,
                localPos
                );
            particleDict.Add(teamId, c);

            // ?????Disable
            DisableTeamParticle(teamId);

            return c;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="teamId"></param>
        protected void RemoveTeamParticle(int teamId)
        {
            if (MagicaPhysicsManager.IsInstance())
            {
                // ?????????
                DisableTeamParticle(teamId);

                // ????????
                var c = particleDict[teamId];
                MagicaPhysicsManager.Instance.Particle.RemoveParticle(c);
                particleDict.Remove(teamId);
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        protected void RemoveParticle()
        {
            if (MagicaPhysicsManager.IsInstance())
            {
                foreach (var teamId in particleDict.Keys)
                {
                    RemoveTeamParticle(teamId);
                }
            }
            particleDict.Clear();
        }

        /// <summary>
        /// ?????????????????(??????null)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected Transform UserTransform(int vindex)
        {
            return transform;
        }

        /// <summary>
        /// ????????????????LocalPosition???(??????0)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected float3 UserTransformLocalPosition(int vindex)
        {
            return transform.localPosition;
        }

        /// <summary>
        /// ????????????????LocalRotation???(??????quaternion.identity)
        /// </summary>
        /// <param name="vindex"></param>
        /// <returns></returns>
        protected quaternion UserTransformLocalRotation(int vindex)
        {
            return transform.localRotation;
        }
    }
}
