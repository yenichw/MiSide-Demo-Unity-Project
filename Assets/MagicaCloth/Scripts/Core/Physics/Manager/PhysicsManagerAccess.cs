// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System;

namespace MagicaCloth
{
    /// <summary>
    /// ?????????????
    /// </summary>
    public abstract class PhysicsManagerAccess : IDisposable
    {
        protected MagicaPhysicsManager manager;

        public UpdateTimeManager UpdateTime
        {
            get
            {
                return manager.UpdateTime;
            }
        }

        protected PhysicsManagerParticleData Particle
        {
            get
            {
                return manager.Particle;
            }
        }

        protected PhysicsManagerBoneData Bone
        {
            get
            {
                return manager.Bone;
            }
        }

        protected PhysicsManagerMeshData Mesh
        {
            get
            {
                return manager.Mesh;
            }
        }

        protected PhysicsManagerTeamData Team
        {
            get
            {
                return manager.Team;
            }
        }

        protected PhysicsManagerWindData Wind
        {
            get
            {
                return manager.Wind;
            }
        }

        protected PhysicsManagerComponent Component
        {
            get
            {
                return manager.Component;
            }
        }

        protected PhysicsManagerCompute Compute
        {
            get
            {
                return manager.Compute;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="manager"></param>
        public void SetParent(MagicaPhysicsManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// ????
        /// </summary>
        public abstract void Create();

        /// <summary>
        /// ??
        /// </summary>
        public abstract void Dispose();
    }
}
