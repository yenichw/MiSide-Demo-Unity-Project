// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????
    /// nextpos???pos????????????
    /// </summary>
    public class ClampPositionConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ????????????
        /// </summary>
        public struct GroupData
        {
            public int teamId;
            public int active;

            public CurveParam limitLength;

            /// <summary>
            /// ??????????(0.0-1.0)
            /// </summary>
            public float3 axisRatio;

            /// <summary>
            /// ????
            /// </summary>
            public float velocityInfluence;

            /// <summary>
            /// ??????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsAxisCheck()
            {
                return axisRatio.x < 0.999f || axisRatio.y < 0.999f || axisRatio.z < 0.999f;
            }
        }
        public FixedNativeList<GroupData> groupList;

        //=========================================================================================
        public override void Create()
        {
            groupList = new FixedNativeList<GroupData>();
        }

        public override void Release()
        {
            groupList.Dispose();
        }

        //=========================================================================================
        public int AddGroup(int teamId, bool active, BezierParam limitLength, float3 axisRatio, float velocityInfluence)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.limitLength.Setup(limitLength);
            gdata.axisRatio = axisRatio;
            gdata.velocityInfluence = velocityInfluence;

            int group = groupList.Add(gdata);
            return group;
        }

        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.clampPositionGroupIndex;
            if (group < 0)
                return;

            // ?????
            groupList.Remove(group);
        }

        public void ChangeParam(int teamId, bool active, BezierParam limitLength, float3 axisRatio, float velocityInfluence)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.clampPositionGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            gdata.limitLength.Setup(limitLength);
            gdata.axisRatio = axisRatio;
            gdata.velocityInfluence = velocityInfluence;
            groupList[group] = gdata;
        }

        //=========================================================================================
        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="dtime"></param>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        public override JobHandle SolverConstraint(int runCount, float dtime, float updatePower, int iteration, JobHandle jobHandle)
        {
            if (groupList.Count == 0)
                return jobHandle;

            // ????????(?????????????)
            var job1 = new ClampPositionJob()
            {
                runCount = runCount,
                maxMoveLength = dtime * Define.Compute.ClampPositionMaxVelocity, // ??1.0m/s

                clampPositionGroupList = groupList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),
                teamIdList = Manager.Particle.teamIdList.ToJobArray(),

                flagList = Manager.Particle.flagList.ToJobArray(),
                depthList = Manager.Particle.depthList.ToJobArray(),
                basePosList = Manager.Particle.basePosList.ToJobArray(),
                baseRotList = Manager.Particle.baseRotList.ToJobArray(),
                frictionList = Manager.Particle.frictionList.ToJobArray(),

                nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                posList = Manager.Particle.posList.ToJobArray(),
            };
            jobHandle = job1.Schedule(Manager.Particle.Length, 64, jobHandle);

            return jobHandle;
        }

        /// <summary>
        /// ???????????
        /// ???????????
        /// </summary>
        [BurstCompile]
        struct ClampPositionJob : IJobParallelFor
        {
            public int runCount;

            // ??????
            public float maxMoveLength;

            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> clampPositionGroupList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> depthList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> basePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> baseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> frictionList;

            public NativeArray<float3> nextPosList;
            public NativeArray<float3> posList;

            // ????????
            public void Execute(int index)
            {
                var flag = flagList[index];
                if (flag.IsValid() == false || flag.IsFixed())
                    return;

                var team = teamDataList[teamIdList[index]];
                if (team.IsActive() == false)
                    return;
                if (team.clampPositionGroupIndex < 0)
                    return;
                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // ???????
                var gdata = clampPositionGroupList[team.clampPositionGroupIndex];
                if (gdata.active == 0)
                    return;

                var nextpos = nextPosList[index];
                var depth = depthList[index];
                var limitLength = gdata.limitLength.Evaluate(depth);

                // ?????????
                limitLength *= team.scaleRatio;

                // basepos???????????
                var basepos = basePosList[index];
                var v = nextpos - basepos; // nextpos

                // ????????????
                var friction = frictionList[index];
                float moveratio = math.saturate(1.0f - friction * Define.Compute.FrictionMoveRatio);


                if (gdata.IsAxisCheck())
                {
                    // ?????
                    float3 axisRatio = gdata.axisRatio;
                    // ??????????
                    quaternion rot = baseRotList[index];
                    // ???????????????
                    quaternion irot = math.inverse(rot);
                    float3 lv = math.mul(irot, v);

                    // Box????
                    float3 axisRatio1 = axisRatio * limitLength;
                    lv = math.clamp(lv, -axisRatio1, axisRatio1);

                    // ???????????????
                    // ????(v)???????????????????????
                    v = math.mul(rot, lv);
                }

                // nextpos???
                v = MathUtility.ClampVector(v, 0.0f, limitLength);

                // ????????
                v = (basepos + v) - nextpos;

                // ????
                var opos = nextpos;
                var fpos = opos + v;

                // ???????????(??????????????????)
                nextpos = math.lerp(opos, fpos, moveratio);

                // ????
                nextPosList[index] = nextpos;

                // ????
                var av = (nextpos - opos) * (1.0f - gdata.velocityInfluence);
                posList[index] = posList[index] + av;
            }
        }
    }
}
