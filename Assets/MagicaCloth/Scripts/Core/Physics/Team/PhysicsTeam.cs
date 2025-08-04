// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ?????
    /// ?????????????1??????????????
    /// ?????????????????????????????
    /// ??????????????????????????????
    /// ?:?????A?B?????????????????
    /// </summary>
    public abstract class PhysicsTeam : CoreComponent
    {
        [SerializeField]
        protected PhysicsTeamData teamData = new PhysicsTeamData();

        /// <summary>
        /// ???????????
        /// </summary>
        [SerializeField]
        private float userBlendWeight = 1.0f;

        /// <summary>
        /// ?????
        /// </summary>
        public enum TeamUpdateMode
        {
            Normal = 0,
            UnityPhysics = 1,
        }
        [SerializeField]
        private TeamUpdateMode updateMode = TeamUpdateMode.Normal;

        /// <summary>
        /// ???????(?????????)
        /// </summary>
        public enum TeamCullingMode
        {
            Reset = 0,
            Pause = 10,
            Off = 100,
        }
        [SerializeField]
        private TeamCullingMode cullingMode = TeamCullingMode.Reset;

        /// <summary>
        /// ????????
        /// ???None??????????????????????????????????(?UseAnimatedDistance???)
        /// </summary>
        public enum TeamSkinningMode
        {
            None = 0,
            UserAnimation = 1,
            //GenerateFromBones = 2,
        }
        [SerializeField]
        private TeamSkinningMode skinningMode = TeamSkinningMode.UserAnimation;
        //[SerializeField]
        //private bool skinningUpdateFixed = false;


        //=========================================================================================
        /// <summary>
        /// ????????ID
        /// </summary>
        protected int teamId = -1;

        /// <summary>
        /// ??????????????????
        /// </summary>
        protected ChunkData particleChunk = new ChunkData();

        /// <summary>
        /// ????/?????????????
        /// </summary>
        protected Transform influenceTarget;


        //=========================================================================================
        /// <summary>
        /// ???????????
        /// </summary>
        /// <returns></returns>
        public override int GetDataHash()
        {
            int hash = teamData.GetDataHash();
            return hash;
        }

        //=========================================================================================
        public int TeamId
        {
            get
            {
                return teamId;
            }
        }

        public PhysicsTeamData TeamData
        {
            get
            {
                return teamData;
            }
        }

        public ChunkData ParticleChunk
        {
            get
            {
                return particleChunk;
            }
        }

        public Transform InfluenceTarget
        {
            set
            {
                influenceTarget = value;
            }
            get
            {
                return influenceTarget;
            }
        }

        public float UserBlendWeight
        {
            get
            {
                return userBlendWeight;
            }
            set
            {
                userBlendWeight = Mathf.Clamp01(value);
            }
        }

        public TeamUpdateMode UpdateMode
        {
            get
            {
                return updateMode;
            }
            protected set
            {
                updateMode = value;
            }
        }

        public TeamCullingMode CullingMode
        {
            get
            {
                return cullingMode;
            }
            set
            {
                cullingMode = value;
            }
        }

        public TeamSkinningMode SkinningMode
        {
            get => skinningMode;
            set => skinningMode = value;
        }

        //public bool SkinningUpdateFixed
        //{
        //    get => skinningUpdateFixed;
        //    set => skinningUpdateFixed = value;
        //}

        /// <summary>
        /// ?????????/??????????????????????????
        /// </summary>
        internal bool UseAnimatedPose
        {
            get
            {
                //return skinningMode == TeamSkinningMode.UserAnimation || skinningMode == TeamSkinningMode.GenerateFromBones;
                return skinningMode == TeamSkinningMode.UserAnimation;
            }
        }

        //=========================================================================================
        protected override void OnInit()
        {
            // ?????
            teamId = MagicaPhysicsManager.Instance.Team.CreateTeam(this, 0);
            TeamData.Init(TeamId);
        }

        /// <summary>
        /// ??
        /// ???OnDestroy()???
        /// </summary>
        protected override void OnDispose()
        {
            // ??
            if (TeamId >= 0)
            {
                TeamData.Dispose(TeamId);

                // ?????
                if (MagicaPhysicsManager.IsInstance())
                    MagicaPhysicsManager.Instance.Team.RemoveTeam(teamId);
            }
        }

        /// <summary>
        /// ??
        /// </summary>
        protected override void OnUpdate()
        {
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        protected override void OnActive()
        {
            // ??????
            MagicaPhysicsManager.Instance.Team.SetEnable(teamId, true);
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        protected override void OnInactive()
        {
            if (MagicaPhysicsManager.IsInstance())
            {
                // ??????
                MagicaPhysicsManager.Instance.Team.SetEnable(teamId, false);
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            if (MagicaPhysicsManager.IsInstance())
                return MagicaPhysicsManager.Instance.Team.IsActive(teamId);
            else
                return false;
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (MagicaPhysicsManager.IsInstance())
                return MagicaPhysicsManager.Instance.Team.IsValid(teamId);
            else
                return false;
        }

        //=========================================================================================
        /// <summary>
        /// ???????1???
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="team"></param>
        /// <param name="wpos"></param>
        /// <param name="wrot"></param>
        /// <param name="lpos"></param>
        /// <param name="lrot"></param>
        /// <param name="radius"></param>
        /// <param name="mass"></param>
        /// <param name="gravity"></param>
        /// <param name="drag"></param>
        /// <param name="depth"></param>
        /// <param name="maxVelocity"></param>
        /// <param name="target"></param>
        /// <param name="targetLocalPos"></param>
        /// <returns></returns>
        //public ChunkData CreateParticle(
        //    uint flag,
        //    int team,
        //    float3 wpos, quaternion wrot,
        //    float3 lpos, quaternion lrot,
        //    float depth,
        //    float radius,
        //    Transform target, float3 targetLocalPos
        //    )
        //{
        //    var c = MagicaPhysicsManager.Instance.Particle.CreateParticle(
        //        flag,
        //        team,
        //        wpos, wrot,
        //        lpos, lrot,
        //        depth,
        //        radius,
        //        target, targetLocalPos
        //        );

        //    // ????????????????
        //    particleChunk = c;

        //    return c;
        //}

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="team"></param>
        /// <param name="count"></param>
        /// <param name="funcFlag"></param>
        /// <param name="funcWpos"></param>
        /// <param name="funcWrot"></param>
        /// <param name="funcLpos"></param>
        /// <param name="funcLrot"></param>
        /// <param name="funcRadius"></param>
        /// <param name="funcMass"></param>
        /// <param name="funcGravity"></param>
        /// <param name="funcDrag"></param>
        /// <param name="funcDepth"></param>
        /// <param name="funcMaxVelocity"></param>
        /// <param name="funcTarget"></param>
        /// <param name="funcTargetLocalPos"></param>
        /// <returns></returns>
        public ChunkData CreateParticle(
            int team,
            int count,
            System.Func<int, uint> funcFlag,
            System.Func<int, float3> funcWpos,
            System.Func<int, quaternion> funcWrot,
            System.Func<int, float> funcDepth,
            System.Func<int, float3> funcRadius,
            System.Func<int, float3> funcTargetLocalPos
            )
        {
            var c = MagicaPhysicsManager.Instance.Particle.CreateParticle(
                team,
                count,
                funcFlag,
                funcWpos,
                funcWrot,
                funcDepth,
                funcRadius,
                funcTargetLocalPos
                );

            // ????????????????
            particleChunk = c;

            return c;
        }

        /// <summary>
        /// ????????(????)
        /// </summary>
        public void RemoveAllParticle()
        {
            MagicaPhysicsManager.Instance.Particle.RemoveParticle(particleChunk);
            particleChunk.Clear();
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        public void EnableParticle(
            System.Func<int, Transform> funcTarget,
            System.Func<int, float3> funcLpos,
            System.Func<int, quaternion> funcLrot
            )
        {
            MagicaPhysicsManager.Instance.Particle.SetEnable(particleChunk, true, funcTarget, funcLpos, funcLrot);
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        public void DisableParticle(
            System.Func<int, Transform> funcTarget,
            System.Func<int, float3> funcLpos,
            System.Func<int, quaternion> funcLrot
            )
        {
            if (MagicaPhysicsManager.IsInstance())
            {
                MagicaPhysicsManager.Instance.Particle.SetEnable(particleChunk, false, funcTarget, funcLpos, funcLrot);
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????????(???????)???
        /// </summary>
        /// <returns></returns>
        public override Define.Error VerifyData()
        {
            return base.VerifyData();
        }
    }
}
