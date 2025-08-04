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
    /// ??????
    /// [Algorithm 1]
    /// ??????????????????????????????
    /// ?????????????????????
    /// ·?????????????????????????????
    /// ·????????????????????????????????????????
    /// </summary>
    public class RestoreRotationConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ?????
        /// todo:?????
        /// </summary>
        [System.Serializable]
        public struct RotationData
        {
            /// <summary>
            /// ??????????
            /// </summary>
            public ushort vertexIndex;

            /// <summary>
            /// ?????????????
            /// </summary>
            public ushort targetVertexIndex;

            /// <summary>
            /// ????????????????(??????)
            /// </summary>
            public float3 localPos;

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return vertexIndex > 0 || targetVertexIndex > 0;
            }
        }
        FixedChunkNativeArray<RotationData> dataList;

        /// <summary>
        /// ????????????????
        /// </summary>
        FixedChunkNativeArray<ReferenceDataIndex> refDataList;

        /// <summary>
        /// ????????????
        /// </summary>
        public struct GroupData
        {
            public int teamId;
            public int active;

            public CurveParam restorePower;

            /// <summary>
            /// ????
            /// </summary>
            public float velocityInfluence;

            public ChunkData dataChunk;
            public ChunkData refChunk;
        }
        public FixedNativeList<GroupData> groupList;

        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<RotationData>();
            refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
            groupList = new FixedNativeList<GroupData>();
        }

        public override void Release()
        {
            dataList.Dispose();
            refDataList.Dispose();
            groupList.Dispose();
        }

        //=========================================================================================
        public int AddGroup(
            int teamId,
            bool active,
            BezierParam power,
            float velocityInfluence,
            RotationData[] dataArray,
            ReferenceDataIndex[] refDataArray
            )
        {
            if (dataArray == null || dataArray.Length == 0 || refDataArray == null || refDataArray.Length == 0)
                return -1;

            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.restorePower.Setup(power);
            gdata.velocityInfluence = velocityInfluence;
            gdata.dataChunk = dataList.AddChunk(dataArray.Length);
            gdata.refChunk = refDataList.AddChunk(refDataArray.Length);

            // ??????????
            dataList.ToJobArray().CopyFromFast(gdata.dataChunk.startIndex, dataArray);
            refDataList.ToJobArray().CopyFromFast(gdata.refChunk.startIndex, refDataArray);

            int group = groupList.Add(gdata);
            return group;
        }

        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.restoreRotationGroupIndex;
            if (group < 0)
                return;

            var cdata = groupList[group];

            // ?????????
            dataList.RemoveChunk(cdata.dataChunk);
            refDataList.RemoveChunk(cdata.refChunk);

            // ?????
            groupList.Remove(group);
        }

        public void ChangeParam(int teamId, bool active, BezierParam power, float velocityInfluence)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.restoreRotationGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            gdata.restorePower.Setup(power);
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

            // ????(?????????????)
            var job1 = new RotationJob()
            {
                updatePower = updatePower,
                runCount = runCount,

                dataList = dataList.ToJobArray(),
                groupList = groupList.ToJobArray(),
                refDataList = refDataList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),
                teamIdList = Manager.Particle.teamIdList.ToJobArray(),

                flagList = Manager.Particle.flagList.ToJobArray(),
                depthList = Manager.Particle.depthList.ToJobArray(),
                baseRotList = Manager.Particle.baseRotList.ToJobArray(),
                nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                frictionList = Manager.Particle.frictionList.ToJobArray(),

                outNextPosList = Manager.Particle.OutNextPosList.ToJobArray(),
                posList = Manager.Particle.posList.ToJobArray(),
            };
            jobHandle = job1.Schedule(Manager.Particle.Length, 64, jobHandle);
            Manager.Particle.SwitchingNextPosList();

            return jobHandle;
        }

        /// <summary>
        /// ???????[Algorithm 1]
        /// ???????????
        /// </summary>
        [BurstCompile]
        struct RotationJob : IJobParallelFor
        {
            public float updatePower;
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<RotationData> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupList;
            [Unity.Collections.ReadOnly]
            public NativeArray<ReferenceDataIndex> refDataList;

            // ???
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> depthList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> baseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> nextPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> frictionList;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> outNextPosList;
            public NativeArray<float3> posList;

            // ????????
            public void Execute(int index)
            {
                // ??????
                var nextpos = nextPosList[index];
                outNextPosList[index] = nextpos;

                var flag = flagList[index];
                if (flag.IsValid() == false || flag.IsFixed())
                    return;

                // ???
                var team = teamDataList[teamIdList[index]];
                if (team.IsActive() == false || team.restoreRotationGroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                int pstart = team.particleChunk.startIndex;
                int vindex = index - pstart;

                // ???????????
                var gdata = groupList[team.restoreRotationGroupIndex];
                if (gdata.active == 0)
                    return;

                // ????
                var refdata = refDataList[gdata.refChunk.startIndex + vindex];
                if (refdata.count > 0)
                {
                    // power
                    float depth = depthList[index];
                    float dataPower = gdata.restorePower.Evaluate(depth);

                    // ???
                    // 90ups??
                    float power = 1.0f - math.pow(1.0f - dataPower, updatePower);

                    float3 addpos = 0;

                    // ???????
                    //float3 bpos = basePosList[index];

                    int dataIndex = gdata.dataChunk.startIndex + refdata.startIndex;
                    for (int i = 0; i < refdata.count; i++, dataIndex++)
                    {
                        var data = dataList[dataIndex];

                        if (data.IsValid() == false)
                            continue;

                        // ?
                        int pindex = pstart + data.targetVertexIndex;
                        quaternion prot = baseRotList[pindex]; // ?????????????
                        var ppos = nextPosList[pindex]; // ??????????
                        //float3 pbpos = basePosList[pindex];
                        //var ppos = nextPosList[pindex];

                        // ?????????
                        //float3 tv = math.mul(prot, data.localPos); // v1.7.0
                        float3 tv = math.mul(prot, data.localPos * team.scaleDirection); // ??????????(v1.7.6)
                        //float3 tv = bpos - pbpos;

                        // ??????
                        float3 v = nextpos - ppos;

                        // ??????
                        var q = MathUtility.FromToRotation(v, tv, power);
                        v = math.mul(q, v);
                        float3 gpos = ppos + v;

                        // ????
                        addpos += gpos - nextpos;
                    }

                    // ????????????
                    float friction = frictionList[index];
                    float moveratio = math.saturate(1.0f - friction * Define.Compute.FrictionMoveRatio);
                    addpos *= moveratio;

                    var opos = nextpos;
                    nextpos += addpos / refdata.count;

                    // ????
                    outNextPosList[index] = nextpos;

                    // ????
                    var av = (nextpos - opos) * (1.0f - gdata.velocityInfluence);
                    posList[index] = posList[index] + av;
                }
            }
        }
    }
}
