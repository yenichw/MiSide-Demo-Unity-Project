// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_2020_1_OR_NEWER
using UnityEngine.Rendering;
#endif

namespace MagicaCloth
{
    /// <summary>
    /// ???????
    /// </summary>
    public class PhysicsManagerMeshData : PhysicsManagerAccess
    {
        //=========================================================================================
        /// <summary>
        /// ?????????
        /// </summary>
        public const uint MeshFlag_Active = 0x00000001;
        public const uint MeshFlag_Skinning = 0x00000004;
        public const uint Meshflag_CalcNormal = 0x00000008;
        public const uint Meshflag_CalcTangent = 0x00000010;
        public const uint Meshflag_Pause = 0x00000020; // ????
        // ?????VirtualMeshInfo?
        // ?????SharedRenderMeshInfo?
        public const uint MeshFlag_ExistNormals = 0x00010000;   // ????
        public const uint MeshFlag_ExistTangents = 0x00020000;  // ????
        public const uint MeshFlag_ExistWeights = 0x00040000;   // ??????
        // ?????RenderMeshInfo?
        public const uint MeshFlag_UpdateUseVertexFront = 0x01000000;   // ???????????????????
        public const uint MeshFlag_UpdateUseVertexBack = 0x02000000;   // ???????????????????
        public const uint MeshFlag_MeshLink = 0x10000000; // ????????????(28bit - 31bit)

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        public struct SharedVirtualMeshInfo
        {
            public int uid;

            // ??????
            public int useCount;

            public int sharedChildMeshStartIndex;
            public int sharedChildMeshCount;

            // ???????(sharedVirtualUvList/sharedVirtualVertexInfoList)
            public ChunkData vertexChunk;

            // ????????????(sharedVirtualWeightList)
            public ChunkData weightChunk;

            // ????????????(sharedVirtualTriangleList)
            public ChunkData triangleChunk;

            // ?????????????????(sharedVirtualVertexToTriangleInfoList)
            public ChunkData vertexToTriangleChunk;
        }
        public FixedNativeList<SharedVirtualMeshInfo> sharedVirtualMeshInfoList;
        public Dictionary<int, int> sharedVirtualMeshIdToIndexDict = new Dictionary<int, int>(); // ????

        /// <summary>
        /// ?????UV
        /// </summary>
        public FixedChunkNativeArray<float2> sharedVirtualUvList;

        /// <summary>
        /// ?????????????????????????
        /// ??4bit = ?????
        /// ??28bit = ????????????????
        /// </summary>
        public FixedChunkNativeArray<uint> sharedVirtualVertexInfoList;

        /// <summary>
        /// ?????????
        /// </summary>
        public FixedChunkNativeArray<MeshData.VertexWeight> sharedVirtualWeightList;

        /// <summary>
        /// ????????????(??????????3??????????)
        /// </summary>
        public FixedChunkNativeArray<int> sharedVirtualTriangleList;

        /// <summary>
        /// ???????????????????????????
        /// ??8bit = ??????????
        /// ??24bit = ????????????(sharedVirtualVertexToTriangleIndexList)?????????
        /// </summary>
        public FixedChunkNativeArray<uint> sharedVirtualVertexToTriangleInfoList;

        /// <summary>
        /// ???????????????????????
        /// </summary>
        public FixedChunkNativeArray<int> sharedVirtualVertexToTriangleIndexList;

        //=========================================================================================
        /// <summary>
        /// ???????????
        /// </summary>
        public const byte VirtualVertexFlag_Use = 0x01; // ????

        /// <summary>
        /// ??????????????
        /// </summary>
        public struct VirtualMeshInfo
        {
            public uint flag;
            public int sharedVirtualMeshIndex;
            public int meshUseCount; // ??????
            public int vertexUseCount; // ?????

            // ???????(useVertexList/posList/normalList/tangentList)
            public ChunkData vertexChunk;

            // ????????(transformIndexList)
            public ChunkData boneChunk;

            // ????????????(triangleNormalList)
            public ChunkData triangleChunk;

            // ??????????????
            public int transformIndex;

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsFlag(uint flag)
            {
                return (this.flag & flag) != 0;
            }

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <param name="sw"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetFlag(uint flag, bool sw)
            {
                if (sw)
                    this.flag |= flag;
                else
                    this.flag &= ~flag;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsActive()
            {
                return IsFlag(MeshFlag_Active);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsUse()
            {
                return IsFlag(MeshFlag_Active) && meshUseCount > 0 && vertexUseCount > 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsPause()
            {
                return IsFlag(Meshflag_Pause);
            }
        }
        public FixedNativeList<VirtualMeshInfo> virtualMeshInfoList;

        /// <summary>
        /// ????
        /// ??16bit = ?????????????????????????
        ///             ??????????+1???????!(0=?????)
        /// ??16bit = ?????(????1??????????)
        /// </summary>
        //public FixedChunkNativeArray<uint> virtualVertexInfoList;

        /// <summary>
        /// ?????????????????????????
        /// ??????????+1???????!(0=?????)
        /// </summary>
        public FixedChunkNativeArray<short> virtualVertexMeshIndexList;

        /// <summary>
        /// ??????(????1??????????)
        /// </summary>
        public FixedChunkNativeArray<byte> virtualVertexUseList;

        /// <summary>
        /// ????????????(????1???????????)
        /// </summary>
        public FixedChunkNativeArray<byte> virtualVertexFixList;

        /// <summary>
        /// ?????
        /// (VirtualVertexFlag_Use~)
        /// </summary>
        public FixedChunkNativeArray<byte> virtualVertexFlagList;

        /// <summary>
        /// ??????????????
        /// </summary>
        public FixedChunkNativeArray<float3> virtualPosList;
        public FixedChunkNativeArray<quaternion> virtualRotList;

        /// <summary>
        /// ??????????
        /// </summary>
        public FixedChunkNativeArray<int> virtualTransformIndexList;

        /// <summary>
        /// ??????????????
        /// </summary>
        public FixedChunkNativeArray<float3> virtualTriangleNormalList;
        public FixedChunkNativeArray<float3> virtualTriangleTangentList;

        /// <summary>
        /// ??????????????????????????????
        /// ??????????+1???????!(0=?????)
        /// </summary>
        public FixedChunkNativeArray<ushort> virtualTriangleMeshIndexList;

        //=========================================================================================
        /// <summary>
        /// ????????????????
        /// </summary>
        public struct SharedChildMeshInfo
        {
            public long cuid;

            public int sharedVirtualMeshIndex;
            public int virtualMeshIndex;
            public int meshUseCount; // ??????

            // ???????(?????????1:1???)
            public ChunkData vertexChunk;

            public ChunkData weightChunk;
        }
        public FixedNativeList<SharedChildMeshInfo> sharedChildMeshInfoList;
        public Dictionary<long, int> sharedChildMeshIdToSharedVirtualMeshIndexDict = new Dictionary<long, int>(); // ?????

        /// <summary>
        /// ?????????????????????????
        /// ??4bit = ?????
        /// ??28bit = ????????????????
        /// </summary>
        public FixedChunkNativeArray<uint> sharedChildVertexInfoList;

        /// <summary>
        /// ?????????
        /// </summary>
        public FixedChunkNativeArray<MeshData.VertexWeight> sharedChildWeightList;

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        public struct SharedRenderMeshInfo
        {
            public int uid;

            // ??????
            public int useCount;

            public uint flag;

            // ???????(vertices/normals/tangents/)
            public ChunkData vertexChunk;

            // ??????????
            public ChunkData bonePerVertexChunk;    // (sharedBonesPerVertexList/sharedBonesPerVertexStartList)
            public ChunkData boneWeightsChunk;      // (sharedBoneWeightsList)

            // ???????????????????????
            public int rendererBoneIndex;

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsFlag(uint flag)
            {
                return (this.flag & flag) != 0;
            }

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <param name="sw"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetFlag(uint flag, bool sw)
            {
                if (sw)
                    this.flag |= flag;
                else
                    this.flag &= ~flag;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsSkinning()
            {
                return IsFlag(MeshFlag_Skinning);
            }
        }
        public FixedNativeList<SharedRenderMeshInfo> sharedRenderMeshInfoList;
        public Dictionary<int, int> sharedRenderMeshIdToIndexDict = new Dictionary<int, int>(); // ????

        public FixedChunkNativeArray<float3> sharedRenderVertices;

        // ??/??
        public FixedChunkNativeArray<float3> sharedRenderNormals;
        public FixedChunkNativeArray<float4> sharedRenderTangents; // ???????????w????

        // ???????(??????????????)
        public FixedChunkNativeArray<byte> sharedBonesPerVertexList;
        public FixedChunkNativeArray<int> sharedBonesPerVertexStartList;
        public FixedChunkNativeArray<BoneWeight1> sharedBoneWeightList;

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        public const uint RenderVertexFlag_Use = 0x00010000; // ?????(????16,17,18,19bit?????)

        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        public const int MaxRenderMeshLinkCount = 4;

        /// <summary>
        /// ????????????????
        /// </summary>
        public struct RenderMeshInfo
        {
            public uint flag;

            public int renderSharedMeshIndex;
            public int sharedRenderMeshVertexStartIndex;

            public int meshUseCount; // ??????

            // ????????(??4):?????? flag ? 28 - 31bit
            public int4 childMeshVertexStartIndex;          // ????????????????????
            public int4 childMeshWeightStartIndex;          // ?????????????????????
            public int4 virtualMeshVertexStartIndex;        // ???????????????????
            public int4 sharedVirtualMeshVertexStartIndex;  // ?????????????????????
            public int4 linkMeshCount;

            // ???????(posList/normalList/tangentList)
            public ChunkData vertexChunk;

            // ??????????(boneWeights)
            public ChunkData boneWeightsChunk;

            // ????????????????
            public int transformIndex;

            // ???????
            public float baseScale;             // ????????

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsFlag(uint flag)
            {
                return (this.flag & flag) != 0;
            }

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <param name="sw"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetFlag(uint flag, bool sw)
            {
                if (sw)
                    this.flag |= flag;
                else
                    this.flag &= ~flag;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsActive()
            {
                return IsFlag(MeshFlag_Active);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsUse()
            {
                return IsFlag(MeshFlag_Active) && meshUseCount > 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsLinkMesh(int index)
            {
                return (flag & (MeshFlag_MeshLink << index)) != 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsPause()
            {
                return IsFlag(Meshflag_Pause);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsSkinning()
            {
                return IsFlag(MeshFlag_Skinning);
            }

            /// <summary>
            /// ???????????
            /// </summary>
            /// <param name="childMeshVertexStart"></param>
            /// <param name="childMeshWeightStart"></param>
            /// <param name="virtualMeshVertexStart"></param>
            /// <param name="sharedVirtualMeshVertexStart"></param>
            /// <returns></returns>
            public bool AddLinkMesh(int renderMeshIndex, int childMeshVertexStart, int childMeshWeightStart, int virtualMeshVertexStart, int sharedVirtualMeshVertexStart)
            {
                //Develop.Log($"AddLInkMesh[{renderMeshIndex}] (childMeshVertexStart:{childMeshVertexStart},childMeshWeightStart:{childMeshWeightStart},virtualMeshVertexStart:{virtualMeshVertexStart},sharedVirtualMeshVertexStart:{sharedVirtualMeshVertexStart}");

                for (int i = 0; i < MaxRenderMeshLinkCount; i++)
                {
                    if (IsLinkMesh(i) && childMeshVertexStartIndex[i] == childMeshVertexStart && virtualMeshVertexStartIndex[i] == virtualMeshVertexStart)
                    {
                        // ?????????
                        linkMeshCount[i]++;
                        SetFlag(MeshFlag_MeshLink << i, true);
                        return true;
                    }
                }

                for (int i = 0; i < MaxRenderMeshLinkCount; i++)
                {
                    if (IsLinkMesh(i) == false)
                    {
                        childMeshVertexStartIndex[i] = childMeshVertexStart;
                        childMeshWeightStartIndex[i] = childMeshWeightStart;
                        virtualMeshVertexStartIndex[i] = virtualMeshVertexStart;
                        sharedVirtualMeshVertexStartIndex[i] = sharedVirtualMeshVertexStart;
                        linkMeshCount[i] = 1;
                        SetFlag(MeshFlag_MeshLink << i, true);
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// ???????????
            /// </summary>
            /// <param name="childMeshVertexStart"></param>
            /// <param name="childMeshWeightStart"></param>
            /// <param name="virtualMeshVertexStart"></param>
            /// <param name="sharedVirtualMeshVertexStart"></param>
            /// <returns></returns>
            public bool RemoveLinkMesh(int renderMeshIndex, int childMeshVertexStart, int childMeshWeightStart, int virtualMeshVertexStart, int sharedVirtualMeshVertexStart)
            {
                //Develop.Log($"RemoveLinkMesh[{renderMeshIndex}] (childMeshVertexStart:{childMeshVertexStart},childMeshWeightStart:{childMeshWeightStart},virtualMeshVertexStart:{virtualMeshVertexStart},sharedVirtualMeshVertexStart:{sharedVirtualMeshVertexStart}");

                for (int i = 0; i < MaxRenderMeshLinkCount; i++)
                {
                    if (IsLinkMesh(i) && childMeshVertexStartIndex[i] == childMeshVertexStart && virtualMeshVertexStartIndex[i] == virtualMeshVertexStart)
                    {
                        linkMeshCount[i]--;
                        if (linkMeshCount[i] == 0)
                        {
                            childMeshVertexStartIndex[i] = 0;
                            childMeshWeightStartIndex[i] = 0;
                            virtualMeshVertexStartIndex[i] = 0;
                            sharedVirtualMeshVertexStartIndex[i] = 0;
                            SetFlag(MeshFlag_MeshLink << i, false);
                        }
                        return true;
                    }
                }
                return false;
            }
        }
        public FixedNativeList<RenderMeshInfo> renderMeshInfoList;

        /// <summary>
        /// ??????????????????????
        /// </summary>
        public const uint RenderStateFlag_Use = 0x00000001;
        public const uint RenderStateFlag_ExistNormal = 0x00000002;
        public const uint RenderStateFlag_ExistTangent = 0x00000004;
        public const uint RenderStateFlag_DelayedCalculated = 0x00000100; // ?????????????

        /// <summary>
        /// ???????????????????
        /// </summary>
        public class RenderMeshState
        {
            /// <summary>
            /// ?????(RenderStateFlag_Use~)
            /// </summary>
            public uint flag;

            public int RenderSharedMeshIndex;
            public int RenderSharedMeshId;
            public int VertexChunkStart;
            public int VertexChunkLength;
            public int BoneWeightChunkStart;
            public int BoneWeightChunkLength;

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <returns></returns>
            public bool IsFlag(uint flag)
            {
                return (this.flag & flag) != 0;
            }

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="flag"></param>
            /// <param name="sw"></param>
            public void SetFlag(uint flag, bool sw)
            {
                if (sw)
                    this.flag |= flag;
                else
                    this.flag &= ~flag;
            }
        }

        /// <summary>
        /// ??:??????????????ID
        /// </summary>
        public Dictionary<int, RenderMeshState> renderMeshStateDict = new Dictionary<int, RenderMeshState>();

        /// <summary>
        /// ????????
        /// ??16bit = ???(RenderFlag_Use~)
        /// ??16bit = ???????????????????????????
        ///             ??????????+1???????!(0=?????)
        /// </summary>
        public FixedChunkNativeArray<uint> renderVertexFlagList;

        /// <summary>
        /// ??????????
        /// ???????????????????????????????
        /// </summary>
        public FixedChunkNativeArray<float3> renderPosList;
        public FixedChunkNativeArray<float3> renderNormalList;
        public FixedChunkNativeArray<float4> renderTangentList; // ??????????????w????

        /// <summary>
        /// ?????????????
        /// </summary>
        public FixedChunkNativeArray<BoneWeight1> renderBoneWeightList;


        //=========================================================================================
        /// <summary>
        /// ????????????
        /// </summary>
        HashSet<BaseMeshDeformer> renderMeshSet = new HashSet<BaseMeshDeformer>();

        // ?????????
        List<RenderMeshDeformer> normalWriteList = new List<RenderMeshDeformer>();

        //=========================================================================================
        /// <summary>
        /// ????
        /// </summary>
        public override void Create()
        {
            // shared virtual mesh
            sharedVirtualMeshInfoList = new FixedNativeList<SharedVirtualMeshInfo>();
            sharedVirtualVertexInfoList = new FixedChunkNativeArray<uint>();
            sharedVirtualWeightList = new FixedChunkNativeArray<MeshData.VertexWeight>();
            sharedVirtualUvList = new FixedChunkNativeArray<float2>();
            sharedVirtualTriangleList = new FixedChunkNativeArray<int>();
            sharedVirtualVertexToTriangleInfoList = new FixedChunkNativeArray<uint>();
            sharedVirtualVertexToTriangleIndexList = new FixedChunkNativeArray<int>();

            // virtual mesh
            virtualMeshInfoList = new FixedNativeList<VirtualMeshInfo>();
            //virtualVertexInfoList = new FixedChunkNativeArray<uint>();
            virtualVertexMeshIndexList = new FixedChunkNativeArray<short>();
            virtualVertexUseList = new FixedChunkNativeArray<byte>();
            virtualVertexFixList = new FixedChunkNativeArray<byte>();
            virtualVertexFlagList = new FixedChunkNativeArray<byte>();
            virtualPosList = new FixedChunkNativeArray<float3>();
            virtualRotList = new FixedChunkNativeArray<quaternion>();
            virtualTransformIndexList = new FixedChunkNativeArray<int>();
            virtualTriangleNormalList = new FixedChunkNativeArray<float3>();
            virtualTriangleTangentList = new FixedChunkNativeArray<float3>();
            virtualTriangleMeshIndexList = new FixedChunkNativeArray<ushort>();

            // shared virtual child mesh
            sharedChildMeshInfoList = new FixedNativeList<SharedChildMeshInfo>();
            sharedChildVertexInfoList = new FixedChunkNativeArray<uint>();
            sharedChildWeightList = new FixedChunkNativeArray<MeshData.VertexWeight>();

            // shared render mesh
            sharedRenderMeshInfoList = new FixedNativeList<SharedRenderMeshInfo>();
            sharedRenderVertices = new FixedChunkNativeArray<float3>();
            sharedRenderNormals = new FixedChunkNativeArray<float3>();
            sharedRenderTangents = new FixedChunkNativeArray<float4>();
            sharedBonesPerVertexList = new FixedChunkNativeArray<byte>();
            sharedBonesPerVertexStartList = new FixedChunkNativeArray<int>();
            sharedBoneWeightList = new FixedChunkNativeArray<BoneWeight1>();

            // render mesh
            renderMeshInfoList = new FixedNativeList<RenderMeshInfo>();
            renderVertexFlagList = new FixedChunkNativeArray<uint>();
            renderPosList = new FixedChunkNativeArray<float3>();
            renderNormalList = new FixedChunkNativeArray<float3>();
            renderTangentList = new FixedChunkNativeArray<float4>();
            renderBoneWeightList = new FixedChunkNativeArray<BoneWeight1>();
        }

        /// <summary>
        /// ??
        /// </summary>
        public override void Dispose()
        {
            if (sharedVirtualMeshInfoList == null)
                return;

            // shared virtual mesh
            sharedVirtualMeshInfoList.Dispose();
            sharedVirtualVertexInfoList.Dispose();
            sharedVirtualWeightList.Dispose();
            sharedVirtualUvList.Dispose();
            sharedVirtualTriangleList.Dispose();
            sharedVirtualVertexToTriangleInfoList.Dispose();
            sharedVirtualVertexToTriangleIndexList.Dispose();

            // virtual mesh
            virtualMeshInfoList.Dispose();
            //virtualVertexInfoList.Dispose();
            virtualVertexMeshIndexList.Dispose();
            virtualVertexUseList.Dispose();
            virtualVertexFixList.Dispose();
            virtualVertexFlagList.Dispose();
            virtualPosList.Dispose();
            virtualRotList.Dispose();
            virtualTransformIndexList.Dispose();
            virtualTriangleNormalList.Dispose();
            virtualTriangleTangentList.Dispose();
            virtualTriangleMeshIndexList.Dispose();

            // shared virtual child mesh
            sharedChildMeshInfoList.Dispose();
            sharedChildVertexInfoList.Dispose();
            sharedChildWeightList.Dispose();

            // shared render mesh
            sharedRenderMeshInfoList.Dispose();
            sharedRenderVertices.Dispose();
            sharedRenderNormals.Dispose();
            sharedRenderTangents.Dispose();
            sharedBonesPerVertexList.Dispose();
            sharedBonesPerVertexStartList.Dispose();
            sharedBoneWeightList.Dispose();

            // render mesh
            renderMeshInfoList.Dispose();
            renderVertexFlagList.Dispose();
            renderPosList.Dispose();
            renderNormalList.Dispose();
            renderTangentList.Dispose();
            renderBoneWeightList.Dispose();
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="bmesh"></param>
        public void AddMesh(BaseMeshDeformer bmesh)
        {
            if (bmesh is RenderMeshDeformer)
                renderMeshSet.Add(bmesh);
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="bmesh"></param>
        public void RemoveMesh(BaseMeshDeformer bmesh)
        {
            if (renderMeshSet.Contains(bmesh))
                renderMeshSet.Remove(bmesh);
        }

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// ??????????????????????
        /// </summary>
        /// <param name="id">????????ID</param>
        /// <param name="vertexCount"></param>
        /// <param name="boneCount"></param>
        /// <param name="triangleCount"></param>
        /// <returns></returns>
        public int AddVirtualMesh(
            int uid,
            int vertexCount,
            int weightCount,
            int boneCount,
            int triangleCount,
            int vertexToTriangleIndexCount,
            Transform transform
            )
        {
            //Develop.Log($"AddVirtualMesh uid:{uid} vcnt:{vertexCount}");
            // ????????????
            int sharedMeshIndex = -1;
            if (uid != 0)
            {
                if (sharedVirtualMeshIdToIndexDict.ContainsKey(uid))
                {
                    // ??
                    sharedMeshIndex = sharedVirtualMeshIdToIndexDict[uid];
                    var sminfo = sharedVirtualMeshInfoList[sharedMeshIndex];
                    sminfo.useCount++; // ??????+
                    sharedVirtualMeshInfoList[sharedMeshIndex] = sminfo;
                }
                else
                {
                    // ??
                    var sminfo = new SharedVirtualMeshInfo();
                    sminfo.uid = uid;
                    sminfo.useCount = 1;

                    // vertices
                    var oc = sharedVirtualVertexInfoList.AddChunk(vertexCount);
                    sharedVirtualUvList.AddChunk(vertexCount);
                    sharedVirtualVertexToTriangleInfoList.AddChunk(vertexCount);
                    sminfo.vertexChunk = oc;

                    //Develop.Log($"SharedVirtualMeshInfo vchunk start:{oc.startIndex} cnt:{oc.dataLength}");

                    // weight
                    oc = sharedVirtualWeightList.AddChunk(weightCount);
                    sminfo.weightChunk = oc;

                    // triangles
                    if (triangleCount > 0)
                    {
                        oc = sharedVirtualTriangleList.AddChunk(triangleCount * 3);
                        sminfo.triangleChunk = oc;
                    }

                    // vertexToTriangleIndex
                    if (vertexToTriangleIndexCount > 0)
                    {
                        oc = sharedVirtualVertexToTriangleIndexList.AddChunk(vertexToTriangleIndexCount);
                        sminfo.vertexToTriangleChunk = oc;
                    }

                    sharedMeshIndex = sharedVirtualMeshInfoList.Add(sminfo);
                    sharedVirtualMeshIdToIndexDict.Add(uid, sharedMeshIndex);
                }
            }

            // ??????????????
            var minfo = new VirtualMeshInfo();
            //minfo.SetFlag(MeshFlag_Active, true);
            minfo.sharedVirtualMeshIndex = sharedMeshIndex;

            //var c = virtualVertexInfoList.AddChunk(vertexCount);
            var c = virtualVertexUseList.AddChunk(vertexCount);
            virtualVertexMeshIndexList.AddChunk(vertexCount);
            virtualVertexFixList.AddChunk(vertexCount);
            virtualVertexFlagList.AddChunk(vertexCount);
            virtualPosList.AddChunk(vertexCount);
            virtualRotList.AddChunk(vertexCount);
            minfo.vertexChunk = c;

            //Develop.Log($"VirtualMeshInfo vchunk start:{c.startIndex} cnt:{c.dataLength}");

            Debug.Assert(boneCount > 0);
            //c = virtualTransformIndexList.AddChunk(boneCount);
            c = new ChunkData(); // ???????(v1.9.4)
            minfo.boneChunk = c;

            if (triangleCount > 0)
            {
                c = virtualTriangleNormalList.AddChunk(triangleCount);
                virtualTriangleTangentList.AddChunk(triangleCount);
                virtualTriangleMeshIndexList.AddChunk(triangleCount);
                minfo.triangleChunk = c;
            }

            // ?????????
            minfo.transformIndex = Bone.AddBone(transform);

            int index = virtualMeshInfoList.Add(minfo);

            // ??/??????????????????????ID?????
            // (+1)?????????!
            virtualVertexMeshIndexList.Fill(minfo.vertexChunk, (short)(index + 1)); // (+1)??????

            // ??????????????????????ID?????
            if (triangleCount > 0)
                virtualTriangleMeshIndexList.Fill(minfo.triangleChunk, (ushort)(index + 1));

            return index;
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public bool IsEmptySharedVirtualMesh(int uid)
        {
            return sharedVirtualMeshIdToIndexDict.ContainsKey(uid) == false;
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="sharedVertices"></param>
        /// <param name="sharedNormals"></param>
        /// <param name="sharedTangents"></param>
        /// <param name="sharedBoneWeights"></param>
        public void SetSharedVirtualMeshData(
            int virtualMeshIndex,
            uint[] sharedVertexInfoList,
            MeshData.VertexWeight[] sharedWeightList,
            Vector2[] sharedUv,
            int[] sharedTriangles,
            uint[] vertexToTriangleInfoList,
            int[] vertexToTriangleIndexList
            )
        {
            var minfo = virtualMeshInfoList[virtualMeshIndex];
            Debug.Assert(minfo.sharedVirtualMeshIndex >= 0);
            var smdata = sharedVirtualMeshInfoList[minfo.sharedVirtualMeshIndex];

            // ???????????????????
            if (smdata.useCount == 1)
            {
                sharedVirtualVertexInfoList.ToJobArray().CopyFromFast(smdata.vertexChunk.startIndex, sharedVertexInfoList);
                sharedVirtualWeightList.ToJobArray().CopyFromFast(smdata.weightChunk.startIndex, sharedWeightList);

                if (sharedUv != null && sharedUv.Length > 0)
                    sharedVirtualUvList.ToJobArray().CopyFromFast(smdata.vertexChunk.startIndex, sharedUv);

                if (vertexToTriangleInfoList != null && vertexToTriangleInfoList.Length > 0)
                    sharedVirtualVertexToTriangleInfoList.ToJobArray().CopyFromFast(smdata.vertexChunk.startIndex, vertexToTriangleInfoList);

                if (vertexToTriangleIndexList != null && vertexToTriangleIndexList.Length > 0)
                    sharedVirtualVertexToTriangleIndexList.ToJobArray().CopyFromFast(smdata.vertexToTriangleChunk.startIndex, vertexToTriangleIndexList);

                if (sharedTriangles != null && sharedTriangles.Length > 0)
                    sharedVirtualTriangleList.ToJobArray().CopyFromFast(smdata.triangleChunk.startIndex, sharedTriangles);
            }
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        public void RemoveVirtualMesh(int virtualMeshIndex)
        {
            if (virtualMeshIndex < 0)
                return;
            if (virtualMeshInfoList.Exists(virtualMeshIndex))
            {
                var minfo = virtualMeshInfoList[virtualMeshIndex];

                // ????????
                int sharedMeshIndex = minfo.sharedVirtualMeshIndex;
                if (sharedMeshIndex >= 0)
                {
                    var sminfo = sharedVirtualMeshInfoList[sharedMeshIndex];
                    sminfo.useCount--; // ??????
                    if (sminfo.useCount == 0)
                    {
                        // ??
                        sharedVirtualVertexInfoList.RemoveChunk(sminfo.vertexChunk.chunkNo);
                        sharedVirtualWeightList.RemoveChunk(sminfo.weightChunk.chunkNo);
                        sharedVirtualUvList.RemoveChunk(sminfo.vertexChunk.chunkNo);
                        sharedVirtualVertexToTriangleInfoList.RemoveChunk(sminfo.vertexChunk.chunkNo);

                        if (sminfo.triangleChunk.dataLength > 0)
                        {
                            sharedVirtualTriangleList.RemoveChunk(sminfo.triangleChunk.chunkNo);
                        }
                        if (sminfo.vertexToTriangleChunk.dataLength > 0)
                        {
                            sharedVirtualVertexToTriangleIndexList.RemoveChunk(sminfo.vertexToTriangleChunk.chunkNo);
                        }
                        sharedVirtualMeshInfoList.Remove(sharedMeshIndex);
                        sharedVirtualMeshIdToIndexDict.Remove(sminfo.uid);
                    }
                    else
                    {
                        sharedVirtualMeshInfoList[sharedMeshIndex] = sminfo;
                    }
                }

                // ????????????
                //virtualVertexInfoList.RemoveChunk(minfo.vertexChunk.chunkNo);
                virtualVertexMeshIndexList.RemoveChunk(minfo.vertexChunk.chunkNo);
                virtualVertexUseList.RemoveChunk(minfo.vertexChunk.chunkNo);
                virtualVertexFixList.RemoveChunk(minfo.vertexChunk.chunkNo);
                virtualVertexFlagList.RemoveChunk(minfo.vertexChunk.chunkNo);
                virtualPosList.RemoveChunk(minfo.vertexChunk.chunkNo);
                virtualRotList.RemoveChunk(minfo.vertexChunk.chunkNo);

                virtualTransformIndexList.RemoveChunk(minfo.boneChunk.chunkNo);

                if (minfo.triangleChunk.dataLength > 0)
                {
                    virtualTriangleNormalList.RemoveChunk(minfo.triangleChunk.chunkNo);
                    virtualTriangleTangentList.RemoveChunk(minfo.triangleChunk.chunkNo);
                    virtualTriangleMeshIndexList.RemoveChunk(minfo.triangleChunk.chunkNo);
                }

                // ??????????????
                Bone.RemoveBone(minfo.transformIndex);
                minfo.transformIndex = 0;

                //Debug.Log("Remove Mesh Chunk:" + meshChunkIndex);
                virtualMeshInfoList.Remove(virtualMeshIndex);
            }
        }

        public bool ExistsVirtualMesh(int virtualMeshIndex)
        {
            return virtualMeshInfoList.Exists(virtualMeshIndex);
        }

        public VirtualMeshInfo GetVirtualMeshInfo(int virtualMeshIndex)
        {
            return virtualMeshInfoList[virtualMeshIndex];
        }

        /// <summary>
        /// ????????????????1????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <returns></returns>
        public bool IsUseVirtualMesh(int virtualMeshIndex)
        {
            return virtualMeshInfoList[virtualMeshIndex].IsUse();
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <returns></returns>
        public bool IsActiveVirtualMesh(int virtualMeshIndex)
        {
            return virtualMeshInfoList[virtualMeshIndex].IsActive();
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="sw"></param>
        public void SetVirtualMeshActive(int virtualMeshIndex, bool sw)
        {
            if (virtualMeshInfoList.Exists(virtualMeshIndex))
            {
                var minfo = virtualMeshInfoList[virtualMeshIndex];
                minfo.SetFlag(MeshFlag_Active, sw);
                virtualMeshInfoList[virtualMeshIndex] = minfo;
            }
        }

        public void AddUseVirtualMesh(int virtualMeshIndex)
        {
            if (virtualMeshInfoList.Exists(virtualMeshIndex))
            {
                var minfo = virtualMeshInfoList[virtualMeshIndex];
                minfo.meshUseCount++;
                virtualMeshInfoList[virtualMeshIndex] = minfo;
            }
        }

        public void RemoveUseVirtualMesh(int virtualMeshIndex)
        {
            if (virtualMeshInfoList.Exists(virtualMeshIndex))
            {
                var minfo = virtualMeshInfoList[virtualMeshIndex];
                minfo.meshUseCount--;
                Debug.Assert(minfo.meshUseCount >= 0);
                virtualMeshInfoList[virtualMeshIndex] = minfo;
            }
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="vindex"></param>
        /// <param name="fix">???????true</param>
        /// <returns>???????true???</returns>
        public bool AddUseVirtualVertex(int virtualMeshIndex, int vindex, bool fix)
        {
            if (virtualMeshInfoList.Exists(virtualMeshIndex))
            {
                //Debug.Log("Add:" + meshChunkIndex + "," + vindex);
                var minfo = virtualMeshInfoList[virtualMeshIndex];
                minfo.vertexUseCount++;
                virtualMeshInfoList[virtualMeshIndex] = minfo;

                int index = minfo.vertexChunk.startIndex + vindex;

                //uint value = virtualVertexInfoList[index] + 1;
                //virtualVertexInfoList[index] = value;

                // ????????
                byte value = (byte)(virtualVertexUseList[index] + 1);
                virtualVertexUseList[index] = value;

                // ????????
                if (fix)
                    virtualVertexFixList[index] += 1;

                bool change = (value == 1);
                return change;
            }
            else
                return false;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="vindex"></param>
        /// <returns>??????true???</returns>
        public bool RemoveUseVirtualVertex(int virtualMeshIndex, int vindex, bool fix)
        {
            if (virtualMeshInfoList.Exists(virtualMeshIndex))
            {
                //Debug.Log("Rem:" + meshChunkIndex + "," + vindex);
                var minfo = virtualMeshInfoList[virtualMeshIndex];
                minfo.vertexUseCount--;
                virtualMeshInfoList[virtualMeshIndex] = minfo;

                int index = minfo.vertexChunk.startIndex + vindex;

                //uint value = virtualVertexInfoList[index] - 1;
                //virtualVertexInfoList[index] = value;

                // ????????
                byte value = (byte)(virtualVertexUseList[index] - 1);
                virtualVertexUseList[index] = value;

                // ????????
                if (fix)
                    virtualVertexFixList[index] -= 1;

                bool change = (value == 0);

                return change;
            }
            else
                return false;
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="tangents"></param>
        public void CopyToVirtualMeshWorldData(int virtualMeshIndex, Vector3[] vertices, Vector3[] normals, Vector3[] tangents)
        {
            var minfo = virtualMeshInfoList[virtualMeshIndex];
            int start = minfo.vertexChunk.startIndex;
            virtualPosList.ToJobArray().CopyToFast(start, vertices);
            var fw = new float3(0, 0, 1);
            var up = new float3(0, 1, 0);
            for (int i = 0; i < minfo.vertexChunk.dataLength; i++)
            {
                var rot = virtualRotList[start + i];
                normals[i] = math.mul(rot, fw);
                tangents[i] = math.mul(rot, up);
            }
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="boneList"></param>
        public void AddVirtualMeshBone(int virtualMeshIndex, List<Transform> boneList)
        {
            if (virtualMeshInfoList.Exists(virtualMeshIndex))
            {
                var minfo = virtualMeshInfoList[virtualMeshIndex];
                var c = virtualTransformIndexList.AddChunk(boneList.Count);
                minfo.boneChunk = c;

                for (int i = 0; i < boneList.Count; i++)
                {
                    virtualTransformIndexList[minfo.boneChunk.startIndex + i] = Bone.AddBone(boneList[i]);
                }

                virtualMeshInfoList[virtualMeshIndex] = minfo;
            }
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        public void RemoveVirtualMeshBone(int virtualMeshIndex)
        {
            if (virtualMeshIndex >= 0)
            {
                if (virtualMeshInfoList.Exists(virtualMeshIndex))
                {
                    var minfo = virtualMeshInfoList[virtualMeshIndex];

                    for (int i = 0; i < minfo.boneChunk.dataLength; i++)
                    {
                        int tindex = virtualTransformIndexList[minfo.boneChunk.startIndex + i];
                        Bone.RemoveBone(tindex);
                        virtualTransformIndexList[minfo.boneChunk.startIndex + i] = 0;
                    }

                    virtualTransformIndexList.RemoveChunk(minfo.boneChunk.chunkNo);
                    minfo.boneChunk.Clear();
                    virtualMeshInfoList[virtualMeshIndex] = minfo;
                }
            }
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        public void ResetFuturePredictionVirtualMeshBone(int virtualMeshIndex)
        {
            if (virtualMeshIndex >= 0)
            {
                if (virtualMeshInfoList.Exists(virtualMeshIndex))
                {
                    var minfo = virtualMeshInfoList[virtualMeshIndex];

                    for (int i = 0; i < minfo.boneChunk.dataLength; i++)
                    {
                        int tindex = virtualTransformIndexList[minfo.boneChunk.startIndex + i];
                        Bone.ResetFuturePrediction(tindex);
                    }
                }
            }
        }

        /// <summary>
        /// ??????????????UnityPhysics?????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="sw"></param>
        public void ChangeVirtualMeshUseUnityPhysics(int virtualMeshIndex, bool sw)
        {
            if (virtualMeshIndex >= 0)
            {
                if (virtualMeshInfoList.Exists(virtualMeshIndex))
                {
                    var minfo = virtualMeshInfoList[virtualMeshIndex];
                    Bone.ChangeUnityPhysicsCount(minfo.transformIndex, sw);

                    for (int i = 0; i < minfo.boneChunk.dataLength; i++)
                    {
                        int tindex = virtualTransformIndexList[minfo.boneChunk.startIndex + i];
                        Bone.ChangeUnityPhysicsCount(tindex, sw);
                    }
                }
            }
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="flag"></param>
        /// <param name="sw"></param>
        public void SetVirtualMeshFlag(int virtualMeshIndex, uint flag, bool sw)
        {
            if (virtualMeshInfoList.Exists(virtualMeshIndex))
            {
                var minfo = virtualMeshInfoList[virtualMeshIndex];
                minfo.SetFlag(flag, sw);
                virtualMeshInfoList[virtualMeshIndex] = minfo;
            }
        }

        public int SharedVirtualMeshCount
        {
            get
            {
                return sharedVirtualMeshInfoList.Count;
            }
        }

        public int VirtualMeshCount
        {
            get
            {
                return virtualMeshInfoList.Count;
            }
        }

        public int VirtualMeshVertexCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < virtualMeshInfoList.Length; i++)
                    cnt += virtualMeshInfoList[i].vertexChunk.dataLength;
                return cnt;
            }
        }

        public int VirtualMeshTriangleCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < virtualMeshInfoList.Length; i++)
                    cnt += virtualMeshInfoList[i].triangleChunk.dataLength;
                return cnt;
            }
        }

        public int VirtualMeshVertexUseCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < virtualMeshInfoList.Length; i++)
                    if (virtualMeshInfoList[i].IsActive())
                        cnt += virtualMeshInfoList[i].vertexChunk.dataLength;
                return cnt;
            }
        }

        public int VirtualMeshUseCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < virtualMeshInfoList.Length; i++)
                    cnt += virtualMeshInfoList[i].IsUse() ? 1 : 0;
                return cnt;
            }
        }

        public int VirtualMeshPauseCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < virtualMeshInfoList.Length; i++)
                    if (virtualMeshInfoList[i].IsUse() && virtualMeshInfoList[i].IsPause())
                        cnt++;
                return cnt;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????
        /// ??????????????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="vertexCount"></param>
        /// <returns></returns>
        public int AddSharedChildMesh(
            long cuid,
            int virtualMeshIndex,
            int vertexCount,
            int weightCount
            )
        {
            var minfo = virtualMeshInfoList[virtualMeshIndex];
            int sharedMergeMeshIndex = minfo.sharedVirtualMeshIndex;

            // ????????????
            int sharedChildMeshIndex = -1;
            if (sharedChildMeshIdToSharedVirtualMeshIndexDict.ContainsKey(cuid))
            {
                // ??
                sharedChildMeshIndex = sharedChildMeshIdToSharedVirtualMeshIndexDict[cuid];
                var sc_minfo = sharedChildMeshInfoList[sharedChildMeshIndex];
                sc_minfo.meshUseCount++; // ??????+
                sharedChildMeshInfoList[sharedChildMeshIndex] = sc_minfo;
            }
            else
            {
                // ??
                var sc_minfo = new SharedChildMeshInfo();
                sc_minfo.cuid = cuid;
                sc_minfo.sharedVirtualMeshIndex = sharedMergeMeshIndex;
                sc_minfo.virtualMeshIndex = virtualMeshIndex;
                sc_minfo.meshUseCount = 1;

                // vertices/normals/triangles/bindpose
                var oc = sharedChildVertexInfoList.AddChunk(vertexCount);
                sc_minfo.vertexChunk = oc;

                // weight
                oc = sharedChildWeightList.AddChunk(weightCount);
                sc_minfo.weightChunk = oc;

                sharedChildMeshIndex = sharedChildMeshInfoList.Add(sc_minfo);

                sharedChildMeshIdToSharedVirtualMeshIndexDict.Add(cuid, sharedChildMeshIndex);
            }

            return sharedChildMeshIndex;
        }

        public bool IsEmptySharedChildMesh(long cuid)
        {
            return sharedChildMeshIdToSharedVirtualMeshIndexDict.ContainsKey(cuid) == false;
        }

        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <param name="meshChunkIndex"></param>
        /// <param name="sharedVertices"></param>
        /// <param name="sharedNormals"></param>
        /// <param name="sharedTangents"></param>
        /// <param name="sharedBoneWeights"></param>
        public void SetSharedChildMeshData(
            int sharedMeshIndex,
            uint[] sharedVertexInfoList,
            MeshData.VertexWeight[] sharedVertexWeightList
            )
        {
            var smdata = sharedChildMeshInfoList[sharedMeshIndex];

            // ???????????????????
            if (smdata.meshUseCount == 1)
            {
                sharedChildVertexInfoList.ToJobArray().CopyFromFast(smdata.vertexChunk.startIndex, sharedVertexInfoList);
                sharedChildWeightList.ToJobArray().CopyFromFast(smdata.weightChunk.startIndex, sharedVertexWeightList);
            }
        }

        public void RemoveSharedChildMesh(int sharedChildMeshIndex)
        {
            // ????????
            var sc_minfo = sharedChildMeshInfoList[sharedChildMeshIndex];
            sc_minfo.meshUseCount--; // ??????
            if (sc_minfo.meshUseCount == 0)
            {
                // ??
                sharedChildVertexInfoList.RemoveChunk(sc_minfo.vertexChunk.chunkNo);
                sharedChildWeightList.RemoveChunk(sc_minfo.weightChunk.chunkNo);

                sharedChildMeshInfoList.Remove(sharedChildMeshIndex);

                sharedChildMeshIdToSharedVirtualMeshIndexDict.Remove(sc_minfo.cuid);
            }
            else
            {
                sharedChildMeshInfoList[sharedChildMeshIndex] = sc_minfo;
            }
        }

        public int SharedRenderMeshCount
        {
            get
            {
                return sharedRenderMeshInfoList.Count;
            }
        }

        public int SharedChildMeshCount
        {
            get
            {
                return sharedChildMeshInfoList.Count;
            }
        }


        //=========================================================================================
        /// <summary>
        /// ???????????
        /// ??????????????????????
        /// </summary>
        /// <param name="vertexCount"></param>
        /// <returns></returns>
        public int AddRenderMesh(
            int uid,
            bool isSkinning,
            Vector3 baseScale,
            int vertexCount,
            int rendererBoneIndex,
            int boneWeightCount
            )
        {
            //Develop.Log($"?AddRenderMesh uid:{uid} vcnt:{vertexCount} rboneindex:{rendererBoneIndex} bonewcnt:{boneWeightCount}, isSkinning:{isSkinning}");
            // ??????????????
            int sharedMeshIndex = -1;
            if (uid != 0)
            {
                if (sharedRenderMeshIdToIndexDict.ContainsKey(uid))
                {
                    // ??
                    sharedMeshIndex = sharedRenderMeshIdToIndexDict[uid];
                    var sminfo = sharedRenderMeshInfoList[sharedMeshIndex];
                    sminfo.useCount++; // ??????+
                    sharedRenderMeshInfoList[sharedMeshIndex] = sminfo;
                }
                else
                {
                    // ??
                    var sminfo = new SharedRenderMeshInfo();
                    sminfo.uid = uid;
                    sminfo.useCount = 1;
                    sminfo.rendererBoneIndex = rendererBoneIndex;
                    if (isSkinning)
                        sminfo.SetFlag(MeshFlag_Skinning, true);

                    // vertices/normals/triangles/bindpose
                    var oc = sharedRenderVertices.AddChunk(vertexCount);
                    sharedRenderNormals.AddChunk(vertexCount);
                    sharedRenderTangents.AddChunk(vertexCount);
                    sminfo.vertexChunk = oc;

                    //Develop.Log($"vchunk s:{oc.startIndex} cnt:{oc.dataLength}");

                    // ???????
                    if (isSkinning)
                    {
                        var bc = sharedBonesPerVertexList.AddChunk(vertexCount);
                        sharedBonesPerVertexStartList.AddChunk(vertexCount);
                        var wc = sharedBoneWeightList.AddChunk(boneWeightCount);
                        sminfo.bonePerVertexChunk = bc;
                        sminfo.boneWeightsChunk = wc;
                    }

                    sharedMeshIndex = sharedRenderMeshInfoList.Add(sminfo);
                    sharedRenderMeshIdToIndexDict.Add(uid, sharedMeshIndex);
                }
            }

            // ????????????????
            var minfo = new RenderMeshInfo();
            //minfo.SetFlag(MeshFlag_Active, true);
            minfo.SetFlag(MeshFlag_Skinning, isSkinning);
            minfo.renderSharedMeshIndex = sharedMeshIndex;
            var sminfo2 = sharedRenderMeshInfoList[sharedMeshIndex];
            minfo.sharedRenderMeshVertexStartIndex = sminfo2.vertexChunk.startIndex;
            var c = renderVertexFlagList.AddChunk(vertexCount);
            renderPosList.AddChunk(vertexCount);
            renderNormalList.AddChunk(vertexCount);
            renderTangentList.AddChunk(vertexCount);
            if (isSkinning)
            {
                minfo.boneWeightsChunk = renderBoneWeightList.AddChunk(boneWeightCount);
            }
            minfo.vertexChunk = c;
            minfo.baseScale = baseScale.magnitude; // ???????:?????
            int index = renderMeshInfoList.Add(minfo);

            // ?????????????????????
            var state = new RenderMeshState();
            state.SetFlag(RenderStateFlag_Use, minfo.IsUse());
            state.RenderSharedMeshIndex = sharedMeshIndex;
            state.RenderSharedMeshId = sminfo2.uid;
            state.VertexChunkStart = c.startIndex;
            state.VertexChunkLength = c.dataLength;
            state.BoneWeightChunkStart = minfo.boneWeightsChunk.startIndex;
            state.BoneWeightChunkLength = minfo.boneWeightsChunk.dataLength;
            renderMeshStateDict.Add(index, state);

            // ??????????????????ID?????
            // (+1)?????????!
            uint flag = (uint)index + 1;
            renderVertexFlagList.Fill(minfo.vertexChunk, flag);

            return index;
        }

        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        public void UpdateMeshState(int renderMeshIndex)
        {
            var state = renderMeshStateDict[renderMeshIndex];
            var sminfo = sharedRenderMeshInfoList[state.RenderSharedMeshIndex];
            state.SetFlag(RenderStateFlag_ExistNormal, sminfo.IsFlag(MeshFlag_ExistNormals));
            state.SetFlag(RenderStateFlag_ExistTangent, sminfo.IsFlag(MeshFlag_ExistTangents));
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <param name="rendererTransform"></param>
        public void AddRenderMeshBone(int renderMeshIndex, Transform rendererTransform)
        {
            var minfo = renderMeshInfoList[renderMeshIndex];

            // ??????????????????
            minfo.transformIndex = Bone.AddBone(rendererTransform);

            renderMeshInfoList[renderMeshIndex] = minfo;
        }

        public bool IsEmptySharedRenderMesh(int uid)
        {
            return sharedRenderMeshIdToIndexDict.ContainsKey(uid) == false;
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <param name="sharedVertices"></param>
        /// <param name="sharedNormals"></param>
        /// <param name="sharedTangents"></param>
        /// <param name="sharedBoneWeights"></param>
        public void SetRenderSharedMeshData(
            int renderMeshIndex,
            bool isSkinning,
            Vector3[] sharedVertices,
            Vector3[] sharedNormals,
            Vector4[] sharedTangents,
            NativeArray<byte> sharedBonesPerVertex,
            NativeArray<BoneWeight1> sharedBoneWeights
            )
        {
            var minfo = renderMeshInfoList[renderMeshIndex];
            Debug.Assert(minfo.renderSharedMeshIndex >= 0);
            var smdata = sharedRenderMeshInfoList[minfo.renderSharedMeshIndex];

            // ???????????????????
            if (smdata.useCount == 1)
            {
                sharedRenderVertices.ToJobArray().CopyFromFast(smdata.vertexChunk.startIndex, sharedVertices);

                // ?????????????
                if (sharedNormals != null && sharedNormals.Length > 0)
                {
                    sharedRenderNormals.ToJobArray().CopyFromFast(smdata.vertexChunk.startIndex, sharedNormals);
                    smdata.SetFlag(MeshFlag_ExistNormals, true);
                }

                // ?????????????
                if (sharedTangents != null && sharedTangents.Length > 0)
                {
                    sharedRenderTangents.ToJobArray().CopyFromFast(smdata.vertexChunk.startIndex, sharedTangents);
                    smdata.SetFlag(MeshFlag_ExistTangents, true);
                }

                // ??????????????????
                if (isSkinning && sharedBonesPerVertex.Length > 0)
                {
                    int vcnt = sharedBonesPerVertex.Length;

                    // ??????????????????????
                    int[] startIndexList = new int[vcnt];
                    int sindex = 0;
                    for (int i = 0; i < vcnt; i++)
                    {
                        startIndexList[i] = sindex;
                        sindex += sharedBonesPerVertex[i];
                    }

                    sharedBonesPerVertexList.ToJobArray().CopyFromFast(smdata.bonePerVertexChunk.startIndex, sharedBonesPerVertex.ToArray());
                    sharedBonesPerVertexStartList.ToJobArray().CopyFromFast(smdata.bonePerVertexChunk.startIndex, startIndexList);
                    sharedBoneWeightList.ToJobArray().CopyFromFast(smdata.boneWeightsChunk.startIndex, sharedBoneWeights.ToArray());
                }

                sharedRenderMeshInfoList[minfo.renderSharedMeshIndex] = smdata;
            }
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        public void RemoveRenderMesh(int renderMeshIndex)
        {
            //Develop.Log($"RemoverRenderMesh index:{renderMeshIndex}");
            if (renderMeshIndex < 0)
                return;
            if (renderMeshInfoList.Exists(renderMeshIndex))
            {
                var minfo = renderMeshInfoList[renderMeshIndex];

                // ????????
                int sharedMeshIndex = minfo.renderSharedMeshIndex;
                if (sharedMeshIndex >= 0)
                {
                    var sminfo = sharedRenderMeshInfoList[sharedMeshIndex];
                    sminfo.useCount--; // ??????
                    if (sminfo.useCount == 0)
                    {
                        // ??
                        //Develop.Log($"???????? vchunk s:{sminfo.vertexChunk.startIndex} cnt:{sminfo.vertexChunk.dataLength}");
                        sharedRenderVertices.RemoveChunk(sminfo.vertexChunk.chunkNo);
                        sharedRenderNormals.RemoveChunk(sminfo.vertexChunk.chunkNo);
                        sharedRenderTangents.RemoveChunk(sminfo.vertexChunk.chunkNo);

                        if (sminfo.bonePerVertexChunk.dataLength > 0)
                        {
                            sharedBonesPerVertexList.RemoveChunk(sminfo.bonePerVertexChunk);
                            sharedBonesPerVertexStartList.RemoveChunk(sminfo.bonePerVertexChunk);
                        }
                        if (sminfo.boneWeightsChunk.dataLength > 0)
                        {
                            sharedBoneWeightList.RemoveChunk(sminfo.boneWeightsChunk);
                        }

                        sharedRenderMeshInfoList.Remove(sharedMeshIndex);
                        sharedRenderMeshIdToIndexDict.Remove(sminfo.uid);
                    }
                    else
                    {
                        sharedRenderMeshInfoList[sharedMeshIndex] = sminfo;
                    }
                }

                // ????????????
                renderVertexFlagList.RemoveChunk(minfo.vertexChunk.chunkNo);
                renderPosList.RemoveChunk(minfo.vertexChunk.chunkNo);
                renderNormalList.RemoveChunk(minfo.vertexChunk.chunkNo);
                renderTangentList.RemoveChunk(minfo.vertexChunk.chunkNo);

                if (minfo.boneWeightsChunk.dataLength > 0)
                {
                    renderBoneWeightList.RemoveChunk(minfo.boneWeightsChunk);
                }

                //Debug.Log("Remove Mesh Chunk:" + meshChunkIndex);
                renderMeshStateDict.Remove(renderMeshIndex);
                renderMeshInfoList.Remove(renderMeshIndex);
            }
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <param name="rendererTransform"></param>
        public void RemoveRenderMeshBone(int renderMeshIndex)
        {
            var minfo = renderMeshInfoList[renderMeshIndex];

            // ??????????????????
            Bone.RemoveBone(minfo.transformIndex);
            //minfo.transformIndex = 0;
            minfo.transformIndex = -1; // ?????

            renderMeshInfoList[renderMeshIndex] = minfo;
        }

        /// <summary>
        /// ????????????????UnityPhysics?????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <param name="sw"></param>
        public void ChangeRenderMeshUseUnityPhysics(int renderMeshIndex, bool sw)
        {
            var minfo = renderMeshInfoList[renderMeshIndex];
            if (minfo.transformIndex >= 0)
                Bone.ChangeUnityPhysicsCount(minfo.transformIndex, sw);
        }

        /// <summary>
        /// ??????????????????1????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <returns></returns>
        public bool IsUseRenderMesh(int renderMeshIndex)
        {
            return renderMeshStateDict[renderMeshIndex].IsFlag(RenderStateFlag_Use);
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <returns></returns>
        public bool IsActiveRenderMesh(int renderMeshIndex)
        {
            return renderMeshInfoList[renderMeshIndex].IsActive();
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="virtualMeshIndex"></param>
        /// <param name="flag"></param>
        /// <param name="sw"></param>
        public void SetRenderMeshFlag(int renderMeshIndex, uint flag, bool sw)
        {
            if (renderMeshInfoList.Exists(renderMeshIndex))
            {
                var minfo = renderMeshInfoList[renderMeshIndex];
                minfo.SetFlag(flag, sw);
                renderMeshInfoList[renderMeshIndex] = minfo;
            }
        }

        public bool IsRenderMeshFlag(int renderMeshIndex, uint flag)
        {
            if (renderMeshInfoList.Exists(renderMeshIndex))
            {
                var minfo = renderMeshInfoList[renderMeshIndex];
                return minfo.IsFlag(flag);
            }
            return false;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <param name="sw"></param>
        public void SetRenderMeshActive(int renderMeshIndex, bool sw)
        {
            if (renderMeshInfoList.Exists(renderMeshIndex))
            {
                var minfo = renderMeshInfoList[renderMeshIndex];
                minfo.SetFlag(MeshFlag_Active, sw);
                minfo.SetFlag(MeshFlag_UpdateUseVertexFront, true);
                minfo.SetFlag(MeshFlag_UpdateUseVertexBack, true);
                renderMeshInfoList[renderMeshIndex] = minfo;
                renderMeshStateDict[renderMeshIndex].SetFlag(RenderStateFlag_Use, minfo.IsUse());
                renderMeshStateDict[renderMeshIndex].SetFlag(RenderStateFlag_DelayedCalculated, false);
            }
        }

        public void AddUseRenderMesh(int renderMeshIndex)
        {
            if (renderMeshInfoList.Exists(renderMeshIndex))
            {
                var minfo = renderMeshInfoList[renderMeshIndex];
                minfo.meshUseCount++;
                renderMeshInfoList[renderMeshIndex] = minfo;
                renderMeshStateDict[renderMeshIndex].SetFlag(RenderStateFlag_Use, minfo.IsUse());
            }
        }

        public void RemoveUseRenderMesh(int renderMeshIndex)
        {
            if (renderMeshInfoList.Exists(renderMeshIndex))
            {
                var minfo = renderMeshInfoList[renderMeshIndex];
                minfo.meshUseCount--;
                Debug.Assert(minfo.meshUseCount >= 0);
                renderMeshInfoList[renderMeshIndex] = minfo;
                renderMeshStateDict[renderMeshIndex].SetFlag(RenderStateFlag_Use, minfo.IsUse());
            }
        }

        public void LinkRenderMesh(int renderMeshIndex, int childMeshVertexStart, int childMeshWeightStart, int virtualMeshVertexStart, int sharedVirtualMeshVertexStart)
        {
            // ??????????????????????
            var minfo = renderMeshInfoList[renderMeshIndex];
            minfo.AddLinkMesh(renderMeshIndex, childMeshVertexStart, childMeshWeightStart, virtualMeshVertexStart, sharedVirtualMeshVertexStart);
            minfo.SetFlag(MeshFlag_UpdateUseVertexFront, true);
            minfo.SetFlag(MeshFlag_UpdateUseVertexBack, true);
            renderMeshInfoList[renderMeshIndex] = minfo;
            renderMeshStateDict[renderMeshIndex].SetFlag(RenderStateFlag_Use, minfo.IsUse());
            renderMeshStateDict[renderMeshIndex].SetFlag(RenderStateFlag_DelayedCalculated, false);
        }

        public void UnlinkRenderMesh(int renderMeshIndex, int childMeshVertexStart, int childMeshWeightStart, int virtualMeshVertexStart, int sharedVirtualMeshVertexStart)
        {
            // ??????????????????????????
            var minfo = renderMeshInfoList[renderMeshIndex];
            minfo.RemoveLinkMesh(renderMeshIndex, childMeshVertexStart, childMeshWeightStart, virtualMeshVertexStart, sharedVirtualMeshVertexStart);
            minfo.SetFlag(MeshFlag_UpdateUseVertexFront, true);
            minfo.SetFlag(MeshFlag_UpdateUseVertexBack, true);
            renderMeshInfoList[renderMeshIndex] = minfo;
            if (renderMeshStateDict.ContainsKey(renderMeshIndex))
            {
                renderMeshStateDict[renderMeshIndex].SetFlag(RenderStateFlag_Use, minfo.IsUse());
                renderMeshStateDict[renderMeshIndex].SetFlag(RenderStateFlag_DelayedCalculated, false);
            }
        }

        /// <summary>
        /// ??????????????????????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="tangents"></param>
        internal void CopyToRenderMeshLocalPositionData(int renderMeshIndex, Mesh mesh, int bufferIndex)
        {
            var state = renderMeshStateDict[renderMeshIndex];
#if UNITY_2020_1_OR_NEWER
            var flag = MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices;
            //Debug.Log($"bufferIndex:{bufferIndex} array0Length:{renderPosList.ToJobArray().Length} array1Length:{renderPosList.ToJobArray(1).Length}  start:{state.VertexChunkStart} length:{state.VertexChunkLength} F:{Time.frameCount}");
            mesh.SetVertices(renderPosList.ToJobArray(bufferIndex), state.VertexChunkStart, state.VertexChunkLength, flag);
#else
            mesh.SetVertices(renderPosList.ToJobArray(bufferIndex), state.VertexChunkStart, state.VertexChunkLength);
#endif
        }

        /// <summary>
        /// ??????????????????????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="tangents"></param>
        internal void CopyToRenderMeshLocalNormalTangentData(int renderMeshIndex, Mesh mesh, int bufferIndex, bool normal, bool tangent)
        {
            var state = renderMeshStateDict[renderMeshIndex];
#if UNITY_2020_1_OR_NEWER
            var flag = MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices;
            if (state.IsFlag(RenderStateFlag_ExistNormal) && normal)
            {
                mesh.SetNormals(renderNormalList.ToJobArray(bufferIndex), state.VertexChunkStart, state.VertexChunkLength, flag);
            }
            if (state.IsFlag(RenderStateFlag_ExistTangent) && tangent)
            {
                mesh.SetTangents(renderTangentList.ToJobArray(bufferIndex), state.VertexChunkStart, state.VertexChunkLength, flag);
            }
#else
            if (state.IsFlag(RenderStateFlag_ExistNormal) && normal)
            {
                mesh.SetNormals(renderNormalList.ToJobArray(bufferIndex), state.VertexChunkStart, state.VertexChunkLength);
            }
            if (state.IsFlag(RenderStateFlag_ExistTangent) && tangent)
            {
                mesh.SetTangents(renderTangentList.ToJobArray(bufferIndex), state.VertexChunkStart, state.VertexChunkLength);
            }
#endif
        }

        /// <summary>
        /// ?????????????????????????
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <param name="vertices"></param>
        internal void CopyToRenderMeshBoneWeightData(int renderMeshIndex, Mesh mesh, Mesh sharedMesh, int bufferIndex)
        {
            var state = renderMeshStateDict[renderMeshIndex];

            NativeArray<BoneWeight1> weights = new NativeArray<BoneWeight1>(state.BoneWeightChunkLength, Allocator.Temp);
            renderBoneWeightList.ToJobArray(bufferIndex).CopyToFast(state.BoneWeightChunkStart, weights);
            mesh.SetBoneWeights(sharedMesh.GetBonesPerVertex(), weights);
            weights.Dispose();
        }

        /// <summary>
        /// ??????????????????????????(?????)
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="tangents"></param>
        internal void CopyToRenderMeshWorldData(int renderMeshIndex, Transform target, Vector3[] vertices, Vector3[] normals, Vector3[] tangents)
        {
            var minfo = renderMeshInfoList[renderMeshIndex];

            // ????????????
            renderPosList.ToJobArray().CopyToFast(minfo.vertexChunk.startIndex, vertices);
            renderNormalList.ToJobArray().CopyToFast(minfo.vertexChunk.startIndex, normals);
            Vector4[] tan4array = new Vector4[minfo.vertexChunk.dataLength];
            renderTangentList.ToJobArray().CopyToFast(minfo.vertexChunk.startIndex, tan4array);

            // ????????
            for (int i = 0; i < minfo.vertexChunk.dataLength; i++)
            {
                vertices[i] = target.TransformPoint(vertices[i]);
                normals[i] = target.InverseTransformDirection(normals[i]);
                tangents[i] = target.InverseTransformDirection(tan4array[i]);
            }
        }

        /// <summary>
        /// ???????????????????(?????)
        /// </summary>
        /// <param name="renderMeshIndex"></param>
        /// <returns></returns>
        internal List<int> GetVertexUseList(int renderMeshIndex)
        {
            var minfo = renderMeshInfoList[renderMeshIndex];
            var useList = new List<int>(minfo.vertexChunk.dataLength);
            for (int i = 0; i < minfo.vertexChunk.dataLength; i++)
            {
                uint flag = renderVertexFlagList[minfo.vertexChunk.startIndex + i];
                // ??16bit??????????
                // ??????0/1???
                useList.Add((flag & 0xffff0000) != 0 ? 1 : 0);
            }

            return useList;
        }

        public int RenderMeshCount
        {
            get
            {
                return renderMeshInfoList.Count;
            }
        }

        public int RenderMeshVertexCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < renderMeshInfoList.Length; i++)
                    cnt += renderMeshInfoList[i].vertexChunk.dataLength;
                return cnt;
            }
        }

        public int RenderMeshUseCount
        {
            get
            {
                int cnt = 0;
                foreach (var state in renderMeshStateDict.Values)
                    cnt += state.IsFlag(RenderStateFlag_Use) ? 1 : 0;
                return cnt;
            }
        }

        public int RenderMeshVertexUseCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < renderMeshInfoList.Length; i++)
                    if (renderMeshInfoList[i].IsActive())
                        cnt += renderMeshInfoList[i].vertexChunk.dataLength;
                return cnt;
            }
        }

        public int RenderMeshPauseCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < renderMeshInfoList.Length; i++)
                    if (renderMeshInfoList[i].IsUse() && renderMeshInfoList[i].IsPause())
                        cnt++;
                return cnt;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????
        /// </summary>
        internal void SetDelayedCalculatedFlag()
        {
            foreach (var mesh in renderMeshSet)
            {
                if (mesh.Parent.IsCalculate)
                {
                    var state = renderMeshStateDict[mesh.MeshIndex];
                    state.SetFlag(RenderStateFlag_DelayedCalculated, true);
                }
            }
        }

        internal void ClearWritingList()
        {
            normalWriteList.Clear();
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        /// <param name="bufferIndex"></param>
        internal void MeshCalculation(int bufferIndex)
        {
            foreach (var bmesh in renderMeshSet)
            {
                if (bmesh != null)
                {
                    var rmesh = bmesh as RenderMeshDeformer;
                    rmesh.MeshCalculation(bufferIndex);

                    // ?????????
                    if (rmesh.IsWriteMeshPosition || rmesh.IsWriteMeshBoneWeight)
                        normalWriteList.Add(rmesh);
                }
            }
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        internal void NormalWriting(int bufferIndex)
        {
            // ?????????
            foreach (var rmesh in normalWriteList)
                rmesh.NormalWriting(bufferIndex);
            normalWriteList.Clear();
        }
    }
}
