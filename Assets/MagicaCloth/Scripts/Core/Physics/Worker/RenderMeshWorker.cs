// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????????????
    /// ????????????????????/????
    /// </summary>
    public class RenderMeshWorker : PhysicsManagerWorker
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
        /// ???????????????????????
        /// </summary>
        /// <returns></returns>
        private bool IsPerformMeshProcessForEachParticle()
        {
            // ??????????????????????????????
            return Manager.Mesh.renderMeshInfoList.Count <
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
            if (Manager.Mesh.renderMeshInfoList.Count == 0)
                return;

            var job = new CalcVertexUseFlagJob()
            {
                updateFlag = PhysicsManagerMeshData.MeshFlag_UpdateUseVertexFront << Manager.Compute.SwapIndex,

                renderMeshInfoList = Manager.Mesh.renderMeshInfoList.ToJobArray(),
                sharedRenderMeshInfoList = Manager.Mesh.sharedRenderMeshInfoList.ToJobArray(),

                //virtualVertexInfoList = Manager.Mesh.virtualVertexInfoList.ToJobArray(),
                virtualVertexUseList = Manager.Mesh.virtualVertexUseList.ToJobArray(),
                virtualVertexFixList = Manager.Mesh.virtualVertexFixList.ToJobArray(),

                sharedChildVertexInfoList = Manager.Mesh.sharedChildVertexInfoList.ToJobArray(),
                sharedChildVertexWeightList = Manager.Mesh.sharedChildWeightList.ToJobArray(),

                sharedRenderVertices = Manager.Mesh.sharedRenderVertices.ToJobArray(),
                sharedRenderNormals = Manager.Mesh.sharedRenderNormals.ToJobArray(),
                sharedRenderTangents = Manager.Mesh.sharedRenderTangents.ToJobArray(),
                sharedBonesPerVertexList = Manager.Mesh.sharedBonesPerVertexList.ToJobArray(),
                sharedBonesPerVertexStartList = Manager.Mesh.sharedBonesPerVertexStartList.ToJobArray(),
                sharedBoneWeightList = Manager.Mesh.sharedBoneWeightList.ToJobArray(),

                renderPosList = Manager.Mesh.renderPosList.ToJobArray(),
                renderNormalList = Manager.Mesh.renderNormalList.ToJobArray(),
                renderTangentList = Manager.Mesh.renderTangentList.ToJobArray(),
                renderBoneWeightList = Manager.Mesh.renderBoneWeightList.ToJobArray(),

                renderVertexFlagList = Manager.Mesh.renderVertexFlagList.ToJobArray(),
            };
            Manager.Compute.MasterJob = job.Schedule(Manager.Mesh.renderMeshInfoList.Length, 1, Manager.Compute.MasterJob);
        }

        [BurstCompile]
        private struct CalcVertexUseFlagJob : IJobParallelFor
        {
            public uint updateFlag;

            public NativeArray<PhysicsManagerMeshData.RenderMeshInfo> renderMeshInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.SharedRenderMeshInfo> sharedRenderMeshInfoList;

            //[Unity.Collections.ReadOnly]
            //public NativeArray<uint> virtualVertexInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> virtualVertexUseList;
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> virtualVertexFixList;
            [Unity.Collections.ReadOnly]
            public NativeArray<uint> sharedChildVertexInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<MeshData.VertexWeight> sharedChildVertexWeightList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> sharedRenderVertices;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> sharedRenderNormals;
            [Unity.Collections.ReadOnly]
            public NativeArray<float4> sharedRenderTangents;
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> sharedBonesPerVertexList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> sharedBonesPerVertexStartList;
            [Unity.Collections.ReadOnly]
            public NativeArray<BoneWeight1> sharedBoneWeightList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> renderPosList;
            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> renderNormalList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float4> renderTangentList;
            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<BoneWeight1> renderBoneWeightList;

            [NativeDisableParallelForRestriction]
            public NativeArray<uint> renderVertexFlagList;

            // ??????????
            public void Execute(int rmindex)
            {
                var r_minfo = renderMeshInfoList[rmindex];
                if (r_minfo.IsUse() == false)
                    return;

                // ??????????????????
                if (r_minfo.IsFlag(updateFlag) == false)
                    return;

                var sr_minfo = sharedRenderMeshInfoList[r_minfo.renderSharedMeshIndex];

                for (int i = 0; i < r_minfo.vertexChunk.dataLength; i++)
                {
                    int index = r_minfo.vertexChunk.startIndex + i;
                    uint flag = renderVertexFlagList[index];

                    // ????????????
                    flag &= 0xffff;

                    int4 data;
                    uint bit = PhysicsManagerMeshData.RenderVertexFlag_Use;
                    for (int l = 0; l < PhysicsManagerMeshData.MaxRenderMeshLinkCount; l++)
                    {
                        if (r_minfo.IsLinkMesh(l))
                        {
                            // data.x = ????????????????????
                            // data.y = ?????????????????????
                            // data.z = ???????????????????
                            // data.w = ?????????????????????
                            data.x = r_minfo.childMeshVertexStartIndex[l];
                            data.y = r_minfo.childMeshWeightStartIndex[l];
                            data.z = r_minfo.virtualMeshVertexStartIndex[l];
                            data.w = r_minfo.sharedVirtualMeshVertexStartIndex[l];

                            int sc_wstart = data.y;
                            int m_vstart = data.z;
                            int sc_vindex = data.x + i;

                            // ?????????????????????????????????????????
                            //int usecnt = 0;
                            //uint pack = sharedChildVertexInfoList[sc_vindex];
                            //int wcnt = DataUtility.Unpack4_28Hi(pack);
                            //int wstart = DataUtility.Unpack4_28Low(pack);
                            //for (int j = 0; j < wcnt; j++)
                            //{
                            //    // ????0??????
                            //    var vw = sharedChildVertexWeightList[sc_wstart + wstart + j];
                            //    //if ((virtualVertexInfoList[m_vstart + vw.parentIndex] & 0xffff) > 0)
                            //    //    usecnt++;
                            //    if (virtualVertexUseList[m_vstart + vw.parentIndex] > 0)
                            //        usecnt++;
                            //}
                            //if (wcnt > 0 && wcnt == usecnt)
                            //{
                            //    // ????
                            //    flag |= bit;
                            //}

                            // ?????????????????????????????????????????
                            uint pack = sharedChildVertexInfoList[sc_vindex];
                            int wcnt = DataUtility.Unpack4_28Hi(pack);
                            int wstart = DataUtility.Unpack4_28Low(pack);
                            int fixcnt = 0;
                            int maxfix = wcnt * 75 / 100; // ?????????(75%??)
                            int j = 0;
                            for (; j < wcnt; j++)
                            {
                                // ????0??????
                                var vw = sharedChildVertexWeightList[sc_wstart + wstart + j];
                                int vindex = m_vstart + vw.parentIndex;

                                // ?????1????????????????????
                                if (virtualVertexUseList[vindex] == 0)
                                    break;

                                // ?????????????
                                if (virtualVertexFixList[vindex] > 0)
                                {
                                    fixcnt++;
                                    if (fixcnt > maxfix)
                                        break; // ???????????????????????????
                                }
                            }
                            if (wcnt == j)
                            {
                                // ????
                                flag |= bit;
                            }
                        }
                        bit = bit << 1;
                    }

                    // ?????????
                    renderVertexFlagList[index] = flag;

                    // ?????
                    int si = r_minfo.sharedRenderMeshVertexStartIndex + i;
                    if ((flag & 0xffff0000) == 0)
                    {
                        // ?????
                        float3 pos = sharedRenderVertices[si];

                        renderPosList[index] = pos;
                        float3 nor = sharedRenderNormals[si];
                        renderNormalList[index] = nor;
                        renderTangentList[index] = sharedRenderTangents[si];
                    }

                    // ???????
                    if (sr_minfo.IsSkinning())
                    {
                        int svindex = sr_minfo.bonePerVertexChunk.startIndex + i;
                        int wstart = sharedBonesPerVertexStartList[svindex];
                        int windex = r_minfo.boneWeightsChunk.startIndex + wstart;
                        int swindex = sr_minfo.boneWeightsChunk.startIndex + wstart;
                        int renderBoneIndex = sr_minfo.rendererBoneIndex;

                        int cnt = sharedBonesPerVertexList[svindex];
                        if ((flag & 0xffff0000) == 0)
                        {
                            // ?????
                            for (int j = 0; j < cnt; j++)
                            {
                                renderBoneWeightList[windex + j] = sharedBoneWeightList[swindex + j];
                            }
                        }
                        else
                        {
                            // ????
                            for (int j = 0; j < cnt; j++)
                            {
                                BoneWeight1 bw = sharedBoneWeightList[swindex + j];
                                bw.boneIndex = renderBoneIndex;
                                renderBoneWeightList[windex + j] = bw;
                            }
                        }
                    }
                }

                // ??????
                r_minfo.SetFlag(updateFlag, false);
                renderMeshInfoList[rmindex] = r_minfo;
            }
        }


        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        public override JobHandle PreUpdate(JobHandle jobHandle)
        {
            // ????
            return jobHandle;
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// ?????????????????????????????????
        /// ??????????/??/?????????????????
        /// </summary>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        public override JobHandle PostUpdate(JobHandle jobHandle)
        {
            if (Manager.Mesh.renderMeshInfoList.Count == 0)
                return jobHandle;

            // ?????????????/??/?????????????????
            if (IsPerformMeshProcessForEachParticle())
            {
                // ????
                var job = new CollectLocalPositionNormalTangentForEachVertexJob()
                {
                    renderMeshInfoList = Manager.Mesh.renderMeshInfoList.ToJobArray(),

                    transformPosList = Manager.Bone.bonePosList.ToJobArray(),
                    transformRotList = Manager.Bone.boneRotList.ToJobArray(),
                    transformSclList = Manager.Bone.boneSclList.ToJobArray(),

                    sharedChildVertexInfoList = Manager.Mesh.sharedChildVertexInfoList.ToJobArray(),
                    sharedChildVertexWeightList = Manager.Mesh.sharedChildWeightList.ToJobArray(),

                    virtualPosList = Manager.Mesh.virtualPosList.ToJobArray(),
                    virtualRotList = Manager.Mesh.virtualRotList.ToJobArray(),

                    renderVertexFlagList = Manager.Mesh.renderVertexFlagList.ToJobArray(),

                    renderPosList = Manager.Mesh.renderPosList.ToJobArray(),
                    renderNormalList = Manager.Mesh.renderNormalList.ToJobArray(),
                    renderTangentList = Manager.Mesh.renderTangentList.ToJobArray(),
                };
                jobHandle = job.Schedule(Manager.Mesh.renderPosList.Length, 128, jobHandle);
            }
            else
            {
                //Debug.Log("Group!");
                // ??????????
                // ????????????????????
                var job = new CollectLocalPositionNormalTangentForEachMeshJob()
                {
                    renderMeshInfoList = Manager.Mesh.renderMeshInfoList.ToJobArray(),

                    transformPosList = Manager.Bone.bonePosList.ToJobArray(),
                    transformRotList = Manager.Bone.boneRotList.ToJobArray(),
                    transformSclList = Manager.Bone.boneSclList.ToJobArray(),

                    sharedChildVertexInfoList = Manager.Mesh.sharedChildVertexInfoList.ToJobArray(),
                    sharedChildVertexWeightList = Manager.Mesh.sharedChildWeightList.ToJobArray(),

                    virtualPosList = Manager.Mesh.virtualPosList.ToJobArray(),
                    virtualRotList = Manager.Mesh.virtualRotList.ToJobArray(),

                    renderVertexFlagList = Manager.Mesh.renderVertexFlagList.ToJobArray(),

                    renderPosList = Manager.Mesh.renderPosList.ToJobArray(),
                    renderNormalList = Manager.Mesh.renderNormalList.ToJobArray(),
                    renderTangentList = Manager.Mesh.renderTangentList.ToJobArray(),
                };
                jobHandle = job.Schedule(Manager.Mesh.renderMeshInfoList.Length, 1, jobHandle);
            }

            return jobHandle;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        [BurstCompile]
        private struct CollectLocalPositionNormalTangentForEachMeshJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.RenderMeshInfo> renderMeshInfoList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> transformPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> transformRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> transformSclList;

            [Unity.Collections.ReadOnly]
            public NativeArray<uint> sharedChildVertexInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<MeshData.VertexWeight> sharedChildVertexWeightList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> virtualPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> virtualRotList;

            [Unity.Collections.ReadOnly]
            public NativeArray<uint> renderVertexFlagList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> renderPosList;
            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> renderNormalList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float4> renderTangentList;

            // ??????????
            public void Execute(int rmindex)
            {
                var r_minfo = renderMeshInfoList[rmindex];
                if (r_minfo.IsUse() == false)
                    return;

                // ????????
                if (r_minfo.IsPause())
                    return;

                // ??????????????????
                int tindex = r_minfo.transformIndex;
                var tpos = transformPosList[tindex];
                var trot = transformRotList[tindex];
                var tscl = transformSclList[tindex];
                quaternion itrot = math.inverse(trot);

                //int vcnt = r_minfo.vertexChunk.dataLength;
                //int r_vstart = r_minfo.vertexChunk.startIndex;

                bool calcNormal = r_minfo.IsFlag(PhysicsManagerMeshData.Meshflag_CalcNormal);
                bool calcTangent = r_minfo.IsFlag(PhysicsManagerMeshData.Meshflag_CalcTangent);

                // ?????????
                float scaleRatio = r_minfo.baseScale > 0.0f ? math.length(tscl) / r_minfo.baseScale : 1.0f;
                float3 scaleDirection = math.sign(tscl);

                // ????
                for (int i = 0; i < r_minfo.vertexChunk.dataLength; i++)
                {
                    int vindex = r_minfo.vertexChunk.startIndex + i;
                    uint flag = renderVertexFlagList[vindex];

                    // ??????
                    if ((flag & 0xffff0000) == 0)
                    {
                        continue;
                    }

                    // ??????
                    CollectionVertex(
                        r_minfo,
                        sharedChildVertexInfoList,
                        sharedChildVertexWeightList,
                        virtualPosList,
                        virtualRotList,
                        tpos,
                        trot,
                        tscl,
                        itrot,
                        scaleRatio,
                        scaleDirection,
                        calcNormal,
                        calcTangent,
                        vindex,
                        i,
                        flag,
                        ref renderPosList,
                        ref renderNormalList,
                        ref renderTangentList
                        );
                }
            }
        }

        /// <summary>
        /// ???????
        /// </summary>
        [BurstCompile]
        private struct CollectLocalPositionNormalTangentForEachVertexJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerMeshData.RenderMeshInfo> renderMeshInfoList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> transformPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> transformRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> transformSclList;

            [Unity.Collections.ReadOnly]
            public NativeArray<uint> sharedChildVertexInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<MeshData.VertexWeight> sharedChildVertexWeightList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> virtualPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> virtualRotList;

            [Unity.Collections.ReadOnly]
            public NativeArray<uint> renderVertexFlagList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> renderPosList;
            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> renderNormalList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float4> renderTangentList;

            // ????
            public void Execute(int vindex)
            {
                uint flag = renderVertexFlagList[vindex];
                // ??????
                if ((flag & 0xffff0000) == 0)
                {
                    return;
                }

                // ??????????????
                int rmindex = DataUtility.Unpack16Low(flag) - 1; // (-1)??????!
                var r_minfo = renderMeshInfoList[rmindex];
                if (r_minfo.IsUse() == false)
                    return;

                // ????????
                if (r_minfo.IsPause())
                    return;

                bool calcNormal = r_minfo.IsFlag(PhysicsManagerMeshData.Meshflag_CalcNormal);
                bool calcTangent = r_minfo.IsFlag(PhysicsManagerMeshData.Meshflag_CalcTangent);

                // ??????????????????
                int tindex = r_minfo.transformIndex;
                var tpos = transformPosList[tindex];
                var trot = transformRotList[tindex];
                var tscl = transformSclList[tindex];
                quaternion itrot = math.inverse(trot);

                // ?????????
                float scaleRatio = r_minfo.baseScale > 0.0f ? math.length(tscl) / r_minfo.baseScale : 1.0f;
                float3 scaleDirection = math.sign(tscl);

                // ????????????
                int i = vindex - r_minfo.vertexChunk.startIndex;

                // ??????
                CollectionVertex(
                    r_minfo,
                    sharedChildVertexInfoList,
                    sharedChildVertexWeightList,
                    virtualPosList,
                    virtualRotList,
                    tpos,
                    trot,
                    tscl,
                    itrot,
                    scaleRatio,
                    scaleDirection,
                    calcNormal,
                    calcTangent,
                    vindex,
                    i,
                    flag,
                    ref renderPosList,
                    ref renderNormalList,
                    ref renderTangentList
                    );
            }
        }

        /// <summary>
        /// 1???????????????
        /// </summary>
        /// <param name="r_minfo"></param>
        /// <param name="sharedChildVertexInfoList"></param>
        /// <param name="sharedChildVertexWeightList"></param>
        /// <param name="virtualPosList"></param>
        /// <param name="virtualRotList"></param>
        /// <param name="tpos"></param>
        /// <param name="trot"></param>
        /// <param name="tscl"></param>
        /// <param name="itrot"></param>
        /// <param name="scaleRatio"></param>
        /// <param name="scaleDirection"></param>
        /// <param name="calcNormal"></param>
        /// <param name="calcTangent"></param>
        /// <param name="vindex"></param>
        /// <param name="i"></param>
        /// <param name="flag"></param>
        /// <param name="renderPosList"></param>
        /// <param name="renderNormalList"></param>
        /// <param name="renderTangentList"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CollectionVertex(
            in PhysicsManagerMeshData.RenderMeshInfo r_minfo,
            in NativeArray<uint> sharedChildVertexInfoList,
            in NativeArray<MeshData.VertexWeight> sharedChildVertexWeightList,
            in NativeArray<float3> virtualPosList,
            in NativeArray<quaternion> virtualRotList,
            in float3 tpos,
            in quaternion trot,
            in float3 tscl,
            in quaternion itrot,
            float scaleRatio,
            in float3 scaleDirection,
            bool calcNormal,
            bool calcTangent,
            int vindex,
            int i,
            uint flag,
            ref NativeArray<float3> renderPosList,
            ref NativeArray<float3> renderNormalList,
            ref NativeArray<float4> renderTangentList
            )
        {
            // ?????????????????????????????
            int4 data;
            float3 sum_pos = 0;
            float3 sum_nor = 0;
            float3 sum_tan = 0;
            float4 sum_tan4 = 0;
            sum_tan4.w = -1;
            int cnt = 0;
            uint bit = PhysicsManagerMeshData.RenderVertexFlag_Use;
            for (int l = 0; l < PhysicsManagerMeshData.MaxRenderMeshLinkCount; l++)
            {
                if (r_minfo.IsLinkMesh(l))
                {
                    // data.x = ????????????????????
                    // data.y = ?????????????????????
                    // data.z = ???????????????????
                    // data.w = ?????????????????????
                    data.x = r_minfo.childMeshVertexStartIndex[l];
                    data.y = r_minfo.childMeshWeightStartIndex[l];
                    data.z = r_minfo.virtualMeshVertexStartIndex[l];
                    data.w = r_minfo.sharedVirtualMeshVertexStartIndex[l];

                    if ((flag & bit) == 0)
                    {
                        bit = bit << 1;
                        continue;
                    }

                    float3 pos = 0;
                    float3 nor = 0;
                    float3 tan = 0;

                    int sc_vindex = data.x + i;
                    int sc_wstart = data.y;
                    int m_vstart = data.z;

                    // ?????
                    uint pack = sharedChildVertexInfoList[sc_vindex];
                    int wcnt = DataUtility.Unpack4_28Hi(pack);
                    int wstart = DataUtility.Unpack4_28Low(pack);

                    if (calcTangent)
                    {
                        for (int j = 0; j < wcnt; j++)
                        {
                            var vw = sharedChildVertexWeightList[sc_wstart + wstart + j];

                            // ????0??????
                            var mpos = virtualPosList[m_vstart + vw.parentIndex];
                            var mrot = virtualRotList[m_vstart + vw.parentIndex];

                            // position
                            //pos += (mpos + math.mul(mrot, vw.localPos * renderScale)) * vw.weight;
                            pos += (mpos + math.mul(mrot, vw.localPos * scaleDirection * scaleRatio)) * vw.weight;

                            // normal
                            //nor += math.mul(mrot, vw.localNor) * vw.weight;
                            nor += math.mul(mrot, vw.localNor * scaleDirection) * vw.weight;

                            // tangent
                            //tan += math.mul(mrot, vw.localTan) * vw.weight;
                            tan += math.mul(mrot, vw.localTan * scaleDirection) * vw.weight;
                        }

                        // ??????????????????
                        pos = math.mul(itrot, (pos - tpos)) / tscl;
                        nor = math.mul(itrot, nor);
                        tan = math.mul(itrot, tan);

                        // ??????????
                        nor *= scaleDirection;
                        tan *= scaleDirection;

                        sum_pos += pos;
                        sum_nor += nor;
                        sum_tan += tan;
                    }
                    else if (calcNormal)
                    {
                        for (int j = 0; j < wcnt; j++)
                        {
                            var vw = sharedChildVertexWeightList[sc_wstart + wstart + j];

                            // ????0??????
                            var mpos = virtualPosList[m_vstart + vw.parentIndex];
                            var mrot = virtualRotList[m_vstart + vw.parentIndex];

                            // position
                            //pos += (mpos + math.mul(mrot, vw.localPos * renderScale)) * vw.weight;
                            pos += (mpos + math.mul(mrot, vw.localPos * scaleDirection * scaleRatio)) * vw.weight;

                            // normal
                            //nor += math.mul(mrot, vw.localNor) * vw.weight;
                            nor += math.mul(mrot, vw.localNor * scaleDirection) * vw.weight;
                        }

                        // ??????????????????
                        pos = math.mul(itrot, (pos - tpos)) / tscl;
                        nor = math.mul(itrot, nor);

                        // ??????????
                        nor *= scaleDirection;

                        sum_pos += pos;
                        sum_nor += nor;
                    }
                    else
                    {
                        for (int j = 0; j < wcnt; j++)
                        {
                            var vw = sharedChildVertexWeightList[sc_wstart + wstart + j];

                            // ????0??????
                            var mpos = virtualPosList[m_vstart + vw.parentIndex];
                            var mrot = virtualRotList[m_vstart + vw.parentIndex];

                            // position
                            //pos += (mpos + math.mul(mrot, vw.localPos * renderScale)) * vw.weight;
                            pos += (mpos + math.mul(mrot, vw.localPos * scaleDirection * scaleRatio)) * vw.weight;
                        }

                        // ??????????????????
                        pos = math.mul(itrot, (pos - tpos)) / tscl;

                        sum_pos += pos;
                    }
                    cnt++;
                }
                bit = bit << 1;
            }
            if (cnt > 0)
            {
                float3 fpos = sum_pos / cnt;
                renderPosList[vindex] = fpos;

                float3 fnor = 0;

                if (calcTangent)
                {
                    fnor = sum_nor / cnt; ;
                    renderNormalList[vindex] = fnor;
                    sum_tan4.xyz = sum_tan / cnt;
                    renderTangentList[vindex] = sum_tan4;
                }
                else if (calcNormal)
                {
                    fnor = sum_nor / cnt; ;
                    renderNormalList[vindex] = fnor;
                }
            }
        }
    }
}
