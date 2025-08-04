// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
// ?????????????
//#define normalizeStretch
//#define normalizeShear // ????????

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
    /// <summary>
    /// ???????
    /// </summary>
    public class VolumeConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ?????
        /// todo:?????
        /// </summary>
        [System.Serializable]
        public struct VolumeData
        {
            /// <summary>
            /// ???????????????????x4
            /// </summary>
            public int vindex0;
            public int vindex1;
            public int vindex2;
            public int vindex3;

            /// <summary>
            /// ???????
            /// </summary>
            public float3x3 ivMat;

            /// <summary>
            /// ??????????????(0.0-1.0)
            /// </summary>
            public float depth;

            /// <summary>
            /// ?????(0-1-2)????????????(3)???
            /// (0=????,1=???,-1=???)
            /// </summary>
            public int direction;

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
        }
        FixedChunkNativeArray<VolumeData> dataList;

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
        public struct GroupData
        {
            public int teamId;

            public int active;

            /// <summary>
            /// ???(0.0-1.0)
            /// </summary>
            public CurveParam stretchStiffness;

            /// <summary>
            /// ????(0.0-1.0)
            /// </summary>
            public CurveParam shearStiffness;

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
        FixedNativeList<GroupData> groupList;

        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<VolumeData>();
            groupIndexList = new FixedChunkNativeArray<short>();
            refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
            writeBuffer = new FixedChunkNativeArray<float3>();
            groupList = new FixedNativeList<GroupData>();
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
        public int AddGroup(int teamId, bool active, BezierParam stretchStiffness, BezierParam shearStiffness, VolumeData[] dataArray, ReferenceDataIndex[] refDataArray, int writeBufferCount)
        {
            if (dataArray == null || dataArray.Length == 0 || refDataArray == null || refDataArray.Length == 0 || writeBufferCount == 0)
                return -1;

            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            // ?????????
            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.stretchStiffness.Setup(stretchStiffness);
            gdata.shearStiffness.Setup(shearStiffness);
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
            int group = teamData.volumeGroupIndex;
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

        public void ChangeParam(int teamId, bool active, BezierParam stretchStiffness, BezierParam shearStiffness)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.volumeGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            gdata.stretchStiffness.Setup(stretchStiffness);
            gdata.shearStiffness.Setup(shearStiffness);
            groupList[group] = gdata;
        }

        //public int ActiveCount
        //{
        //    get
        //    {
        //        int cnt = 0;
        //        for (int i = 0; i < groupList.Length; i++)
        //            if (groupList[i].active == 1)
        //                cnt++;
        //        return cnt;
        //    }
        //}

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <returns></returns>
        public override int GetIterationCount()
        {
            return base.GetIterationCount();
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

            // ????1:??????
            var job = new VolumeCalcJob()
            {
                runCount = runCount,

                updatePower = updatePower,
                groupDataList = groupList.ToJobArray(),
                dataList = dataList.ToJobArray(),
                groupIndexList = groupIndexList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                //flagList = Manager.Particle.flagList.ToJobArray(),
                nextPosList = Manager.Particle.InNextPosList.ToJobArray(),

                writeBuffer = writeBuffer.ToJobArray(),
            };
            jobHandle = job.Schedule(dataList.Length, 64, jobHandle);

            // ????2:????????
            var job2 = new VolumeSumJob()
            {
                runCount = runCount,

                groupDataList = groupList.ToJobArray(),
                refDataList = refDataList.ToJobArray(),
                writeBuffer = writeBuffer.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                teamIdList = Manager.Particle.teamIdList.ToJobArray(),
                flagList = Manager.Particle.flagList.ToJobArray(),

                inoutNextPosList = Manager.Particle.InNextPosList.ToJobArray(),
            };
            jobHandle = job2.Schedule(Manager.Particle.Length, 64, jobHandle);

            return jobHandle;
        }

        [BurstCompile]
        struct VolumeCalcJob : IJobParallelFor
        {
            public float updatePower;
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<VolumeData> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<short> groupIndexList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

            //[Unity.Collections.ReadOnly]
            //public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> nextPosList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> writeBuffer;

            // ????????
            public void Execute(int index)
            {
                var data = dataList[index];
                if (data.IsValid() == false)
                    return;

                int gindex = groupIndexList[index];
                var gdata = groupDataList[gindex];
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
                int pindex3 = data.vindex3 + pstart;

                float3 nextpos0 = nextPosList[pindex0];
                float3 nextpos1 = nextPosList[pindex1];
                float3 nextpos2 = nextPosList[pindex2];
                float3 nextpos3 = nextPosList[pindex3];

                // ???
                float stretchStiffness = (1.0f - math.pow(1.0f - gdata.stretchStiffness.Evaluate(data.depth), updatePower));
                float shearStiffness = (1.0f - math.pow(1.0f - gdata.shearStiffness.Evaluate(data.depth), updatePower));

                // ?????
                float3 add3 = 0;
#if false
                if (data.direction != 0)
                {
                    var v1 = nextpos1 - nextpos0;
                    var v2 = nextpos2 - nextpos0;
                    //var v3 = nextpos3 - nextpos0;
                    var n = math.normalize(math.cross(v1, v2));
                    n *= data.direction; // ??
                    var n0 = nextpos0 + n * 0.005f; // ??
                    //var n0 = nextpos0 + n * 0.001f; // ??
                    var v3 = nextpos3 - n0;
                    float d = math.dot(n, v3);
                    if (d < 0.0f)
                    {
                        //float3 add = n * -d;
                        float3 add = n * (-d * stretchStiffness);
                        //add3 = add;
                        //nextpos3 += add3;

                        //corr3 += add;
                        add *= 0.5f;
                        corr0 -= add;
                        corr1 -= add;
                        corr2 -= add;
                        corr3 += add;
                    }
                }
#endif

                // ???????
                float3x3 ivMat = data.ivMat;
                //float3[] c = new float3[3];
                float3x3 c = 0;
                c[0] = ivMat.c0;
                c[1] = ivMat.c1;
                c[2] = ivMat.c2;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        float3x3 P;

                        P.c0 = (nextpos1 + corr1) - (nextpos0 + corr0);     // Gauss - Seidel
                        P.c1 = (nextpos2 + corr2) - (nextpos0 + corr0);
                        P.c2 = (nextpos3 + corr3) - (nextpos0 + corr0);

                        float3 fi = math.mul(P, c[i]);
                        float3 fj = math.mul(P, c[j]);
                        //float3 fi = math.mul(P, ivMat[i]);
                        //float3 fj = math.mul(P, ivMat[j]);

                        float Sij = math.dot(fi, fj);

#if normalizeShear
                        float wi = 0, wj = 0, s1 = 0, s3 = 0;
                        //if (normalizeShear && i != j)
                        if (i != j)
                        {
                            wi = math.length(fi);
                            wj = math.length(fj);
                            s1 = 1.0f / (wi * wj);
                            s3 = s1 * s1 * s1;
                        }
#endif

                        //float3[] d = new float3[4];
                        //d[0] = 0;

                        //for (int k = 0; k < 3; k++)
                        //{
                        //    d[k + 1] = fj * ivMat[i][k] + fi * ivMat[j][k];

                        //    //if (normalizeShear && i != j)
                        //    if (i != j)
                        //    {
                        //        d[k + 1] = s1 * d[k + 1] - Sij * s3 * (wj * wj * fi * ivMat[i][k] + wi * wi * fj * ivMat[j][k]);
                        //    }

                        //    d[0] -= d[k + 1];
                        //}

                        float3x4 d = 0;
                        d[0] = 0;

                        for (int k = 0; k < 3; k++)
                        {
                            d[k + 1] = fj * ivMat[i][k] + fi * ivMat[j][k];

#if normalizeShear
                            //if (normalizeShear && i != j)
                            if (i != j)
                            {
                                d[k + 1] = s1 * d[k + 1] - Sij * s3 * (wj * wj * fi * ivMat[i][k] + wi * wi * fj * ivMat[j][k]);
                            }
#endif

                            d[0] -= d[k + 1];
                        }

#if normalizeShear
                        //if (normalizeShear && i != j)
                        if (i != j)
                            Sij *= s1;
#endif

                        //float lambda =
                        //    invMass0 * math.lengthsq(d[0]) +
                        //    invMass1 * math.lengthsq(d[1]) +
                        //    invMass2 * math.lengthsq(d[2]) +
                        //    invMass3 * math.lengthsq(d[3]);
                        float lambda =
                            math.lengthsq(d[0]) +
                            math.lengthsq(d[1]) +
                            math.lengthsq(d[2]) +
                            math.lengthsq(d[3]);

                        if (math.abs(lambda) < 1e-6f)     // foo: threshold should be scale dependent
                            continue;

                        if (i == j)
                        {   // diagonal, stretch
#if normalizeStretch
                            float s = math.sqrt(Sij);
                            //lambda = 2.0f * s * (s - 1.0f) / lambda * stretchStiffness[i];
                            lambda = 2.0f * s * (s - 1.0f) / lambda * stretchStiffness;
#else
                            //lambda = (Sij - 1.0f) / lambda * stretchStiffness[i];
                            lambda = (Sij - 1.0f) / lambda * stretchStiffness;
#endif
                        }
                        else
                        {       // off diagonal, shear
                            //lambda = Sij / lambda * shearStiffness[i + j - 1];
                            lambda = Sij / lambda * shearStiffness;
                        }

                        //corr0 -= lambda * invMass0 * d[0];
                        //corr1 -= lambda * invMass1 * d[1];
                        //corr2 -= lambda * invMass2 * d[2];
                        //corr3 -= lambda * invMass3 * d[3];
                        corr0 -= lambda * d[0];
                        corr1 -= lambda * d[1];
                        corr2 -= lambda * d[2];
                        corr3 -= lambda * d[3];
                    }
                }

                // ?????????
                int wstart = gdata.writeDataChunk.startIndex;
                int windex0 = data.writeIndex0 + wstart;
                int windex1 = data.writeIndex1 + wstart;
                int windex2 = data.writeIndex2 + wstart;
                int windex3 = data.writeIndex3 + wstart;
                writeBuffer[windex0] = corr0;
                writeBuffer[windex1] = corr1;
                writeBuffer[windex2] = corr2;
                writeBuffer[windex3] = corr3 + add3;
            }
        }

        [BurstCompile]
        struct VolumeSumJob : IJobParallelFor
        {
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupDataList;
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
                if (team.volumeGroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // ???????
                var gdata = groupDataList[team.volumeGroupIndex];
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
                }
            }
        }
    }
}
