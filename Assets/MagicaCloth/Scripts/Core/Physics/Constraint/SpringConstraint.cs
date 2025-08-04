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
    /// ???????
    /// BasePos????????
    /// </summary>
    public class SpringConstraint : PhysicsManagerConstraint
    {
        /// <summary>
        /// ????????????
        /// </summary>
        public struct GroupData
        {
            public int teamId;
            public int active;

            /// <summary>
            /// ??????
            /// </summary>
            public float spring;
        }
        public FixedNativeList<GroupData> groupList;

        //=========================================================================================
        public override void Create()
        {
            groupList = new FixedNativeList<GroupData>();
        }

        public override void Release()
        {
            groupList.Dispose();
        }

        //=========================================================================================
        public int AddGroup(int teamId, bool active, float spring)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];

            var gdata = new GroupData();
            gdata.teamId = teamId;
            gdata.active = active ? 1 : 0;
            gdata.spring = spring;

            int group = groupList.Add(gdata);
            return group;
        }

        public override void RemoveTeam(int teamId)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.springGroupIndex;
            if (group < 0)
                return;

            // ?????
            groupList.Remove(group);
        }

        public void ChangeParam(int teamId, bool active, float spring)
        {
            var teamData = MagicaPhysicsManager.Instance.Team.teamDataList[teamId];
            int group = teamData.springGroupIndex;
            if (group < 0)
                return;

            var gdata = groupList[group];
            gdata.active = active ? 1 : 0;
            gdata.spring = spring;
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
            //if (ActiveCount == 0)
            if (groupList.Count == 0)
                return jobHandle;

            // ???????(?????????????)
            var job1 = new SpringJob()
            {
                updatePower = updatePower,
                runCount = runCount,

                groupList = groupList.ToJobArray(),

                teamDataList = Manager.Team.teamDataList.ToJobArray(),
                teamIdList = Manager.Particle.teamIdList.ToJobArray(),

                flagList = Manager.Particle.flagList.ToJobArray(),
                basePosList = Manager.Particle.basePosList.ToJobArray(),

                nextPosList = Manager.Particle.InNextPosList.ToJobArray()
            };
            jobHandle = job1.Schedule(Manager.Particle.Length, 64, jobHandle);

            return jobHandle;
        }

        /// <summary>
        /// ??????????
        /// ???????????
        /// </summary>
        [BurstCompile]
        struct SpringJob : IJobParallelFor
        {
            public float updatePower;
            public int runCount;

            [Unity.Collections.ReadOnly]
            public NativeArray<GroupData> groupList;

            // ???
            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerTeamData.TeamData> teamDataList;
            [Unity.Collections.ReadOnly]
            public NativeArray<int> teamIdList;

            [Unity.Collections.ReadOnly]
            public NativeArray<PhysicsManagerParticleData.ParticleFlag> flagList;
            [Unity.Collections.ReadOnly]
            public NativeArray<float3> basePosList;

            public NativeArray<float3> nextPosList;

            // ????????
            public void Execute(int index)
            {
                var flag = flagList[index];
                if (flag.IsValid() == false || flag.IsFixed())
                    return;

                var team = teamDataList[teamIdList[index]];
                if (team.IsActive() == false)
                    return;
                if (team.springGroupIndex < 0)
                    return;
                // ????
                if (team.IsUpdate(runCount) == false)
                    return;

                // ???????
                var gdata = groupList[team.springGroupIndex];
                if (gdata.active == 0)
                    return;

                var nextpos = nextPosList[index];

                // basepos????????
                var basepos = basePosList[index];
                float power = 1.0f - math.pow(1.0f - gdata.spring, updatePower);
                nextpos = math.lerp(nextpos, basepos, power);

                // ????
                nextPosList[index] = nextpos;
            }
        }
    }
}
