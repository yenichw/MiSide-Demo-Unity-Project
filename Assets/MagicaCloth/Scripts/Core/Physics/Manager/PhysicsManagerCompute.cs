// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace MagicaCloth
{
    /// <summary>
    /// ????
    /// </summary>
    public class PhysicsManagerCompute : PhysicsManagerAccess
    {
        /// <summary>
        /// ??????????
        /// </summary>
        //[Header("?????????")]
        //[Range(1, 8)]
        //public int solverIteration = 2;
        private int solverIteration = 1;

        /// <summary>
        /// ????
        /// </summary>
        List<PhysicsManagerConstraint> constraints = new List<PhysicsManagerConstraint>();

        public ClampPositionConstraint ClampPosition { get; private set; }
        public ClampDistanceConstraint ClampDistance { get; private set; }
        //public ClampDistance2Constraint ClampDistance2 { get; private set; }
        public ClampRotationConstraint ClampRotation { get; private set; }
        public SpringConstraint Spring { get; private set; }
        public RestoreDistanceConstraint RestoreDistance { get; private set; }
        public RestoreRotationConstraint RestoreRotation { get; private set; }
        public TriangleBendConstraint TriangleBend { get; private set; }
        public ColliderCollisionConstraint Collision { get; private set; }
        public PenetrationConstraint Penetration { get; private set; }
        public ColliderExtrusionConstraint ColliderExtrusion { get; private set; }
        public TwistConstraint Twist { get; private set; }
        public CompositeRotationConstraint CompositeRotation { get; private set; }
        //public ColliderAfterCollisionConstraint AfterCollision { get; private set; }
        //public EdgeCollisionConstraint EdgeCollision { get; private set; }
        //public VolumeConstraint Volume { get; private set; }

        /// <summary>
        /// ???????
        /// </summary>
        List<PhysicsManagerWorker> workers = new List<PhysicsManagerWorker>();
        public RenderMeshWorker RenderMeshWorker { get; private set; }
        public VirtualMeshWorker VirtualMeshWorker { get; private set; }
        public MeshParticleWorker MeshParticleWorker { get; private set; }
        public SpringMeshWorker SpringMeshWorker { get; private set; }
        public AdjustRotationWorker AdjustRotationWorker { get; private set; }
        public LineWorker LineWorker { get; private set; }
        public TriangleWorker TriangleWorker { get; private set; }
        public BaseSkinningWorker BaseSkinningWorker { get; private set; }

        /// <summary>
        /// ???????????
        /// ????????????????????
        /// </summary>
        JobHandle jobHandle;
        private bool runMasterJob = false;

        private int swapIndex = 0;

        /// <summary>
        /// ???????
        /// </summary>
        public CustomSampler SamplerCalcMesh { get; set; }
        public CustomSampler SamplerWriteMesh { get; set; }

        //=========================================================================================
        /// <summary>
        /// ????
        /// </summary>
        public override void Create()
        {
            // ?????
            // ?????????????????

            // ?????
            ColliderExtrusion = new ColliderExtrusionConstraint();
            constraints.Add(ColliderExtrusion);
            Penetration = new PenetrationConstraint();
            constraints.Add(Penetration);
            Collision = new ColliderCollisionConstraint();
            constraints.Add(Collision);

            // ????
            ClampDistance = new ClampDistanceConstraint();
            constraints.Add(ClampDistance);

            // ?????????????
            Spring = new SpringConstraint();
            constraints.Add(Spring);
            Twist = new TwistConstraint();
            constraints.Add(Twist);
            RestoreDistance = new RestoreDistanceConstraint();
            constraints.Add(RestoreDistance);
            RestoreRotation = new RestoreRotationConstraint();
            constraints.Add(RestoreRotation);
            CompositeRotation = new CompositeRotationConstraint();
            constraints.Add(CompositeRotation);

            // ????
            TriangleBend = new TriangleBendConstraint();
            constraints.Add(TriangleBend);
            //Volume = new VolumeConstraint();
            //constraints.Add(Volume);

            // ????2
            ClampPosition = new ClampPositionConstraint();
            constraints.Add(ClampPosition);
            ClampRotation = new ClampRotationConstraint();
            constraints.Add(ClampRotation);

            foreach (var con in constraints)
                con.Init(manager);

            // ???????
            // ??????????????????
            RenderMeshWorker = new RenderMeshWorker();
            workers.Add(RenderMeshWorker);
            VirtualMeshWorker = new VirtualMeshWorker();
            workers.Add(VirtualMeshWorker);
            MeshParticleWorker = new MeshParticleWorker();
            workers.Add(MeshParticleWorker);
            SpringMeshWorker = new SpringMeshWorker();
            workers.Add(SpringMeshWorker);
            AdjustRotationWorker = new AdjustRotationWorker();
            workers.Add(AdjustRotationWorker);
            LineWorker = new LineWorker();
            workers.Add(LineWorker);
            TriangleWorker = new TriangleWorker();
            workers.Add(TriangleWorker);
            BaseSkinningWorker = new BaseSkinningWorker();
            workers.Add(BaseSkinningWorker);
            foreach (var worker in workers)
                worker.Init(manager);


            // ???????
            SamplerCalcMesh = CustomSampler.Create("CalcMesh");
            SamplerWriteMesh = CustomSampler.Create("WriteMesh");
        }

        /// <summary>
        /// ??
        /// </summary>
        public override void Dispose()
        {
            if (constraints != null)
            {
                foreach (var con in constraints)
                    con.Release();
            }
            if (workers != null)
            {
                foreach (var worker in workers)
                    worker.Release();
            }
        }

        /// <summary>
        /// ?????????/?????????????????????
        /// </summary>
        /// <param name="teamId"></param>
        public void RemoveTeam(int teamId)
        {
            if (MagicaPhysicsManager.Instance.Team.IsValidData(teamId) == false)
                return;

            if (constraints != null)
            {
                foreach (var con in constraints)
                    con.RemoveTeam(teamId);
            }
            if (workers != null)
            {
                foreach (var worker in workers)
                    worker.RemoveGroup(teamId);
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????
        /// </summary>
        internal void UpdateRestoreBone(PhysicsTeam.TeamUpdateMode updateMode)
        {
            // ??????1???????????
            if (Team.ActiveTeamCount > 0)
            {
                // ???????????????
                Bone.ResetBoneFromTransform(updateMode == PhysicsTeam.TeamUpdateMode.UnityPhysics);
            }
        }

        /// <summary>
        /// ??????????
        /// </summary>
        internal void UpdateReadBone()
        {
            // ??????1???????????
            if (Team.ActiveTeamCount > 0)
            {
                // ???????????????
                Bone.ReadBoneFromTransform();
            }
        }

        /// <summary>
        /// ????????????????????
        /// </summary>
        internal void UpdateTeamAlways()
        {
            // ??????????????
            Team.PreUpdateTeamAlways();
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="update"></param>
        internal void UpdateStartSimulation(UpdateTimeManager update)
        {
            // ?????????????????????????????????
            if (MagicaPhysicsManager.Instance.IsActive == false)
                return;

            // ??
            float deltaTime = update.DeltaTime;
            float physicsDeltaTime = update.PhysicsDeltaTime;
            float updatePower = update.UpdatePower;
            float updateDeltaTime = update.UpdateIntervalTime;
            int ups = update.UpdatePerSecond;

            // ??????1???????????
            if (Team.ActiveTeamCount > 0)
            {
                // ???????????
                int updateCount = Team.CalcMaxUpdateCount(ups, deltaTime, physicsDeltaTime, updateDeltaTime);
                //Debug.Log($"updateCount:{updateCount} dtime:{deltaTime} pdtime:{physicsDeltaTime} fixedCount:{update.FixedUpdateCount}");

                // ???
                //Wind.UpdateWind();

                // ??????????????????????????????
                Team.PreUpdateTeamData(deltaTime, physicsDeltaTime, updateDeltaTime, ups, updateCount);

                // ??????
                WarmupWorker();

                // ??????????????????
                Particle.UpdateBoneToParticle();

                // ???????????
                //MasterJob = RenderMeshWorker.PreUpdate(MasterJob); // ????
                MasterJob = VirtualMeshWorker.PreUpdate(MasterJob); // ???????????????????????
                MasterJob = MeshParticleWorker.PreUpdate(MasterJob); // ?????????????????????????
                //MasterJob = SpringMeshWorker.PreUpdate(MasterJob); // ????
                //MasterJob = AdjustRotationWorker.PreUpdate(MasterJob); // ????
                //MasterJob = LineWorker.PreUpdate(MasterJob); // ????
                MasterJob = BaseSkinningWorker.PreUpdate(MasterJob); // ???????????basePos/baseRot??????

                // ?????????????
                Particle.UpdateResetParticle();

                // ????
                for (int i = 0, cnt = updateCount; i < cnt; i++)
                {
                    UpdatePhysics(updateCount, i, updatePower, updateDeltaTime);
                }

                // ???????
                PostUpdatePhysics(updateDeltaTime);

                // ???????????
                MasterJob = TriangleWorker.PostUpdate(MasterJob); // ???????????
                MasterJob = LineWorker.PostUpdate(MasterJob); // ????????
                MasterJob = AdjustRotationWorker.PostUpdate(MasterJob); // ??????????(Adjust Rotation)
                Particle.UpdateParticleToBone(); // ???????????????????(??????????)
                MasterJob = SpringMeshWorker.PostUpdate(MasterJob); // ?????????
                MasterJob = MeshParticleWorker.PostUpdate(MasterJob); // ????????????????????
                MasterJob = VirtualMeshWorker.PostUpdate(MasterJob); // ????????????(?????????????????)
                MasterJob = RenderMeshWorker.PostUpdate(MasterJob); // ??????????????(????????????????????)

                // ?????????????????????
                Bone.ConvertWorldToLocal();

                // ?????????
                Team.PostUpdateTeamData();

            }
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        internal void UpdateCompleteSimulation()
        {
            // ???????????
            CompleteJob();
            runMasterJob = true;

            //Debug.Log($"runMasterJob = true! F:{Time.frameCount}");
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        internal void UpdateWriteBone()
        {
            // ???????????????????
            Bone.WriteBoneToTransform(manager.IsDelay ? 1 : 0);
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        internal void MeshCalculation()
        {
            // ??????????
            SamplerCalcMesh.Begin();

            Mesh.ClearWritingList();

            if (Mesh.VirtualMeshCount > 0 && runMasterJob)
            {
                Mesh.MeshCalculation(manager.IsDelay ? 1 : 0);
            }

            // ??????????
            SamplerCalcMesh.End();
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        internal void NormalWritingMesh()
        {
            // ??????????
            SamplerWriteMesh.Begin();

            // ????????????
            if (Mesh.VirtualMeshCount > 0 && runMasterJob)
            {
                Mesh.NormalWriting(manager.IsDelay ? 1 : 0);
            }

            // ??????????
            SamplerWriteMesh.End();
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        internal void UpdateReadWriteBone()
        {
            // ??????1???????????
            if (Team.ActiveTeamCount > 0)
            {
                // ???????????????
                Bone.ReadBoneFromTransform();

                if (runMasterJob)
                {
                    // ???????????????????
                    Bone.WriteBoneToTransform(manager.IsDelay ? 1 : 0);
                }
            }
        }

        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        internal void UpdateSyncBuffer()
        {
            Bone.writeBoneIndexList.SyncBuffer();
            Bone.writeBonePosList.SyncBuffer();
            Bone.writeBoneRotList.SyncBuffer();
            Bone.boneFlagList.SyncBuffer();

            InitJob();
            Bone.CopyBoneBuffer();
            CompleteJob();
        }

        /// <summary>
        /// ???????????????????????
        /// </summary>
        internal void UpdateSwapBuffer()
        {
            Mesh.renderPosList.SwapBuffer();
            Mesh.renderNormalList.SwapBuffer();
            Mesh.renderTangentList.SwapBuffer();
            Mesh.renderBoneWeightList.SwapBuffer();

            swapIndex ^= 1;

            // ???????????????
            Mesh.SetDelayedCalculatedFlag();
        }

        //=========================================================================================
        public JobHandle MasterJob
        {
            get
            {
                return jobHandle;
            }
            set
            {
                jobHandle = value;
            }
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        public void InitJob()
        {
            jobHandle = default(JobHandle);
        }

        public void ScheduleJob()
        {
            JobHandle.ScheduleBatchedJobs();
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        public void CompleteJob()
        {
            jobHandle.Complete();
            jobHandle = default(JobHandle);
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        public int SwapIndex
        {
            get
            {
                return swapIndex;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// ???1??????????????????
        /// ???????1????????????????????!
        /// </summary>
        /// <param name="updateCount"></param>
        /// <param name="runCount"></param>
        /// <param name="dtime"></param>
        void UpdatePhysics(int updateCount, int runCount, float updatePower, float updateDeltaTime)
        {
            if (Particle.Count == 0)
                return;

            // ??????+????
            var job1 = new ForceAndVelocityJob()
            {
                updateDeltaTime = updateDeltaTime,
                updatePower = updatePower,
                runCount = runCount,

                teamDataList = Team.teamDataList.ToJobArray(),
                teamMassList = Team.teamMassList.ToJobArray(),
                teamGravityList = Team.teamGravityList.ToJobArray(),
                teamDragList = Team.teamDragList.ToJobArray(),
                teamDepthInfluenceList = Team.teamDepthInfluenceList.ToJobArray(),
                teamWindInfoList = Team.teamWindInfoList.ToJobArray(),
                //teamMaxVelocityList = Team.teamMaxVelocityList.ToJobArray(),
                //teamDirectionalDampingList = Team.teamDirectionalDampingList.ToJobArray(),

                flagList = Particle.flagList.ToJobArray(),
                teamIdList = Particle.teamIdList.ToJobArray(),
                depthList = Particle.depthList.ToJobArray(),

                snapBasePosList = Particle.snapBasePosList.ToJobArray(),
                snapBaseRotList = Particle.snapBaseRotList.ToJobArray(),
                basePosList = Particle.basePosList.ToJobArray(),
                baseRotList = Particle.baseRotList.ToJobArray(),
                oldBasePosList = Particle.oldBasePosList.ToJobArray(),
                oldBaseRotList = Particle.oldBaseRotList.ToJobArray(),

                nextPosList = Particle.InNextPosList.ToJobArray(),
                nextRotList = Particle.InNextRotList.ToJobArray(),
                oldPosList = Particle.oldPosList.ToJobArray(),
                oldRotList = Particle.oldRotList.ToJobArray(),
                frictionList = Particle.frictionList.ToJobArray(),
                //oldSlowPosList = Particle.oldSlowPosList.ToJobArray(),

                posList = Particle.posList.ToJobArray(),
                rotList = Particle.rotList.ToJobArray(),
                velocityList = Particle.velocityList.ToJobArray(),

                //boneRotList = Bone.boneRotList.ToJobArray(),

                // wind
                windDataList = Wind.windDataList.ToJobArray(),

                // bone
                bonePosList = Bone.bonePosList.ToJobArray(),
                boneRotList = Bone.boneRotList.ToJobArray(),
            };
            jobHandle = job1.Schedule(Particle.Length, 64, jobHandle);

            // ??????
            if (constraints != null)
            {
                // ???????????
                for (int i = 0; i < solverIteration; i++)
                {
                    foreach (var con in constraints)
                    {
                        if (con != null /*&& con.enabled*/)
                        {
                            // ?????????
                            for (int j = 0; j < con.GetIterationCount(); j++)
                            {
                                jobHandle = con.SolverConstraint(runCount, updateDeltaTime, updatePower, j, jobHandle);
                            }
                        }
                    }
                }
            }

            // ????
            var job2 = new FixPositionJob()
            {
                updatePower = updatePower,
                updateDeltaTime = updateDeltaTime,
                runCount = runCount,

                teamDataList = Team.teamDataList.ToJobArray(),
                teamMaxVelocityList = Team.teamMaxVelocityList.ToJobArray(),

                flagList = Particle.flagList.ToJobArray(),
                teamIdList = Particle.teamIdList.ToJobArray(),
                depthList = Particle.depthList.ToJobArray(),

                nextPosList = Particle.InNextPosList.ToJobArray(),
                nextRotList = Particle.InNextRotList.ToJobArray(),

                //basePosList = Particle.basePosList.ToJobArray(),
                //baseRotList = Particle.baseRotList.ToJobArray(),

                oldPosList = Particle.oldPosList.ToJobArray(),
                oldRotList = Particle.oldRotList.ToJobArray(),

                frictionList = Particle.frictionList.ToJobArray(),

                velocityList = Particle.velocityList.ToJobArray(),
                rotList = Particle.rotList.ToJobArray(),
                posList = Particle.posList.ToJobArray(),
                localPosList = Particle.localPosList.ToJobArray(),

                collisionNormalList = Particle.collisionNormalList.ToJobArray(),
                staticFrictionList = Particle.staticFrictionList.ToJobArray(),
            };
            jobHandle = job2.Schedule(Particle.Length, 64, jobHandle);
        }

        [BurstCompile]
        struct ForceAndVelocityJob : IJobParallelFor
        {
            public float updateDeltaTime;
            public float updatePower;
            public int runCount;

            // team
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<CurveParam> teamMassList;
            [Unity.Collections.ReadOnly]
            public NativeArray<CurveParam> teamGravityList;
            [Unity.Collections.ReadOnly]
            public NativeArray<CurveParam> teamDragList;
            [Unity.Collections.ReadOnly]
            public NativeArray<CurveParam> teamDepthInfluenceList;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.WindInfo> teamWindInfoList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<CurveParam> teamMaxVelocityList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<CurveParam> teamDirectionalDampingList;

            // particle
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> depthList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> snapBasePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> snapBaseRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> basePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> baseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> oldBasePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> oldBaseRotList;

            public NativeArray<float3> nextPosList;
            public NativeArray<quaternion> nextRotList;
            public NativeArray<float> frictionList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> posList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> rotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> oldPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> oldRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> velocityList;

            // wind
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerWindData.WindData> windDataList;

            // bone
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> bonePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> boneRotList;

            // ????????
            public void Execute(int index)
            {
                var flag = flagList[index];
                if (flag.IsValid() == false)
                    return;

                // ??????
                int teamId = teamIdList[index];
                var teamData = teamDataList[teamId];

                // ????????????????(???????????)
                if (teamId != 0 && teamData.IsUpdate(runCount) == false)
                    return;

                var oldpos = oldPosList[index];
                var oldrot = oldRotList[index];
                float3 nextPos = oldpos;
                quaternion nextRot = oldrot;
                var friction = frictionList[index];

                // ???????????(v1.11.1)
                var oldBasePos = oldBasePosList[index];
                var oldBaseRot = oldBaseRotList[index];
                var snapBasePos = snapBasePosList[index];
                var snapBaseRot = snapBaseRotList[index];
                float stime = teamData.startTime + updateDeltaTime * runCount;
                float oldtime = teamData.startTime - updateDeltaTime;
                float interval = teamData.time - oldtime;
                float step = interval >= 1e-06f ? math.saturate((stime - oldtime) / interval) : 0.0f;
                float3 basePos = math.lerp(oldBasePos, snapBasePos, step);
                quaternion baseRot = math.slerp(oldBaseRot, snapBaseRot, step);
                baseRot = math.normalize(baseRot); // ??
                basePosList[index] = basePos;
                baseRotList[index] = baseRot;


                if (flag.IsFixed())
                {
                    // ?????????????
                    nextPos = basePos;
                    nextRot = baseRot;

                    // nextPos/nextRot?1????????
                    var oldNextPos = nextPosList[index];
                    var oldNextRot = nextRotList[index];

                    // ??????oldpos/rot???posList/rotList?????
                    if (flag.IsCollider() && teamId == 0)
                    {
                        // ??????????
                        // ??????????????(1.7.5)
                        // ????????????/?????????????????????????????????
                        oldpos = MathUtility.ClampDistance(nextPos, oldNextPos, Define.Compute.GlobalColliderMaxMoveDistance);
                        oldrot = MathUtility.ClampAngle(nextRot, oldNextRot, math.radians(Define.Compute.GlobalColliderMaxRotationAngle));
                    }
                    else
                    {
                        oldpos = oldNextPos;
                        oldrot = oldNextRot;
                    }

#if false
                    // nextPos/nextRot?1????????
                    var oldNextPos = nextPosList[index];
                    var oldNextRot = nextRotList[index];

                    // oldpos/rot?????????????
                    // oldpos/rot ?? BasePos/Rot ? step ????????????
                    float stime = teamData.startTime + updateDeltaTime * runCount;
                    float oldtime = teamData.startTime - updateDeltaTime;
                    float interval = teamData.time - oldtime;
                    float step = interval >= 1e-06f ? math.saturate((stime - oldtime) / interval) : 0.0f;

                    nextPos = math.lerp(oldpos, basePosList[index], step);
                    nextRot = math.slerp(oldrot, baseRotList[index], step);
                    nextRot = math.normalize(nextRot);

                    // ??????oldpos/rot???posList/rotList?????
                    if (flag.IsCollider() && teamId == 0)
                    {
                        // ??????????
                        // ??????????????(1.7.5)
                        // ????????????/?????????????????????????????????
                        oldpos = MathUtility.ClampDistance(nextPos, oldNextPos, Define.Compute.GlobalColliderMaxMoveDistance);
                        oldrot = MathUtility.ClampAngle(nextRot, oldNextRot, math.radians(Define.Compute.GlobalColliderMaxRotationAngle));
                    }
                    else
                    {
                        oldpos = oldNextPos;
                        oldrot = oldNextRot;
                    }
#endif

                    // debug
                    //nextPos = basePosList[index];
                    //nextRot = baseRotList[index];
                }
                else
                {
                    // ????????
                    var depth = depthList[index];
                    //var maxVelocity = teamMaxVelocityList[teamId].Evaluate(depth);
                    var drag = teamDragList[teamId].Evaluate(depth);
                    var gravity = teamGravityList[teamId].Evaluate(depth);
                    var gravityDirection = teamData.gravityDirection;
                    var mass = teamMassList[teamId].Evaluate(depth);
                    var depthInfluence = teamDepthInfluenceList[teamId].Evaluate(depth);
                    var velocity = velocityList[index];

                    // ?????????
                    //maxVelocity *= teamData.scaleRatio;

                    // mass???????????????????????????????
                    //mass = (mass - 1.0f) * teamData.forceMassInfluence + 1.0f;

                    // ???????????
                    velocity *= teamData.velocityWeight;

                    // ????
                    //velocity = MathUtility.ClampVector(velocity, 0.0f, maxVelocity);

                    // ????(90ups??)
                    // ???????????????????(????force???????????)
                    velocity *= math.pow(1.0f - drag, updatePower);

                    // ????
                    // ??????????????????
                    float3 force = 0;

                    // ??(?????????)
                    // (????????????????????)
                    force += gravityDirection * (gravity * mass);

                    // ??????
                    {
                        float3 exForce = 0;
                        switch (teamData.forceMode)
                        {
                            case PhysicsManagerTeamData.ForceMode.VelocityAdd:
                                exForce += teamData.impactForce;
                                break;
                            case PhysicsManagerTeamData.ForceMode.VelocityAddWithoutMass:
                                exForce += teamData.impactForce * mass;
                                break;
                            case PhysicsManagerTeamData.ForceMode.VelocityChange:
                                exForce += teamData.impactForce;
                                velocity = 0;
                                break;
                            case PhysicsManagerTeamData.ForceMode.VelocityChangeWithoutMass:
                                exForce += teamData.impactForce * mass;
                                velocity = 0;
                                break;
                        }

                        // ??
                        exForce += teamData.externalForce;

                        // ?(?????????)
                        if (teamData.IsFlag(PhysicsManagerTeamData.Flag_Wind))
                            exForce += Wind(teamId, teamData, snapBasePos) * mass;

                        // ???????
                        exForce *= depthInfluence;

                        force += exForce;
                    }

                    // ???????????
                    force *= teamData.scaleRatio;

                    // ????(?????)
                    velocity += (force / mass) * updateDeltaTime;

                    // ?????????????
                    nextPos = oldpos + velocity * updateDeltaTime;
                }

                // ?????? ==============================================================
                // ????
                friction = friction * Define.Compute.FrictionDampingRate;
                frictionList[index] = friction;
                //frictionList[index] = 0;

                // ??????
                posList[index] = oldpos;
                rotList[index] = oldrot;

                // ????
                nextPosList[index] = nextPos;
                nextRotList[index] = nextRot;
            }

            /// <summary>
            /// ????
            /// </summary>
            /// <param name="teamId"></param>
            /// <param name="teamData"></param>
            /// <param name="pos"></param>
            /// <returns></returns>
            float3 Wind(int teamId, in PhysicsManagerTeamData.TeamData teamData, in float3 pos)
            {
                var windInfo = teamWindInfoList[teamId];

                // ?????
                // ?????????????????????????
                float sync = math.lerp(3.0f, 0.1f, teamData.forceWindSynchronization);
                var noiseBasePos = new float2(pos.x, pos.z) * sync;

                float3 externalForce = 0;
                for (int i = 0; i < 4; i++)
                {
                    int windId = windInfo.windDataIndexList[i];
                    if (windId < 0)
                        continue;

                    var windData = windDataList[windId];
                    float3 windForce = PhysicsManagerWindData.CalcWindForce(
                        teamData.time,
                        noiseBasePos,
                        windInfo.windDirectionList[i],
                        windInfo.windMainList[i],
                        windData.turbulence,
                        windData.frequency,
                        teamData.forceWindRandomScale
                        );

                    externalForce += windForce;
                }

                // ?????????
                externalForce *= teamData.forceWindInfluence;

                return externalForce;
            }
        }

        [BurstCompile]
        struct FixPositionJob : IJobParallelFor
        {
            public float updatePower;
            public float updateDeltaTime;
            public int runCount;

            // ???
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<CurveParam> teamMaxVelocityList;

            // ????????
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> depthList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> nextPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> nextRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> frictionList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<float3> basePosList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<quaternion> baseRotList;

            public NativeArray<float3> velocityList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> rotList;

            public NativeArray<float3> oldPosList;
            public NativeArray<quaternion> oldRotList;

            public NativeArray<float3> posList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> localPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> collisionNormalList;
            public NativeArray<float> staticFrictionList;

            // ????????
            public void Execute(int index)
            {
                var flag = flagList[index];
                if (flag.IsValid() == false)
                    return;

                // ??????
                int teamId = teamIdList[index];
                var teamData = teamDataList[teamId];

                // ????????????????
                if (teamData.IsUpdate(runCount) == false)
                    return;

                // ????(m/s)
                if (flag.IsFixed() == false)
                {
                    // ??????????
                    var nextPos = nextPosList[index];
                    var nextRot = nextRotList[index];
                    nextRot = math.normalize(nextRot); // ??????????????????????

                    float3 velocity = 0;

                    // posList?????????????????????
                    var pos = posList[index];
                    var oldpos = oldPosList[index];

                    // ?????????
                    float friction = frictionList[index];
                    var cn = collisionNormalList[index];
                    bool isCollision = math.lengthsq(cn) > Define.Compute.Epsilon; // ?????

#if true
                    // ????
                    float staticFriction = staticFrictionList[index];
                    if (isCollision && friction > 0.0f)
                    {
                        // ???????????????
                        var v = nextPos - oldpos;
                        v = v - MathUtility.Project(v, cn);
                        float tangentVelocity = math.length(v) / updateDeltaTime; // ?????????
                        float stopVelocity = teamData.staticFriction * teamData.scaleRatio; // ????

                        if (tangentVelocity < stopVelocity)
                        {
                            staticFriction = math.saturate(staticFriction + 0.02f * updatePower); // ????
                        }
                        else
                        {
                            // ?????????????
                            var vel = tangentVelocity - stopVelocity;
                            var value = math.max(vel / 0.2f, 0.05f) * updatePower;
                            staticFriction = math.saturate(staticFriction - value);
                        }

                        // ????????????????????????????
                        v *= staticFriction;
                        nextPos -= v;
                        pos -= v;
                    }
                    else
                    {
                        staticFriction = math.saturate(staticFriction - 0.05f * updatePower); // ????
                    }
                    staticFrictionList[index] = staticFriction;
#endif

                    // ????(m/s)
                    velocity = (nextPos - pos) / updateDeltaTime;
                    velocity *= teamData.velocityWeight; // ???????????

#if true
                    // ??????????(????????????????????)
                    if (friction > Define.Compute.Epsilon && isCollision && math.lengthsq(velocity) >= Define.Compute.Epsilon)
                    {
                        var dot = math.dot(cn, math.normalize(velocity));
                        dot = 0.5f + 0.5f * dot; // 1.0(front) - 0.5(side) - 0.0(back)
                        dot *= dot; // ???????
                        dot = 1.0f - dot; // 0.0(front) - 0.75(side) - 1.0(back)
                        velocity -= velocity * (dot * math.saturate(friction * teamData.dynamicFriction * 1.5f)); // ??????????????
                    }
#else
                    // ?????????(?)
                    friction *= teamData.friction; // ??????????
                    velocity *= math.pow(1.0f - math.saturate(friction), updatePower);
#endif

                    // ????
                    var depth = depthList[index];
                    var maxVelocity = teamMaxVelocityList[teamId].Evaluate(depth);
                    maxVelocity *= teamData.scaleRatio; // ???????
                    velocity = MathUtility.ClampVector(velocity, 0.0f, maxVelocity);

                    // ???????(localPos???)
                    var realVelocity = (nextPos - oldpos) / updateDeltaTime;
                    realVelocity = MathUtility.ClampVector(realVelocity, 0.0f, maxVelocity); // ?????????
                    localPosList[index] = realVelocity;

                    // ????
                    velocityList[index] = velocity;

                    oldPosList[index] = nextPos;
                    oldRotList[index] = nextRot;

                }
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="updateDeltaTime"></param>
        void PostUpdatePhysics(float updateDeltaTime)
        {
            if (Particle.Count == 0)
                return;

            var job = new PostUpdatePhysicsJob()
            {
                updateDeltaTime = updateDeltaTime,

                teamDataList = Team.teamDataList.ToJobArray(),

                flagList = Particle.flagList.ToJobArray(),
                teamIdList = Particle.teamIdList.ToJobArray(),

                snapBasePosList = Particle.snapBasePosList.ToJobArray(),
                snapBaseRotList = Particle.snapBaseRotList.ToJobArray(),
                basePosList = Particle.basePosList.ToJobArray(),
                baseRotList = Particle.baseRotList.ToJobArray(),
                oldBasePosList = Particle.oldBasePosList.ToJobArray(),
                oldBaseRotList = Particle.oldBaseRotList.ToJobArray(),

                oldPosList = Particle.oldPosList.ToJobArray(),
                oldRotList = Particle.oldRotList.ToJobArray(),

                velocityList = Particle.velocityList.ToJobArray(),
                localPosList = Particle.localPosList.ToJobArray(),

                posList = Particle.posList.ToJobArray(),
                rotList = Particle.rotList.ToJobArray(),
                nextPosList = Particle.InNextPosList.ToJobArray(),
                nextRotList = Particle.InNextRotList.ToJobArray(),

                oldSlowPosList = Particle.oldSlowPosList.ToJobArray(),
            };
            jobHandle = job.Schedule(Particle.Length, 64, jobHandle);
        }

        [BurstCompile]
        struct PostUpdatePhysicsJob : IJobParallelFor
        {
            public float updateDeltaTime;

            // ???
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

            // ????????
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;

            // ????????
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> snapBasePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> snapBaseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> basePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> baseRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> oldBasePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> oldBaseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> velocityList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> localPosList;

            public NativeArray<float3> oldPosList;
            public NativeArray<quaternion> oldRotList;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> posList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> rotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> nextPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> nextRotList;

            public NativeArray<float3> oldSlowPosList;

            // ????????
            public void Execute(int index)
            {
                var flag = flagList[index];
                if (flag.IsValid() == false)
                    return;

                // ??????
                int teamId = teamIdList[index];
                var teamData = teamDataList[teamId];

                float3 viewPos = 0;
                quaternion viewRot = quaternion.identity;

                //var basePos = basePosList[index];
                //var baseRot = baseRotList[index];
                var snapBasePos = snapBasePosList[index];
                var snapBaseRot = snapBaseRotList[index];

                if (flag.IsFixed() == false)
                {
                    // ????
                    // 1????????????????????????????????????
                    //var velocity = velocityList[index]; // ??
                    //var velocity = posList[index]; // ?????(?????????????????)
                    var velocity = localPosList[index]; // ?????

                    var futurePos = oldPosList[index] + velocity * updateDeltaTime;
                    var oldViewPos = oldSlowPosList[index];
                    float addTime = teamData.addTime;
                    float oldTime = teamData.time - addTime;
                    float futureTime = teamData.time + (updateDeltaTime - teamData.nowTime);
                    float interval = futureTime - oldTime;
                    //Debug.Log($"addTime:{teamData.addTime} interval:{interval}");
                    if (addTime > 1e-06f && interval > 1e-06f)
                    {
                        float ratio = teamData.addTime / interval;
                        viewPos = math.lerp(oldViewPos, futurePos, ratio);
                    }
                    else
                    {
                        viewPos = oldViewPos;
                    }
                    viewRot = oldRotList[index];
                    viewRot = math.normalize(viewRot); // ??????????????????????
#if false
                    // ???????
                    futurePos = oldPosList[index];
                    viewPos = futurePos;
#endif

                    oldSlowPosList[index] = viewPos;
                }
                else
                {
                    // ?????????????????????
                    //viewPos = basePos;
                    //viewRot = baseRot;
                    viewPos = snapBasePos;
                    viewRot = snapBaseRot;

                    // ????????????basePos?????(?????)
                    if (teamData.IsRunning())
                    {
                        // ???????????
                        oldPosList[index] = nextPosList[index];
                        oldRotList[index] = nextRotList[index];
                    }
                }

                // ????
                if (teamData.blendRatio < 0.99f)
                {
                    //viewPos = math.lerp(basePos, viewPos, teamData.blendRatio);
                    //viewRot = math.slerp(baseRot, viewRot, teamData.blendRatio);
                    viewPos = math.lerp(snapBasePos, viewPos, teamData.blendRatio);
                    viewRot = math.slerp(snapBaseRot, viewRot, teamData.blendRatio);
                    viewRot = math.normalize(viewRot); // ??????????????????????
                }

                // test
                //viewPos = snapBasePos;
                //viewRot = snapBaseRot;


                // ????
                posList[index] = viewPos;
                rotList[index] = viewRot;

                // 1??????????
                if (teamData.IsRunning())
                {
                    oldBasePosList[index] = basePosList[index];
                    oldBaseRotList[index] = baseRotList[index];
                }
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????
        /// </summary>
        void WarmupWorker()
        {
            if (workers == null || workers.Count == 0)
                return;

            for (int i = 0; i < workers.Count; i++)
            {
                var worker = workers[i];
                worker.Warmup();
            }
        }
    }
}
