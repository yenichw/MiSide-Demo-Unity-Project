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
    /// </summary>
    public class AdjustRotationWorker : PhysicsManagerWorker
    {
        // ???????
        const int AdjustMode_Fixed = 0; // ???BaseRot?????(v1.7.3)
        const int AdjustMode_XYMove = 1;
        const int AdjustMode_XZMove = 2;
        const int AdjustMode_YZMove = 3;

        /// <summary>
        /// ?????
        /// ????????????RotationLine???????
        /// </summary>
        [System.Serializable]
        public struct AdjustRotationData
        {
            /// <summary>
            /// ??????????
            /// </summary>
            public int keyIndex;

            /// <summary>
            /// ?????????????
            /// ???????????????????????????????????????????????
            /// ????????0??????????(-1)?????????!
            /// </summary>
            public int targetIndex;

            /// <summary>
            /// ?????????????(??????)
            /// </summary>
            public float3 localPos;

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return keyIndex != 0 || targetIndex != 0;
            }
        }
        FixedChunkNativeArray<AdjustRotationData> dataList;

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
            public int adjustMode;

            /// <summary>
            /// AdjustMode?XY/XZ/YZMove????????????
            /// </summary>
            public float3 axisRotationPower;

            public ChunkData chunk;
        }
        public FixedNativeList<GroupData> groupList;

        /// <summary>
        /// ??????????????
        /// </summary>
        ExNativeMultiHashMap<int, int> particleMap;

        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<AdjustRotationData>();
            groupList = new FixedNativeList<GroupData>();
            particleMap = new ExNativeMultiHashMap<int, int>();
        }

        public override void Release()
        {
            dataList.Dispose();
            groupList.Dispose();
            particleMap.Dispose();
        }

        public int AddGroup(int teamId, bool active, int adjustMode, float3 axisRotationPower, AdjustRotationData[] dataArray)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.adjustMode = adjustMode;
            gdata.axisRotationPower = axisRotationPower;
            if (dataArray != null && dataArray.Length > 0)
            {
                // ????RotationLine????????
                var c = this.dataList.AddChunk(dataArray.Length);
                gdata.chunk = c;

                // ??????????
                dataList.ToJobArray().CopyFromFast(c.startIndex, dataArray);

                // ???????????????
                int pstart = teamData.particleChunk.startIndex;
                for (int i = 0; i < dataArray.Length; i++)
                {
                    var data = dataArray[i];
                    int dindex = c.startIndex + i;
                    particleMap.Add(pstart + data.keyIndex, dindex);
                }
            }

            int group = groupList.Add(gdata);
            return group;
        }

        public override void RemoveGroup(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.adjustRotationGroupIndex;
            if (group < 0)
                return;

            var cdata = groupList[group];

            // ?????????????????
            if (cdata.chunk.dataLength > 0)
            {
                int dstart = cdata.chunk.startIndex;
                int pstart = teamData.particleChunk.startIndex;
                for (int i = 0; i < cdata.chunk.dataLength; i++)
                {
                    int dindex = dstart + i;
                    var data = dataList[dindex];
                    particleMap.Remove(pstart + data.keyIndex, dindex);
                }

                // ?????????
                dataList.RemoveChunk(cdata.chunk);
            }

            // ?????
            groupList.Remove(group);
        }

        public void ChangeParam(int teamId, bool active, int adjustMode, float3 axisRotationPower)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.adjustRotationGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            gdata.adjustMode = adjustMode;
            gdata.axisRotationPower = axisRotationPower;
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

            // ??????(?????????????)
            var job1 = new AdjustRotationJob()
            {
                dataList = dataList.ToJobArray(),
                groupList = groupList.ToJobArray(),
                particleMap = particleMap.Map,

                teamDataList = Manager.Team.teamDataList.ToJobArray(),
                teamIdList = Manager.Particle.teamIdList.ToJobArray(),

                flagList = Manager.Particle.flagList.ToJobArray(),
                //basePosList = Manager.Particle.basePosList.ToJobArray(),
                //baseRotList = Manager.Particle.baseRotList.ToJobArray(),
                snapBasePosList = Manager.Particle.snapBasePosList.ToJobArray(),
                snapBaseRotList = Manager.Particle.snapBaseRotList.ToJobArray(),
                posList = Manager.Particle.posList.ToJobArray(),

                rotList = Manager.Particle.rotList.ToJobArray(),
            };
            jobHandle = job1.Schedule(Manager.Particle.Length, 64, jobHandle);

            return jobHandle;
        }

        /// <summary>
        /// ???????
        /// ???????????
        /// </summary>
        [BurstCompile]
        struct AdjustRotationJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<AdjustRotationData> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupList;
            [Unity.Collections.ReadOnly]
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
            public NativeParallelMultiHashMap<int, int> particleMap;
#else
            public NativeMultiHashMap<int, int> particleMap;
#endif

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<float3> basePosList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<quaternion> baseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> snapBasePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> snapBaseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> posList;

            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> rotList;

            /// <summary>
            /// ????????
            /// </summary>
            /// <param name="index"></param>
            public void Execute(int index)
            {
                var flag = flagList[index];
                if (flag.IsValid() == false)
                    return;

                // ???
                var team = teamDataList[teamIdList[index]];
                if (team.IsActive() == false || team.adjustRotationGroupIndex < 0)
                    return;
                // ????????
                if (team.IsPause())
                    return;
                int start = team.particleChunk.startIndex;

                // ???????
                var gdata = groupList[team.adjustRotationGroupIndex];
                if (gdata.active == 0)
                    return;

                // ??
                //quaternion baserot = baseRotList[index]; // ?????????????
                quaternion baserot = snapBaseRotList[index]; // ?????????????
                var nextrot = baserot;

                // ????
                var nextpos = posList[index];

                if (gdata.adjustMode == AdjustMode_Fixed)
                {
                    // ???[Fixed]????BaseRot?????
                }
                else
                {
                    // ?????????
                    // ??????????
                    //var lpos = nextpos - basePosList[index];
                    var lpos = nextpos - snapBasePosList[index];
                    lpos /= team.scaleRatio; // ?????????
                    lpos = math.mul(math.inverse(baserot), lpos);

                    // ???????
                    lpos *= gdata.axisRotationPower;

                    // ??????
                    quaternion lq = quaternion.identity;
                    if (gdata.adjustMode == AdjustMode_XYMove)
                    {
                        lq = quaternion.EulerZXY(-lpos.y, lpos.x, 0);
                    }
                    else if (gdata.adjustMode == AdjustMode_XZMove)
                    {
                        lq = quaternion.EulerZXY(lpos.z, 0, -lpos.x);
                    }
                    else if (gdata.adjustMode == AdjustMode_YZMove)
                    {
                        lq = quaternion.EulerZXY(0, lpos.z, -lpos.y);
                    }

                    // ????
                    nextrot = math.mul(nextrot, lq);
                    nextrot = math.normalize(nextrot); // ??????????????????
                }

                // ????
                rotList[index] = nextrot;
            }
        }
    }
}
