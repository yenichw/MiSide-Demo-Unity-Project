// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace MagicaCloth
{
    /// <summary>
    /// ?????????
    /// </summary>
    public class PhysicsManagerParticleData : PhysicsManagerAccess
    {
        /// <summary>
        /// ????????????
        /// </summary>
        public const uint Flag_Enable = 0x00000001; // ?????
        public const uint Flag_Kinematic = 0x00000002; // ????
        public const uint Flag_Hold = 0x00000004; // ?????(?????????????)
        //public const uint Flag_Collision = 0x00000008; // ???????
        public const uint Flag_Collider = 0x00000010; // ?????
        public const uint Flag_Plane = 0x00000020; // ????
        public const uint Flag_CapsuleX = 0x00000040; // ????X???
        public const uint Flag_CapsuleY = 0x00000080; // ????Y???
        public const uint Flag_CapsuleZ = 0x00000100; // ????Z???
        public const uint Flag_Box = 0x00000200; // ????(??)
        public const uint Flag_TriangleRotation = 0x00000400;   // TriangleWorker???????

        public const uint Flag_Transform_Read_Pos = 0x00001000; // pos????????????????
        public const uint Flag_Transform_Read_Rot = 0x00002000; // rot????????????????
        public const uint Flag_Transform_Read_Base = 0x00004000; // basePos/baseRot????????????????
        public const uint Flag_Transform_Read_Local = 0x00008000; // ????????(=???????????)
        public const uint Flag_Transform_Read_Scl = 0x00010000; // ???????????
        public const uint Flag_Transform_Write = 0x00020000; // ?????????pos/rot?????
        public const uint Flag_Transform_Restore = 0x00040000; // ????????????localPos/localRot???
        public const uint Flag_Transform_UnityPhysics = 0x00080000; // FixedUpdate????
        public const uint Flag_Transform_Parent = 0x00100000; // ????????????????

        public const uint Flag_Step_Update = 0x01000000; // Old -> Base ????????????
        public const uint Flag_Reset_Position = 0x02000000; // ???????Base???????
        //public const uint Flag_Friction = 0x04000000; // ??(?????????)

        /// <summary>
        /// ???????????
        /// </summary>
        public struct ParticleFlag
        {
            /// <summary>
            /// ?????????
            /// </summary>
            public uint flag;

            /// <summary>
            /// ???????
            /// </summary>
            /// <param name="flags"></param>
            public ParticleFlag(params uint[] flags)
            {
                flag = 0;
                foreach (var f in flags)
                    flag |= f;
            }

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

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return (flag & Flag_Enable) != 0;
            }

            /// <summary>
            /// ????????
            /// </summary>
            /// <param name="sw"></param>
            public void SetEnable(bool sw)
            {
                if (sw)
                    flag |= Flag_Enable;
                else
                    flag &= ~Flag_Enable;
            }

            /// <summary>
            /// ??????????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsFixed()
            {
                return (flag & (Flag_Kinematic | Flag_Hold)) != 0;
            }

            /// <summary>
            /// ???????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsMove()
            {
                return (flag & (Flag_Kinematic | Flag_Hold)) == 0;
            }

            /// <summary>
            /// ????????????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsKinematic()
            {
                return (flag & Flag_Kinematic) != 0;
            }

            /// <summary>
            /// ????????????????????????
            /// </summary>
            /// <returns></returns>
            //public bool IsCollision()
            //{
            //    return (flag & Flag_Collision) != 0;
            //}

            /// <summary>
            /// ???????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsHold()
            {
                return (flag & Flag_Hold) != 0;
            }

            /// <summary>
            /// ???????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsCollider()
            {
                return (flag & Flag_Collider) != 0;
            }

            /// <summary>
            /// ?????????????????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsReadTransform()
            {
                return (flag & (Flag_Transform_Read_Pos | Flag_Transform_Read_Rot | Flag_Transform_Read_Base)) != 0;
            }

            /// <summary>
            /// ??????????????????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsWriteTransform()
            {
                return (flag & Flag_Transform_Write) != 0;
            }

            /// <summary>
            /// ????????????????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsRestoreTransform()
            {
                return (flag & Flag_Transform_Restore) != 0;
                //return (flag & (Flag_Transform_Restore_Pos | Flag_Transform_Restore_Rot)) != 0;
            }

            /// <summary>
            /// ?????????FixedUpdate????????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsUnityPhysics()
            {
                return (flag & Flag_Transform_UnityPhysics) != 0;
            }

            /// <summary>
            /// ??????????????????????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsReadSclTransform()
            {
                return (flag & Flag_Transform_Read_Scl) != 0;
            }

            /// <summary>
            /// ?????????????????????????????????
            /// </summary>
            /// <returns></returns>
            public bool IsParentTransform()
            {
                return (flag & Flag_Transform_Parent) != 0;
            }

        }

        //=========================================================================================
        // ?????????(?????????????????)

        /// <summary>
        /// ??????
        /// </summary>
        public FixedChunkNativeArray<ParticleFlag> flagList;

        /// <summary>
        /// ???????ID(0=?????)
        /// </summary>
        public FixedChunkNativeArray<int> teamIdList;

        /// <summary>
        /// ???????
        /// </summary>
        public FixedChunkNativeArray<float3> posList;

        /// <summary>
        /// ???????
        /// </summary>
        public FixedChunkNativeArray<quaternion> rotList;

        /// <summary>
        /// 1????????
        /// </summary>
        public FixedChunkNativeArray<float3> oldPosList;

        /// <summary>
        /// 1????????
        /// </summary>
        public FixedChunkNativeArray<quaternion> oldRotList;

        /// <summary>
        /// 1????????(??????)
        /// </summary>
        public FixedChunkNativeArray<float3> oldSlowPosList;

        /// <summary>
        /// ????????????
        /// </summary>
        public FixedChunkNativeArray<float3> localPosList;

        /// <summary>
        /// ??????????
        /// </summary>
        public FixedChunkNativeArray<float3> basePosList;

        /// <summary>
        /// ??????????
        /// </summary>
        public FixedChunkNativeArray<quaternion> baseRotList;

        /// <summary>
        /// ??????????
        /// </summary>
        public FixedChunkNativeArray<float3> snapBasePosList;

        /// <summary>
        /// ??????????
        /// </summary>
        public FixedChunkNativeArray<quaternion> snapBaseRotList;

        /// <summary>
        /// 1??????????
        /// </summary>
        public FixedChunkNativeArray<float3> oldBasePosList;

        /// <summary>
        /// 1??????????
        /// </summary>
        public FixedChunkNativeArray<quaternion> oldBaseRotList;




        /// <summary>
        /// ???????????
        /// </summary>
        public FixedChunkNativeArray<float> depthList;

        /// <summary>
        /// ?????
        /// </summary>
        public FixedChunkNativeArray<float3> radiusList;

        /// <summary>
        /// ?????????????????????
        /// ??????(-1)
        /// </summary>
        public FixedChunkNativeArray<int> restoreTransformIndexList;

        /// <summary>
        /// ????/???????????????????????
        /// ??????(-1)
        /// </summary>
        public FixedChunkNativeArray<int> transformIndexList;

        /// <summary>
        /// ??????????
        /// </summary>
        public FixedChunkNativeArray<float> frictionList;

        /// <summary>
        /// ????????????
        /// </summary>
        public FixedChunkNativeArray<float> staticFrictionList;

        /// <summary>
        /// ????????
        /// </summary>
        public FixedChunkNativeArray<float3> velocityList;

        /// <summary>
        /// ???????ID(0=??)
        /// </summary>
        public FixedChunkNativeArray<int> collisionLinkIdList;

        /// <summary>
        /// ????????????
        /// </summary>
        public FixedChunkNativeArray<float3> collisionNormalList;

        /// <summary>
        /// ????????0
        /// </summary>
        FixedChunkNativeArray<float3> nextPos0List;

        /// <summary>
        /// ????????1
        /// </summary>
        FixedChunkNativeArray<float3> nextPos1List;

        /// <summary>
        /// ?????????????????
        /// </summary>
        int nextPosSwitch = 0;

        /// <summary>
        /// ????????0
        /// </summary>
        FixedChunkNativeArray<quaternion> nextRot0List;

        /// <summary>
        /// ????????1
        /// </summary>
        FixedChunkNativeArray<quaternion> nextRot1List;

        /// <summary>
        /// ?????????????????
        /// </summary>
        int nextRotSwitch = 0;

        //=========================================================================================
        /// <summary>
        /// ??????
        /// </summary>
        private int colliderCount;

        //=========================================================================================
        /// <summary>
        /// ????
        /// </summary>
        public override void Create()
        {
            flagList = new FixedChunkNativeArray<ParticleFlag>();
            teamIdList = new FixedChunkNativeArray<int>();
            posList = new FixedChunkNativeArray<float3>();
            rotList = new FixedChunkNativeArray<quaternion>();
            oldPosList = new FixedChunkNativeArray<float3>();
            oldRotList = new FixedChunkNativeArray<quaternion>();
            oldSlowPosList = new FixedChunkNativeArray<float3>();
            localPosList = new FixedChunkNativeArray<float3>();
            basePosList = new FixedChunkNativeArray<float3>();
            baseRotList = new FixedChunkNativeArray<quaternion>();
            snapBasePosList = new FixedChunkNativeArray<float3>();
            snapBaseRotList = new FixedChunkNativeArray<quaternion>();
            oldBasePosList = new FixedChunkNativeArray<float3>();
            oldBaseRotList = new FixedChunkNativeArray<quaternion>();
            depthList = new FixedChunkNativeArray<float>();
            radiusList = new FixedChunkNativeArray<float3>();
            restoreTransformIndexList = new FixedChunkNativeArray<int>();
            transformIndexList = new FixedChunkNativeArray<int>();
            frictionList = new FixedChunkNativeArray<float>();
            staticFrictionList = new FixedChunkNativeArray<float>();
            velocityList = new FixedChunkNativeArray<float3>();
            collisionLinkIdList = new FixedChunkNativeArray<int>();
            collisionNormalList = new FixedChunkNativeArray<float3>();
            nextPos0List = new FixedChunkNativeArray<float3>();
            nextPos1List = new FixedChunkNativeArray<float3>();
            nextRot0List = new FixedChunkNativeArray<quaternion>();
            nextRot1List = new FixedChunkNativeArray<quaternion>();

            // ??????[0]?????????0?????????
            var c = CreateParticle(Flag_Kinematic, 0, 0, quaternion.identity, 0.0f, 1.0f, 0);
            SetEnable(c, false, null, null, null);
        }

        /// <summary>
        /// ??
        /// </summary>
        public override void Dispose()
        {
            if (flagList == null)
                return;

            flagList.Dispose();
            teamIdList.Dispose();
            posList.Dispose();
            rotList.Dispose();
            oldPosList.Dispose();
            oldRotList.Dispose();
            oldSlowPosList.Dispose();
            localPosList.Dispose();
            basePosList.Dispose();
            baseRotList.Dispose();
            snapBasePosList.Dispose();
            snapBaseRotList.Dispose();
            oldBasePosList.Dispose();
            oldBaseRotList.Dispose();
            depthList.Dispose();
            radiusList.Dispose();
            restoreTransformIndexList.Dispose();
            transformIndexList.Dispose();
            frictionList.Dispose();
            staticFrictionList.Dispose();
            velocityList.Dispose();
            collisionLinkIdList.Dispose();
            collisionNormalList.Dispose();
            nextPos0List.Dispose();
            nextPos1List.Dispose();
            nextRot0List.Dispose();
            nextRot1List.Dispose();
        }

        //=========================================================================================
        /// <summary>
        /// ???????1???
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="wpos"></param>
        /// <param name="wrot"></param>
        /// <param name="radius"></param>
        /// <param name="mass"></param>
        /// <param name="gravity"></param>
        /// <param name="drag"></param>
        /// <param name="depth"></param>
        /// <param name="maxVelocity"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public ChunkData CreateParticle(
            uint flag,
            int team,
            float3 wpos, quaternion wrot,
            float depth,
            float3 radius,
            float3 targetLocalPos
            )
        {
            flag |= Flag_Enable; // ????????
            var pf = new ParticleFlag(flag);
            var c = flagList.Add(pf);
            teamIdList.Add(team);
            posList.Add(wpos);
            rotList.Add(wrot);
            oldPosList.Add(wpos);
            oldRotList.Add(wrot);
            oldSlowPosList.Add(wpos);
            localPosList.Add(targetLocalPos);
            basePosList.Add(wpos);
            baseRotList.Add(wrot);
            snapBasePosList.Add(wpos);
            snapBaseRotList.Add(wrot);
            oldBasePosList.Add(wpos);
            oldBaseRotList.Add(wrot);
            depthList.Add(depth);
            radiusList.Add(radius);
            frictionList.Add(0.0f);
            staticFrictionList.Add(0.0f);
            velocityList.Add(0);
            collisionLinkIdList.Add(0);
            collisionNormalList.Add(0);
            nextPos0List.Add(0);
            nextPos1List.Add(0);
            nextRot0List.Add(quaternion.identity);
            nextRot1List.Add(quaternion.identity);

            // ????????????
            int restoreTransformIndex = -1;
            int transformIndex = -1;
            restoreTransformIndexList.Add(restoreTransformIndex);
            transformIndexList.Add(transformIndex);

            // ?????????
            if (pf.IsCollider())
                colliderCount++;

            return c;
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="team"></param>
        /// <param name="count"></param>
        /// <param name="funcFlag"></param>
        /// <param name="funcWpos"></param>
        /// <param name="funcWrot"></param>
        /// <param name="funcLpos"></param>
        /// <param name="funcLrot"></param>
        /// <param name="funcRadius"></param>
        /// <param name="funcMass"></param>
        /// <param name="funcGravity"></param>
        /// <param name="funcDrag"></param>
        /// <param name="funcDepth"></param>
        /// <param name="funcMaxVelocity"></param>
        /// <param name="funcTarget"></param>
        /// <param name="funcTargetLocalPos"></param>
        /// <returns></returns>
        public ChunkData CreateParticle(
            int team,
            int count,
            System.Func<int, uint> funcFlag,
            System.Func<int, float3> funcWpos,
            System.Func<int, quaternion> funcWrot,
            System.Func<int, float> funcDepth,
            System.Func<int, float3> funcRadius,
            System.Func<int, float3> funcTargetLocalPos
            )
        {
            var c = flagList.AddChunk(count);
            teamIdList.AddChunk(count);
            posList.AddChunk(count);
            rotList.AddChunk(count);
            oldPosList.AddChunk(count);
            oldRotList.AddChunk(count);
            oldSlowPosList.AddChunk(count);
            localPosList.AddChunk(count);
            basePosList.AddChunk(count);
            baseRotList.AddChunk(count);
            snapBasePosList.AddChunk(count);
            snapBaseRotList.AddChunk(count);
            oldBasePosList.AddChunk(count);
            oldBaseRotList.AddChunk(count);
            depthList.AddChunk(count);
            radiusList.AddChunk(count);
            frictionList.AddChunk(count);
            staticFrictionList.AddChunk(count);
            velocityList.AddChunk(count);
            collisionLinkIdList.AddChunk(count);
            collisionNormalList.AddChunk(count);
            nextPos0List.AddChunk(count);
            nextPos1List.AddChunk(count);
            nextRot0List.AddChunk(count);
            nextRot1List.AddChunk(count);
            restoreTransformIndexList.AddChunk(count);
            transformIndexList.AddChunk(count);

            teamIdList.Fill(c, team);
            nextRot0List.Fill(c, quaternion.identity);
            nextRot1List.Fill(c, quaternion.identity);

            for (int i = 0; i < count; i++)
            {
                int pindex = c.startIndex + i;

                uint flag = Flag_Enable;
                float3 wpos = 0;
                quaternion wrot = quaternion.identity;
                float3 tlpos = 0;
                float depth = 0;
                float3 radius = 0;
                int restoreTransformIndex = -1;
                int transformIndex = -1;

                if (funcFlag != null)
                    flag |= funcFlag(i);
                var pf = new ParticleFlag(flag);
                if (funcWpos != null)
                    wpos = funcWpos(i);
                if (funcWrot != null)
                    wrot = funcWrot(i);
                if (funcTargetLocalPos != null)
                    tlpos = funcTargetLocalPos(i);
                if (funcDepth != null)
                    depth = funcDepth(i);
                if (funcRadius != null)
                    radius = funcRadius(i);

                flagList[pindex] = pf;
                posList[pindex] = wpos;
                rotList[pindex] = wrot;
                oldPosList[pindex] = wpos;
                oldRotList[pindex] = wrot;
                oldSlowPosList[pindex] = wpos;
                localPosList[pindex] = tlpos;
                basePosList[pindex] = wpos;
                baseRotList[pindex] = wrot;
                snapBasePosList[pindex] = wpos;
                snapBaseRotList[pindex] = wrot;
                oldBasePosList[pindex] = wpos;
                oldBaseRotList[pindex] = wrot;
                depthList[pindex] = depth;
                radiusList[pindex] = radius;
                restoreTransformIndexList[pindex] = restoreTransformIndex;
                transformIndexList[pindex] = transformIndex;

                // ?????????
                if (pf.IsCollider())
                    colliderCount++;
            }

            return c;
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="index"></param>
        public void RemoveParticle(ChunkData c)
        {
            for (int i = 0; i < c.dataLength; i++)
            {
                int pindex = c.startIndex + i;

                var pf = flagList[pindex];

                // ?????????
                if (pf.IsCollider())
                    colliderCount--;
            }

            flagList.RemoveChunk(c);
            teamIdList.RemoveChunk(c);
            posList.RemoveChunk(c);
            rotList.RemoveChunk(c);
            oldPosList.RemoveChunk(c);
            oldRotList.RemoveChunk(c);
            oldSlowPosList.RemoveChunk(c);
            localPosList.RemoveChunk(c);
            basePosList.RemoveChunk(c);
            baseRotList.RemoveChunk(c);
            snapBasePosList.RemoveChunk(c);
            snapBaseRotList.RemoveChunk(c);
            oldBasePosList.RemoveChunk(c);
            oldBaseRotList.RemoveChunk(c);
            depthList.RemoveChunk(c);
            radiusList.RemoveChunk(c);

            frictionList.RemoveChunk(c);
            staticFrictionList.RemoveChunk(c);
            velocityList.RemoveChunk(c);
            collisionLinkIdList.RemoveChunk(c);
            collisionNormalList.RemoveChunk(c);
            nextPos0List.RemoveChunk(c);
            nextPos1List.RemoveChunk(c);
            nextRot0List.RemoveChunk(c);
            nextRot1List.RemoveChunk(c);

            restoreTransformIndexList.RemoveChunk(c);
            transformIndexList.RemoveChunk(c);
        }

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sw"></param>
        public void SetEnable(
            ChunkData c,
            bool sw,
            System.Func<int, Transform> funcTarget,
            System.Func<int, float3> funcLpos,
            System.Func<int, quaternion> funcLrot
            )
        {
            for (int i = 0; i < c.dataLength; i++)
            {
                int index = c.startIndex + i;

                var flag = flagList[index];
                flag.SetEnable(sw);
                if (sw)
                {
                    // ???
                    // ?????????????
                    flag.SetFlag(Flag_Reset_Position, true);

                    // ?????
                    if (funcTarget != null)
                    {
                        var target = funcTarget(i);
                        if (target != null)
                        {
                            // ??????????????
                            if (flag.IsReadTransform() && transformIndexList[index] == -1)
                            {
                                // ????????????
                                int windex = flag.IsWriteTransform() ? index : -1;

                                // ???????????????
                                bool parent = flag.IsParentTransform();

                                var transformIndex = Bone.AddBone(target, windex, parent);
                                transformIndexList[index] = transformIndex;

                                // ????UnityPhysics????
                                if (flag.IsUnityPhysics())
                                    Bone.ChangeUnityPhysicsCount(transformIndex, true);
                            }

                            // ????????????
                            if (flag.IsRestoreTransform() && restoreTransformIndexList[index] == -1)
                            {
                                float3 lpos = funcLpos != null ? funcLpos(i) : 0;
                                quaternion lrot = funcLrot != null ? funcLrot(i) : quaternion.identity;
                                restoreTransformIndexList[index] = Bone.AddRestoreBone(target, lpos, lrot, transformIndexList[index]);
                            }
                        }
                    }
                }
                else
                {
                    // ???
                    // ???????
                    // ????????????
                    if (flag.IsRestoreTransform())
                    {
                        var restoreTransformIndex = restoreTransformIndexList[index];
                        if (restoreTransformIndex >= 0)
                        {
                            Bone.RemoveRestoreBone(restoreTransformIndex);
                            restoreTransformIndexList[index] = -1;
                        }
                    }

                    // ????/??????????????
                    if (flag.IsReadTransform())
                    {
                        var transformIndex = transformIndexList[index];
                        if (transformIndex >= 0)
                        {
                            // ????UnityPhysics????
                            if (flag.IsUnityPhysics())
                                Bone.ChangeUnityPhysicsCount(transformIndex, false);

                            // ?????
                            int windex = flag.IsWriteTransform() ? index : -1;
                            Bone.RemoveBone(transformIndex, windex);
                            transformIndexList[index] = -1;
                        }
                    }
                }

                flagList[index] = flag;
            }
        }

        /// <summary>
        /// ???????????
        /// </summary>
        /// <param name="index"></param>
        /// <param name="radius"></param>
        public void SetRadius(int index, float3 radius)
        {
            radiusList[index] = radius;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <param name="index"></param>
        /// <param name="lpos"></param>
        public void SetLocalPos(int index, Vector3 lpos)
        {
            localPosList[index] = lpos;
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        /// <param name="index"></param>
        /// <param name="sw"></param>
        //public void SetCollision(int index, bool sw)
        //{
        //    var flag = flagList[index];
        //    flag.SetFlag(Flag_Collision, sw);
        //    flagList[index] = flag;
        //}

        /// <summary>
        /// ????????????????????
        /// [0]?????????????????-1??
        /// </summary>
        public int Count
        {
            get
            {
                if (flagList == null)
                    return 0;

                return Mathf.Max(flagList.Count - 1, 0);
            }
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        public int Length
        {
            get
            {
                if (flagList == null)
                    return 0;

                return flagList.Length;
            }
        }

        /// <summary>
        /// ??????????
        /// </summary>
        public int ColliderCount
        {
            get
            {
                return colliderCount;
            }
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsValid(int index)
        {
            if (index < 0 || index >= Length)
                return false;

            return flagList[index].IsValid();
        }

        public int GetTeamId(int index)
        {
            return teamIdList[index];
        }

        /// <summary>
        /// ?????????????????????????????????
        /// </summary>
        /// <param name="index"></param>
        public void ResetFuturePredictionTransform(int index)
        {
            var transformIndex = transformIndexList[index];
            if (transformIndex >= 0)
            {
                Bone.ResetFuturePrediction(transformIndex);
            }
        }

        /// <summary>
        /// ?????????????????????????????????
        /// </summary>
        /// <param name="c"></param>
        public void ResetFuturePredictionTransform(ChunkData c)
        {
            for (int i = 0, index = c.startIndex; i < c.dataLength; i++, index++)
            {
                ResetFuturePredictionTransform(index);
            }
        }


        //=========================================================================================
        // nextPos????????
        /// <summary>
        /// ???nextPosList???
        /// </summary>
        public FixedChunkNativeArray<float3> InNextPosList
        {
            get
            {
                return nextPosSwitch == 0 ? nextPos0List : nextPos1List;
            }
        }

        /// <summary>
        /// ???nextPosList???
        /// </summary>
        public FixedChunkNativeArray<float3> OutNextPosList
        {
            get
            {
                return nextPosSwitch == 0 ? nextPos1List : nextPos0List;
            }
        }

        /// <summary>
        /// ???nextPosList?In/Out????
        /// </summary>
        public void SwitchingNextPosList()
        {
            nextPosSwitch = (nextPosSwitch + 1) % 2;
        }

        //=========================================================================================
        // nextRot????????
        /// <summary>
        /// ???nextRotList???
        /// </summary>
        public FixedChunkNativeArray<quaternion> InNextRotList
        {
            get
            {
                return nextRotSwitch == 0 ? nextRot0List : nextRot1List;
            }
        }

        /// <summary>
        /// ???nextRotList???
        /// </summary>
        public FixedChunkNativeArray<quaternion> OutNextRotList
        {
            get
            {
                return nextRotSwitch == 0 ? nextRot1List : nextRot0List;
            }
        }

        /// <summary>
        /// ???nextRotList?In/Out????
        /// </summary>
        public void SwitchingNextRotList()
        {
            nextRotSwitch = (nextRotSwitch + 1) % 2;
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????????
        /// ?????????????
        /// </summary>
        public void UpdateBoneToParticle()
        {
            if (Count == 0)
                return;

            var job = new CopyBoneToParticleJob()
            {
                teamData = Team.teamDataList.ToJobArray(),
                teamWorldInfluenceList = Team.teamWorldInfluenceList.ToJobArray(),

                flagList = flagList.ToJobArray(),
                depthList = depthList.ToJobArray(),
                transformIndexList = transformIndexList.ToJobArray(),
                localPosList = localPosList.ToJobArray(),
                teamIdList = teamIdList.ToJobArray(),
                velocityList = velocityList.ToJobArray(),

                bonePosList = Bone.bonePosList.ToJobArray(),
                boneRotList = Bone.boneRotList.ToJobArray(),
                boneSclList = Bone.boneSclList.ToJobArray(),

                posList = posList.ToJobArray(),
                oldPosList = oldPosList.ToJobArray(),
                oldRotList = oldRotList.ToJobArray(),
                oldSlowPosList = oldSlowPosList.ToJobArray(),

                rotList = rotList.ToJobArray(),

                //basePosList = basePosList.ToJobArray(),
                //baseRotList = baseRotList.ToJobArray(),
                snapBasePosList = snapBasePosList.ToJobArray(),
                snapBaseRotList = snapBaseRotList.ToJobArray(),
                oldBasePosList = oldBasePosList.ToJobArray(),
                oldBaseRotList = oldBaseRotList.ToJobArray(),
                nextPosList = InNextPosList.ToJobArray(),
            };
            Compute.MasterJob = job.Schedule(Particle.Length, 64, Compute.MasterJob);
        }

        [BurstCompile]
        struct CopyBoneToParticleJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamData;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.WorldInfluence> teamWorldInfluenceList;

            // ????????
            [Unity.Collections.ReadOnly]
            public NativeArray<ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> depthList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> transformIndexList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> localPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;
            public NativeArray<float3> velocityList;

            // ??????????
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> bonePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> boneRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> boneSclList;

            // ????????
            public NativeArray<float3> posList;
            public NativeArray<float3> oldPosList;
            public NativeArray<quaternion> oldRotList;
            public NativeArray<float3> oldSlowPosList;

            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> rotList;
            //[Unity.Collections.WriteOnly]
            //public NativeArray<float3> basePosList;
            //[Unity.Collections.WriteOnly]
            //public NativeArray<quaternion> baseRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> snapBasePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> snapBaseRotList;
            public NativeArray<float3> oldBasePosList;
            public NativeArray<quaternion> oldBaseRotList;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> nextPosList;

            // ????????
            public void Execute(int index)
            {
                var flag = flagList[index];
                if (flag.IsValid() == false)
                    return;

                float depth = depthList[index];

                // ??????????
                int teamId = teamIdList[index];
                var tdata = teamData[teamId];
                var wdata = teamWorldInfluenceList[teamId];
                float moveInfluence = wdata.moveInfluence.Evaluate(depth);
                float rotInfluence = wdata.rotInfluence.Evaluate(depth);

                // ??????????????0???
                if (tdata.IsFlag(PhysicsManagerTeamData.Flag_Reset_Keep))
                {
                    moveInfluence = 0;
                    rotInfluence = 0;
                }

                var oldpos = oldPosList[index];
                float3 offset = 0;

                // ????/??????
                {
                    // ???
                    float moveIgnoreRatio = wdata.moveIgnoreRatio;
                    float rotationIgnoreRatio = wdata.rotationIgnoreRatio;

                    // ????
                    float moveRatio = (1.0f - moveIgnoreRatio) * (1.0f - moveInfluence) + moveIgnoreRatio;
                    float3 offpos = wdata.moveOffset * moveRatio;

                    // ????
                    float rotRatio = (1.0f - rotationIgnoreRatio) * (1.0f - rotInfluence) + rotationIgnoreRatio;
                    quaternion offrot = math.slerp(quaternion.identity, wdata.rotationOffset, rotRatio);

                    // ???????????????
                    var lpos = oldpos - wdata.oldPosition;
                    lpos = math.mul(offrot, lpos);
                    lpos += offpos;
                    var npos = wdata.oldPosition + lpos;
                    offset = npos - oldpos;

                    // ???????????
                    var vel = velocityList[index];
                    vel = math.mul(offrot, vel);
                    velocityList[index] = vel;

                    // ???????????
                    var oldrot = oldRotList[index];
                    oldrot = math.mul(offrot, oldrot);
                    oldRotList[index] = oldrot;

                    // ????????(v1.11.1)
                    oldBasePosList[index] = oldBasePosList[index] + offset;
                    var oldBaseRot = oldBaseRotList[index];
                    oldBaseRot = math.mul(offrot, oldBaseRot);
                    oldBaseRotList[index] = oldBaseRot;
                }

                oldPosList[index] = oldpos + offset;
                oldSlowPosList[index] = oldSlowPosList[index] + offset;
                if (flag.IsFixed())
                {
                    // ??????????????nextPos???????????????????????(1.8.3)
                    nextPosList[index] = oldpos + offset;
                }

                // ????????????????
                if (flag.IsReadTransform() == false)
                    return;

                // ???????????????????
                var tindex = transformIndexList[index];
                if (tindex < 0)
                {
                    // ?????Disable???????????????????????????!(v1.12.0)
                    return;
                }
                var bpos = bonePosList[tindex];
                var brot = boneRotList[tindex];

                // ?????????
                if (flag.IsFlag(Flag_Transform_Read_Local))
                {
                    var bscl = boneSclList[tindex];
                    bpos = bpos + math.mul(brot, localPosList[index] * bscl);
                }

                // ?????????
                if (flag.IsFlag(Flag_Transform_Read_Base))
                {
                    //basePosList[index] = bpos;
                    //baseRotList[index] = brot;
                    snapBasePosList[index] = bpos;
                    snapBaseRotList[index] = brot;
                }

                // ??????????
                if (flag.IsFlag(Flag_Transform_Read_Pos))
                {
                    posList[index] = bpos;
                }
                if (flag.IsFlag(Flag_Transform_Read_Rot))
                {
                    rotList[index] = brot;
                }
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????
        /// </summary>
        public void UpdateResetParticle()
        {
            if (Count == 0)
                return;

            var job = new ResetParticleJob()
            {
                teamData = Team.teamDataList.ToJobArray(),

                flagList = flagList.ToJobArray(),
                teamIdList = teamIdList.ToJobArray(),

                snapBasePosList = snapBasePosList.ToJobArray(),
                snapBaseRotList = snapBaseRotList.ToJobArray(),
                basePosList = basePosList.ToJobArray(),
                baseRotList = baseRotList.ToJobArray(),
                oldBasePosList = oldBasePosList.ToJobArray(),
                oldBaseRotList = oldBaseRotList.ToJobArray(),

                posList = posList.ToJobArray(),
                rotList = rotList.ToJobArray(),

                oldPosList = oldPosList.ToJobArray(),
                oldRotList = oldRotList.ToJobArray(),
                oldSlowPosList = oldSlowPosList.ToJobArray(),

                velocityList = velocityList.ToJobArray(),

                nextPosList = InNextPosList.ToJobArray(),
                nextRotList = InNextRotList.ToJobArray(),

                localPosList = localPosList.ToJobArray(),
            };
            Compute.MasterJob = job.Schedule(Particle.Length, 64, Compute.MasterJob);
        }

        [BurstCompile]
        struct ResetParticleJob : IJobParallelFor
        {
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamData;

            // ????????
            public NativeArray<ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> snapBasePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> snapBaseRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> basePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> baseRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> oldBasePosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> oldBaseRotList;

            [Unity.Collections.WriteOnly]
            public NativeArray<float3> posList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> rotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> oldPosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> oldRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> oldSlowPosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> velocityList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> nextPosList;
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> nextRotList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> localPosList;

            // ????????
            public void Execute(int index)
            {
                var flag = flagList[index];
                if (flag.IsValid() == false)
                    return;

                // ??????????
                int teamId = teamIdList[index];
                var tdata = teamData[teamId];

                if (tdata.IsPause())
                    return;

                // ??????
                if (tdata.IsFlag(PhysicsManagerTeamData.Flag_Reset_Position) || flag.IsFlag(Flag_Reset_Position))
                {
                    //var basePos = basePosList[index];
                    //var baseRot = baseRotList[index];
                    var basePos = snapBasePosList[index];
                    var baseRot = snapBaseRotList[index];

                    basePosList[index] = basePos;
                    baseRotList[index] = baseRot;
                    oldBasePosList[index] = basePos;
                    oldBaseRotList[index] = baseRot;

                    posList[index] = basePos;
                    rotList[index] = baseRot;
                    oldPosList[index] = basePos;
                    oldRotList[index] = baseRot;
                    oldSlowPosList[index] = basePos;
                    velocityList[index] = 0;
                    nextPosList[index] = basePos;
                    nextRotList[index] = baseRot;

                    // ?????????localPos????????????????
                    if (flag.IsKinematic() == false)
                    {
                        localPosList[index] = 0;
                    }

                    // ??????
                    flag.SetFlag(Flag_Reset_Position, false);
                    flagList[index] = flag;
                }
            }
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????????
        /// </summary>
        public bool UpdateParticleToBone()
        {
            if (Count > 0 && Bone.WriteBoneCount > 0)
            {
                var job = new CopyParticleToBoneJob()
                {
                    flagList = flagList.ToJobArray(),
                    posList = posList.ToJobArray(),
                    rotList = rotList.ToJobArray(),

                    transformParticleIndexMap = Bone.writeBoneParticleIndexMap.Map,
                    writeBoneIndexList = Bone.writeBoneIndexList.ToJobArray(),
                    bonePosList = Bone.bonePosList.ToJobArray(),
                    boneRotList = Bone.boneRotList.ToJobArray(),
                };
                Compute.MasterJob = job.Schedule(Bone.writeBoneList.Length, 64, Compute.MasterJob);
                return true;
            }

            return false;
        }

        [BurstCompile]
        struct CopyParticleToBoneJob : IJobParallelFor
        {
            // ????????
            [Unity.Collections.ReadOnly]
            public NativeArray<ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> posList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> rotList;

            // ??????????
            [Unity.Collections.ReadOnly]
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
            public NativeParallelMultiHashMap<int, int> transformParticleIndexMap;
#else
            public NativeMultiHashMap<int, int> transformParticleIndexMap;
#endif
            [Unity.Collections.ReadOnly]
            public NativeArray<int> writeBoneIndexList;
            [NativeDisableParallelForRestriction]
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> bonePosList;
            [NativeDisableParallelForRestriction]
            [Unity.Collections.WriteOnly]
            public NativeArray<quaternion> boneRotList;

            // ??????????????
            public void Execute(int index)
            {
                int pindex;
#if MAGICACLOTH_USE_COLLECTIONS_130 && !MAGICACLOTH_USE_COLLECTIONS_200
                NativeParallelMultiHashMapIterator<int> iterator;
#else
                NativeMultiHashMapIterator<int> iterator;
#endif
                if (transformParticleIndexMap.TryGetFirstValue(index, out pindex, out iterator))
                {
                    // ????????????????????????????????
                    var flag = flagList[pindex];
                    if (flag.IsValid() == false)
                        return;

                    var pos = posList[pindex];
                    var rot = rotList[pindex];

                    // ??????????
                    int bindex = writeBoneIndexList[index] - 1; // +1????????-1??
                    bonePosList[bindex] = pos;
                    boneRotList[bindex] = rot;
                }
            }
        }

    }
}
