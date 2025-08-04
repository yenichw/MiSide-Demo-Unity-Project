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
    /// ???????????
    /// </summary>
    public class LineWorker : PhysicsManagerWorker
    {
        /// <summary>
        /// ???
        /// </summary>
        [System.Serializable]
        public struct LineRotationData
        {
            /// <summary>
            /// ????????
            /// </summary>
            public int vertexIndex;

            /// <summary>
            /// ?????????
            /// </summary>
            //public int parentVertexIndex;

            /// <summary>
            /// ?????
            /// </summary>
            public int childCount;

            /// <summary>
            /// ?????????????????
            /// </summary>
            public int childStartDataIndex;

            /// <summary>
            /// ????????????(Transform.localPosition???)
            /// </summary>
            public float3 localPos;

            /// <summary>
            /// ????????????(Transform.localRotation???)
            /// </summary>
            public quaternion localRot;

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            //public bool IsValid()
            //{
            //    return vertexIndex != 0 || parentVertexIndex != 0;
            //}
        }
        FixedChunkNativeArray<LineRotationData> dataList;

        [System.Serializable]
        public struct LineRotationRootInfo
        {
            public ushort startIndex;
            public ushort dataLength;
        }
        FixedChunkNativeArray<LineRotationRootInfo> rootInfoList;

        /// <summary>
        /// ????????????
        /// </summary>
        public struct GroupData
        {
            public int teamId;

            /// <summary>
            /// ????
            /// </summary>
            public int avarage;

            public ChunkData dataChunk;
            public ChunkData rootInfoChunk;
        }
        public FixedNativeList<GroupData> groupList;

        /// <summary>
        /// ???????????????
        /// </summary>
        FixedChunkNativeArray<int> rootTeamList;

        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<LineRotationData>();
            rootInfoList = new FixedChunkNativeArray<LineRotationRootInfo>();
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

        public int AddGroup(
            int teamId,
            bool avarage,
            LineRotationData[] dataArray,
            LineRotationRootInfo[] rootInfoArray
            )
        {
            if (dataArray == null || dataArray.Length == 0 || rootInfoArray == null || rootInfoArray.Length == 0)
                return -1;

            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.avarage = avarage ? 1 : 0;
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

        public override void RemoveGroup(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.lineWorkerGroupIndex;
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

        public void ChangeParam(int teamId, bool avarage)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.lineWorkerGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.avarage = avarage ? 1 : 0;
            groupList[group] = gdata;
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public override void Warmup()
        {
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        public override JobHandle PreUpdate(JobHandle jobHandle)
        {
            return jobHandle;
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        public override JobHandle PostUpdate(JobHandle jobHandle)
        {
            if (groupList.Count == 0)
                return jobHandle;

            // ????????(????????)
            var job1 = new LineRotationJob()
            {
                fixedUpdateCount = Manager.UpdateTime.FixedUpdateCount,

                dataList = dataList.ToJobArray(),
                rootInfoList = rootInfoList.ToJobArray(),
                rootTeamList = rootTeamList.ToJobArray(),
                groupList = groupList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                flagList = Manager.Particle.flagList.ToJobArray(),

                posList = Manager.Particle.posList.ToJobArray(),
                rotList = Manager.Particle.rotList.ToJobArray(),
            };
            jobHandle = job1.Schedule(rootTeamList.Length, 8, jobHandle);

            return jobHandle;
        }

        /// <summary>
        /// ????????
        /// </summary>
        [BurstCompile]
        struct LineRotationJob : IJobParallelFor
        {
            public int fixedUpdateCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<LineRotationData> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<LineRotationRootInfo> rootInfoList;
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
            public NativeArray<float3> posList;

            [NativeDisableParallelForRestriction]
            public NativeArray<quaternion> rotList;

            // ????????
            public void Execute(int rootIndex)
            {
                // ???
                int teamIndex = rootTeamList[rootIndex];
                if (teamIndex == 0)
                    return;
                var team = teamDataList[teamIndex];
                if (team.IsActive() == false || team.lineWorkerGroupIndex < 0)
                    return;
                // ????????
                if (team.IsPause())
                    return;

                // ??????UnityPhysics????????
                bool isPhysicsUpdate = team.IsPhysicsUpdate();

                // ???????
                var gdata = groupList[team.lineWorkerGroupIndex];

                // ???
                var rootInfo = rootInfoList[rootIndex];
                int dstart = gdata.dataChunk.startIndex;
                int dataIndex = rootInfo.startIndex + dstart;
                int dataCount = rootInfo.dataLength;
                int pstart = team.particleChunk.startIndex;

                if (dataCount <= 1)
                    return;

                for (int i = 0; i < dataCount; i++)
                {
                    var data = dataList[dataIndex + i];

                    var pindex = data.vertexIndex;
                    pindex += pstart;

                    var flag = flagList[pindex];
                    if (flag.IsValid() == false)
                        continue;

                    // ???????
                    var pos = posList[pindex];
                    var rot = rotList[pindex];

                    // ??????
                    if (data.childCount > 0)
                    {
                        // ?????????
                        float3 ctv = 0;
                        float3 cv = 0;

                        for (int j = 0; j < data.childCount; j++)
                        {
                            var cdata = dataList[data.childStartDataIndex + dstart + j];
                            int cindex = cdata.vertexIndex + pstart;

                            // ?????
                            var cflag = flagList[cindex];


                            // ??????
                            var cpos = posList[cindex];

                            // ?????????
                            //float3 tv = math.normalize(math.mul(rot, cdata.localPos));
                            float3 tv = math.normalize(math.mul(rot, cdata.localPos * team.scaleDirection)); // ??????????(v1.7.6)
                            ctv += tv;

                            // ????????
                            float3 v = math.normalize(cpos - pos);
                            cv += v;

                            // ???????????????????????????????
                            if (cflag.IsFlag(PhysicsManagerParticleData.Flag_TriangleRotation))
                                continue;

                            // ??
                            var q = MathUtility.FromToRotation(tv, v);

                            // ????????
                            //var crot = math.mul(rot, cdata.localRot);
                            var crot = math.mul(rot, new quaternion(cdata.localRot.value * team.quaternionScale)); // ??????????(v1.7.6)
                            crot = math.mul(q, crot);
                            rotList[cindex] = crot;
                        }

                        // ??????????????????????????????
                        if (flag.IsFlag(PhysicsManagerParticleData.Flag_TriangleRotation))
                            continue;

                        // ???????????(v1.5.2)
                        if (team.IsFlag(PhysicsManagerTeamData.Flag_FixedNonRotation) && flag.IsKinematic())
                            continue;

                        // ???????????????
                        var cq = MathUtility.FromToRotation(ctv, cv);

                        // ????
                        if (gdata.avarage == 1)
                        {
                            cq = math.slerp(quaternion.identity, cq, 0.5f); // 50%
                        }

                        // ???????????
                        rot = math.mul(cq, rot);
                        rotList[pindex] = math.normalize(rot); // ??????????????????
                    }
                }
            }
        }
    }
}
