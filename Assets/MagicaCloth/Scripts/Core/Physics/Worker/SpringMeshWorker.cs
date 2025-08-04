// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
    /// <summary>
    /// ?????????????
    /// </summary>
    public class SpringMeshWorker : PhysicsManagerWorker
    {
        public struct SpringData
        {
            /// <summary>
            /// ????????????????
            /// </summary>
            public int particleIndex;

            /// <summary>
            /// ????(0.0-1.0)
            /// </summary>
            public float weight;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        ExNativeMultiHashMap<int, SpringData> springMap;

        /// <summary>
        /// ?????????
        /// </summary>
        FixedNativeListWithCount<int> springVertexList;

        /// <summary>
        /// ID?????
        /// </summary>
        //int idSeed;

        /// <summary>
        /// ????????????
        /// </summary>
        /// <returns></returns>
        Dictionary<int, List<int>> groupIndexDict = new Dictionary<int, List<int>>();

        //=========================================================================================
        public override void Create()
        {
            springMap = new ExNativeMultiHashMap<int, SpringData>();
            springVertexList = new FixedNativeListWithCount<int>();
            springVertexList.SetEmptyElement(-1);
        }

        public override void Release()
        {
            springMap.Dispose();
            springVertexList.Dispose();
        }

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="group"></param>
        /// <param name="vertexIndex">???????????????</param>
        /// <param name="particleIndex">???????????????</param>
        /// <param name="weight">??(0.0-1.0)</param>
        /// <returns></returns>
        public void Add(int group, int vertexIndex, int particleIndex, float weight)
        {
            var data = new SpringData()
            {
                particleIndex = particleIndex,
                weight = math.saturate(weight)
            };
            springMap.Add(vertexIndex, data);
            springVertexList.Add(vertexIndex);

            if (groupIndexDict.ContainsKey(group) == false)
            {
                groupIndexDict.Add(group, new List<int>());
            }
            groupIndexDict[group].Add(vertexIndex);
        }

        /// <summary>
        /// ??(????)
        /// </summary>
        /// <param name="group"></param>
        public override void RemoveGroup(int group)
        {
            if (groupIndexDict.ContainsKey(group))
            {
                var clist = groupIndexDict[group];
                foreach (var index in clist)
                {
                    springVertexList.Remove(index);
                    springMap.Remove(index);
                }
                groupIndexDict.Remove(group);
            }
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
        public override JobHandle PreUpdate(JobHandle jobHandle)
        {
            // ????
            return jobHandle;
        }

        //=========================================================================================
        public override JobHandle PostUpdate(JobHandle jobHandle)
        {
            if (springMap.Count == 0)
                return jobHandle;

            var job = new SpringJob()
            {
                springVertexList = springVertexList.ToJobArray(),
                springMap = springMap.Map,

                flagList = Manager.Particle.flagList.ToJobArray(),
                particlePosList = Manager.Particle.posList.ToJobArray(),
                particleRotList = Manager.Particle.rotList.ToJobArray(),
                //particleBasePosList = Manager.Particle.basePosList.ToJobArray(),
                //particleBaseRotList = Manager.Particle.baseRotList.ToJobArray(),
                snapBasePosList = Manager.Particle.snapBasePosList.ToJobArray(),
                snapBaseRotList = Manager.Particle.snapBaseRotList.ToJobArray(),

                virtualPosList = Manager.Mesh.virtualPosList.ToJobArray(),
                virtualVertexFlagList = Manager.Mesh.virtualVertexFlagList.ToJobArray(),
                virtualVertexMeshIndexList = Manager.Mesh.virtualVertexMeshIndexList.ToJobArray(),

                virtualMeshInfoList = Manager.Mesh.virtualMeshInfoList.ToJobArray(),
            };
            jobHandle = job.Schedule(springVertexList.Length, 64, jobHandle);

            return jobHandle;
        }

        [BurstCompile]
        private struct SpringJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<int> springVertexList;
            [Unity.Collections.ReadOnly]
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
            public NativeParallelMultiHashMap<int, SpringData> springMap;
#else
            public NativeMultiHashMap<int, SpringData> springMap;
#endif

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> particlePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> particleRotList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<float3> particleBasePosList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<quaternion> particleBaseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> snapBasePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> snapBaseRotList;

            [NativeDisableParallelForRestriction]
            public NativeArray<float3> virtualPosList;
            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<byte> virtualVertexFlagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<short> virtualVertexMeshIndexList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.VirtualMeshInfo> virtualMeshInfoList;

#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
            NativeParallelMultiHashMapIterator<int> iterator;
#else
            NativeMultiHashMapIterator<int> iterator;
#endif

            // ???????????
            public void Execute(int index)
            {
                int vindex = springVertexList[index];
                if (vindex < 0)
                    return;

                // ??????????????
                int mindex = virtualVertexMeshIndexList[vindex];
                var m_minfo = virtualMeshInfoList[mindex - 1]; // (-1)??????!
                if (m_minfo.IsUse() == false)
                    return;
                // ????
                if (m_minfo.IsPause())
                    return;

                SpringData data;

                // ???????????????
                float totalWeight = 0;
                if (springMap.TryGetFirstValue(vindex, out data, out iterator))
                {
                    do
                    {
                        var flag = flagList[data.particleIndex];
                        if (flag.IsValid())
                            totalWeight += data.weight;
                    }
                    while (springMap.TryGetNextValue(out data, ref iterator));
                }

                if (totalWeight > 0 && springMap.TryGetFirstValue(vindex, out data, out iterator))
                {
                    var vpos = virtualPosList[vindex];
                    float3 pos = 0;

                    do
                    {
                        int pindex = data.particleIndex;
                        var flag = flagList[data.particleIndex];
                        if (flag.IsValid() == false)
                            continue;

                        // ??????????
                        var ppos = particlePosList[pindex];
                        var prot = particleRotList[pindex];

                        // ??????????
                        //var pbpos = particleBasePosList[pindex];
                        //var pbrot = particleBaseRotList[pindex];
                        var pbpos = snapBasePosList[pindex];
                        var pbrot = snapBaseRotList[pindex];
                        var ivpbrot = math.inverse(pbrot);

                        // (1)??????Base???????
                        var lpos = math.mul(ivpbrot, (vpos - pbpos));

                        // (2)????????????????
                        var npos = math.mul(prot, lpos) + ppos;

                        // (3)????
                        npos = math.lerp(vpos, npos, data.weight);

                        // (4)??????
                        pos += npos * (data.weight / totalWeight);
                    }
                    while (springMap.TryGetNextValue(out data, ref iterator));

                    // ????
                    virtualPosList[vindex] = pos;

                    // ?????????/???????
                    virtualVertexFlagList[vindex] = PhysicsManagerMeshData.VirtualVertexFlag_Use;
                }
            }
        }
    }
}
