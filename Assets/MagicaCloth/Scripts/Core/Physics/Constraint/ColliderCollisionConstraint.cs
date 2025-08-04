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
    /// ????????????
    /// </summary>
    public class ColliderCollisionConstraint : PhysicsManagerConstraint
    {
        public override void Create()
        {
        }

        public override void RemoveTeam(int teamId)
        {
        }

        public void ChangeParam(int teamId, bool useCollision)
        {
            Manager.Team.SetFlag(teamId, PhysicsManagerTeamData.Flag_Collision, useCollision);
        }

        public override void Release()
        {
        }

        //=========================================================================================
        public override JobHandle SolverConstraint(int runCount, float dtime, float updatePower, int iteration, JobHandle jobHandle)
        {
            if (Manager.Particle.ColliderCount <= 0)
                return jobHandle;

            // ???????
            var job1 = new CollisionJob()
            {
                runCount = runCount,

                flagList = Manager.Particle.flagList.ToJobArray(),
                teamIdList = Manager.Particle.teamIdList.ToJobArray(),
                radiusList = Manager.Particle.radiusList.ToJobArray(),
                nextPosList = Manager.Particle.InNextPosList.ToJobArray(),
                nextRotList = Manager.Particle.InNextRotList.ToJobArray(),
                posList = Manager.Particle.posList.ToJobArray(),
                rotList = Manager.Particle.rotList.ToJobArray(),
                localPosList = Manager.Particle.localPosList.ToJobArray(),
                basePosList = Manager.Particle.basePosList.ToJobArray(),
                baseRotList = Manager.Particle.baseRotList.ToJobArray(),
                transformIndexList = Manager.Particle.transformIndexList.ToJobArray(),

                colliderList = Manager.Team.colliderList.ToJobArray(),

                boneSclList = Manager.Bone.boneSclList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),

                frictionList = Manager.Particle.frictionList.ToJobArray(),

                collisionLinkIdList = Manager.Particle.collisionLinkIdList.ToJobArray(),
                collisionNormalList = Manager.Particle.collisionNormalList.ToJobArray(),
            };
            jobHandle = job1.Schedule(Manager.Particle.Length, 64, jobHandle);

            return jobHandle;
        }

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// ?????????????
        /// </summary>
        [BurstCompile]
        struct CollisionJob : IJobParallelFor
        {
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> radiusList;
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> nextPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> nextRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> posList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> rotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> localPosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> basePosList;
            [Unity.Collections.ReadOnly]
            public NativeArray<quaternion> baseRotList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> transformIndexList;

            [Unity.Collections.ReadOnly]
            public NativeArray<int> colliderList;

            [Unity.Collections.ReadOnly]
            public NativeArray<float3> boneSclList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;

            public NativeArray<float> frictionList;

            [Unity.Collections.WriteOnly]
            public NativeArray<int> collisionLinkIdList;
            [Unity.Collections.WriteOnly]
            public NativeArray<float3> collisionNormalList;

            // ????????
            public void Execute(int index)
            {
                var flag = flagList[index];
                if (flag.IsValid() == false || flag.IsFixed() || flag.IsCollider())
                    return;

                // ???
                var team = teamIdList[index];
                var teamData = teamDataList[team];
                if (teamData.IsActive() == false)
                    return;
                if (teamData.IsFlag(PhysicsManagerTeamData.Flag_Collision) == false)
                    return;
                // ????
                if (teamData.IsUpdate(runCount) == false)
                    return;

                float3 nextpos = nextPosList[index];
                var radius = radiusList[index].x;
                //var basepos = basePosList[index];

                // ?????????
                radius *= teamData.scaleRatio;

                // ????????[?????(0)]->[??????(team)]
                int colliderTeam = 0;

                // ?????????
                float mindist = 100.0f;

                // ?????????
                int collisionColliderId = 0;
                float3 collisionNormal = 0;
                float3 n = 0;

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

                        float dist = 100.0f;
                        if (cflag.IsFlag(PhysicsManagerParticleData.Flag_Plane))
                        {
                            // ?????????
                            dist = PlaneColliderDetection(ref nextpos, radius, cindex, out n);
                        }
                        else if (cflag.IsFlag(PhysicsManagerParticleData.Flag_CapsuleX))
                        {
                            // ???????????
                            dist = CapsuleColliderDetection(ref nextpos, radius, cindex, new float3(1, 0, 0), out n);
                        }
                        else if (cflag.IsFlag(PhysicsManagerParticleData.Flag_CapsuleY))
                        {
                            // ???????????
                            dist = CapsuleColliderDetection(ref nextpos, radius, cindex, new float3(0, 1, 0), out n);
                        }
                        else if (cflag.IsFlag(PhysicsManagerParticleData.Flag_CapsuleZ))
                        {
                            // ???????????
                            dist = CapsuleColliderDetection(ref nextpos, radius, cindex, new float3(0, 0, 1), out n);
                        }
                        else if (cflag.IsFlag(PhysicsManagerParticleData.Flag_Box))
                        {
                            // ???????????
                            // ??????
                        }
                        else
                        {
                            // ????????
                            dist = SphereColliderDetection(ref nextpos, radius, cindex, out n);
                        }

                        // ????(??)??
                        if (dist < mindist && dist <= Define.Compute.CollisionFrictionRange)
                        {
                            collisionColliderId = cindex;
                            collisionNormal = n;
                            mindist = dist;
                        }
                    }

                    // ???????????
                    if (team > 0)
                        colliderTeam = team;
                    else
                        break;
                }

                // ????(friction)??
                if (collisionColliderId > 0)
                {
                    // ???????????????
                    // ??????(???????????????.0.0~???1.0~?????????1.0????)
                    //var friction = math.max(1.0f - mindist / Define.Compute.CollisionFrictionRange, 0.0f); // ??????????????????!

                    // ???????????????(0.0~???1.0)
                    var friction = 1.0f - math.saturate(mindist / Define.Compute.CollisionFrictionRange);
                    frictionList[index] = math.max(friction, frictionList[index]); // ????
                }
                collisionLinkIdList[index] = collisionColliderId;
                collisionNormalList[index] = collisionNormal;

                // ????
                nextPosList[index] = nextpos;

                // ???????????100%?????
                // ????????????????????!
                // ??????????????????????????
            }

            //=====================================================================================
            /// <summary>
            /// ?????
            /// </summary>
            /// <param name="nextpos"></param>
            /// <param name="pos"></param>
            /// <param name="radius"></param>
            /// <param name="cindex"></param>
            /// <param name="friction"></param>
            /// <returns></returns>
            float SphereColliderDetection(ref float3 nextpos, float radius, int cindex, out float3 normal)
            {
                var cpos = nextPosList[cindex];
                var cradius = radiusList[cindex];

                // ????
                var tindex = transformIndexList[cindex];
                var cscl = boneSclList[tindex];
                cradius *= math.abs(cscl.x); // X??????

                // ????????????????????????????????????????
                float3 c = 0, n = 0, v = 0;
                var coldpos = posList[cindex];
                v = nextpos - coldpos;
                n = math.normalize(v);
                c = cpos + n * (cradius.x + radius);

                // ????
                normal = n;

                // c = ????
                // n = ????
                // ???????????
                return MathUtility.IntersectPointPlaneDist(c, n, nextpos, out nextpos);
            }

            /// <summary>
            /// ????????
            /// </summary>
            /// <param name="nextpos"></param>
            /// <param name="pos"></param>
            /// <param name="radius"></param>
            /// <param name="cindex"></param>
            /// <param name="dir"></param>
            /// <param name="friction"></param>
            /// <returns></returns>
            float CapsuleColliderDetection(ref float3 nextpos, float radius, int cindex, float3 dir, out float3 normal)
            {
                var cpos = nextPosList[cindex];
                var crot = nextRotList[cindex];

                // x = ??(??)
                // y = ????
                // z = ????
                var cradius = radiusList[cindex];

                // ????
                var tindex = transformIndexList[cindex];
                var cscl = boneSclList[tindex];
                float scl = math.dot(math.abs(cscl), dir); // dir????????????
                cradius *= scl;

                float3 c = 0, n = 0;

                var coldpos = posList[cindex];
                var coldrot = rotList[cindex];

                // ?????????
                float3 l = math.mul(coldrot, dir * cradius.x);
                float3 spos = coldpos - l;
                float3 epos = coldpos + l;
                float sr = cradius.y;
                float er = cradius.z;

                // ????????????????????????
                float t = MathUtility.ClosestPtPointSegmentRatio(nextpos, spos, epos);
                float r = math.lerp(sr, er, t);
                float3 d = math.lerp(spos, epos, t);
                float3 v = nextpos - d;

                // ?????????????????
                var iq = math.inverse(coldrot);
                float3 lv = math.mul(iq, v);

                // ???????????
                l = math.mul(crot, dir * cradius.x);
                spos = cpos - l;
                epos = cpos + l;
                d = math.lerp(spos, epos, t);
                v = math.mul(crot, lv);
                n = math.normalize(v);
                c = d + n * (r + radius);

                // ????
                normal = n;

                // c = ????
                // n = ????
                // ???????????
                return MathUtility.IntersectPointPlaneDist(c, n, nextpos, out nextpos);
            }

            /// <summary>
            /// ??????
            /// </summary>
            /// <param name="nextpos"></param>
            /// <param name="radius"></param>
            /// <param name="cindex"></param>
            float PlaneColliderDetection(ref float3 nextpos, float radius, int cindex, out float3 normal)
            {
                // ????
                var cpos = nextPosList[cindex];
                var crot = nextRotList[cindex];

                // ????
                float3 n = math.mul(crot, math.up());

                // ??????????????
                cpos += n * radius;

                // ????
                normal = n;

                // c = ????
                // n = ????
                // ???????????
                // ?????????(????????0.0)
                return MathUtility.IntersectPointPlaneDist(cpos, n, nextpos, out nextpos);
            }
        }
    }
}
