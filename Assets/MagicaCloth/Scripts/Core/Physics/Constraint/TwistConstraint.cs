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
    /// ???????
    /// </summary>
    public class TwistConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ?????
        /// </summary>
        [System.Serializable]
        public struct TwistData
        {
            /// <summary>
            /// ??????????
            /// </summary>
            public ushort vertexIndex0;
            public ushort vertexIndex1;

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return vertexIndex0 > 0 || vertexIndex1 > 0;
            }
        }
        FixedChunkNativeArray<TwistData> dataList;

        /// <summary>
        /// ????????????????
        /// </summary>
        FixedChunkNativeArray<ReferenceDataIndex> refDataList;

        /// <summary>
        /// ??????????????????
        /// </summary>
        //FixedChunkNativeArray<short> groupIndexList;

        /// <summary>
        /// ????????????
        /// </summary>
        public struct GroupData
        {
            public int teamId;
            public int active;

            public float recoveryPower;

            public ChunkData dataChunk;
            public ChunkData refChunk;
        }
        public FixedNativeList<GroupData> groupList;


        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<TwistData>();
            //groupIndexList = new FixedChunkNativeArray<short>();
            refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
            groupList = new FixedNativeList<GroupData>();
        }

        public override void Release()
        {
            dataList.Dispose();
            //groupIndexList.Dispose();
            refDataList.Dispose();
            groupList.Dispose();
        }

        //=========================================================================================
        public int AddGroup(int teamId, bool active, float recoveryPower, TwistData[] dataArray, ReferenceDataIndex[] refArray)
        {
            if (dataArray == null || dataArray.Length == 0)
                return -1;

            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.recoveryPower = recoveryPower;
            gdata.dataChunk = dataList.AddChunk(dataArray.Length);
            gdata.refChunk = refDataList.AddChunk(refArray.Length);
            //groupIndexList.AddChunk(refArray.Length);

            // ??????????
            dataList.ToJobArray().CopyFromFast(gdata.dataChunk.startIndex, dataArray);
            refDataList.ToJobArray().CopyFromFast(gdata.refChunk.startIndex, refArray);

            int group = groupList.Add(gdata);

            // ??????????????????
            //groupIndexList.Fill(gdata.refChunk, (short)group);

            return group;
        }

        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.twistGroupIndex;
            if (group < 0)
                return;

            var cdata = groupList[group];

            // ?????????
            dataList.RemoveChunk(cdata.dataChunk);
            //groupIndexList.RemoveChunk(cdata.refChunk);
            refDataList.RemoveChunk(cdata.refChunk);

            // ?????
            groupList.Remove(group);
        }

        public void ChangeParam(int teamId, bool active, float recoveryPower)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.twistGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            gdata.recoveryPower = recoveryPower;
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

#if false
            // ?????
            var job1 = new TwistJob()
            {
                runCount = runCount,

                dataList = dataList.ToJobArray(),
                groupIndexList = groupIndexList.ToJobArray(),
                groupList = groupList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                //depthList = Manager.Particle.depthList.ToJobArray(),
                //flagList = Manager.Particle.flagList.ToJobArray(),
                basePosList = Manager.Particle.basePosList.ToJobArray(),
                frictionList = Manager.Particle.frictionList.ToJobArray(),
                nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                posList = Manager.Particle.posList.ToJobArray(),
            };
            jobHandle = job1.Schedule(dataList.Length, 2, jobHandle); // ????????2
#endif
            // ?????
            var job = new TwistJob2()
            {
                runCount = runCount,
                updatePower = updatePower,

                dataList = dataList.ToJobArray(),
                refDataList = refDataList.ToJobArray(),
                groupList = groupList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),
                teamIdList = Manager.Particle.teamIdList.ToJobArray(),

                flagList = Manager.Particle.flagList.ToJobArray(),
                basePosList = Manager.Particle.basePosList.ToJobArray(),
                nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                frictionList = Manager.Particle.frictionList.ToJobArray(),

                outNextPosList = Manager.Particle.OutNextPosList.ToJobArray(),
                posList = Manager.Particle.posList.ToJobArray(),
            };
            jobHandle = job.Schedule(Manager.Particle.Length, 64, jobHandle);
            Manager.Particle.SwitchingNextPosList();

            return jobHandle;
        }

        [BurstCompile]
        struct TwistJob2 : IJobParallelFor
        {
            public int runCount;
            public float updatePower;

            [Unity.Collections.ReadOnly]
            public NativeArray<TwistData> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<ReferenceDataIndex> refDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupList;

            // team
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;

            // particle
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> basePosList;
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
                var nextpos0 = nextPosList[index];
                outNextPosList[index] = nextpos0;

                var flag = flagList[index];
                if (flag.IsValid() == false || flag.IsFixed())
                    return;

                var team = teamDataList[teamIdList[index]];
                if (team.twistGroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // ????
                var gdata = groupList[team.twistGroupIndex];
                if (gdata.active == 0)
                    return;

                int pstart = team.particleChunk.startIndex;
                int vindex = index - pstart;

                // ????
                var refdata = refDataList[gdata.refChunk.startIndex + vindex];
                if (refdata.count == 0)
                    return;

                // ?????
                var basepos0 = basePosList[index];
                var friction0 = frictionList[index];
                float moveratio = math.saturate(1.0f - friction0 * Define.Compute.FrictionMoveRatio);

                // ??
                float stiffness = math.saturate(gdata.recoveryPower * updatePower);

                // ?????
                float3 addpos = 0;
                int addcnt = 0;
                int dataIndex = gdata.dataChunk.startIndex + refdata.startIndex;
                for (int i = 0; i < refdata.count; i++, dataIndex++)
                {
                    var data = dataList[dataIndex];
                    if (data.IsValid() == false)
                        continue;

                    // ????????
                    int tindex = pstart + data.vertexIndex1;
                    var nextpos1 = nextPosList[tindex];
                    var basepos1 = basePosList[tindex];

#if false
                    // ??????
                    var v = basepos1 - basepos0;
                    float len = math.length(v);
                    float3 n = math.normalize(v);

                    // ????????????
                    float3 cen = (nextpos0 + nextpos1) * 0.5f;
                    float3 cen0 = cen - n * (len * 0.25f);
                    float3 cen1 = cen + n * (len * 0.25f);

                    // ??????????????????????
                    float3 outPos0;
                    MathUtility.IntersectPointPlane(cen0, -n, nextpos0, out outPos0);
                    //float3 outPos1;
                    //MathUtility.IntersectPointPlane(cen1, n, nextpos1, out outPos1);

                    // ??
                    // ?????????
                    float stiffness0 = 1.0f;
                    //float stiffness1 = 1.0f;

                    // ?????
                    float3 addPos0 = (outPos0 - nextpos0) * stiffness0;
                    //float3 addPos1 = (outPos1 - nextpos1) * stiffness1;

                    // ????
                    addPos0 *= moveratio;
                    //addPos1 *= moveratio1;

                    float3 add = addPos0;
#else
                    // ??????????
                    // ??????????????????
                    // ???????
                    float3 tv = basepos1 - basepos0;

                    // ???????
                    float3 v = nextpos1 - nextpos0;
                    float3 cen = nextpos0 + v * 0.5f;

                    // ????
                    var q = MathUtility.FromToRotation(v, tv, stiffness);
                    v = math.mul(q, v);

                    float3 fpos = cen - v * 0.5f;

                    float3 add = fpos - nextpos0;
                    add *= moveratio; // ????
#endif

                    // ????
                    addpos += add;
                    addcnt++;
                }

                // ????
                if (addcnt > 0)
                {
                    addpos /= addcnt;

                    outNextPosList[index] = nextpos0 + addpos;

                    // ????
                    const float influence = 0.1f; // ???????
                    posList[index] = posList[index] + (addpos * (1.0f - influence));
                }
            }
        }



#if false
        /// <summary>
        /// ????????
        /// </summary>
        [BurstCompile]
        struct TwistJob : IJobParallelFor
        {
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<TwistData> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<short> groupIndexList;
            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupList;

            // ???
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<float> depthList;

            //[Unity.Collections.ReadOnly]
            //public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> basePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> frictionList;

            [NativeDisableParallelForRestriction]
            public NativeArray<float3> nextPosList;

            [NativeDisableParallelForRestriction]
            public NativeArray<float3> posList;

            // ???????
            public void Execute(int dataIndex)
            {
                var data = dataList[dataIndex];
                if (data.IsValid() == false)
                    return;

                // group
                int groupIndex = groupIndexList[dataIndex];
                var gdata = groupList[groupIndex];
                if (gdata.active == 0)
                    return;

                // team
                int teamIndex = gdata.teamId;
                var team = teamDataList[teamIndex];
                if (team.IsActive() == false || team.twistGroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // particle
                int pstart = team.particleChunk.startIndex;
                int pindex0 = data.vertexIndex0 + pstart;
                int pindex1 = data.vertexIndex1 + pstart;

                float3 basePos0 = basePosList[pindex0];
                float3 basePos1 = basePosList[pindex1];
                float3 nextPos0 = nextPosList[pindex0];
                float3 nextPos1 = nextPosList[pindex1];

                // ???
                //var v = basePos1 - basePos0;
                //float len = math.length(v);
                //float3 n = math.normalize(v);
                var bv = basePos1 - basePos0;
                bv = math.normalize(bv);

                // ??????
                // ????????????
                float3 cen = (nextPos0 + nextPos1) * 0.5f;
                //float3 cen0 = cen - n * (len * 0.25f);
                //float3 cen1 = cen + n * (len * 0.25f);
                var v = nextPos1 - nextPos0;
                //v = math.normalize(v);

                // ??
                // ?????????
                //float stiffness0 = 1.0f;
                //float stiffness1 = 1.0f;
                float stiffness = 0.5f;


                // ??????????????????????
                //float3 outPos0;
                //MathUtility.IntersectPointPlane(cen0, -n, nextPos0, out outPos0);
                //float3 outPos1;
                //MathUtility.IntersectPointPlane(cen1, n, nextPos1, out outPos1);

                var q = MathUtility.FromToRotation(v, bv, stiffness);
                var rv = math.mul(q, v);


                // ?????
                //float3 addPos0 = (outPos0 - nextPos0) * stiffness0;
                //float3 addPos1 = (outPos1 - nextPos1) * stiffness1;
                float3 fpos0 = cen - rv * 0.5f;
                float3 fpos1 = cen + rv * 0.5f;
                float3 addPos0 = fpos0 - nextPos0;
                float3 addPos1 = fpos1 - nextPos1;

                // ??
                float friction0 = frictionList[pindex0];
                float friction1 = frictionList[pindex1];
                float moveratio0 = math.saturate(1.0f - friction0 * Define.Compute.FrictionMoveRatio);
                float moveratio1 = math.saturate(1.0f - friction1 * Define.Compute.FrictionMoveRatio);
                //stiffness0 *= moveratio0;
                //stiffness1 *= moveratio1;
                addPos0 *= moveratio0;
                addPos1 *= moveratio1;


                // ????
                nextPos0 += addPos0;
                nextPos1 += addPos1;
                nextPosList[pindex0] = nextPos0;
                nextPosList[pindex1] = nextPos1;

                // ????
                const float influence = 0.5f; // ?????(0.1)
                posList[pindex0] = posList[pindex0] + addPos0 * (1.0f - influence);
                posList[pindex1] = posList[pindex1] + addPos1 * (1.0f - influence);
            }
        }
#endif
    }
}
