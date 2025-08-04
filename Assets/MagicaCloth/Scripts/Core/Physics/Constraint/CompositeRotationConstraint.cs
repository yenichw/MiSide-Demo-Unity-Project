// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MagicaCloth
{
    /// <summary>
    /// ??????(v1.11.0??)
    /// [Algorithm 2]
    /// ClampRotation?RestoreRotation????????
    /// ???????????????????????
    /// </summary>
    public class CompositeRotationConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ?????
        /// Clmap/Restore??
        /// </summary>
        [System.Serializable]
        public struct RotationData
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
            /// ????????????????(??????)
            /// </summary>
            public float3 localPos;

            /// <summary>
            /// ????????????????
            /// </summary>
            public quaternion localRot;

            /// <summary>
            /// ???????????
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return vertexIndex > 0 || parentVertexIndex > 0;
            }
        }
        FixedChunkNativeArray<RotationData> dataList;

        [System.Serializable]
        public struct RootInfo
        {
            public ushort startIndex;
            public ushort dataLength;
        }
        FixedChunkNativeArray<RootInfo> rootInfoList;

        /// <summary>
        /// ???????
        /// </summary>
        public struct GroupData
        {
            public int teamId;
            public int useClamp;
            public int useRestore;

            /// <summary>
            /// ????
            /// </summary>
            public CurveParam maxAngle;

            public CurveParam restorePower;

            /// <summary>
            /// ????
            /// </summary>
            public float restoreVelocityInfluence;

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
        FixedChunkNativeArray<float> lengthBuffer;

        //=========================================================================================
        public override void Create()
        {
            dataList = new FixedChunkNativeArray<RotationData>();
            rootInfoList = new FixedChunkNativeArray<RootInfo>();
            groupList = new FixedNativeList<GroupData>();
            rootTeamList = new FixedChunkNativeArray<int>();
            lengthBuffer = new FixedChunkNativeArray<float>();
        }

        public override void Release()
        {
            dataList.Dispose();
            rootInfoList.Dispose();
            groupList.Dispose();
            rootTeamList.Dispose();
            lengthBuffer.Dispose();
        }

        //=========================================================================================
        public int AddGroup(
            int teamId,
            bool useClamp,
            BezierParam maxAngle,
            bool useRestore,
            BezierParam restorePower,
            float velocityInfluence,
            RotationData[] dataArray,
            RootInfo[] rootInfoArray
            )
        {
            if (dataArray == null || dataArray.Length == 0 || rootInfoArray == null || rootInfoArray.Length == 0)
                return -1;

            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.useClamp = useClamp ? 1 : 0;
            gdata.maxAngle.Setup(maxAngle);
            gdata.useRestore = useRestore ? 1 : 0;
            gdata.restorePower.Setup(restorePower);
            gdata.restoreVelocityInfluence = velocityInfluence;
            gdata.dataChunk = dataList.AddChunk(dataArray.Length);
            gdata.rootInfoChunk = rootInfoList.AddChunk(rootInfoArray.Length);

            // ??????????
            dataList.ToJobArray().CopyFromFast(gdata.dataChunk.startIndex, dataArray);
            rootInfoList.ToJobArray().CopyFromFast(gdata.rootInfoChunk.startIndex, rootInfoArray);

            int group = groupList.Add(gdata);

            // ???????????????
            var c = rootTeamList.AddChunk(rootInfoArray.Length);
            rootTeamList.Fill(c, teamId);

            // ??????
            lengthBuffer.AddChunk(dataArray.Length);

            return group;
        }

        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            int group = teamData.compositeRotationGroupIndex;
            if (group >= 0)
            {
                var cdata = groupList[group];

                // ?????????
                dataList.RemoveChunk(cdata.dataChunk);
                rootInfoList.RemoveChunk(cdata.rootInfoChunk);
                rootTeamList.RemoveChunk(cdata.rootInfoChunk);
                lengthBuffer.RemoveChunk(cdata.dataChunk);

                // ?????
                groupList.Remove(group);
            }
        }

        public void ChangeParam(
            int teamId,
            bool useClamp,
            BezierParam maxAngle,
            bool useRestore,
            BezierParam restorePower,
            float velocityInfluence
            )
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.compositeRotationGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.useClamp = useClamp ? 1 : 0;
            gdata.maxAngle.Setup(maxAngle);
            gdata.useRestore = useRestore ? 1 : 0;
            gdata.restorePower.Setup(restorePower);
            gdata.restoreVelocityInfluence = velocityInfluence;
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
            if (groupList.Count > 0)
            {
                // ????(????????)
                var job = new RotationRootLineJob()
                {
                    updatePower = updatePower,
                    runCount = runCount,
                    maxMoveSpeed = dtime * Define.Compute.ClampRotationMaxVelocity2, // ??2.0m/s

                    dataList = dataList.ToJobArray(),
                    rootInfoList = rootInfoList.ToJobArray(),
                    rootTeamList = rootTeamList.ToJobArray(),
                    groupList = groupList.ToJobArray(),

                    teamDataList = Manager.Team.teamDataList.ToJobArray(),
                    teamGravityList = Manager.Team.teamGravityList.ToJobArray(),

                    depthList = Manager.Particle.depthList.ToJobArray(),
                    flagList = Manager.Particle.flagList.ToJobArray(),
                    frictionList = Manager.Particle.frictionList.ToJobArray(),

                    basePosList = Manager.Particle.basePosList.ToJobArray(),
                    baseRotList = Manager.Particle.baseRotList.ToJobArray(),

                    nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                    nextRotList = Manager.Particle.InNextRotList.ToJobArray(),
                    posList = Manager.Particle.posList.ToJobArray(),

                    lengthBuffer = lengthBuffer.ToJobArray(),
                };
                jobHandle = job.Schedule(rootTeamList.Length, 4, jobHandle);
            }

            return jobHandle;
        }

        /// <summary>
        /// ???????[Algorithm 2]
        /// </summary>
        [BurstCompile]
        struct RotationRootLineJob : IJobParallelFor
        {
            public float updatePower;
            public int runCount;
            public float maxMoveSpeed;

            [Unity.Collections.ReadOnly]
            public NativeArray<RotationData> dataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<RootInfo> rootInfoList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> rootTeamList;
            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<CurveParam> teamGravityList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float> depthList;
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float> frictionList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> basePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> baseRotList;
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> nextPosList;
            [NativeDisableParallelForRestriction]
            public NativeArray<quaternion> nextRotList;
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> posList;

            [NativeDisableParallelForRestriction]
            public NativeArray<float> lengthBuffer;

            // ????????
            public void Execute(int rootIndex)
            {
                // ???
                int teamIndex = rootTeamList[rootIndex];
                if (teamIndex == 0)
                    return;
                var team = teamDataList[teamIndex];
                if (team.IsActive() == false || team.compositeRotationGroupIndex < 0)
                    return;

                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // ???????
                var gdata = groupList[team.compositeRotationGroupIndex];
                if (gdata.useClamp == 0 && gdata.useRestore == 0)
                    return;

                // ???????????????
                bool useAnimatedPose = team.IsFlag(PhysicsManagerTeamData.Flag_AnimatedPose);

                // ???
                var rootInfo = rootInfoList[rootIndex];
                int dataIndex = rootInfo.startIndex + gdata.dataChunk.startIndex;
                int dataCount = rootInfo.dataLength;
                int pstart = team.particleChunk.startIndex;

                // (1)?????????????????
                if (gdata.useClamp == 1)
                {
                    for (int i = 0; i < dataCount; i++)
                    {
                        var data = dataList[dataIndex + i];
                        int pindex = data.parentVertexIndex;
                        if (pindex < 0)
                            continue;

                        var index = data.vertexIndex;
                        index += pstart;
                        pindex += pstart;

                        var npos = nextPosList[index];
                        var ppos = nextPosList[pindex];

                        // ???????
                        float vlen = math.distance(npos, ppos);

                        lengthBuffer[dataIndex + i] = vlen;
                    }
                }

                // 2??????????????
                const int iteration = 2;
                for (int j = 0; j < iteration; j++)
                {
                    // (2)??????????????
                    for (int i = 0; i < dataCount; i++)
                    {
                        var data = dataList[dataIndex + i];
                        int pindex = data.parentVertexIndex;
                        if (pindex < 0)
                            continue;

                        int index = data.vertexIndex;

                        index += pstart;
                        pindex += pstart;

                        // ????
                        var cflag = flagList[index];
                        if (cflag.IsValid() == false)
                            continue;
                        var cpos = nextPosList[index];
                        var crot = nextRotList[index];
                        float cdepth = depthList[index];
                        float cfriction = frictionList[index];
                        float cmoveratio = math.saturate(1.0f - cfriction * Define.Compute.FrictionMoveRatio);

                        // ????
                        var pflag = flagList[pindex];
                        var ppos = nextPosList[pindex];
                        var pbrot = baseRotList[pindex];
                        var prot = nextRotList[pindex];
                        float pfriction = frictionList[pindex];
                        float pmoveratio = math.saturate(1.0f - pfriction * Define.Compute.FrictionMoveRatio);

                        // ???????????
                        var gravity = math.abs(teamGravityList[teamIndex].Evaluate(cdepth));
                        float3 gravityVector = gravity > Define.Compute.Epsilon ? team.gravityDirection : 0;

                        // ??????
                        float3 localPos = data.localPos;
                        quaternion localRot = data.localRot;
                        if (useAnimatedPose)
                        {
                            // ?????????????
                            var brot = baseRotList[index];
                            var bpos = basePosList[index];
                            var pbpos = basePosList[pindex];
                            var v = bpos - pbpos;
                            if (math.lengthsq(v) < 1e-09f)
                                continue;
                            v = math.normalize(v);
                            var ipq = math.inverse(pbrot);
                            localPos = math.mul(ipq, v);
                            localRot = math.mul(ipq, brot);
                        }
                        else
                        {
                            // ??????????
                            localPos = localPos * team.scaleDirection;
                            localRot = localRot.value * team.quaternionScale;
                        }

                        //=====================================================
                        // Clamp
                        //=====================================================
                        if (gdata.useClamp == 1)
                        {
                            // ?????
                            var trot = pflag.IsMove() ? prot : pbrot; // ????
                            //var trot = pbrot; // ???????

                            // ??????
                            float3 v = cpos - ppos;

                            // ???????
                            float3 tv = math.mul(trot, localPos);

                            // ???????
                            float vlen = math.length(v); // ?????(??????????????????????)
                            float blen = lengthBuffer[dataIndex + i]; // ??????
                            vlen = math.lerp(vlen, blen, 0.5f); // ???????????(0.2?)
                            v = math.normalize(v) * vlen;

                            // ??????????
                            float maxAngleDeg = gdata.maxAngle.Evaluate(cdepth);
                            float maxAngleRad = math.radians(maxAngleDeg);
                            float angle = math.acos(math.dot(v, tv));
                            float qratio = 0.0f;

#if true
                            if (cflag.IsFixed() == false)
                            {
                                float3 rv = v;
                                if (angle > maxAngleRad)
                                {
                                    MathUtility.ClampAngle(v, tv, maxAngleRad, out rv);

                                    qratio = 1.0f - maxAngleRad / angle;
                                }

                                // ??????
                                const float rotRatio = 0.5f; // 0.5????????
                                float3 rotPos = ppos + v * rotRatio;

                                // ?????????????
                                float3 pfpos = rotPos - rv * rotRatio;
                                float3 cfpos = rotPos + rv * (1.0f - rotRatio);

                                // ??
                                float3 padd = pfpos - ppos;
                                float3 cadd = cfpos - cpos;

                                // ????(????)
                                //padd = MathUtility.ClampVector(padd, 0.0f, maxMoveSpeed);
                                //cadd = MathUtility.ClampVector(cadd, 0.0f, maxMoveSpeed);

                                // ????
                                padd *= pmoveratio;
                                cadd *= cmoveratio;

                                // ????
                                // ?????????????????
                                float influence = math.lerp(0.5f, 0.2f, math.pow(math.saturate(maxAngleDeg / 90.0f), 0.5f));

                                // ????
                                if (cflag.IsMove())
                                {
                                    nextPosList[index] = cpos + cadd;
                                    // ????
                                    posList[index] = posList[index] + (cadd * (1.0f - influence));

                                    cpos += cadd;
                                }
                                if (pflag.IsMove())
                                {
                                    nextPosList[pindex] = ppos + padd;
                                    // ????
                                    posList[pindex] = posList[pindex] + (padd * (1.0f - influence));

                                    ppos += padd;
                                }

                                // ??????
                                v = cpos - ppos;
                            }
#else
                            if (cflag.IsFixed() == false)
                            {
                                float3 rv = v;
                                if (angle > maxAngle)
                                {
                                    MathUtility.ClampAngle(v, tv, maxAngle, out rv);

                                    qratio = 1.0f - maxAngle / angle;
                                }

                                float3 cfpos = ppos + math.normalize(rv) * vlen;

                                // ??
                                //float3 padd = pfpos - ppos;
                                float3 cadd = cfpos - cpos;

                                // ????
                                //padd *= pmoveratio;
                                cadd *= cmoveratio;

                                // ????
                                const float influence = 0.2f;
                                if (cflag.IsMove())
                                {
                                    nextPosList[index] = cpos + cadd;

                                    // ????
                                    posList[index] = posList[index] + (cadd * (1.0f - influence));

                                    cpos += cadd;
                                }

                                // ??????
                                v = cpos - ppos;
                            }
#endif

                            //=====================================================
                            // ????
                            //=====================================================
                            var nrot = math.mul(trot, localRot);
                            var q = MathUtility.FromToRotation(tv, v);
                            nrot = math.mul(q, nrot);
                            nextRotList[index] = nrot;
                        }

                        //=====================================================
                        // Restore
                        //=====================================================
                        if (gdata.useRestore == 1)
                        {
                            // ??????
                            float3 v = cpos - ppos;

                            // ???????(???????????)
                            float3 tv = math.mul(pbrot, localPos);

                            float restorePower = gdata.restorePower.Evaluate(cdepth);
                            restorePower = 1.0f - math.pow(1.0f - restorePower, updatePower);

                            // ??????
                            var q = MathUtility.FromToRotation(v, tv, restorePower);
                            float3 rv = math.mul(q, v);

                            // ??????
                            float rotRatio = GetRotRatio(tv, gravityVector, gravity);
                            float3 rotPos = ppos + v * rotRatio;

                            // ?????????????
                            float3 pfpos = rotPos - rv * rotRatio;
                            float3 cfpos = rotPos + rv * (1.0f - rotRatio);

                            // ??
                            float3 padd = pfpos - ppos;
                            float3 cadd = cfpos - cpos;

                            // ????
                            padd *= pmoveratio;
                            cadd *= cmoveratio;

                            // ????
                            float influence = gdata.restoreVelocityInfluence;
                            if (cflag.IsMove())
                            {
                                nextPosList[index] = cpos + cadd;
                                // ????
                                posList[index] = posList[index] + (cadd * (1.0f - influence));

                                cpos += cadd;
                            }
                            if (pflag.IsMove())
                            {
                                nextPosList[pindex] = ppos + padd;
                                // ????
                                posList[pindex] = posList[pindex] + (padd * (1.0f - influence));

                                ppos += padd;
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// ??????????????????????????????
            /// 0.4??????????????????0.2????????????????????????????
            /// ??????????????????????????????
            /// </summary>
            /// <param name="tv"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            float GetRotRatio(float3 tv, float3 gravityVector, float gravity, float minRatio = 0.25f, float maxRatio = 0.45f)
            {
#if true
                // ??????(0.0-1.0)
                float dot = math.dot(math.normalize(tv), gravityVector);
                dot = dot * 0.5f + 0.5f;

                // ????????????????????
                float pow = math.lerp(4.0f, 1.0f, math.saturate(gravity / 9.8f)); // 4.0 - 1.0?

                // ?????????(0.0-1.0)
                dot = math.pow(dot, pow);

                // ?????????????????????
                // ??0?????????????
                //float rotRatio = math.lerp(0.25f, 0.45f, dot);
                float rotRatio = math.lerp(minRatio, maxRatio, dot);

                return rotRatio;
#else
                return 0.3f; // ?????
#endif
            }
        }

    }
}
