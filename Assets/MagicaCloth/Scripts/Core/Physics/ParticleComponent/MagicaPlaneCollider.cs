// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ??????
    /// ?????????Y+??????????????
    /// </summary>
    [HelpURL("https://magicasoft.jp/magica-cloth-plane-collider/")]
    [AddComponentMenu("MagicaCloth/MagicaPlaneCollider")]
    public class MagicaPlaneCollider : ColliderComponent
    {
        //=========================================================================================
        public override ComponentType GetComponentType()
        {
            return ComponentType.PlaneCollider;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                DataUpdate();
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        internal override void DataUpdate()
        {
            base.DataUpdate();

            // localPos
            foreach (var c in particleDict.Values)
            {
                for (int i = 0; i < c.dataLength; i++)
                {
                    MagicaPhysicsManager.Instance.Particle.SetLocalPos(c.startIndex + i, Center);
                }
            }
        }

        //=========================================================================================
        protected override ChunkData CreateColliderParticleReal(int teamId)
        {
            uint flag = 0;
            flag |= PhysicsManagerParticleData.Flag_Kinematic;
            flag |= PhysicsManagerParticleData.Flag_Collider;
            flag |= PhysicsManagerParticleData.Flag_Transform_Read_Pos;
            flag |= PhysicsManagerParticleData.Flag_Transform_Read_Rot;
            flag |= PhysicsManagerParticleData.Flag_Transform_Read_Base;
            flag |= PhysicsManagerParticleData.Flag_Plane;
            flag |= PhysicsManagerParticleData.Flag_Reset_Position;
            flag |= PhysicsManagerParticleData.Flag_Transform_Read_Local;

            var c = CreateParticle(
                flag,
                teamId, // team
                0.0f, // depth
                1.0f, // radius
                Center
                );

            if (c.IsValid())
                MagicaPhysicsManager.Instance.Team.AddColliderParticle(teamId, c.startIndex);

            return c;
        }

        /// <summary>
        /// ????????????p????????p????dir????
        /// ????????
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="p"></param>
        /// <param name="dir"></param>
        public override bool CalcNearPoint(Vector3 pos, out Vector3 p, out Vector3 dir, out Vector3 d, bool skinning)
        {
            dir = Vector3.zero;

            // ????
            //var cpos = transform.position;
            var cpos = transform.TransformPoint(Center);

            // ????
            var cdir = transform.up;

            // ???????
            var v = pos - cpos;
            var gv = Vector3.Project(v, cdir);

            p = pos - gv;
            d = p;
            dir = gv.normalized;

            return Vector3.Dot(v, gv) <= 0.0f;
        }
    }
}
