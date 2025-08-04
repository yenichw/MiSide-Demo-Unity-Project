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
    public class EdgeCollisionConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ?????
        /// todo:?????
        /// </summary>
        [System.Serializable]
        public struct EdgeCollisionData
        {
            /// <summary>
            /// ?????????????????
            /// </summary>
            public ushort vindex0;
            public ushort vindex1;

            /// <summary>
            /// ??????????????
            /// </summary>
            public int writeIndex0;
            public int writeIndex1;

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return vindex0 > 0 && vindex1 > 0;
            }
        }
        FixedChunkNativeArray<EdgeCollisionData> dataList;

        /// <summary>
        /// ????????????????
        /// </summary>
        FixedChunkNativeArray<short> groupIndexList;

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        FixedChunkNativeArray<ReferenceDataIndex> refDataList;

        /// <summary>
        /// ??????????????
        /// </summary>
        FixedChunkNativeArray<float3> writeBuffer;

        /// <summary>
        /// ????????????
        /// </summary>
        public struct GroupData
        {
            public int teamId;

            public int active;

            public float edgeRadius;

            /// <summary>
            /// ???????
            /// </summary>
            public ChunkData dataChunk;

            /// <summary>
            /// ???????????
            /// </summary>
            public ChunkData groupIndexChunk;

            /// <summary>
            /// ?????????????
            /// </summary>
            public ChunkData refDataChunk;

            /// <summary>
            /// ???????????????
            /// </summary>
            public ChunkData writeDataChunk;
        }
        FixedNativeList<GroupData> groupList;

        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<EdgeCollisionData>();
            groupIndexList = new FixedChunkNativeArray<short>();
            refDataList = new FixedChunkNativeArray<ReferenceDataIndex>();
            writeBuffer = new FixedChunkNativeArray<float3>();
            groupList = new FixedNativeList<GroupData>();
        }

        public override void Release()
        {
            dataList.Dispose();
            groupIndexList.Dispose();
            refDataList.Dispose();
            writeBuffer.Dispose();
            groupList.Dispose();
        }

        //=========================================================================================
        public int AddGroup(int teamId, bool active, float edgeRadius, EdgeCollisionData[] dataArray, ReferenceDataIndex[] refDataArray, int writeBufferCount)
        {
            if (dataArray == null || dataArray.Length == 0 || refDataArray == null || refDataArray.Length == 0 || writeBufferCount == 0)
                return -1;

            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            // ?????????
            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.edgeRadius = edgeRadius;
            //gdata.stiffness.Setup(stiffness);
            gdata.dataChunk = dataList.AddChunk(dataArray.Length);
            gdata.groupIndexChunk = groupIndexList.AddChunk(dataArray.Length);
            gdata.refDataChunk = refDataList.AddChunk(refDataArray.Length);
            gdata.writeDataChunk = writeBuffer.AddChunk(writeBufferCount);

            // ??????????
            dataList.ToJobArray().CopyFromFast(gdata.dataChunk.startIndex, dataArray);
            refDataList.ToJobArray().CopyFromFast(gdata.refDataChunk.startIndex, refDataArray);

            int group = groupList.Add(gdata);

            // ????????????????
            groupIndexList.Fill(gdata.groupIndexChunk, (short)group);


            return group;
        }

        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.edgeCollisionGroupIndex;
            if (group < 0)
                return;

            var cdata = groupList[group];

            // ?????????
            dataList.RemoveChunk(cdata.dataChunk);
            refDataList.RemoveChunk(cdata.refDataChunk);
            writeBuffer.RemoveChunk(cdata.writeDataChunk);
            groupIndexList.RemoveChunk(cdata.groupIndexChunk);

            // ?????
            groupList.Remove(group);
        }

        public void ChangeParam(int teamId, bool active, float edgeRadius)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.edgeCollisionGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            gdata.edgeRadius = edgeRadius;
            //gdata.stiffness.Setup(stiffness);
            groupList[group] = gdata;
        }

        //public int ActiveCount
        //{
        //    get
        //    {
        //        int cnt = 0;
        //        for (int i = 0; i < groupList.Length; i++)
        //            if (groupList[i].active == 1)
        //                cnt++;
        //        return cnt;
        //    }
        //}

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

            // ????1:????????
            var job = new EdgeCollisionCalcJob()
            {
                updatePower = updatePower,
                runCount = runCount,

                groupDataList = groupList.ToJobArray(),
                dataList = dataList.ToJobArray(),
                groupIndexList = groupIndexList.ToJobArray(),

                //colliderMap = Manager.Team.colliderMap.Map,
                colliderList = Manager.Team.colliderList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                flagList = Manager.Particle.flagList.ToJobArray(),
                radiusList = Manager.Particle.radiusList.ToJobArray(),
                posList = Manager.Particle.posList.ToJobArray(),
                rotList = Manager.Particle.rotList.ToJobArray(),
                nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                nextRotList = Manager.Particle.InNextRotList.ToJobArray(),
                localPosList = Manager.Particle.localPosList.ToJobArray(),
                //oldPosList = Manager.Particle.oldPosList.ToJobArray(),
                transformIndexList = Manager.Particle.transformIndexList.ToJobArray(),

                boneSclList = Manager.Bone.boneSclList.ToJobArray(),

                writeBuffer = writeBuffer.ToJobArray(),
            };
            jobHandle = job.Schedule(dataList.Length, 64, jobHandle);

            // ????2:??????????
            var job2 = new EdgeCollisionSumJob()
            {
                runCount = runCount,

                groupDataList = groupList.ToJobArray(),
                refDataList = refDataList.ToJobArray(),
                writeBuffer = writeBuffer.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                teamIdList = Manager.Particle.teamIdList.ToJobArray(),
                flagList = Manager.Particle.flagList.ToJobArray(),

                inoutNextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                frictionList = Manager.Particle.frictionList.ToJobArray(),
            };
            jobHandle = job2.Schedule(Manager.Particle.Length, 64, jobHandle);

            return jobHandle;
        }

        [BurstCompile]
        struct EdgeCollisionCalcJob : IJobParallelFor
        {
            public float updatePower;
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<EdgeCollisionData> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<short> groupIndexList;

            //[Unity.Collections.ReadOnly]
            //public NativeMultiHashMap<int, int> colliderMap;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> colliderList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> radiusList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> posList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> rotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> nextPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> nextRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> localPosList;
            //[Unity.Collections.ReadOnly]
            //public NativeArray<float3> oldPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> transformIndexList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> boneSclList;

            [Unity.Collections.WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> writeBuffer;

            // ????????
            public void Execute(int index)
            {
                var data = dataList[index];
                if (data.IsValid() == false)
                    return;

                int gindex = groupIndexList[index];
                var gdata = groupDataList[gindex];
                if (gdata.teamId == 0 || gdata.active == 0)
                    return;

                var tdata = teamDataList[gdata.teamId];
                if (tdata.IsActive() == false)
                    return;
                // ????
                if (tdata.IsUpdate(runCount) == false)
                    return;

                int pstart = tdata.particleChunk.startIndex;

                float3 corr0 = 0;
                float3 corr1 = 0;

                int pindex0 = data.vindex0 + pstart;
                int pindex1 = data.vindex1 + pstart;

                float3 nextpos0 = nextPosList[pindex0];
                float3 nextpos1 = nextPosList[pindex1];

                //float3 oldpos0 = oldPosList[pindex0];
                //float3 oldpos1 = oldPosList[pindex1];

                // ??????
                float radius = gdata.edgeRadius;

                // ?????????corr???
                // ????????[?????(0)]->[??????(team)]
                int colliderTeam = 0;
                bool hitresult = false;
                for (int i = 0; i < 2; i++)
                {
                    // ??????????????
                    var c = teamDataList[colliderTeam].colliderChunk;

                    int dataIndex = c.startIndex;
                    for (int j = 0; j < c.useLength; j++, dataIndex++)
                    {
                        int cindex = colliderList[dataIndex];

                        var cflag = flagList[cindex];
                        if (cflag.IsValid() == false)
                            continue;

                        bool hit = false;

                        if (cflag.IsFlag(PhysicsManagerParticleData.Flag_Plane))
                        {
                            // ?????????
                            //hit = PlaneColliderDetection(ref nextpos, radius, cindex);
                        }
                        else if (cflag.IsFlag(PhysicsManagerParticleData.Flag_CapsuleX))
                        {
                            // ???????????
                            hit = CapsuleColliderDetection(nextpos0, nextpos1, ref corr0, ref corr1, radius, cindex, new float3(1, 0, 0));
                            //hit = CapsuleColliderDetection(nextpos0, nextpos1, oldpos0, oldpos1, ref corr0, ref corr1, radius, cindex, new float3(1, 0, 0));
                        }
                        else if (cflag.IsFlag(PhysicsManagerParticleData.Flag_CapsuleY))
                        {
                            // ???????????
                            hit = CapsuleColliderDetection(nextpos0, nextpos1, ref corr0, ref corr1, radius, cindex, new float3(0, 1, 0));
                            //hit = CapsuleColliderDetection(nextpos0, nextpos1, oldpos0, oldpos1, ref corr0, ref corr1, radius, cindex, new float3(0, 1, 0));
                        }
                        else if (cflag.IsFlag(PhysicsManagerParticleData.Flag_CapsuleZ))
                        {
                            // ???????????
                            hit = CapsuleColliderDetection(nextpos0, nextpos1, ref corr0, ref corr1, radius, cindex, new float3(0, 0, 1));
                            //hit = CapsuleColliderDetection(nextpos0, nextpos1, oldpos0, oldpos1, ref corr0, ref corr1, radius, cindex, new float3(0, 0, 1));
                        }
                        else if (cflag.IsFlag(PhysicsManagerParticleData.Flag_Box))
                        {
                            // ???????????
                            // ??????
                        }
                        else
                        {
                            // ????????
                            hit = SphereColliderDetection(nextpos0, nextpos1, ref corr0, ref corr1, radius, cindex);
                            //hit = SphereColliderDetection(nextpos0, nextpos1, oldpos0, oldpos1, ref corr0, ref corr1, radius, cindex);
                        }

                        hitresult = hit ? true : hitresult;

                        //if (hit)
                        //{
                        //    // ????!
                        //    // ????
                        //    //frictionList[index] = math.max(frictionList[index], teamData.friction);
                        //}
                    }

                    // ???????????
                    colliderTeam = gdata.teamId;
                }

                // ?????

                // ?????????
                int wstart = gdata.writeDataChunk.startIndex;
                int windex0 = data.writeIndex0 + wstart;
                int windex1 = data.writeIndex1 + wstart;
                writeBuffer[windex0] = corr0;
                writeBuffer[windex1] = corr1;
            }

            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="nextpos0">??????</param>
            /// <param name="nextpos1">??????</param>
            /// <param name="corr0"></param>
            /// <param name="corr1"></param>
            /// <param name="radius"></param>
            /// <param name="cindex"></param>
            /// <returns></returns>
            bool SphereColliderDetection(float3 nextpos0, float3 nextpos1, ref float3 corr0, ref float3 corr1, float radius, int cindex)
            //bool SphereColliderDetection(float3 nextpos0, float3 nextpos1, float3 oldpos0, float3 oldpos1, ref float3 corr0, ref float3 corr1, float radius, int cindex)
            {
                var cpos = nextPosList[cindex];
                var coldpos = posList[cindex];
                var cradius = radiusList[cindex];

                // ????
                var tindex = transformIndexList[cindex];
                var cscl = boneSclList[tindex];
                cradius *= cscl.x; // X??????


                // ???????????????????
                float3 d = MathUtility.ClosestPtPointSegment(coldpos, nextpos0, nextpos1);
                //float3 d = MathUtility.ClosestPtPointSegment(coldpos, oldpos0, oldpos1);
                float3 n = math.normalize(d - coldpos);
                float3 c = cpos + n * (cradius.x + radius);

                // c = ????
                // n = ????
                // ???????????
                float3 outpos0, outpos1;
                bool ret0 = MathUtility.IntersectPointPlane(c, n, nextpos0, out outpos0);
                bool ret1 = MathUtility.IntersectPointPlane(c, n, nextpos1, out outpos1);

                if (ret0)
                    corr0 += outpos0 - nextpos0;
                if (ret1)
                    corr1 += outpos1 - nextpos1;

                return ret0 || ret1;
            }

            /// <summary>
            /// ????????
            /// </summary>
            /// <param name="nextpos0">??????</param>
            /// <param name="nextpos1">??????</param>
            /// <param name="corr0"></param>
            /// <param name="corr1"></param>
            /// <param name="radius"></param>
            /// <param name="cindex"></param>
            /// <param name="dir"></param>
            /// <returns></returns>
            bool CapsuleColliderDetection(float3 nextpos0, float3 nextpos1, ref float3 corr0, ref float3 corr1, float radius, int cindex, float3 dir)
            //bool CapsuleColliderDetection(float3 nextpos0, float3 nextpos1, float3 oldpos0, float3 oldpos1, ref float3 corr0, ref float3 corr1, float radius, int cindex, float3 dir)
            {
                var cpos = nextPosList[cindex];
                var crot = nextRotList[cindex];
                var coldpos = posList[cindex];
                var coldrot = rotList[cindex];

                // x = ??(??)
                // y = ????
                // z = ????
                //var lpos = localPosList[cindex];
                var cradius = radiusList[cindex];

                // ????
                var tindex = transformIndexList[cindex];
                var cscl = boneSclList[tindex];
                float scl = math.dot(cscl, dir); // dir????????????
                cradius *= scl;

                // ?????????????????????????
                float3 oldl = math.mul(coldrot, dir * cradius.x);
                float3 soldpos = coldpos - oldl;
                float3 eoldpos = coldpos + oldl;
                float3 c1, c2;
                float s, t;
                MathUtility.ClosestPtSegmentSegment(soldpos, eoldpos, nextpos0, nextpos1, out s, out t, out c1, out c2);
                //MathUtility.ClosestPtSegmentSegment(soldpos, eoldpos, oldpos0, oldpos1, out s, out t, out c1, out c2);
                float3 v = c2 - c1;

                // ????????????
                float3 l = math.mul(crot, dir * cradius.x);
                float3 spos = cpos - l;
                float3 epos = cpos + l;
                float sr = cradius.y;
                float er = cradius.z;

                // ???????????????????
                var iq = math.inverse(coldrot);
                float3 lv = math.mul(iq, v);
                v = math.mul(crot, lv);

                // ????????
                float r = math.lerp(sr, er, s);

                // ?????
                float3 n = math.normalize(v);
                float3 q = math.lerp(spos, epos, s);
                float3 c = q + n * (r + radius);

                // c = ????
                // n = ????
                // ???????????
                float3 outpos0, outpos1;
                bool ret0 = MathUtility.IntersectPointPlane(c, n, nextpos0, out outpos0);
                bool ret1 = MathUtility.IntersectPointPlane(c, n, nextpos1, out outpos1);

                if (ret0)
                    corr0 += outpos0 - nextpos0;
                if (ret1)
                    corr1 += outpos1 - nextpos1;

                return ret0 || ret1;
            }
        }

        [BurstCompile]
        struct EdgeCollisionSumJob : IJobParallelFor
        {
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<ReferenceDataIndex> refDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> writeBuffer;

            // ???
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;

            public NativeArray<float3> inoutNextPosList;
            public NativeArray<float> frictionList;

            // ????????
            public void Execute(int pindex)
            {
                var flag = flagList[pindex];
                if (flag.IsValid() == false || flag.IsFixed())
                    return;

                // ???
                var team = teamDataList[teamIdList[pindex]];
                if (team.IsActive() == false)
                    return;
                if (team.edgeCollisionGroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // ???????
                var gdata = groupDataList[team.edgeCollisionGroupIndex];
                if (gdata.active == 0)
                    return;

                // ??
                int start = team.particleChunk.startIndex;
                int index = pindex - start;

                var refdata = refDataList[gdata.refDataChunk.startIndex + index];
                if (refdata.count > 0)
                {
                    float3 corr = 0;
                    var bindex = gdata.writeDataChunk.startIndex + refdata.startIndex;
                    for (int i = 0; i < refdata.count; i++)
                    {
                        corr += writeBuffer[bindex];
                        bindex++;
                    }
                    corr /= refdata.count;

                    // ??
                    inoutNextPosList[pindex] = inoutNextPosList[pindex] + corr;

                    // ??
                    //if (math.lengthsq(corr) > 0.00001f)
                    //if (math.lengthsq(corr) > 0.0f)
                    {
                        // ????
                        //frictionList[pindex] = math.max(frictionList[pindex], team.friction);
                    }
                }
            }
        }
    }
}
