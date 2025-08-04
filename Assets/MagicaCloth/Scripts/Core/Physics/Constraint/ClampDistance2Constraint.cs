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
    /// ????????
    /// ????????????????????
    /// ?????????????????????(1.8.0)
    /// </summary>
    public class ClampDistance2Constraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ?????
        /// </summary>
        [System.Serializable]
        public struct ClampDistance2Data
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
            /// ???????
            /// </summary>
            public float length;
        }
        FixedChunkNativeArray<ClampDistance2Data> dataList;

        [System.Serializable]
        public struct ClampDistance2RootInfo
        {
            public ushort startIndex;
            public ushort dataLength;
        }
        FixedChunkNativeArray<ClampDistance2RootInfo> rootInfoList;

        /// <summary>
        /// ????????????
        /// </summary>
        public struct GroupData
        {
            public int teamId;
            public int active;

            /// <summary>
            /// ??????
            /// </summary>
            public float minRatio;

            /// <summary>
            /// ??????
            /// </summary>
            public float maxRatio;

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
        //FixedChunkNativeArray<float> lengthBuffer;

        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<ClampDistance2Data>();
            rootInfoList = new FixedChunkNativeArray<ClampDistance2RootInfo>();
            groupList = new FixedNativeList<GroupData>();
            rootTeamList = new FixedChunkNativeArray<int>();
        }

        public override void Release()
        {
            dataList.Dispose();
            rootInfoList.Dispose();
            groupList.Dispose();
            rootTeamList.Dispose();
        }

        //=========================================================================================
        public int AddGroup(
            int teamId,
            bool active,
            float minRatio,
            float maxRatio,
            float velocityInfluence,
            ClampDistance2Data[] dataArray,
            ClampDistance2RootInfo[] rootInfoArray
            )
        {
            if (dataArray == null || dataArray.Length == 0 || rootInfoArray == null || rootInfoArray.Length == 0)
                return -1;

            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.minRatio = minRatio;
            gdata.maxRatio = maxRatio;
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

            return group;
        }

        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.clampDistance2GroupIndex;
            if (group < 0)
                return;

            var cdata = groupList[group];

            // ?????????
            dataList.RemoveChunk(cdata.dataChunk);
            rootInfoList.RemoveChunk(cdata.rootInfoChunk);
            rootTeamList.RemoveChunk(cdata.rootInfoChunk);

            // ?????
            groupList.Remove(group);
        }

        public void ChangeParam(
            int teamId,
            bool active,
            float minRatio,
            float maxRatio,
            float velocityInfluence
            )
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.clampDistance2GroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            gdata.minRatio = minRatio;
            gdata.maxRatio = maxRatio;
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

            // ??????(????????)
            var job1 = new ClampDistance2Job()
            {
                runCount = runCount,

                dataList = dataList.ToJobArray(),
                rootInfoList = rootInfoList.ToJobArray(),
                rootTeamList = rootTeamList.ToJobArray(),
                groupList = groupList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                flagList = Manager.Particle.flagList.ToJobArray(),
                frictionList = Manager.Particle.frictionList.ToJobArray(),
                nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                posList = Manager.Particle.posList.ToJobArray(),
            };
            jobHandle = job1.Schedule(rootTeamList.Length, 8, jobHandle);

            return jobHandle;
        }


        /// <summary>
        /// ?????????
        /// </summary>
        [BurstCompile]
        struct ClampDistance2Job : IJobParallelFor
        {
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<ClampDistance2Data> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<ClampDistance2RootInfo> rootInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> rootTeamList;
            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupList;

            // ???
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> frictionList;

            [NativeDisableParallelForRestriction]
            public NativeArray<float3> nextPosList;

            [NativeDisableParallelForRestriction]
            public NativeArray<float3> posList;

            // ????????
            public void Execute(int rootIndex)
            {
                // ???
                int teamIndex = rootTeamList[rootIndex];
                if (teamIndex == 0)
                    return;
                var team = teamDataList[teamIndex];
                if (team.IsActive() == false || team.clampDistance2GroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // ???????
                var gdata = groupList[team.clampDistance2GroupIndex];
                if (gdata.active == 0)
                    return;

                // ???
                var rootInfo = rootInfoList[rootIndex];
                int dataIndex = rootInfo.startIndex + gdata.dataChunk.startIndex;
                int dataCount = rootInfo.dataLength;
                int pstart = team.particleChunk.startIndex;

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
                    if (flag.IsValid() == false || flag.IsFixed())
                        continue;

                    var npos = nextPosList[index];
                    var opos = npos;

                    var ppos = nextPosList[pindex];

                    // ???????
                    var v = npos - ppos;

                    // ??????
                    var len = data.length * team.scaleRatio;
                    v = MathUtility.ClampVector(v, len * gdata.minRatio, len * gdata.maxRatio);

                    npos = ppos + v;

                    // ??
                    nextPosList[index] = npos;

                    // ????
                    var av = (npos - opos) * (1.0f - gdata.velocityInfluence);
                    posList[index] = posList[index] + av;
                }
            }
        }
    }
}
