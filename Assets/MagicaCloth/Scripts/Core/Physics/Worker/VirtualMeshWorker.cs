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
    /// ??????????
    /// </summary>
    public class VirtualMeshWorker : PhysicsManagerWorker
    {


        //=========================================================================================
        public override void Create()
        {
        }

        public override void Release()
        {
        }

        public override void RemoveGroup(int group)
        {
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <returns></returns>
        private bool IsPerformMeshProcessForEachParticle()
        {
            // ????????????????????????????
            return Manager.Mesh.virtualMeshInfoList.Count <
                Manager.UpdateTime.WorkerMaximumCount * Define.RenderMesh.WorkerMultiplesOfVertexCollection;
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
        /// ???????????????????????
        /// </summary>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        public override JobHandle PreUpdate(JobHandle jobHandle)
        {
            if (Manager.Mesh.VirtualMeshUseCount == 0)
                return jobHandle;

            // ???????????
            // ???????????????????????
            var job = new ReadMeshPositionJob()
            {
                virtualMeshInfoList = Manager.Mesh.virtualMeshInfoList.ToJobArray(),
                sharedVirtualMeshInfoList = Manager.Mesh.sharedVirtualMeshInfoList.ToJobArray(),

                //virtualVertexInfoList = Manager.Mesh.virtualVertexInfoList.ToJobArray(),
                virtualVertexMeshIndexList = Manager.Mesh.virtualVertexMeshIndexList.ToJobArray(),
                virtualVertexUseList = Manager.Mesh.virtualVertexUseList.ToJobArray(),
                virtualTransformIndexList = Manager.Mesh.virtualTransformIndexList.ToJobArray(),

                sharedVirtualVertexInfoList = Manager.Mesh.sharedVirtualVertexInfoList.ToJobArray(),
                sharedVirtualWeightList = Manager.Mesh.sharedVirtualWeightList.ToJobArray(),

                transformPosList = Manager.Bone.bonePosList.ToJobArray(),
                transformRotList = Manager.Bone.boneRotList.ToJobArray(),
                transformSclList = Manager.Bone.boneSclList.ToJobArray(),

                virtualPosList = Manager.Mesh.virtualPosList.ToJobArray(),
                virtualRotList = Manager.Mesh.virtualRotList.ToJobArray(),

                virtualVertexFlagList = Manager.Mesh.virtualVertexFlagList.ToJobArray(),
            };
            jobHandle = job.Schedule(Manager.Mesh.virtualPosList.Length, 64, jobHandle);

            return jobHandle;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        [BurstCompile]
        private struct ReadMeshPositionJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.VirtualMeshInfo> virtualMeshInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.SharedVirtualMeshInfo> sharedVirtualMeshInfoList;

            //[Unity.Collections.ReadOnly]
            //public NativeArray<uint> virtualVertexInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<short> virtualVertexMeshIndexList;
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> virtualVertexUseList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> virtualTransformIndexList;

            [Unity.Collections.ReadOnly]
            public NativeArray<uint> sharedVirtualVertexInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<MeshData.VertexWeight> sharedVirtualWeightList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> transformPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> transformRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> transformSclList;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> virtualPosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> virtualRotList;

            [Unity.Collections.WriteOnly]
            public NativeArray<byte> virtualVertexFlagList;

            // ??????????
            public void Execute(int vindex)
            {
                // ??????
                if (virtualVertexUseList[vindex] == 0)
                    return;

                // ??????????????
                int mindex = virtualVertexMeshIndexList[vindex];
                var m_minfo = virtualMeshInfoList[mindex - 1]; // (-1)??????!
                if (m_minfo.IsUse() == false)
                    return;
                // ????????
                if (m_minfo.IsPause())
                    return;

                // ????????
                virtualVertexFlagList[vindex] = 0;

                var s_minfo = sharedVirtualMeshInfoList[m_minfo.sharedVirtualMeshIndex];

                int i = vindex - m_minfo.vertexChunk.startIndex;
                int s_vindex = s_minfo.vertexChunk.startIndex + i;
                int s_wstart = s_minfo.weightChunk.startIndex;
                int m_bstart = m_minfo.boneChunk.startIndex;

                // ???????(??????????????)
                float3 spos = 0;
                float3 snor = 0;
                float3 stan = 0;

                uint pack = sharedVirtualVertexInfoList[s_vindex];
                int wcnt = DataUtility.Unpack4_28Hi(pack);
                int wstart = DataUtility.Unpack4_28Low(pack);
                for (int j = 0; j < wcnt; j++)
                {
                    var vw = sharedVirtualWeightList[s_wstart + wstart + j];

                    // ???
                    int tindex = virtualTransformIndexList[m_bstart + vw.parentIndex];
                    var tpos = transformPosList[tindex];
                    var trot = transformRotList[tindex];
                    var tscl = transformSclList[tindex];

                    // ????0??????
                    spos += (tpos + math.mul(trot, vw.localPos * tscl)) * vw.weight;

                    // ??/??
                    if (tscl.x < 0 || tscl.y < 0 || tscl.z < 0)
                    {
                        // ??????????
                        // ??/??????????????????????????????????
                        var q = quaternion.LookRotation(vw.localNor, vw.localTan);
                        q = new quaternion(q.value * new float4(-math.sign(tscl), 1));
                        snor += math.mul(trot, math.mul(q, new float3(0, 0, 1))) * vw.weight;
                        stan += math.mul(trot, math.mul(q, new float3(0, 1, 0))) * vw.weight;
                    }
                    else
                    {
                        snor += math.mul(trot, vw.localNor) * vw.weight;
                        stan += math.mul(trot, vw.localTan) * vw.weight;
                    }
                }

                virtualPosList[vindex] = spos;
                virtualRotList[vindex] = quaternion.LookRotation(snor, stan);
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        public override JobHandle PostUpdate(JobHandle jobHandle)
        {
            if (Manager.Mesh.VirtualMeshUseCount == 0)
                return jobHandle;

            if (IsPerformMeshProcessForEachParticle())
            {
                // ????
                // ????????????????????
                var job = new CalcMeshTriangleNormalTangentJob()
                {
                    virtualMeshInfoList = Manager.Mesh.virtualMeshInfoList.ToJobArray(),
                    sharedVirtualMeshInfoList = Manager.Mesh.sharedVirtualMeshInfoList.ToJobArray(),

                    virtualTriangleMeshIndexList = Manager.Mesh.virtualTriangleMeshIndexList.ToJobArray(),
                    //virtualVertexInfoList = Manager.Mesh.virtualVertexInfoList.ToJobArray(),
                    virtualVertexUseList = Manager.Mesh.virtualVertexUseList.ToJobArray(),
                    virtualPosList = Manager.Mesh.virtualPosList.ToJobArray(),

                    sharedTriangles = Manager.Mesh.sharedVirtualTriangleList.ToJobArray(),
                    sharedMeshUv = Manager.Mesh.sharedVirtualUvList.ToJobArray(),

                    virtualTriangleNormalList = Manager.Mesh.virtualTriangleNormalList.ToJobArray(),
                    virtualTriangleTangentList = Manager.Mesh.virtualTriangleTangentList.ToJobArray(),

                    transformSclList = Manager.Bone.boneSclList.ToJobArray(),
                };
                jobHandle = job.Schedule(Manager.Mesh.virtualTriangleMeshIndexList.Length, 128, jobHandle);

                // ?????????????????????/??????
                var job2 = new CalcVertexNormalTangentFromTriangleJob()
                {
                    virtualMeshInfoList = Manager.Mesh.virtualMeshInfoList.ToJobArray(),
                    sharedVirtualMeshInfoList = Manager.Mesh.sharedVirtualMeshInfoList.ToJobArray(),
                    //virtualVertexInfoList = Manager.Mesh.virtualVertexInfoList.ToJobArray(),
                    virtualVertexMeshIndexList = Manager.Mesh.virtualVertexMeshIndexList.ToJobArray(),
                    virtualVertexUseList = Manager.Mesh.virtualVertexUseList.ToJobArray(),
                    virtualVertexFlagList = Manager.Mesh.virtualVertexFlagList.ToJobArray(),

                    sharedVirtualVertexToTriangleInfoList = Manager.Mesh.sharedVirtualVertexToTriangleInfoList.ToJobArray(),
                    sharedVirtualVertexToTriangleIndexList = Manager.Mesh.sharedVirtualVertexToTriangleIndexList.ToJobArray(),

                    virtualTriangleNormalList = Manager.Mesh.virtualTriangleNormalList.ToJobArray(),
                    virtualTriangleTangentList = Manager.Mesh.virtualTriangleTangentList.ToJobArray(),

                    virtualRotList = Manager.Mesh.virtualRotList.ToJobArray(),
                };
                jobHandle = job2.Schedule(Manager.Mesh.virtualPosList.Length, 128, jobHandle);
            }
            else
            {
                // ???????????/????(??????)
                // ?????????????????
                var job = new CalcMeshTriangleNormalTangentForEachMeshJob()
                {
                    virtualMeshInfoList = Manager.Mesh.virtualMeshInfoList.ToJobArray(),
                    sharedVirtualMeshInfoList = Manager.Mesh.sharedVirtualMeshInfoList.ToJobArray(),

                    virtualVertexUseList = Manager.Mesh.virtualVertexUseList.ToJobArray(),
                    virtualVertexFlagList = Manager.Mesh.virtualVertexFlagList.ToJobArray(),
                    virtualPosList = Manager.Mesh.virtualPosList.ToJobArray(),

                    sharedTriangles = Manager.Mesh.sharedVirtualTriangleList.ToJobArray(),
                    sharedMeshUv = Manager.Mesh.sharedVirtualUvList.ToJobArray(),
                    sharedVirtualVertexToTriangleInfoList = Manager.Mesh.sharedVirtualVertexToTriangleInfoList.ToJobArray(),
                    sharedVirtualVertexToTriangleIndexList = Manager.Mesh.sharedVirtualVertexToTriangleIndexList.ToJobArray(),

                    transformSclList = Manager.Bone.boneSclList.ToJobArray(),

                    virtualTriangleNormalList = Manager.Mesh.virtualTriangleNormalList.ToJobArray(),
                    virtualTriangleTangentList = Manager.Mesh.virtualTriangleTangentList.ToJobArray(),

                    virtualRotList = Manager.Mesh.virtualRotList.ToJobArray(),
                };
                jobHandle = job.Schedule(Manager.Mesh.virtualMeshInfoList.Length, 1, jobHandle);
            }

            return jobHandle;
        }

        /// <summary>
        /// ??????????????????????
        /// </summary>
        [BurstCompile]
        private struct CalcMeshTriangleNormalTangentJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.VirtualMeshInfo> virtualMeshInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.SharedVirtualMeshInfo> sharedVirtualMeshInfoList;

            [Unity.Collections.ReadOnly]
            public NativeArray<ushort> virtualTriangleMeshIndexList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<uint> virtualVertexInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> virtualVertexUseList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> virtualPosList;

            [Unity.Collections.ReadOnly]
            public NativeArray<int> sharedTriangles;
            [Unity.Collections.ReadOnly]
            public NativeArray<float2> sharedMeshUv;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> virtualTriangleNormalList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> virtualTriangleTangentList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> transformSclList;

            // ???????????????
            public void Execute(int tindex)
            {
                // ????0???
                //virtualTriangleNormalList[tindex] = 0;
                //virtualTriangleTangentList[tindex] = 0;
                int mindex = virtualTriangleMeshIndexList[tindex];
                if (mindex == 0)
                    return;

                // ??????????????
                var m_minfo = virtualMeshInfoList[mindex - 1]; // (-1)??????!
                if (m_minfo.IsUse() == false)
                    return;

                // ????
                if (m_minfo.IsPause())
                    return;

                var s_minfo = sharedVirtualMeshInfoList[m_minfo.sharedVirtualMeshIndex];

                int m_vstart = m_minfo.vertexChunk.startIndex;
                int s_vstart = s_minfo.vertexChunk.startIndex;
                int s_tstart = s_minfo.triangleChunk.startIndex;

                int i = tindex - m_minfo.triangleChunk.startIndex;

                int sindex0 = sharedTriangles[s_tstart + i * 3];
                int sindex1 = sharedTriangles[s_tstart + i * 3 + 1];
                int sindex2 = sharedTriangles[s_tstart + i * 3 + 2];

                int vindex0 = m_vstart + sindex0;
                int vindex1 = m_vstart + sindex1;
                int vindex2 = m_vstart + sindex2;

                // ???????????
                if (virtualVertexUseList[vindex0] == 0 || virtualVertexUseList[vindex1] == 0 || virtualVertexUseList[vindex2] == 0)
                    return;

                // ??
                var pos0 = virtualPosList[vindex0];
                var pos1 = virtualPosList[vindex1];
                var pos2 = virtualPosList[vindex2];

                var v0 = pos1 - pos0;
                var v1 = pos2 - pos0;
                // ?????????????????????
                v0 *= 1000;
                v1 *= 1000;
                var tn = math.normalize(math.cross(v0, v1));

                // ??(?????UV??????????????????)
                var w0 = sharedMeshUv[s_vstart + sindex0];
                var w1 = sharedMeshUv[s_vstart + sindex1];
                var w2 = sharedMeshUv[s_vstart + sindex2];
                float3 distBA = pos1 - pos0;
                float3 distCA = pos2 - pos0;
                float2 tdistBA = w1 - w0;
                float2 tdistCA = w2 - w0;
                float area = tdistBA.x * tdistCA.y - tdistBA.y * tdistCA.x;
                float3 tan = 1;
                if (area == 0.0f)
                {
                    // error
                }
                else
                {
                    float delta = 1.0f / area;
                    tan = new float3(
                        (distBA.x * tdistCA.y) + (distCA.x * -tdistBA.y),
                        (distBA.y * tdistCA.y) + (distCA.y * -tdistBA.y),
                        (distBA.z * tdistCA.y) + (distCA.z * -tdistBA.y)
                        ) * delta;
                    // ??????????
                    tan = -tan;
                }

                // ??????????(v1.7.6)
                var bscl = transformSclList[m_minfo.transformIndex];
                if (bscl.x < 0)
                {
                    tn = -tn;
                }
                else if (bscl.y < 0)
                {
                    tn = -tn;
                    tan = -tan;
                }

                virtualTriangleNormalList[tindex] = tn;
                virtualTriangleTangentList[tindex] = tan;
            }
        }

        /// <summary>
        /// ??????????????????????????????????
        /// </summary>
        [BurstCompile]
        private struct CalcVertexNormalTangentFromTriangleJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.VirtualMeshInfo> virtualMeshInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.SharedVirtualMeshInfo> sharedVirtualMeshInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<short> virtualVertexMeshIndexList;
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> virtualVertexUseList;
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> virtualVertexFlagList;

            [Unity.Collections.ReadOnly]
            public NativeArray<uint> sharedVirtualVertexToTriangleInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> sharedVirtualVertexToTriangleIndexList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> virtualTriangleNormalList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> virtualTriangleTangentList;

            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> virtualRotList;

            // ??????????
            public void Execute(int vindex)
            {
                // ?????????
                var vflag = virtualVertexFlagList[vindex];
                if (vflag == 0)
                    return;

                // ??????
                if (virtualVertexUseList[vindex] == 0)
                    return;

                // ??????????????
                int mindex = virtualVertexMeshIndexList[vindex];
                var m_minfo = virtualMeshInfoList[mindex - 1]; // (-1)??????!
                if (m_minfo.IsUse() == false)
                    return;

                // ????
                if (m_minfo.IsPause())
                    return;

                var s_minfo = sharedVirtualMeshInfoList[m_minfo.sharedVirtualMeshIndex];
                int s_vstart = s_minfo.vertexChunk.startIndex;
                int i = vindex - m_minfo.vertexChunk.startIndex;
                int m_tstart = m_minfo.triangleChunk.startIndex;

                uint pack = sharedVirtualVertexToTriangleInfoList[s_vstart + i];
                int s_tcnt = DataUtility.Unpack8_24Hi(pack);
                int s_tstart = DataUtility.Unpack8_24Low(pack);
                if (s_tcnt == 0)
                    return;
                s_tstart += s_minfo.vertexToTriangleChunk.startIndex;

                float3 nor = 0;
                float3 tan = 0;
                for (int j = 0; j < s_tcnt; j++)
                {
                    int tindex = sharedVirtualVertexToTriangleIndexList[s_tstart + j] + m_tstart;

                    nor += virtualTriangleNormalList[tindex];
                    tan += virtualTriangleTangentList[tindex];
                }

                // ??0?????(v1.9.2)
                if (math.lengthsq(nor) > Define.Compute.Epsilon)
                {
                    nor = math.normalize(nor);
                    tan = math.normalize(tan);
                    virtualRotList[vindex] = quaternion.LookRotation(nor, tan);
                }
            }
        }

        /// <summary>
        /// ?????????????/???????
        /// ?????????
        /// </summary>
        [BurstCompile]
        private struct CalcMeshTriangleNormalTangentForEachMeshJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.VirtualMeshInfo> virtualMeshInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.SharedVirtualMeshInfo> sharedVirtualMeshInfoList;

            [Unity.Collections.ReadOnly]
            public NativeArray<byte> virtualVertexUseList;
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> virtualVertexFlagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> virtualPosList;

            [Unity.Collections.ReadOnly]
            public NativeArray<int> sharedTriangles;
            [Unity.Collections.ReadOnly]
            public NativeArray<float2> sharedMeshUv;
            [Unity.Collections.ReadOnly]
            public NativeArray<uint> sharedVirtualVertexToTriangleInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> sharedVirtualVertexToTriangleIndexList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> transformSclList;

            //[Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> virtualTriangleNormalList;
            //[Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> virtualTriangleTangentList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<quaternion> virtualRotList;

            // ????????
            public void Execute(int mindex)
            {
                if (mindex == 0)
                    return;

                // ??????????????
                var m_minfo = virtualMeshInfoList[mindex - 1]; // (-1)??????!
                if (m_minfo.IsUse() == false)
                    return;

                // ????
                if (m_minfo.IsPause())
                    return;

                var s_minfo = sharedVirtualMeshInfoList[m_minfo.sharedVirtualMeshIndex];
                int s_vstart = s_minfo.vertexChunk.startIndex;
                int s_tstart = s_minfo.triangleChunk.startIndex;
                int m_vstart = m_minfo.vertexChunk.startIndex;

                // ????????
                var m_scl = transformSclList[m_minfo.transformIndex];

                // ?????????????
                var tc = m_minfo.triangleChunk;
                for (int i = 0, tindex = tc.startIndex; i < tc.dataLength; i++, tindex++)
                {
                    int sindex0 = sharedTriangles[s_tstart + i * 3];
                    int sindex1 = sharedTriangles[s_tstart + i * 3 + 1];
                    int sindex2 = sharedTriangles[s_tstart + i * 3 + 2];

                    int vindex0 = m_vstart + sindex0;
                    int vindex1 = m_vstart + sindex1;
                    int vindex2 = m_vstart + sindex2;

                    // ???????????
                    if (virtualVertexUseList[vindex0] == 0 || virtualVertexUseList[vindex1] == 0 || virtualVertexUseList[vindex2] == 0)
                        continue;

                    // ??
                    var pos0 = virtualPosList[vindex0];
                    var pos1 = virtualPosList[vindex1];
                    var pos2 = virtualPosList[vindex2];

                    var v0 = pos1 - pos0;
                    var v1 = pos2 - pos0;
                    // ?????????????????????
                    v0 *= 1000;
                    v1 *= 1000;
                    var tn = math.normalize(math.cross(v0, v1));

                    // ??(?????UV??????????????????)
                    var w0 = sharedMeshUv[s_vstart + sindex0];
                    var w1 = sharedMeshUv[s_vstart + sindex1];
                    var w2 = sharedMeshUv[s_vstart + sindex2];
                    float3 distBA = pos1 - pos0;
                    float3 distCA = pos2 - pos0;
                    float2 tdistBA = w1 - w0;
                    float2 tdistCA = w2 - w0;
                    float area = tdistBA.x * tdistCA.y - tdistBA.y * tdistCA.x;
                    float3 tan = 1;
                    if (area == 0.0f)
                    {
                        // error
                    }
                    else
                    {
                        float delta = 1.0f / area;
                        tan = new float3(
                            (distBA.x * tdistCA.y) + (distCA.x * -tdistBA.y),
                            (distBA.y * tdistCA.y) + (distCA.y * -tdistBA.y),
                            (distBA.z * tdistCA.y) + (distCA.z * -tdistBA.y)
                            ) * delta;
                        // ??????????
                        tan = -tan;
                    }

                    // ??????????(v1.7.6)
                    if (m_scl.x < 0)
                    {
                        tn = -tn;
                    }
                    else if (m_scl.y < 0)
                    {
                        tn = -tn;
                        tan = -tan;
                    }

                    virtualTriangleNormalList[tindex] = tn;
                    virtualTriangleTangentList[tindex] = tan;
                }


                // ????????
                var s_v2t_start = s_minfo.vertexToTriangleChunk.startIndex;
                var vc = m_minfo.vertexChunk;
                for (int i = 0, vindex = vc.startIndex; i < vc.dataLength; i++, vindex++)
                {
                    // ?????????
                    var vflag = virtualVertexFlagList[vindex];
                    if (vflag == 0)
                        continue;

                    // ??????
                    if (virtualVertexUseList[vindex] == 0)
                        continue;

                    uint pack = sharedVirtualVertexToTriangleInfoList[s_vstart + i];
                    int tcnt = DataUtility.Unpack8_24Hi(pack);
                    int tstart = DataUtility.Unpack8_24Low(pack);
                    if (tcnt == 0)
                        return;
                    tstart += s_v2t_start;

                    float3 nor = 0;
                    float3 tan = 0;
                    for (int j = 0; j < tcnt; j++)
                    {
                        int tindex = sharedVirtualVertexToTriangleIndexList[tstart + j] + tc.startIndex;

                        nor += virtualTriangleNormalList[tindex];
                        tan += virtualTriangleTangentList[tindex];
                    }

                    // ??0?????(v1.9.2)
                    if (math.lengthsq(nor) > Define.Compute.Epsilon)
                    {
                        nor = math.normalize(nor);
                        tan = math.normalize(tan);
                        virtualRotList[vindex] = quaternion.LookRotation(nor, tan);
                    }
                }
            }
        }
    }
}
