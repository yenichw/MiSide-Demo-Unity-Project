// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????????????????
    /// </summary>
    public abstract class ColliderComponent : ParticleComponent
    {
        /// <summary>
        /// ????????
        /// </summary>
        [SerializeField]
        protected bool isGlobal;

        [SerializeField]
        private Vector3 center;

        //=========================================================================================
        public Vector3 Center
        {
            get
            {
                return center;
            }
            set
            {
                center = value;
                ReserveDataUpdate();
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            // ?????????
            if (isGlobal)
            {
                CreateColliderParticle(0);
            }
        }

        /// <summary>
        /// ??
        /// </summary>
        protected override void OnDispose()
        {
            // ???????????????
            List<int> teamList = new List<int>();
            foreach (var teamId in particleDict.Keys)
            {
                teamList.Add(teamId);
            }
            foreach (var teamId in teamList)
            {
                RemoveColliderParticle(teamId);
            }
            base.OnDispose();
        }

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// </summary>
        /// <returns></returns>
        public override int GetDataHash()
        {
            return isGlobal.GetDataHash();
        }

        /// <summary>
        /// ????????????p????????p????dir????
        /// ????????
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="p">???</param>
        /// <param name="dir">????????????????</param>
        /// <param name="d">?????????</param>
        public abstract bool CalcNearPoint(Vector3 pos, out Vector3 p, out Vector3 dir, out Vector3 d, bool skinning);

        /// <summary>
        /// ??????????????
        /// ????????
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector3 CalcLocalPos(Vector3 pos)
        {
            // ?????????
            var rot = transform.rotation;
            var v = pos - transform.position;
            return Quaternion.Inverse(rot) * v;
        }

        /// <summary>
        /// ??????????????
        /// ????????
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Vector3 CalcLocalDir(Vector3 dir)
        {
            return transform.InverseTransformDirection(dir);
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public ChunkData CreateColliderParticle(int teamId)
        {
            var c = CreateColliderParticleReal(teamId);

            // ????????????????????
            if (c.IsValid() && Status.IsActive)
                EnableTeamParticle(teamId);

            return c;
        }

        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <param name="teamId"></param>
        public void RemoveColliderParticle(int teamId)
        {
            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            if (particleDict.ContainsKey(teamId))
            {
                var c = particleDict[teamId];
                for (int i = 0; i < c.dataLength; i++)
                {
                    int pindex = c.startIndex + i;
                    MagicaPhysicsManager.Instance.Team.RemoveColliderParticle(teamId, pindex);
                }

                RemoveTeamParticle(teamId);
            }
        }

        //=========================================================================================
        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        protected abstract ChunkData CreateColliderParticleReal(int teamId);
    }
}
