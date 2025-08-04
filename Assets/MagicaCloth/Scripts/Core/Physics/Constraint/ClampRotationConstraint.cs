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
    /// ????????[Algorithm 1]
    /// </summary>
    public class ClampRotationConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ?????
        /// </summary>
        [System.Serializable]
        public struct ClampRotationData
        {
            /// <summary>
            /// ??????????
            /// </summary>
            public int vertexIndex;

            /// <summary>
            /// ?????????
            /// </summary>
            public int parentVertexIndex;

            /// <summary>
            /// ????????????????(??????)(v1.7.0)
            /// </summary>
            public float3 localPos;

            /// <summary>
            /// ????????????????(v1.7.0)
            /// </summary>
            public quaternion localRot;

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return vertexIndex > 0 || parentVertexIndex > 0;
            }
        }

        //=========================================================================================
        // Algorithm 1
        //=========================================================================================
        FixedChunkNativeArray<ClampRotationData> dataList;

        [System.Serializable]
        public struct ClampRotationRootInfo
        {
            public ushort startIndex;
            public ushort dataLength;
        }
        FixedChunkNativeArray<ClampRotationRootInfo> rootInfoList;

        /// <summary>
        /// ????????????
        /// </summary>
        public struct GroupData
        {
            public int teamId;
            public int active;

            /// <summary>
            /// ????
            /// </summary>
            public CurveParam maxAngle;

            /// <summary>
            /// ????
            /// </summary>
            public float velocityInfluence;

            public ChunkData dataChunk;
            public ChunkData rootInfoChunk;
        }
        public FixedNativeList<GroupData> groupList;

        /// <summary>
        /// ???????????????
        /// </summary>
        FixedChunkNativeArray<int> rootTeamList;

        /// <summary>
        /// ??????????????
        /// </summary>
        FixedChunkNativeArray<float> lengthBuffer;

        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<ClampRotationData>();
            rootInfoList = new FixedChunkNativeArray<ClampRotationRootInfo>();
            groupList = new FixedNativeList<GroupData>();
            rootTeamList = new FixedChunkNativeArray<int>();
            lengthBuffer = new FixedChunkNativeArray<float>();
        }

        public override void Release()
        {
            dataList.Dispose();
            rootInfoList.Dispose();
            groupList.Dispose();
            rootTeamList.Dispose();
            lengthBuffer.Dispose();
        }

        //=========================================================================================
        public int AddGroup(
            int teamId,
            bool active,
            BezierParam maxAngle,
            float velocityInfluence,
            ClampRotationData[] dataArray,
            ClampRotationRootInfo[] rootInfoArray
            )
        {
            if (dataArray == null || dataArray.Length == 0 || rootInfoArray == null || rootInfoArray.Length == 0)
                return -1;

            //var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.maxAngle.Setup(maxAngle);
            gdata.velocityInfluence = velocityInfluence;
            gdata.dataChunk = dataList.AddChunk(dataArray.Length);
            gdata.rootInfoChunk = rootInfoList.AddChunk(rootInfoArray.Length);

            // ??????????
            dataList.ToJobArray().CopyFromFast(gdata.dataChunk.startIndex, dataArray);
            rootInfoList.ToJobArray().CopyFromFast(gdata.rootInfoChunk.startIndex, rootInfoArray);

            int group = groupList.Add(gdata);

            // ???????????????
            var c = rootTeamList.AddChunk(rootInfoArray.Length);
            rootTeamList.Fill(c, teamId);

            // ??????
            lengthBuffer.AddChunk(dataArray.Length);

            return group;
        }

        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            // Algorithm 1
            int group1 = teamData.clampRotationGroupIndex;
            if (group1 >= 0)
            {
                var cdata = groupList[group1];

                // ?????????
                dataList.RemoveChunk(cdata.dataChunk);
                rootInfoList.RemoveChunk(cdata.rootInfoChunk);
                rootTeamList.RemoveChunk(cdata.rootInfoChunk);
                lengthBuffer.RemoveChunk(cdata.dataChunk);

                // ?????
                groupList.Remove(group1);
            }
        }

        public void ChangeParam(int teamId, bool active, BezierParam maxAngle, float velocityInfluence)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.clampRotationGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            gdata.maxAngle.Setup(maxAngle);
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
            //=======================================================
            // Algorithm 1
            //=======================================================
            if (groupList.Count > 0)
            {
                // ????(????????)
                var job1 = new ClampRotationJob()
                {
                    runCount = runCount,
                    maxMoveLength = dtime * Define.Compute.ClampRotationMaxVelocity, // ??1.0m/s

                    dataList = dataList.ToJobArray(),
                    rootInfoList = rootInfoList.ToJobArray(),
                    rootTeamList = rootTeamList.ToJobArray(),
                    groupList = groupList.ToJobArray(),

                    teamDataList = Manager.Team.teamDataList.ToJobArray(),

                    flagList = Manager.Particle.flagList.ToJobArray(),
                    //basePosList = Manager.Particle.basePosList.ToJobArray(),
                    //baseRotList = Manager.Particle.baseRotList.ToJobArray(),
                    depthList = Manager.Particle.depthList.ToJobArray(),
                    frictionList = Manager.Particle.frictionList.ToJobArray(),

                    nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                    nextRotList = Manager.Particle.InNextRotList.ToJobArray(),

                    posList = Manager.Particle.posList.ToJobArray(),

                    lengthBuffer = lengthBuffer.ToJobArray(),
                };
                jobHandle = job1.Schedule(rootTeamList.Length, 8, jobHandle);
            }

            return jobHandle;
        }

        //=========================================================================================
        // Algorithm 1
        //=========================================================================================
        /// <summary>
        /// ???????????
        /// </summary>
        [BurstCompile]
        struct ClampRotationJob : IJobParallelFor
        {
            public int runCount;
            public float maxMoveLength;

            [Unity.Collections.ReadOnly]
            public NativeArray<ClampRotationData> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<ClampRotationRootInfo> rootInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> rootTeamList;
            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupList;

            // ???
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> depthList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<float3> basePosList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<quaternion> baseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> frictionList;

            [NativeDisableParallelForRestriction]
            public NativeArray<float3> nextPosList;
            [NativeDisableParallelForRestriction]
            public NativeArray<quaternion> nextRotList;

            [NativeDisableParallelForRestriction]
            public NativeArray<float3> posList;

            [NativeDisableParallelForRestriction]
            public NativeArray<float> lengthBuffer;

            // ????????
            public void Execute(int rootIndex)
            {
                // ???
                int teamIndex = rootTeamList[rootIndex];
                if (teamIndex == 0)
                    return;
                var team = teamDataList[teamIndex];
                if (team.IsActive() == false || team.clampRotationGroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // ???????
                var gdata = groupList[team.clampRotationGroupIndex];
                if (gdata.active == 0)
                    return;

                // ???
                var rootInfo = rootInfoList[rootIndex];
                int dataIndex = rootInfo.startIndex + gdata.dataChunk.startIndex;
                int dataCount = rootInfo.dataLength;
                int pstart = team.particleChunk.startIndex;

                // (1)?????????????????
                for (int i = 0; i < dataCount; i++)
                {
                    var data = dataList[dataIndex + i];
                    int pindex = data.parentVertexIndex;
                    if (pindex < 0)
                        continue;

                    var index = data.vertexIndex;
                    index += pstart;
                    pindex += pstart;

                    var npos = nextPosList[index];
                    var ppos = nextPosList[pindex];

                    // ???????
                    float vlen = math.distance(npos, ppos);

                    lengthBuffer[dataIndex + i] = vlen;
                }


                // (2)??????
                for (int i = 0; i < dataCount; i++)
                {
                    var data = dataList[dataIndex + i];
                    int pindex = data.parentVertexIndex;
                    if (pindex < 0)
                        continue;

                    var index = data.vertexIndex;

                    index += pstart;
                    pindex += pstart;

                    var flag = flagList[index];
                    if (flag.IsValid() == false)
                        continue;

                    var npos = nextPosList[index];
                    var nrot = nextRotList[index];
                    var opos = npos;

                    var ppos = nextPosList[pindex];
                    var prot = nextRotList[pindex];

                    float depth = depthList[index];
                    //float stiffness = gdata.stiffness.Evaluate(depth);


                    // ???????pos/rot?????
                    //var bpos = basePosList[index];
                    //var brot = baseRotList[index];
                    //var pbpos = basePosList[pindex];
                    //var pbrot = baseRotList[pindex];
                    //float3 bv = math.normalize(bpos - pbpos);
                    //var ipbrot = math.inverse(pbrot);
                    //float3 localPos = math.mul(ipbrot, bv);
                    //quaternion localRot = math.mul(ipbrot, brot);


                    // ?????????
                    //float3 tv = math.mul(prot, localPos);
                    //float3 tv = math.mul(prot, data.localPos); // v1.7.0
                    float3 tv = math.mul(prot, data.localPos * team.scaleDirection); // ??????????(v1.7.6)

                    // ?????
                    float vlen = math.distance(npos, ppos); // ?????(??????????????????????)
                    float blen = lengthBuffer[dataIndex + i]; // ??????
                    vlen = math.clamp(vlen, 0.0f, blen * 1.2f);

                    // ??????
                    float3 v = math.normalize(npos - ppos);

                    // ??????????
                    float maxAngle = gdata.maxAngle.Evaluate(depth);
                    maxAngle = math.radians(maxAngle);

                    float angle = math.acos(math.dot(v, tv));

                    if (flag.IsFixed() == false)
                    {
                        if (angle > maxAngle)
                        {
                            MathUtility.ClampAngle(v, tv, maxAngle, out v);
                        }

                        var mv = (ppos + v * vlen) - npos;

                        // ????????
                        mv = MathUtility.ClampVector(mv, 0.0f, maxMoveLength);

                        var fpos = npos + mv;

                        // ????????????
                        float friction = frictionList[index];
                        float moveratio = math.saturate(1.0f - friction * Define.Compute.FrictionMoveRatio);

                        // ???????????(??????????????????)
                        npos = math.lerp(npos, fpos, moveratio);

                        nextPosList[index] = npos;

                        // ????????(v1.8.0)
                        v = math.normalize(npos - ppos);

                        // ????
                        var av = (npos - opos) * (1.0f - gdata.velocityInfluence);
                        posList[index] = posList[index] + av;
                    }

                    // ????
                    nrot = math.mul(prot, new quaternion(data.localRot.value * team.quaternionScale)); // ??????????(v1.7.6)
                    var q = MathUtility.FromToRotation(tv, v);
                    nrot = math.mul(q, nrot);

                    nextRotList[index] = nrot;
                }
            }
        }
    }
}
