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
    /// ?????????????
    /// </summary>
    public class TriangleBendConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ?????
        /// todo:?????
        /// </summary>
        [System.Serializable]
        public struct TriangleBendData
        {
            /// <summary>
            /// ?????????????????????x4
            /// 2??????p2-p3??????p0/p1??????
            ///   p2 +
            ///     /|\
            /// p0 + | + p1
            ///     \|/
            ///   p3 +
            ///   
            /// A(p0-p2-p3) B(p1-p3-p2)
            /// 
            /// ???vindex3?-1?????????????????????????????
            ///   v0 +
            ///     / \
            /// v1 +---+ v2
            ///     
            ///   v3 = -1 
            public int vindex0;
            public int vindex1;
            public int vindex2;
            public int vindex3;

            /// <summary>
            /// ????(????)
            /// </summary>
            public float restAngle;

            /// <summary>
            /// ????? [Algorithm 2]
            /// </summary>
            public float direction;

            /// <summary>
            /// ??????????????(0.0-1.0)
            /// </summary>
            public float depth;

            /// <summary>
            /// ??????????????
            /// </summary>
            public int writeIndex0;
            public int writeIndex1;
            public int writeIndex2;
            public int writeIndex3;

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return vindex0 > 0 && vindex1 > 0;
            }

            /// <summary>
            /// ??????????????
            /// </summary>
            /// <returns></returns>
            public bool IsPositionBend()
            {
                return vindex3 < 0;
            }

            /// <summary>
            /// ????????????
            /// </summary>
            /// <returns></returns>
            //public bool IsStrict()
            //{
            //    return math.abs(direction) > 1e-06f;
            //}
        }
        FixedChunkNativeArray<TriangleBendData> dataList;

        /// <summary>
        /// ????????????????
        /// </summary>
        FixedChunkNativeArray<short> groupIndexList;

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        FixedChunkNativeArray<ReferenceDataIndex> refDataList;

        /// <summary>
        /// ??????????????
        /// </summary>
        FixedChunkNativeArray<float3> writeBuffer;

        /// <summary>
        /// ????????????
        /// </summary>
        public struct TriangleBendGroupData
        {
            public int teamId;

            public int active;

            /// <summary>
            /// ??????(ClothParams.Algorithm???)
            /// </summary>
            public int algorithm;

            /// <summary>
            /// ???????????????????
            /// </summary>
            //public int useIncludeFixed;

            /// <summary>
            /// ????????(0.0-1.0)
            /// </summary>
            public CurveParam stiffness;

            /// <summary>
            /// ???????
            /// </summary>
            public ChunkData dataChunk;

            /// <summary>
            /// ???????????
            /// </summary>
            public ChunkData groupIndexChunk;

            /// <summary>
            /// ?????????????
            /// </summary>
            public ChunkData refDataChunk;

            /// <summary>
            /// ???????????????
            /// </summary>
            public ChunkData writeDataChunk;
        }
        FixedNativeList<TriangleBendGroupData> groupList;

        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<TriangleBendData>();
            groupIndexList = new FixedChunkNativeArray<short>();
            refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
            writeBuffer = new FixedChunkNativeArray<float3>();
            groupList = new FixedNativeList<TriangleBendGroupData>();
        }

        public override void Release()
        {
            dataList.Dispose();
            groupIndexList.Dispose();
            refDataList.Dispose();
            writeBuffer.Dispose();
            groupList.Dispose();
        }

        //=========================================================================================
        public int AddGroup(
            int teamId, bool active,
            ClothParams.Algorithm algorithm,
            BezierParam stiffness,
            //bool useIncludeFixed,
            TriangleBendData[] dataArray, ReferenceDataIndex[] refDataArray, int writeBufferCount)
        {
            if (dataArray == null || dataArray.Length == 0 || refDataArray == null || refDataArray.Length == 0 || writeBufferCount == 0)
                return -1;

            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            // ?????????
            var gdata = new TriangleBendGroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.algorithm = (int)algorithm;
            //gdata.useIncludeFixed = useIncludeFixed ? 1 : 0;
            gdata.stiffness.Setup(stiffness);
            gdata.dataChunk = dataList.AddChunk(dataArray.Length);
            gdata.groupIndexChunk = groupIndexList.AddChunk(dataArray.Length);
            gdata.refDataChunk = refDataList.AddChunk(refDataArray.Length);
            gdata.writeDataChunk = writeBuffer.AddChunk(writeBufferCount);

            // ??????????
            dataList.ToJobArray().CopyFromFast(gdata.dataChunk.startIndex, dataArray);
            refDataList.ToJobArray().CopyFromFast(gdata.refDataChunk.startIndex, refDataArray);

            int group = groupList.Add(gdata);

            // ????????????????
            groupIndexList.Fill(gdata.groupIndexChunk, (short)group);


            return group;
        }

        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.triangleBendGroupIndex;
            if (group < 0)
                return;

            var cdata = groupList[group];

            // ?????????
            dataList.RemoveChunk(cdata.dataChunk);
            refDataList.RemoveChunk(cdata.refDataChunk);
            writeBuffer.RemoveChunk(cdata.writeDataChunk);
            groupIndexList.RemoveChunk(cdata.groupIndexChunk);

            // ?????
            groupList.Remove(group);
        }

        public void ChangeParam(int teamId, bool active, BezierParam stiffness/*, bool useIncludeFixed*/)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.triangleBendGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            //gdata.useIncludeFixed = useIncludeFixed ? 1 : 0;
            gdata.stiffness.Setup(stiffness);
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

            // ????1:??????
            var job = new TriangleBendCalcJob()
            {
                updatePower = updatePower,
                runCount = runCount,

                triangleBendGroupDataList = groupList.ToJobArray(),
                triangleBendList = dataList.ToJobArray(),
                groupIndexList = groupIndexList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                basePosList = Manager.Particle.basePosList.ToJobArray(),

                writeBuffer = writeBuffer.ToJobArray(),
            };
            jobHandle = job.Schedule(dataList.Length, 64, jobHandle);

            // ????2:????????
            var job2 = new TriangleBendSumJob()
            {
                runCount = runCount,

                triangleBendGroupDataList = groupList.ToJobArray(),
                refDataList = refDataList.ToJobArray(),
                writeBuffer = writeBuffer.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                teamIdList = Manager.Particle.teamIdList.ToJobArray(),
                flagList = Manager.Particle.flagList.ToJobArray(),

                inoutNextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                posList = Manager.Particle.posList.ToJobArray(),
            };
            jobHandle = job2.Schedule(Manager.Particle.Length, 64, jobHandle);

            return jobHandle;
        }

        [BurstCompile]
        struct TriangleBendCalcJob : IJobParallelFor
        {
            public float updatePower;
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<TriangleBendGroupData> triangleBendGroupDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<TriangleBendData> triangleBendList;
            [Unity.Collections.ReadOnly]
            public NativeArray<short> groupIndexList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> nextPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> basePosList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> writeBuffer;

            // ????????
            public void Execute(int index)
            {
                var data = triangleBendList[index];
                if (data.IsValid() == false)
                    return;

                int gindex = groupIndexList[index];
                var gdata = triangleBendGroupDataList[gindex];
                if (gdata.teamId == 0 || gdata.active == 0)
                    return;

                var tdata = teamDataList[gdata.teamId];
                if (tdata.IsActive() == false)
                    return;
                // ????
                if (tdata.IsUpdate(runCount) == false)
                    return;

                int pstart = tdata.particleChunk.startIndex;

                float3 corr0 = 0;
                float3 corr1 = 0;
                float3 corr2 = 0;
                float3 corr3 = 0;

                int pindex0 = data.vindex0 + pstart;
                int pindex1 = data.vindex1 + pstart;
                int pindex2 = data.vindex2 + pstart;
                int pindex3 = data.IsPositionBend() ? -1 : (data.vindex3 + pstart);

                float3 nextpos0 = nextPosList[pindex0];
                float3 nextpos1 = nextPosList[pindex1];
                float3 nextpos2 = nextPosList[pindex2];
                float3 nextpos3 = data.IsPositionBend() ? 0 : nextPosList[pindex3];

                // ???
                //bool isStrict = data.IsStrict();
                bool isStrict = gdata.algorithm == 1;

                // ???
                float stiffness = gdata.stiffness.Evaluate(data.depth);
                stiffness = (1.0f - math.pow(1.0f - stiffness, updatePower));

                // ??????
                if (data.IsPositionBend() == false)
                {
                    // ??????????
                    float3 e = nextpos3 - nextpos2;
                    float elen = math.length(e);
                    if (elen > 1e-06f)
                    {
                        float invElen = 1.0f / elen;

                        float3 n1 = math.cross(nextpos2 - nextpos0, nextpos3 - nextpos0);
                        float n1_lsq = math.lengthsq(n1);
                        float3 n2 = math.cross(nextpos3 - nextpos1, nextpos2 - nextpos1);
                        float n2_lsq = math.lengthsq(n2);

                        if (n1_lsq > 0 && n2_lsq > 0) // v1.12.0
                        {
                            n1 /= n1_lsq;
                            n2 /= n2_lsq;

                            float3 d0 = elen * n1;
                            float3 d1 = elen * n2;
                            float3 d2 = math.dot(nextpos0 - nextpos3, e) * invElen * n1 + math.dot(nextpos1 - nextpos3, e) * invElen * n2;
                            float3 d3 = math.dot(nextpos2 - nextpos0, e) * invElen * n1 + math.dot(nextpos2 - nextpos1, e) * invElen * n2;

                            n1 = math.normalize(n1);
                            n2 = math.normalize(n2);
                            float dot = math.dot(n1, n2);
                            dot = math.clamp(dot, -1.0f, 1.0f);
                            float phi = math.acos(dot);

                            float lambda =
                                math.lengthsq(d0) +
                                math.lengthsq(d1) +
                                math.lengthsq(d2) +
                                math.lengthsq(d3);

                            // ???
                            float direction = math.dot(math.cross(n1, n2), e);

                            // Strict??????????????
                            phi = math.select(phi, phi * math.sign(direction), isStrict);

                            // ???
                            float rest = math.abs(phi - data.restAngle);

                            //if (stiffness > 0.5f && rest > 1.5f)
                            //    stiffness = 0.5f;

                            // ????????
                            if (isStrict)
                            {
                                // ??????????stiffness?????
                                float ratio = math.max(math.pow(math.saturate(rest / math.PI), 0.5f), 0.1f);
                                stiffness *= ratio;
                            }

                            lambda = (phi - data.restAngle) / lambda * stiffness;

                            // ????
                            lambda = math.select(lambda * math.sign(direction), lambda, isStrict);

                            corr0 = lambda * d0;
                            corr1 = lambda * d1;
                            corr2 = lambda * d2;
                            corr3 = lambda * d3;
                        }
                    }
                }
                // v1.11.0????????
                //else
                //{
                //    // ???????????
                //    if (gdata.useIncludeFixed == 1)
                //    {
                //        // ??????????????
                //        stiffness *= 0.2f; // ??
                //        float3 basepos0 = basePosList[pindex0];
                //        float3 basepos1 = basePosList[pindex1];
                //        float3 basepos2 = basePosList[pindex2];
                //        corr0 = (basepos0 - nextpos0) * stiffness;
                //        corr1 = (basepos1 - nextpos1) * stiffness;
                //        corr2 = (basepos2 - nextpos2) * stiffness;

                //        // todo:Interlock???????????????????
                //    }
                //}

                // ?????????
                int wstart = gdata.writeDataChunk.startIndex;
                int windex0 = data.writeIndex0 + wstart;
                int windex1 = data.writeIndex1 + wstart;
                int windex2 = data.writeIndex2 + wstart;
                writeBuffer[windex0] = corr0;
                writeBuffer[windex1] = corr1;
                writeBuffer[windex2] = corr2;
                if (data.IsPositionBend() == false)
                {
                    int windex3 = data.writeIndex3 + wstart;
                    writeBuffer[windex3] = corr3;
                }
            }
        }

        [BurstCompile]
        struct TriangleBendSumJob : IJobParallelFor
        {
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<TriangleBendGroupData> triangleBendGroupDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<ReferenceDataIndex> refDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> writeBuffer;

            // ???
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

            public NativeArray<float3> inoutNextPosList;
            public NativeArray<float3> posList;

            // ????????
            public void Execute(int pindex)
            {
                var flag = flagList[pindex];
                if (flag.IsValid() == false || flag.IsFixed())
                    return;

                // ???
                var team = teamDataList[teamIdList[pindex]];
                if (team.IsActive() == false)
                    return;
                if (team.triangleBendGroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // ???????
                var gdata = triangleBendGroupDataList[team.triangleBendGroupIndex];
                if (gdata.active == 0)
                    return;

                // ??
                int start = team.particleChunk.startIndex;
                int index = pindex - start;

                var refdata = refDataList[gdata.refDataChunk.startIndex + index];
                if (refdata.count > 0)
                {
                    float3 corr = 0;
                    var bindex = gdata.writeDataChunk.startIndex + refdata.startIndex;
                    for (int i = 0; i < refdata.count; i++)
                    {
                        corr += writeBuffer[bindex];
                        bindex++;
                    }
                    corr /= refdata.count;

                    // ??
                    inoutNextPosList[pindex] = inoutNextPosList[pindex] + corr;

                    // ????(Strict?????)
                    var av = corr * (1.0f - Define.Compute.TriangleBendVelocityInfluence); // ??????(0.5)
                    posList[pindex] = posList[pindex] + av;
                }
            }
        }
    }
}
