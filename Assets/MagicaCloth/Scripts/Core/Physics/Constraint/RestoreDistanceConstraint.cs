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
    /// </summary>
    public class RestoreDistanceConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ?????
        /// </summary>
        public const int StructType = 0;
        public const int BendType = 1;
        public const int NearType = 2;
        public const int TypeCount = 3; // ????

        /// <summary>
        /// ?????????
        /// todo:?????
        /// </summary>
        [System.Serializable]
        public struct RestoreDistanceData
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
            /// ????????(v1.7.0)
            /// </summary>
            public float length;

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return vertexIndex > 0 || targetVertexIndex > 0;
            }
        }

        FixedChunkNativeArray<RestoreDistanceData>[] dataList;

        /// <summary>
        /// ?????????????????????
        /// </summary>
        FixedChunkNativeArray<ReferenceDataIndex>[] refDataList;


        /// <summary>
        /// ????????????
        /// </summary>
        public struct RestoreDistanceGroupData
        {
            public int teamId;

            /// <summary>
            /// ??
            /// </summary>
            public CurveParam mass;

            /// <summary>
            /// ????
            /// </summary>
            public float velocityInfluence;

            /// <summary>
            /// ????
            /// </summary>
            public CurveParam structStiffness;
            public ChunkData structDataChunk;
            public ChunkData structRefChunk;

            /// <summary>
            /// ?????
            /// </summary>
            public int useBend;
            public CurveParam bendStiffness;
            public ChunkData bendDataChunk;
            public ChunkData bendRefChunk;

            /// <summary>
            /// ???
            /// </summary>
            public int useNear;
            public CurveParam nearStiffness;
            public ChunkData nearDataChunk;
            public ChunkData nearRefChunk;

            public bool IsValid(int type)
            {
                if (type == StructType)
                    return true; // ??ON
                else if (type == BendType)
                    return useBend == 1;
                else
                    return useNear == 1;
            }

            public CurveParam GetStiffness(int type)
            {
                if (type == StructType)
                    return structStiffness;
                else if (type == BendType)
                    return bendStiffness;
                else
                    return nearStiffness;
            }

            public ChunkData GetDataChunk(int type)
            {
                //return structDataChunk;
                if (type == StructType)
                    return structDataChunk;
                else if (type == BendType)
                    return bendDataChunk;
                else
                    return nearDataChunk;
            }

            public ChunkData GetRefChunk(int type)
            {
                //return structRefChunk;
                if (type == StructType)
                    return structRefChunk;
                else if (type == BendType)
                    return bendRefChunk;
                else
                    return nearRefChunk;
            }
        }
        public FixedNativeList<RestoreDistanceGroupData> groupList;


        //=========================================================================================
        public override void Create()
        {
            groupList = new FixedNativeList<RestoreDistanceGroupData>();
            dataList = new FixedChunkNativeArray<RestoreDistanceData>[TypeCount];
            refDataList = new FixedChunkNativeArray<ReferenceDataIndex>[TypeCount];
            for (int i = 0; i < TypeCount; i++)
            {
                dataList[i] = new FixedChunkNativeArray<RestoreDistanceData>();
                refDataList[i] = new FixedChunkNativeArray<ReferenceDataIndex>();
            }
        }

        public override void Release()
        {
            groupList.Dispose();
            for (int i = 0; i < TypeCount; i++)
            {
                dataList[i].Dispose();
                refDataList[i].Dispose();
            }
            dataList = null;
            refDataList = null;
        }

        //=========================================================================================
        public int AddGroup(
            int teamId,
            BezierParam mass,
            float velocityInfluence,
            BezierParam structStiffness,
            RestoreDistanceData[] structDataArray,
            ReferenceDataIndex[] structRefDataArray,
            bool useBend,
            BezierParam bendStiffness,
            RestoreDistanceData[] bendDataArray,
            ReferenceDataIndex[] bendRefDataArray,
            bool useNear,
            BezierParam nearStiffness,
            RestoreDistanceData[] nearDataArray,
            ReferenceDataIndex[] nearRefDataArray
            )
        {
            if (structDataArray == null || structDataArray.Length == 0 || structRefDataArray == null || structRefDataArray.Length == 0)
                return -1;

            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            var gdata = new RestoreDistanceGroupData();
            gdata.teamId = teamId;
            gdata.mass.Setup(mass);
            gdata.velocityInfluence = velocityInfluence;
            gdata.useBend = useBend ? 1 : 0;
            gdata.useNear = useNear ? 1 : 0;

            gdata.structStiffness.Setup(structStiffness);
            gdata.structDataChunk = dataList[StructType].AddChunk(structDataArray.Length);
            gdata.structRefChunk = refDataList[StructType].AddChunk(structRefDataArray.Length);
            // ??????????
            dataList[StructType].ToJobArray().CopyFromFast(gdata.structDataChunk.startIndex, structDataArray);
            refDataList[StructType].ToJobArray().CopyFromFast(gdata.structRefChunk.startIndex, structRefDataArray);

            if (bendDataArray != null && bendDataArray.Length > 0)
            {
                gdata.bendStiffness.Setup(bendStiffness);
                gdata.bendDataChunk = dataList[BendType].AddChunk(bendDataArray.Length);
                gdata.bendRefChunk = refDataList[BendType].AddChunk(bendRefDataArray.Length);
                // ??????????
                dataList[BendType].ToJobArray().CopyFromFast(gdata.bendDataChunk.startIndex, bendDataArray);
                refDataList[BendType].ToJobArray().CopyFromFast(gdata.bendRefChunk.startIndex, bendRefDataArray);
            }

            if (nearDataArray != null && nearDataArray.Length > 0)
            {
                gdata.nearStiffness.Setup(nearStiffness);
                gdata.nearDataChunk = dataList[NearType].AddChunk(nearDataArray.Length);
                gdata.nearRefChunk = refDataList[NearType].AddChunk(nearRefDataArray.Length);
                // ??????????
                dataList[NearType].ToJobArray().CopyFromFast(gdata.nearDataChunk.startIndex, nearDataArray);
                refDataList[NearType].ToJobArray().CopyFromFast(gdata.nearRefChunk.startIndex, nearRefDataArray);
            }

            return groupList.Add(gdata);
        }

        /// <summary>
        /// ??(???)
        /// </summary>
        /// <param name="group"></param>
        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.restoreDistanceGroupIndex;
            if (group < 0)
                return;

            var cdata = groupList[group];

            // ?????????
            for (int i = 0; i < TypeCount; i++)
            {
                var dc = cdata.GetDataChunk(i);
                var rc = cdata.GetRefChunk(i);
                if (dc.dataLength > 0)
                {
                    dataList[i].RemoveChunk(dc);
                }
                if (rc.dataLength > 0)
                {
                    refDataList[i].RemoveChunk(rc);
                }
            }

            // ?????
            groupList.Remove(group);
        }

        public void ChangeParam(
            int teamId,
            BezierParam mass,
            float velocityInfluence,
            BezierParam structStiffness,
            bool useBend,
            BezierParam bendStiffness,
            bool useNear,
            BezierParam nearStiffness
            )
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.restoreDistanceGroupIndex;
            if (group < 0)
                return;

            var cdata = groupList[group];
            cdata.mass.Setup(mass);
            cdata.velocityInfluence = velocityInfluence;
            cdata.structStiffness.Setup(structStiffness);
            cdata.bendStiffness.Setup(bendStiffness);
            cdata.nearStiffness.Setup(nearStiffness);
            cdata.useBend = useBend ? 1 : 0;
            cdata.useNear = useNear ? 1 : 0;
            groupList[group] = cdata;
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <returns></returns>
        public override int GetIterationCount()
        {
            return base.GetIterationCount();
            //return 2;
        }

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
            //for (int type = 0; type < TypeCount; type++)
            for (int type = TypeCount - 1; type >= 0; type--) // ???????????????
            {
                if (dataList[type].Count == 0)
                    continue;

                var job1 = new DistanceJob()
                {
                    updatePower = updatePower,
                    runCount = runCount,
                    type = type,

                    restoreDistanceDataList = dataList[type].ToJobArray(),
                    restoreDistanceGroupDataList = groupList.ToJobArray(),
                    refDataList = refDataList[type].ToJobArray(),

                    teamDataList = Manager.Team.teamDataList.ToJobArray(),
                    teamIdList = Manager.Particle.teamIdList.ToJobArray(),

                    flagList = Manager.Particle.flagList.ToJobArray(),
                    depthList = Manager.Particle.depthList.ToJobArray(),
                    frictionList = Manager.Particle.frictionList.ToJobArray(),
                    basePosList = Manager.Particle.basePosList.ToJobArray(),
                    nextPosList = Manager.Particle.InNextPosList.ToJobArray(),

                    outNextPosList = Manager.Particle.OutNextPosList.ToJobArray(),

                    posList = Manager.Particle.posList.ToJobArray(),
                };
                jobHandle = job1.Schedule(Manager.Particle.Length, 64, jobHandle);
                Manager.Particle.SwitchingNextPosList();
            }

            return jobHandle;
        }

        /// <summary>
        /// ???????
        /// ???????????
        /// </summary>
        [BurstCompile]
        struct DistanceJob : IJobParallelFor
        {
            public float updatePower;
            public int runCount;
            public int type;

            [Unity.Collections.ReadOnly]
            public NativeArray<RestoreDistanceData> restoreDistanceDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<RestoreDistanceGroupData> restoreDistanceGroupDataList;
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
            public NativeArray<float> frictionList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> basePosList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<quaternion> baseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> nextPosList;

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

                var team = teamDataList[teamIdList[index]];
                if (team.restoreDistanceGroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                int pstart = team.particleChunk.startIndex;
                int vindex = index - pstart;

                // ???????????
                var gdata = restoreDistanceGroupDataList[team.restoreDistanceGroupIndex];
                if (gdata.IsValid(type) == false)
                    return;

                // ????
                var dataChunk = gdata.GetDataChunk(type);
                var refChunk = gdata.GetRefChunk(type);
                var stiffnessData = gdata.GetStiffness(type);

                // ???????????????
                bool useAnimatedPose = team.IsFlag(PhysicsManagerTeamData.Flag_AnimatedPose);

                // ????1??????????
                // ???????!
                // 10??????????????30?????????????????????????
                const float FrictionMass = 5.0f; // 20(v1.6.1)
                float friction = frictionList[index];

                var refdata = refDataList[refChunk.startIndex + vindex];
                if (refdata.count > 0)
                {
                    float depth = depthList[index];
                    float stiffness = stiffnessData.Evaluate(depth);
                    stiffness = math.saturate(stiffness * updatePower);
                    float3 addpos = 0;
                    float mass = gdata.mass.Evaluate(depth);
                    // ????????????????
                    mass += friction * FrictionMass;
                    float3 bpos = basePosList[index];

                    int dataIndex = dataChunk.startIndex + refdata.startIndex;
                    for (int i = 0; i < refdata.count; i++, dataIndex++)
                    {
                        var data = restoreDistanceDataList[dataIndex];

                        if (data.IsValid() == false)
                            continue;

                        // ??????????????????
                        int tindex = pstart + data.targetVertexIndex;
                        float3 tnextpos = nextPosList[tindex];

                        // ?????
                        float3 v = tnextpos - nextpos;
                        float vlen = math.length(v);
                        if (vlen < 0.00001f)
                            continue;


                        // ????
                        float rlen = data.length; // v1.7.0
                        // ?????????
                        rlen *= team.scaleRatio;

                        if (useAnimatedPose)
                        {
                            // ?????????????????
                            //rlen = math.distance(bpos, basePosList[tindex]); // ??????????
                            //rlen = math.max(math.distance(bpos, basePosList[tindex]), rlen); // ????????
                            rlen = (math.distance(bpos, basePosList[tindex]) + rlen) * 0.5f; // ??
                        }

                        float clen = vlen - rlen;

                        // ???
                        float tdepth = depthList[tindex];
                        float tmass = gdata.mass.Evaluate(tdepth);
                        float tfriction = frictionList[tindex];
                        // ????????????????
                        tmass += tfriction * FrictionMass;

                        float m1 = tmass / (tmass + mass);

                        // ??
                        m1 *= stiffness;

                        // ??????
                        float3 add1 = v * (m1 * clen / vlen);

                        // ????
                        addpos += add1;
                    }

                    // ????
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
