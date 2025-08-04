// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace MagicaCloth
{
    /// <summary>
    /// ??????
    /// </summary>
    public class PhysicsManagerBoneData : PhysicsManagerAccess
    {
        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        public const byte Flag_Reset = 0x01; // ????
        public const byte Flag_Restore = 0x10;  // ???????
        public const byte Flag_Write = 0x20; // ?????????

        /// <summary>
        /// ????????
        /// </summary>
        public FixedTransformAccessArray boneList;

        /// <summary>
        /// ?????????
        /// </summary>
        public FixedNativeList<byte> boneFlagList;

        /// <summary>
        /// ????????????(?????????????????)
        /// </summary>
        public FixedNativeList<float3> bonePosList;

        /// <summary>
        /// ????????????(?????????????????)
        /// </summary>
        public FixedNativeList<quaternion> boneRotList;

        /// <summary>
        /// ??????????????(??????????????)
        /// </summary>
        public FixedNativeList<float3> boneSclList;

        /// <summary>
        /// ????????????(-1=??)
        /// </summary>
        public FixedNativeList<int> boneParentIndexList;

        /// <summary>
        /// ????????????(?????)
        /// </summary>
        public FixedNativeList<float3> basePosList;

        /// <summary>
        /// ????????????(?????)
        /// </summary>
        public FixedNativeList<quaternion> baseRotList;

        /// <summary>
        /// ????UnityPhysics?????????????(1?????)
        /// </summary>
        public FixedNativeList<short> boneUnityPhysicsList;

        /// <summary>
        /// ????????????
        /// </summary>
        public FixedNativeList<float3> futurePosList;

        /// <summary>
        /// ????????????
        /// </summary>
        public FixedNativeList<quaternion> futureRotList;

        //=========================================================================================
        /// <summary>
        /// ????????
        /// </summary>
        public FixedTransformAccessArray restoreBoneList;

        /// <summary>
        /// ?????????????????
        /// </summary>
        public FixedNativeList<float3> restoreBoneLocalPosList;

        /// <summary>
        /// ?????????????????
        /// </summary>
        public FixedNativeList<quaternion> restoreBoneLocalRotList;

        /// <summary>
        /// ?????????????????
        /// </summary>
        public FixedNativeList<int> restoreBoneIndexList;

        //=========================================================================================
        // ???????????
        /// <summary>
        /// ??????????
        /// </summary>
        public FixedTransformAccessArray writeBoneList;

        /// <summary>
        /// ?????????????????????(+1???????!)
        /// </summary>
        public FixedNativeList<int> writeBoneIndexList;

        /// <summary>
        /// ????????????????????????
        /// </summary>
        public ExNativeMultiHashMap<int, int> writeBoneParticleIndexMap;

        /// <summary>
        /// ????????????????????????????
        /// </summary>
        Dictionary<int, int> boneToWriteIndexDict = new Dictionary<int, int>();

        /// <summary>
        /// ????????????
        /// ????????????????????????
        /// </summary>
        public FixedNativeList<float3> writeBonePosList;

        /// <summary>
        /// ????????????
        /// ????????????????????????
        /// </summary>
        public FixedNativeList<quaternion> writeBoneRotList;

        //=========================================================================================
        /// <summary>
        /// ????????????????true
        /// </summary>
        public bool hasBoneChanged { get; private set; }

        /// <summary>
        /// ???????
        /// </summary>
        private CustomSampler SamplerReadBoneScale { get; set; }

        //=========================================================================================
        /// <summary>
        /// ????
        /// </summary>
        public override void Create()
        {
            boneList = new FixedTransformAccessArray();
            boneFlagList = new FixedNativeList<byte>();
            bonePosList = new FixedNativeList<float3>();
            boneRotList = new FixedNativeList<quaternion>();
            boneSclList = new FixedNativeList<float3>();
            boneParentIndexList = new FixedNativeList<int>();
            basePosList = new FixedNativeList<float3>();
            baseRotList = new FixedNativeList<quaternion>();
            boneUnityPhysicsList = new FixedNativeList<short>();
            futurePosList = new FixedNativeList<float3>();
            futureRotList = new FixedNativeList<quaternion>();

            restoreBoneList = new FixedTransformAccessArray();
            restoreBoneLocalPosList = new FixedNativeList<float3>();
            restoreBoneLocalRotList = new FixedNativeList<quaternion>();
            restoreBoneIndexList = new FixedNativeList<int>();

            writeBoneList = new FixedTransformAccessArray();
            writeBoneIndexList = new FixedNativeList<int>();
            writeBoneParticleIndexMap = new ExNativeMultiHashMap<int, int>();
            writeBonePosList = new FixedNativeList<float3>();
            writeBoneRotList = new FixedNativeList<quaternion>();

            // ???????
            SamplerReadBoneScale = CustomSampler.Create("ReadBoneScale");
        }

        /// <summary>
        /// ??
        /// </summary>
        public override void Dispose()
        {
            if (boneList == null)
                return;

            boneList.Dispose();
            boneFlagList.Dispose();
            bonePosList.Dispose();
            boneRotList.Dispose();
            boneSclList.Dispose();
            boneParentIndexList.Dispose();
            basePosList.Dispose();
            baseRotList.Dispose();
            boneUnityPhysicsList.Dispose();
            futurePosList.Dispose();
            futureRotList.Dispose();

            restoreBoneList.Dispose();
            restoreBoneLocalPosList.Dispose();
            restoreBoneLocalRotList.Dispose();
            restoreBoneIndexList.Dispose();

            writeBoneList.Dispose();
            writeBoneIndexList.Dispose();
            writeBoneParticleIndexMap.Dispose();
            writeBonePosList.Dispose();
            writeBoneRotList.Dispose();
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="target"></param>
        /// <param name="lpos"></param>
        /// <param name="lrot"></param>
        /// <returns></returns>
        public int AddRestoreBone(Transform target, float3 lpos, quaternion lrot, int boneIndex)
        {
            int restoreBoneIndex;
            if (restoreBoneList.Exist(target))
            {
                // ??????+
                restoreBoneIndex = restoreBoneList.Add(target);
            }
            else
            {
                // ???????????
                restoreBoneIndex = restoreBoneList.Add(target);
                restoreBoneLocalPosList.Add(lpos);
                restoreBoneLocalRotList.Add(lrot);
                restoreBoneIndexList.Add(boneIndex);
                hasBoneChanged = true;
            }

            return restoreBoneIndex;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="restoreBoneIndex"></param>
        public void RemoveRestoreBone(int restoreBoneIndex)
        {
            restoreBoneList.Remove(restoreBoneIndex);

            if (restoreBoneList.Exist(restoreBoneIndex) == false)
            {
                // ??????
                restoreBoneLocalPosList.Remove(restoreBoneIndex);
                restoreBoneLocalRotList.Remove(restoreBoneIndex);
                restoreBoneIndexList.Remove(restoreBoneIndex);
                hasBoneChanged = true;
            }
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        public int RestoreBoneCount
        {
            get
            {
                return restoreBoneList.Count;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pindex"></param>
        /// <param name="addParent">????????????????</param>
        /// <returns></returns>
        public int AddBone(Transform target, int pindex = -1, bool addParent = false)
        {
            int boneIndex;
            if (boneList.Exist(target))
            {
                // ??????+
                boneIndex = boneList.Add(target);
                // ???????????????????(v1.10.2)
                if (addParent)
                {
                    boneParentIndexList[boneIndex] = boneList.GetIndex(target.parent);
                }
                boneFlagList.Add(Flag_Reset); // ??????
            }
            else
            {
                // ??
                var pos = float3.zero;
                var rot = quaternion.identity;
                boneIndex = boneList.Add(target);
                boneFlagList.Add(Flag_Reset); // ??????
                bonePosList.Add(pos);
                boneRotList.Add(rot);
                boneSclList.Add(float3.zero);
                if (addParent)
                    boneParentIndexList.Add(boneList.GetIndex(target.parent));
                else
                    boneParentIndexList.Add(-1);
                basePosList.Add(pos);
                baseRotList.Add(rot);
                boneUnityPhysicsList.Add(0);
                futurePosList.Add(pos);
                futureRotList.Add(rot);
                hasBoneChanged = true;
            }

            //Debug.Log("AddBone:" + target.name + " index:" + boneIndex + " parent?:" + boneParentIndexList[boneIndex]);

            // ??????
            if (pindex >= 0)
            {
                if (boneToWriteIndexDict.ContainsKey(boneIndex))
                {
                    Debug.LogWarning($"[{target.name}] is already registered as a write bone.");
                }
                else
                {
                    //Debug.Log("AddWriteBone:" + target.name + " index:" + boneIndex + " parent?:" + boneParentIndexList[boneIndex]);

                    if (writeBoneList.Exist(target))
                    {
                        // ??????+
                        writeBoneList.Add(target);
                    }
                    else
                    {
                        // ??
                        writeBoneList.Add(target);
                        //Debug.Log("write bone index:" + boneIndex);
                        writeBoneIndexList.Add(boneIndex + 1); // +1????????!
                        writeBonePosList.Add(float3.zero);
                        writeBoneRotList.Add(quaternion.identity);
                        hasBoneChanged = true;
                    }
                    int writeIndex = writeBoneList.GetIndex(target);

                    boneToWriteIndexDict.Add(boneIndex, writeIndex);

                    // ??????????????????????
                    writeBoneParticleIndexMap.Add(writeIndex, pindex);
                }
            }

            return boneIndex;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="boneIndex"></param>
        /// <param name="pindex"></param>
        /// <returns></returns>
        public bool RemoveBone(int boneIndex, int pindex = -1)
        {
            //Debug.Log("RemoveBone: index:" + boneIndex + " parent?:" + boneParentIndexList[boneIndex]);

            bool del = false;
            boneList.Remove(boneIndex);
            if (boneList.Exist(boneIndex) == false)
            {
                // ??????
                boneFlagList.Remove(boneIndex);
                bonePosList.Remove(boneIndex);
                boneRotList.Remove(boneIndex);
                boneSclList.Remove(boneIndex);
                boneParentIndexList.Remove(boneIndex);
                basePosList.Remove(boneIndex);
                baseRotList.Remove(boneIndex);
                boneUnityPhysicsList.Remove(boneIndex);
                futurePosList.Remove(boneIndex);
                futureRotList.Remove(boneIndex);
                hasBoneChanged = true;
                del = true;
            }

            // ??????????
            if (pindex >= 0 && boneToWriteIndexDict.ContainsKey(boneIndex))
            {
                int writeIndex = boneToWriteIndexDict[boneIndex];

                writeBoneList.Remove(writeIndex);
                writeBoneIndexList.Remove(writeIndex);
                writeBoneParticleIndexMap.Remove(writeIndex, pindex);
                writeBonePosList.Remove(writeIndex);
                writeBoneRotList.Remove(writeIndex);
                hasBoneChanged = true;

                if (writeBoneList.Exist(writeIndex) == false)
                {
                    boneToWriteIndexDict.Remove(boneIndex);
                    //Debug.Log("RemoveWriteBone: index:" + boneIndex);
                }
            }

            return del;
        }

        /// <summary>
        /// ????UnityPhysics????????????
        /// </summary>
        /// <param name="boneIndex"></param>
        /// <param name="sw"></param>
        public void ChangeUnityPhysicsCount(int boneIndex, bool sw)
        {
            //Debug.Log($"Change Bone Physics Count [{boneIndex}]->{sw}");
            boneUnityPhysicsList[boneIndex] += (short)(sw ? 1 : -1);
            Debug.Assert(boneUnityPhysicsList[boneIndex] >= 0);
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="boneIndex"></param>
        public void ResetFuturePrediction(int boneIndex)
        {
            //Debug.Log($"ResetFuturePrediction:{boneIndex} F:{Time.frameCount}");
            var flag = boneFlagList[boneIndex];
            flag |= Flag_Reset;
            boneFlagList[boneIndex] = flag;
        }

        /// <summary>
        /// ???????????
        /// </summary>
        public int ReadBoneCount
        {
            get
            {
                return boneList.Count;
            }
        }

        /// <summary>
        /// ???????????
        /// </summary>
        public int WriteBoneCount
        {
            get
            {
                return writeBoneList.Count;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        public void ResetBoneFromTransform(bool fixedUpdate)
        {
            // ?????????
            if (RestoreBoneCount > 0)
            {
                var job = new RestoreBoneJob()
                {
                    fixedUpdate = fixedUpdate,
                    boneUnityPhysicsList = boneUnityPhysicsList.ToJobArray(),
                    boneFlagList = boneFlagList.ToJobArray(),
                    restoreBoneLocalPosList = restoreBoneLocalPosList.ToJobArray(),
                    restoreBoneLocalRotList = restoreBoneLocalRotList.ToJobArray(),
                    restoreBoneIndexList = restoreBoneIndexList.ToJobArray(),
                };
                Compute.MasterJob = job.Schedule(restoreBoneList.GetTransformAccessArray(), Compute.MasterJob);
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        [BurstCompile]
        struct RestoreBoneJob : IJobParallelForTransform
        {
            public bool fixedUpdate;

            [Unity.Collections.ReadOnly]
            public NativeArray<short> boneUnityPhysicsList;
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> boneFlagList;


            [Unity.Collections.ReadOnly]
            public NativeArray<float3> restoreBoneLocalPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> restoreBoneLocalRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> restoreBoneIndexList;

            // ???????
            public void Execute(int index, TransformAccess transform)
            {
                var bindex = restoreBoneIndexList[index];
                bool isUnityPhysics = boneUnityPhysicsList[bindex] > 0;
                if (isUnityPhysics == fixedUpdate)
                {
                    var flag = boneFlagList[bindex];
                    if ((flag & Flag_Restore) == 0)
                        return;

                    transform.localPosition = restoreBoneLocalPosList[index];
                    transform.localRotation = restoreBoneLocalRotList[index];
                }
            }
        }

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        public void ReadBoneFromTransform()
        {
            // ?????????
            if (ReadBoneCount > 0)
            {
                var updateTime = manager.UpdateTime;

                // ???????
                float futureRate = updateTime.IsDelay ? updateTime.FuturePredictionRate : 0.0f;

                // ???????????????
                if (futureRate < 0.01f)
                {
                    // ???????????(??????????????????????????!)
                    var job = new ReadBoneJob0()
                    {
                        fixedUpdateCount = updateTime.FixedUpdateCount,

                        bonePosList = bonePosList.ToJobArray(),
                        boneRotList = boneRotList.ToJobArray(),
                        boneSclList = boneSclList.ToJobArray(),
                        basePosList = basePosList.ToJobArray(),
                        baseRotList = baseRotList.ToJobArray(),
                        futurePosList = futurePosList.ToJobArray(),
                        futureRotList = futureRotList.ToJobArray(),

                        boneUnityPhysicsList = boneUnityPhysicsList.ToJobArray(),
                        boneFlagList = boneFlagList.ToJobArray(),
                    };
                    Compute.MasterJob = job.Schedule(boneList.GetTransformAccessArray(), Compute.MasterJob);
                }
                else
                {
                    // ??????
                    // Update???????????????
                    float normalFutureRatio = updateTime.DeltaTime > Define.Compute.Epsilon ?
                        math.clamp((updateTime.AverageDeltaTime / updateTime.DeltaTime) * futureRate, 0.0f, 2.0f) : 0.0f;

                    // FixedUpdate???????????????
                    float fixedFutureRatio = updateTime.FixedUpdateCount > 0 ? (1.0f / updateTime.FixedUpdateCount) * futureRate : 0.0f;
#if true
                    // ????????????????????????????FixedUpdate????????
                    float fixedNextTime = Time.time + Time.smoothDeltaTime;
                    float fixedInterval = fixedNextTime - Time.fixedTime;
                    int nextFixedCount = math.max((int)(fixedInterval / Time.fixedDeltaTime), 1);
                    fixedFutureRatio *= nextFixedCount;
#endif

                    //Debug.Log($"normalFutureRatio = {normalFutureRatio}");

                    // ???????????(??????????????????????????!)
                    var job = new ReadBoneJob1()
                    {
                        fixedUpdateCount = updateTime.FixedUpdateCount,
                        normalFutureRatio = normalFutureRatio,
                        fixedFutureRatio = fixedFutureRatio,
                        normalDeltaTime = Time.smoothDeltaTime,
                        fixedDeltaTime = Time.fixedDeltaTime,

                        bonePosList = bonePosList.ToJobArray(),
                        boneRotList = boneRotList.ToJobArray(),
                        boneSclList = boneSclList.ToJobArray(),
                        basePosList = basePosList.ToJobArray(),
                        baseRotList = baseRotList.ToJobArray(),
                        boneUnityPhysicsList = boneUnityPhysicsList.ToJobArray(),
                        futurePosList = futurePosList.ToJobArray(),
                        futureRotList = futureRotList.ToJobArray(),
                        boneFlagList = boneFlagList.ToJobArray(),
                    };
                    Compute.MasterJob = job.Schedule(boneList.GetTransformAccessArray(), Compute.MasterJob);
                }
            }
        }

        /// <summary>
        /// ?????????(??????)
        /// </summary>
        [BurstCompile]
        struct ReadBoneJob0 : IJobParallelForTransform
        {
            public int fixedUpdateCount;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> bonePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> boneRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> boneSclList;

            //[Unity.Collections.WriteOnly]
            public NativeArray<float3> basePosList;
            //[Unity.Collections.WriteOnly]
            public NativeArray<quaternion> baseRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> futurePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> futureRotList;

            [Unity.Collections.ReadOnly]
            public NativeArray<short> boneUnityPhysicsList;
            public NativeArray<byte> boneFlagList;

            // ?????????
            public void Execute(int index, TransformAccess transform)
            {
                // UnityPhysics?????????????????Transfrom?????????????????
                bool unityPhysics = boneUnityPhysicsList[index] > 0;
                var flag = boneFlagList[index];
                bool reset = (flag & Flag_Reset) != 0;

                if (unityPhysics == false || fixedUpdateCount > 0 || reset)
                {
                    // ????
                    // UnityPhysics???????????????
                    // ????????
                    float3 pos = transform.position;
                    quaternion rot = transform.rotation;

                    bonePosList[index] = pos;
                    boneRotList[index] = rot;

                    basePosList[index] = pos;
                    baseRotList[index] = rot;

                    futurePosList[index] = pos;
                    futureRotList[index] = rot;

                    // lossyScale??(???Unity2019.2.14????)
                    // ?????????????????????(???Transform.lossyScale???)
                    float4x4 m = transform.localToWorldMatrix;
                    var irot = math.inverse(rot);
                    var m2 = math.mul(new float4x4(irot, float3.zero), m);
                    var scl = new float3(m2.c0.x, m2.c1.y, m2.c2.z);
                    boneSclList[index] = scl;
                }
                else
                {
                    // UnityPhysics???????????????
                    bonePosList[index] = basePosList[index];
                    boneRotList[index] = baseRotList[index];
                }

                // ??????????
                if (reset && (unityPhysics == false || fixedUpdateCount > 0))
                {
                    flag = (byte)(flag & ~Flag_Reset);
                    boneFlagList[index] = flag;
                }
            }
        }

        /// <summary>
        /// ?????????(??????)
        /// </summary>
        [BurstCompile]
        struct ReadBoneJob1 : IJobParallelForTransform
        {
            public int fixedUpdateCount;
            public float normalFutureRatio;
            public float fixedFutureRatio;
            public float normalDeltaTime;
            public float fixedDeltaTime;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> bonePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> boneRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> boneSclList;

            public NativeArray<float3> basePosList;
            public NativeArray<quaternion> baseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<short> boneUnityPhysicsList;
            public NativeArray<float3> futurePosList;
            public NativeArray<quaternion> futureRotList;
            public NativeArray<byte> boneFlagList;

            // ?????????
            public void Execute(int index, TransformAccess transform)
            {
                bool unityPhysics = boneUnityPhysicsList[index] > 0;
                var flag = boneFlagList[index];
                bool reset = (flag & Flag_Reset) != 0;

                if (unityPhysics == false || fixedUpdateCount > 0 || reset)
                {
                    // ????
                    // UnityPhysics???????????????
                    // ????????
                    float3 pos = transform.position;
                    quaternion rot = transform.rotation;

                    if (reset)
                    {
                        // ????
                        //Debug.Log($"reset bone :{index}");
                        basePosList[index] = pos;
                        baseRotList[index] = rot;

                        bonePosList[index] = pos;
                        boneRotList[index] = rot;

                        futurePosList[index] = pos;
                        futureRotList[index] = rot;
                    }
                    else
                    {
                        // ??:????
                        //Debug.Log($"read bone :{index}");
                        var oldPos = basePosList[index];
                        var oldRot = baseRotList[index];

                        basePosList[index] = pos;
                        baseRotList[index] = rot;

                        // ????(v1.11.1)
                        float moveRatio = 0;
                        float angRatio = 0;
                        float deltaLength = math.distance(oldPos, pos);
                        float deltaAngle = math.degrees(math.abs(MathUtility.Angle(oldRot, rot)));
                        float dtime = unityPhysics ? fixedDeltaTime : normalDeltaTime;
                        if (dtime > Define.Compute.Epsilon)
                        {
                            float moveSpeed = deltaLength / dtime;
                            float angSpeed = deltaAngle / dtime;
                            //if (deltaLength > 1e-06f)
                            //    Debug.Log($"read bone :{index}, movesp:{moveSpeed}, angsp:{angSpeed}");
                            const float maxMoveSpeed = 1.0f;
                            moveRatio = moveSpeed > maxMoveSpeed ? maxMoveSpeed / moveSpeed : 1.0f;
                            const float maxAngleSpeed = 360.0f; // deg
                            angRatio = angSpeed > maxAngleSpeed ? maxAngleSpeed / angSpeed : 1.0f;
                        }

                        // ????
                        float ratio = unityPhysics ? fixedFutureRatio : normalFutureRatio; // ??????????????
                        pos = math.lerp(oldPos, pos, 1.0f + ratio * moveRatio);
                        rot = math.slerp(oldRot, rot, 1.0f + ratio * angRatio);
                        rot = math.normalize(rot);

                        bonePosList[index] = pos;
                        boneRotList[index] = rot;

                        // ?????????????
                        futurePosList[index] = pos;
                        futureRotList[index] = rot;
                    }

                    // lossyScale??(???Unity2019.2.14????)
                    // ?????????????????????(???Transform.lossyScale???)
                    float4x4 m = transform.localToWorldMatrix;
                    var irot = math.inverse(rot);
                    var m2 = math.mul(new float4x4(irot, float3.zero), m);
                    var scl = new float3(m2.c0.x, m2.c1.y, m2.c2.z);
                    boneSclList[index] = scl;
                }
                else
                {
                    // UnityPhysics???????????????
                    // ???????????????
                    bonePosList[index] = futurePosList[index];
                    boneRotList[index] = futureRotList[index];
                }

                // ??????????
                if (reset && (unityPhysics == false || fixedUpdateCount > 0))
                {
                    flag = (byte)(flag & ~Flag_Reset);
                    boneFlagList[index] = flag;
                }
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????????????
        /// </summary>
        public void ConvertWorldToLocal()
        {
            if (WriteBoneCount > 0)
            {
                var job = new ConvertWorldToLocalJob()
                {
                    writeBoneIndexList = writeBoneIndexList.ToJobArray(),
                    boneFlagList = boneFlagList.ToJobArray(),
                    bonePosList = bonePosList.ToJobArray(),
                    boneRotList = boneRotList.ToJobArray(),
                    boneSclList = boneSclList.ToJobArray(),
                    boneParentIndexList = boneParentIndexList.ToJobArray(),

                    writeBonePosList = writeBonePosList.ToJobArray(),
                    writeBoneRotList = writeBoneRotList.ToJobArray(),
                };
                Compute.MasterJob = job.Schedule(writeBoneIndexList.Length, 16, Compute.MasterJob);
            }
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        [BurstCompile]
        struct ConvertWorldToLocalJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<int> writeBoneIndexList;

            [Unity.Collections.ReadOnly]
            public NativeArray<byte> boneFlagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> bonePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> boneRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> boneSclList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> boneParentIndexList;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> writeBonePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> writeBoneRotList;

            // ?????????
            public void Execute(int index)
            {
                int bindex = writeBoneIndexList[index];
                if (bindex == 0)
                    return;
                bindex--; // +1????????-1??

                // ???????????
                var flag = boneFlagList[bindex];
                if ((flag & Flag_Write) == 0)
                    return;

                var pos = bonePosList[bindex];
                var rot = boneRotList[bindex];

                int parentIndex = boneParentIndexList[bindex];
                if (parentIndex >= 0)
                {
                    // ??????????????????
                    var ppos = bonePosList[parentIndex];
                    var prot = boneRotList[parentIndex];
                    var pscl = boneSclList[parentIndex];
                    var iprot = math.inverse(prot);

                    var v = pos - ppos;
                    var lpos = math.mul(iprot, v);
                    lpos /= pscl;
                    var lrot = math.mul(iprot, rot);

                    // ??????????
                    if (pscl.x < 0 || pscl.y < 0 || pscl.z < 0)
                        lrot = new quaternion(lrot.value * new float4(-math.sign(pscl), 1));

                    writeBonePosList[index] = lpos;
                    writeBoneRotList[index] = lrot;
                }
                else
                {
                    // ???????????????????
                    writeBonePosList[index] = pos;
                    writeBoneRotList[index] = rot;
                }
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????????
        /// </summary>
        public void WriteBoneToTransform(int bufferIndex)
        {
            if (WriteBoneCount > 0)
            {
                var job = new WriteBontToTransformJob2()
                {
                    fixedUpdateCount = manager.UpdateTime.FixedUpdateCount,

                    boneFlagList = boneFlagList.ToJobArray(bufferIndex),
                    writeBoneIndexList = writeBoneIndexList.ToJobArray(bufferIndex),
                    boneParentIndexList = boneParentIndexList.ToJobArray(),
                    writeBonePosList = writeBonePosList.ToJobArray(bufferIndex),
                    writeBoneRotList = writeBoneRotList.ToJobArray(bufferIndex),
                    boneUnityPhysicsList = boneUnityPhysicsList.ToJobArray(),
                };
                Compute.MasterJob = job.Schedule(writeBoneList.GetTransformAccessArray(), Compute.MasterJob);
            }
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        [BurstCompile]
        struct WriteBontToTransformJob2 : IJobParallelForTransform
        {
            public int fixedUpdateCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<byte> boneFlagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> writeBoneIndexList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> boneParentIndexList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> writeBonePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> writeBoneRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<short> boneUnityPhysicsList;

            // ??????????????
            public void Execute(int index, TransformAccess transform)
            {
                if (index >= writeBoneIndexList.Length)
                    return;

                int bindex = writeBoneIndexList[index];
                if (bindex == 0)
                    return;
                bindex--; // +1????????-1??

                // ???????????
                var flag = boneFlagList[bindex];
                if ((flag & Flag_Write) == 0)
                    return;

                bool unityPhysics = boneUnityPhysicsList[bindex] > 0;
                if (unityPhysics == false || fixedUpdateCount > 0)
                {
                    var pos = writeBonePosList[index];
                    var rot = writeBoneRotList[index];

                    int parentIndex = boneParentIndexList[bindex];
                    //Debug.Log($"Write Bone:{bindex} Parent:{parentIndex} Pos:{pos}");

                    if (parentIndex >= 0)
                    {
                        // ????????????????????
                        transform.localPosition = pos;
                        transform.localRotation = rot;
                    }
                    else
                    {
                        // ?????????????????
                        transform.position = pos;
                        transform.rotation = rot;
                    }
                }
            }
        }

        //=========================================================================================
        /// <summary>
        /// ????????????????????
        /// ??????????
        /// </summary>
        public void CopyBoneBuffer()
        {
            var job0 = new CopyBoneJob0()
            {
                bonePosList = writeBonePosList.ToJobArray(),
                boneRotList = writeBoneRotList.ToJobArray(),

                backBonePosList = writeBonePosList.ToJobArray(1),
                backBoneRotList = writeBoneRotList.ToJobArray(1),
            };
            var jobHandle0 = job0.Schedule(writeBonePosList.Length, 16);

            var job1 = new CopyBoneJob1()
            {
                writeBoneIndexList = writeBoneIndexList.ToJobArray(),

                backWriteBoneIndexList = writeBoneIndexList.ToJobArray(1),
            };
            var jobHandle1 = job1.Schedule(writeBoneIndexList.Length, 16);

            var job2 = new CopyBoneJob2()
            {
                boneFlagList = boneFlagList.ToJobArray(),
                backBoneFlagList = boneFlagList.ToJobArray(1),
            };
            var jobHandle2 = job2.Schedule(boneFlagList.Length, 16);

            Compute.MasterJob = JobHandle.CombineDependencies(jobHandle0, jobHandle1, jobHandle2);
        }

        [BurstCompile]
        struct CopyBoneJob0 : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> bonePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> boneRotList;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> backBonePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> backBoneRotList;

            public void Execute(int index)
            {
                backBonePosList[index] = bonePosList[index];
                backBoneRotList[index] = boneRotList[index];
            }
        }

        [BurstCompile]
        struct CopyBoneJob1 : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<int> writeBoneIndexList;

            [Unity.Collections.WriteOnly]
            public NativeArray<int> backWriteBoneIndexList;

            public void Execute(int index)
            {
                backWriteBoneIndexList[index] = writeBoneIndexList[index];
            }
        }

        [BurstCompile]
        struct CopyBoneJob2 : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<byte> boneFlagList;

            [Unity.Collections.WriteOnly]
            public NativeArray<byte> backBoneFlagList;

            public void Execute(int index)
            {
                backBoneFlagList[index] = boneFlagList[index];
            }
        }
    }
}
