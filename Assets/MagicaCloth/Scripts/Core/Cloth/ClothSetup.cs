// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ?????????
    /// </summary>
    public class ClothSetup
    {
        // ?????????????
        int teamBoneIndex = -1;

        // ???????????????
        //int teamDirectionalDampingBoneIndex;

        /// <summary>
        /// ??????????
        /// </summary>
        float distanceBlendRatio = 1.0f;

        //=========================================================================================
        /// <summary>
        /// ??????
        /// </summary>
        /// <param name="team"></param>
        /// <param name="meshData">???????(????null)</param>
        /// <param name="clothData"></param>
        /// <param name="param"></param>
        /// <param name="funcUserFlag">????????????????</param>
        /// <param name="funcUserTransform">?????????????????????</param>
        public void ClothInit(
            PhysicsTeam team,
            MeshData meshData,
            ClothData clothData,
            ClothParams param,
            System.Func<int, uint> funcUserFlag
            )
        {
            var manager = MagicaPhysicsManager.Instance;
            var compute = manager.Compute;

            // ????????
            manager.Team.SetMass(team.TeamId, param.GetMass());
            manager.Team.SetGravity(team.TeamId, param.GetGravity());
            manager.Team.SetGravityDirection(team.TeamId, param.GravityDirection);
            manager.Team.SetDrag(team.TeamId, param.GetDrag());
            manager.Team.SetMaxVelocity(team.TeamId, param.GetMaxVelocity());
            manager.Team.SetDepthInfluence(team.TeamId, param.GetDepthInfluence());
            manager.Team.SetFriction(team.TeamId, param.DynamicFriction, param.StaticFriction);
            manager.Team.SetExternalForce(team.TeamId, param.MassInfluence, param.WindInfluence, param.WindRandomScale, param.WindSynchronization);
            //manager.Team.SetDirectionalDamping(team.TeamId, param.GetDirectionalDamping());

            // ????????
            manager.Team.SetWorldInfluence(
                team.TeamId,
                param.MaxMoveSpeed,
                param.MaxRotationSpeed,
                param.GetWorldMoveInfluence(),
                param.GetWorldRotationInfluence(),
                param.UseResetTeleport,
                param.TeleportDistance,
                param.TeleportRotation,
                param.ResetStabilizationTime,
                param.TeleportResetMode,
                param.UseClampRotation,
                param.GetClampRotationAngle(clothData.clampRotationAlgorithm)
                );

            int vcnt = clothData.VertexUseCount;
            Debug.Assert(vcnt > 0);
            Debug.Assert(clothData.useVertexList.Count > 0);

            // ????????(??????)
            var c = team.CreateParticle(team.TeamId, clothData.useVertexList.Count,
                // flag
                (i) =>
                {
                    bool isFix = clothData.IsFixedVertex(i) || clothData.IsExtendVertex(i); // ????????
                    uint flag = 0;
                    if (funcUserFlag != null)
                        flag = funcUserFlag(i); // ???????
                    if (isFix)
                        flag |= (PhysicsManagerParticleData.Flag_Kinematic | PhysicsManagerParticleData.Flag_Step_Update);
                    if (clothData.IsFlag(i, ClothData.VertexFlag_TriangleRotation))
                        flag |= PhysicsManagerParticleData.Flag_TriangleRotation; // TriangleWorker???????
                    //flag |= (param.UseCollision && !isFix) ? PhysicsManagerParticleData.Flag_Collision : 0;
                    flag |= PhysicsManagerParticleData.Flag_Reset_Position;
                    return flag;
                },
                // wpos
                null,
                // wrot
                null,
                // depth
                (i) =>
                {
                    return clothData.vertexDepthList[i];
                },
                // radius
                (i) =>
                {
                    float depth = clothData.vertexDepthList[i];
                    return param.GetRadius(depth);
                },
                // target local pos
                null
                );
            manager.Team.SetParticleChunk(team.TeamId, c);

            // ?????????
            if (param.UseSpring)
            {
                // ?????
                int group = compute.Spring.AddGroup(
                    team.TeamId,
                    param.UseSpring,
                    param.GetSpringPower()
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.springGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

            // ??????
            if (param.UseClampPositionLength)
            {
                // ?????
                int group = compute.ClampPosition.AddGroup(
                    team.TeamId,
                    param.UseClampPositionLength,
                    param.GetClampPositionLength(),
                    param.ClampPositionAxisRatio,
                    param.ClampPositionVelocityInfluence
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.clampPositionGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

            // ??????????????
            if (param.UseClampDistanceRatio && clothData.ClampDistanceConstraintCount > 0)
            {
                // ?????
                int group = compute.ClampDistance.AddGroup(
                    team.TeamId,
                    param.UseClampDistanceRatio,
                    param.ClampDistanceMinRatio,
                    param.ClampDistanceMaxRatio,
                    param.ClampDistanceVelocityInfluence,
                    clothData.rootDistanceDataList,
                    clothData.rootDistanceReferenceList
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.clampDistanceGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

#if false
            // ??????????????
            if(param.UseClampDistanceRatio && clothData.ClampDistance2ConstraintCount > 0)
            {
                // ?????
                int group = compute.ClampDistance2.AddGroup(
                    team.TeamId,
                    param.UseClampDistanceRatio,
                    param.ClampDistanceMinRatio,
                    param.ClampDistanceMaxRatio,
                    param.ClampDistanceVelocityInfluence,
                    clothData.clampDistance2DataList,
                    clothData.clampDistance2RootInfoList
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.clampDistance2GroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }
#endif

            // ??????
            if (clothData.StructDistanceConstraintCount > 0 || clothData.BendDistanceConstraintCount > 0 || clothData.NearDistanceConstraintCount > 0)
            {
                // ?????
                int group = compute.RestoreDistance.AddGroup(
                    team.TeamId,
                    param.GetMass(),
                    param.RestoreDistanceVelocityInfluence,
                    param.GetStructDistanceStiffness(),
                    clothData.structDistanceDataList,
                    clothData.structDistanceReferenceList,
                    param.UseBendDistance,
                    param.GetBendDistanceStiffness(),
                    clothData.bendDistanceDataList,
                    clothData.bendDistanceReferenceList,
                    param.UseNearDistance,
                    param.GetNearDistanceStiffness(),
                    clothData.nearDistanceDataList,
                    clothData.nearDistanceReferenceList
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.restoreDistanceGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

            // ??????[Algorithm 1]
            if (clothData.restoreRotationAlgorithm == ClothParams.Algorithm.Algorithm_1)
            {
                if (param.UseRestoreRotation && clothData.RestoreRotationConstraintCount > 0)
                {
                    // ?????
                    int group = compute.RestoreRotation.AddGroup(
                        team.TeamId,
                        param.UseRestoreRotation,
                        param.GetRestoreRotationPower(clothData.restoreRotationAlgorithm),
                        param.GetRestoreRotationVelocityInfluence(clothData.restoreRotationAlgorithm),
                        clothData.restoreRotationDataList,
                        clothData.restoreRotationReferenceList
                        );
                    var teamData = manager.Team.teamDataList[team.TeamId];
                    teamData.restoreRotationGroupIndex = (short)group;
                    manager.Team.teamDataList[team.TeamId] = teamData;
                }
            }

            // ????????[Algorithm 1]
            if (clothData.clampRotationAlgorithm == ClothParams.Algorithm.Algorithm_1)
            {
                if (param.UseClampRotation)
                {
                    // ?????
                    int group = compute.ClampRotation.AddGroup(
                        team.TeamId,
                        param.UseClampRotation,
                        param.GetClampRotationAngle(clothData.clampRotationAlgorithm),
                        param.ClampRotationVelocityInfluence,
                        clothData.clampRotationDataList,
                        clothData.clampRotationRootInfoList
                        );
                    var teamData = manager.Team.teamDataList[team.TeamId];
                    teamData.clampRotationGroupIndex = (short)group;
                    manager.Team.teamDataList[team.TeamId] = teamData;
                }
            }

            // ??????[Algorithm 2]
            if (param.UseClampRotation || param.UseRestoreRotation)
            {
                if (clothData.CompositeRotationCount > 0)
                {
                    int group = compute.CompositeRotation.AddGroup(
                        team.TeamId,
                        param.UseClampRotation,
                        param.GetClampRotationAngle(ClothParams.Algorithm.Algorithm_2),
                        param.UseRestoreRotation,
                        param.GetRestoreRotationPower(ClothParams.Algorithm.Algorithm_2),
                        param.GetRestoreRotationVelocityInfluence(ClothParams.Algorithm.Algorithm_2),
                        clothData.compositeRotationDataList,
                        clothData.compositeRotationRootInfoList
                        );
                    var teamData = manager.Team.teamDataList[team.TeamId];
                    teamData.compositeRotationGroupIndex = (short)group;
                    manager.Team.teamDataList[team.TeamId] = teamData;
                }
            }

            // ?????
            if (clothData.TwistConstraintCount > 0 && clothData.triangleBendAlgorithm == ClothParams.Algorithm.Algorithm_2)
            {
                // ?????
                int group = compute.Twist.AddGroup(
                    team.TeamId,
                    param.UseTriangleBend && param.GetUseTwistCorrection(clothData.triangleBendAlgorithm),
                    param.TwistRecoveryPower,
                    clothData.twistDataList,
                    clothData.twistReferenceList
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.twistGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

            // ????????????
            if (param.UseTriangleBend && clothData.TriangleBendConstraintCount > 0)
            {
                int group = compute.TriangleBend.AddGroup(
                    team.TeamId,
                    param.UseTriangleBend,
                    clothData.triangleBendAlgorithm,
                    param.GetTriangleBendStiffness(clothData.triangleBendAlgorithm),
                    //param.UseTrianlgeBendIncludeFixed,
                    clothData.triangleBendDataList,
                    clothData.triangleBendReferenceList,
                    clothData.triangleBendWriteBufferCount
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.triangleBendGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

            // ??????????
            if (param.UseCollision)
            {
                var teamData = manager.Team.teamDataList[team.TeamId];

                // ???????
                //teamData.SetFlag(PhysicsManagerTeamData.Flag_Collision_KeepShape, param.KeepInitialShape);
                teamData.SetFlag(PhysicsManagerTeamData.Flag_Collision, param.UseCollision);

#if false
                // ??????????
                if (param.UseEdgeCollision && clothData.EdgeCollisionConstraintCount > 0)
                {
                    int group = compute.EdgeCollision.AddGroup(
                        team.TeamId,
                        param.UseEdgeCollision,
                        param.EdgeCollisionRadius,
                        clothData.edgeCollisionDataList,
                        clothData.edgeCollisionReferenceList,
                        clothData.edgeCollisionWriteBufferCount
                        );
                    teamData.edgeCollisionGroupIndex = (short)group;
                }
#endif

                manager.Team.teamDataList[team.TeamId] = teamData;
            }

            // ????
            if (param.UsePenetration && clothData.PenetrationCount > 0)
            {
                int group = compute.Penetration.AddGroup(
                    team.TeamId,
                    param.UsePenetration,
                    //param.GetPenetrationMode(),
                    clothData.penetrationMode, // ??????????
                    param.GetPenetrationDistance(),
                    param.GetPenetrationRadius(),
                    param.PenetrationMaxDepth,
                    clothData.penetrationDataList,
                    clothData.penetrationReferenceList,
                    clothData.penetrationDirectionDataList
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.penetrationGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

#if false // ????
            // ????????(????)
            if (team.SkinningMode == PhysicsTeam.TeamSkinningMode.GenerateFromBones && clothData.BaseSkinningCount > 0)
            {
                int group = compute.BaseSkinningWorker.AddGroup(
                    team.TeamId,
                    true,
                    team.SkinningUpdateFixed,
                    clothData.baseSkinningDataList,
                    clothData.baseSkinningBindPoseList
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.baseSkinningGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }
#endif

#if false
            // ???????
            if (param.UseVolume && clothData.VolumeConstraintCount > 0)
            {
                //var sw = new StopWatch().Start();

                int group = compute.Volume.AddGroup(
                    team.TeamId,
                    param.UseVolume,
                    param.GetVolumeStretchStiffness(),
                    param.GetVolumeShearStiffness(),
                    clothData.volumeDataList,
                    clothData.volumeReferenceList,
                    clothData.volumeWriteBufferCount
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.volumeGroupIndex = group;
                manager.Team.teamDataList[team.TeamId] = teamData;

                //sw.Stop();
                //Debug.Log("Volume.AddGroup():" + sw.ElapsedMilliseconds);
            }
#endif

            // ????(???????):BoneSpring / MeshSpring??
            if (team is MagicaBoneSpring || team is MagicaMeshSpring)
            {
                // ?????
                int group = compute.AdjustRotationWorker.AddGroup(
                    team.TeamId,
                    true,
                    (int)param.AdjustRotationMode,
                    param.AdjustRotationVector,
                    clothData.adjustRotationDataList
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.adjustRotationGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

            // ???????(????)
            if (clothData.lineRotationDataList != null && clothData.lineRotationDataList.Length > 0)
            {
                // ?????
                int group = compute.LineWorker.AddGroup(
                    team.TeamId,
                    param.UseLineAvarageRotation,
                    clothData.lineRotationDataList,
                    clothData.lineRotationRootInfoList
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.lineWorkerGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

            // ???????????(????)
            if (clothData.triangleRotationDataList != null && clothData.triangleRotationDataList.Length > 0)
            {
                // ?????
                int group = compute.TriangleWorker.AddGroup(
                    team.TeamId,
                    clothData.triangleRotationDataList,
                    clothData.triangleRotationIndexList
                    );
                var teamData = manager.Team.teamDataList[team.TeamId];
                teamData.triangleWorkerGroupIndex = (short)group;
                manager.Team.teamDataList[team.TeamId] = teamData;
            }

            // ????
            manager.Team.SetFlag(team.TeamId, PhysicsManagerTeamData.Flag_FixedNonRotation, param.UseFixedNonRotation);
        }

        //=========================================================================================
        /// <summary>
        /// ?????
        /// </summary>
        public void ClothDispose(PhysicsTeam team)
        {
            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            // ??????????
            MagicaPhysicsManager.Instance.Compute.RemoveTeam(team.TeamId);

            // ????????
            team.RemoveAllParticle();
        }

        //=========================================================================================
        public void ClothActive(PhysicsTeam team, ClothParams param, ClothData clothData)
        {
            var manager = MagicaPhysicsManager.Instance;

            // ??????????????
            Transform influenceTarget = param.GetInfluenceTarget() ? param.GetInfluenceTarget() : team.transform;
            teamBoneIndex = manager.Bone.AddBone(influenceTarget);
            manager.Team.SetBoneIndex(team.TeamId, teamBoneIndex, clothData.initScale);
            team.InfluenceTarget = influenceTarget;

            // ???????????????
            // ????
            //manager.Team.AddSkinningBoneIndex(team.TeamId, team.TeamData.SkinningBoneList);
        }

        public void ClothInactive(PhysicsTeam team)
        {
            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            var manager = MagicaPhysicsManager.Instance;

            // ??????????
            manager.Bone.RemoveBone(teamBoneIndex);
            manager.Team.SetBoneIndex(team.TeamId, -1, Vector3.zero);

            // ???????????????
            manager.Team.RemoveSkinningBoneIndex(team.TeamId);
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        /// <param name="boneReplaceDict"></param>
        internal void ReplaceBone<T>(PhysicsTeam team, ClothParams param, Dictionary<T, Transform> boneReplaceDict) where T : class
        {
            // ??????? ClothActive() ???????!

            // ???????????????
            Transform influenceTarget = param.GetInfluenceTarget();
            if (influenceTarget)
                param.SetInfluenceTarget(MeshUtility.GetReplaceBone(influenceTarget, boneReplaceDict));
            //if (influenceTarget && boneReplaceDict.ContainsKey(influenceTarget))
            //{
            //    param.SetInfluenceTarget(boneReplaceDict[influenceTarget]);
            //}
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        internal HashSet<Transform> GetUsedBones(PhysicsTeam team, ClothParams param)
        {
            var bones = new HashSet<Transform>();
            bones.Add(param.GetInfluenceTarget());
            return bones;
        }

        /// <summary>
        /// UnityPhysics???????
        /// </summary>
        /// <param name="sw"></param>
        public void ChangeUseUnityPhysics(bool sw)
        {
            MagicaPhysicsManager.Instance.Bone.ChangeUnityPhysicsCount(teamBoneIndex, sw);
        }

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        public float DistanceBlendRatio
        {
            get
            {
                return distanceBlendRatio;
            }
            set
            {
                distanceBlendRatio = value;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        public void ChangeData(PhysicsTeam team, ClothParams param, ClothData clothData)
        {
            if (Application.isPlaying == false)
                return;

            if (MagicaPhysicsManager.IsInstance() == false)
                return;

            if (team == null)
                return;

            var manager = MagicaPhysicsManager.Instance;
            var compute = manager.Compute;

            bool changeMass = false;

            // ??
            if (param.ChangedParam(ClothParams.ParamType.Radius))
            {
                // ???????????
                for (int i = 0; i < team.ParticleChunk.dataLength; i++)
                {
                    int pindex = team.ParticleChunk.startIndex + i;
                    float depth = manager.Particle.depthList[pindex];
                    float radius = param.GetRadius(depth);
                    manager.Particle.SetRadius(pindex, radius);
                }
            }

            // ??
            if (param.ChangedParam(ClothParams.ParamType.Mass))
            {
                manager.Team.SetMass(team.TeamId, param.GetMass());
                changeMass = true;
            }

            // ????
            if (param.ChangedParam(ClothParams.ParamType.Gravity))
            {
                manager.Team.SetGravity(team.TeamId, param.GetGravity());
                manager.Team.SetGravityDirection(team.TeamId, param.GravityDirection);
                //manager.Team.SetDirectionalDamping(team.TeamId, param.GetDirectionalDamping());
                //manager.Team.SetFlag(team.TeamId, PhysicsManagerTeamData.Flag_DirectionalDamping, param.UseDirectionalDamping);
            }

            // ????
            if (param.ChangedParam(ClothParams.ParamType.Drag))
            {
                manager.Team.SetDrag(team.TeamId, param.GetDrag());
            }

            // ????
            if (param.ChangedParam(ClothParams.ParamType.MaxVelocity))
            {
                manager.Team.SetMaxVelocity(team.TeamId, param.GetMaxVelocity());
            }

            // ??
            if (param.ChangedParam(ClothParams.ParamType.ExternalForce))
            {
                manager.Team.SetExternalForce(team.TeamId, param.MassInfluence, param.WindInfluence, param.WindRandomScale, param.WindSynchronization);
                manager.Team.SetDepthInfluence(team.TeamId, param.GetDepthInfluence());
            }

            // ??????????
            if (param.ChangedParam(ClothParams.ParamType.ColliderCollision))
                manager.Team.SetFriction(team.TeamId, param.DynamicFriction, param.StaticFriction);

            // ?????????????
            if (param.ChangedParam(ClothParams.ParamType.WorldInfluence))
            {
                manager.Team.SetWorldInfluence(
                    team.TeamId,
                    param.MaxMoveSpeed,
                    param.MaxRotationSpeed,
                    param.GetWorldMoveInfluence(),
                    param.GetWorldRotationInfluence(),
                    param.UseResetTeleport,
                    param.TeleportDistance,
                    param.TeleportRotation,
                    param.ResetStabilizationTime,
                    param.TeleportResetMode,
                    param.UseClampRotation,
                    param.GetClampRotationAngle(clothData.clampRotationAlgorithm)
                    );
            }

            // ??????????????
            if (param.ChangedParam(ClothParams.ParamType.RestoreDistance) || changeMass)
            {
                compute.RestoreDistance.ChangeParam(
                    team.TeamId,
                    param.GetMass(),
                    param.RestoreDistanceVelocityInfluence,
                    param.GetStructDistanceStiffness(),
                    param.UseBendDistance,
                    param.GetBendDistanceStiffness(),
                    param.UseNearDistance,
                    param.GetNearDistanceStiffness()
                    );
            }

            // ????????????????????
            if (param.ChangedParam(ClothParams.ParamType.TriangleBend))
            {
                compute.TriangleBend.ChangeParam(
                    team.TeamId,
                    param.UseTriangleBend,
                    param.GetTriangleBendStiffness(clothData.triangleBendAlgorithm)
                    //param.UseTrianlgeBendIncludeFixed
                    );

                compute.Twist.ChangeParam(
                    team.TeamId,
                    param.UseTriangleBend && param.GetUseTwistCorrection(clothData.triangleBendAlgorithm),
                    param.TwistRecoveryPower
                    );
            }

            // ???????????????
            //if (param.ChangedParam(ClothParams.ParamType.Volume))
            //{
            //    compute.Volume.ChangeParam(team.TeamId, param.UseVolume, param.GetVolumeStretchStiffness(), param.GetVolumeShearStiffness());
            //}

            // ??????????????????????
            if (param.ChangedParam(ClothParams.ParamType.ClampDistance))
            {
                compute.ClampDistance.ChangeParam(team.TeamId, param.UseClampDistanceRatio, param.ClampDistanceMinRatio, param.ClampDistanceMaxRatio, param.ClampDistanceVelocityInfluence);
            }

#if false
            // ?????????????????????????
            if (param.ChangedParam(ClothParams.ParamType.ClampDistance))
            {
                compute.ClampDistance2.ChangeParam(team.TeamId, param.UseClampDistanceRatio, param.ClampDistanceMinRatio, param.ClampDistanceMaxRatio, param.ClampDistanceVelocityInfluence);
            }
#endif

            // ??????????????
            if (param.ChangedParam(ClothParams.ParamType.ClampPosition))
            {
                compute.ClampPosition.ChangeParam(team.TeamId, param.UseClampPositionLength, param.GetClampPositionLength(), param.ClampPositionAxisRatio, param.ClampPositionVelocityInfluence);
            }

            // ??????????????
            if (param.ChangedParam(ClothParams.ParamType.RestoreRotation))
            {
                var algo = clothData.clampRotationAlgorithm;
                if (algo == ClothParams.Algorithm.Algorithm_1)
                {
                    // [Algorithm 1]
                    compute.RestoreRotation.ChangeParam(
                    team.TeamId,
                    param.UseRestoreRotation,
                    param.GetRestoreRotationPower(clothData.restoreRotationAlgorithm),
                    param.GetRestoreRotationVelocityInfluence(clothData.restoreRotationAlgorithm)
                    );
                }
                else if (algo == ClothParams.Algorithm.Algorithm_2)
                {
                    // [Algorithm 2]
                    compute.CompositeRotation.ChangeParam(
                        team.TeamId,
                        param.UseClampRotation,
                        param.GetClampRotationAngle(algo),
                        param.UseRestoreRotation,
                        param.GetRestoreRotationPower(algo),
                        param.GetRestoreRotationVelocityInfluence(algo)
                        );
                }
            }

            // ??????????????
            if (param.ChangedParam(ClothParams.ParamType.ClampRotation))
            {
                var algo = clothData.clampRotationAlgorithm;
                if (algo == ClothParams.Algorithm.Algorithm_1)
                {
                    // [Algorithm 1]
                    compute.ClampRotation.ChangeParam(
                        team.TeamId,
                        param.UseClampRotation,
                        param.GetClampRotationAngle(algo),
                        param.ClampRotationVelocityInfluence
                        );
                }
                else if (algo == ClothParams.Algorithm.Algorithm_2)
                {
                    // [Algorithm 2]
                    compute.CompositeRotation.ChangeParam(
                        team.TeamId,
                        param.UseClampRotation,
                        param.GetClampRotationAngle(algo),
                        param.UseRestoreRotation,
                        param.GetRestoreRotationPower(algo),
                        param.GetRestoreRotationVelocityInfluence(algo)
                        );
                }

                // Algorithm??
                manager.Team.SetClampRotation(
                    team.TeamId,
                    param.UseClampRotation,
                    param.GetClampRotationAngle(algo)
                    );
            }

            // ?????????????????(???????)
            if (param.ChangedParam(ClothParams.ParamType.AdjustRotation))
            {
                compute.AdjustRotationWorker.ChangeParam(team.TeamId, true, (int)param.AdjustRotationMode, param.AdjustRotationVector);
            }

            // ???????
            if (param.ChangedParam(ClothParams.ParamType.ColliderCollision))
            {
                //manager.Team.SetFlag(team.TeamId, PhysicsManagerTeamData.Flag_Collision_KeepShape, param.KeepInitialShape);
                compute.Collision.ChangeParam(team.TeamId, param.UseCollision);
                //compute.EdgeCollision.ChangeParam(team.TeamId, param.UseCollision && param.UseEdgeCollision, param.EdgeCollisionRadius);
            }

            // ???????????????
            if (param.ChangedParam(ClothParams.ParamType.Spring))
            {
                compute.Spring.ChangeParam(team.TeamId, param.UseSpring, param.GetSpringPower());
            }

            // ????
            if (param.ChangedParam(ClothParams.ParamType.RotationInterpolation))
            {
                compute.LineWorker.ChangeParam(team.TeamId, param.UseLineAvarageRotation);
                manager.Team.SetFlag(team.TeamId, PhysicsManagerTeamData.Flag_FixedNonRotation, param.UseFixedNonRotation);
            }

            // ????
            if (param.ChangedParam(ClothParams.ParamType.Penetration))
            {
                compute.Penetration.ChangeParam(
                    team.TeamId,
                    param.UsePenetration,
                    param.GetPenetrationDistance(),
                    param.GetPenetrationRadius(),
                    param.PenetrationMaxDepth
                    );
            }

            // ????????
            // ????
            //if (param.ChangedParam(ClothParams.ParamType.BaseSkinning))
            //{
            //    compute.BaseSkinningWorker.ChangeParam(team.TeamId, team.SkinningUpdateFixed);
            //}

            //????????
            param.ClearChangeParam();
        }
    }
}
