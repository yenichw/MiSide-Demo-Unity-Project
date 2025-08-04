// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using Unity.Jobs;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ?????????
    /// </summary>
    public abstract class PhysicsManagerConstraint
    {
        // ??1??????
        [Range(1, 4)]
        public int iteration = 1;

        /// <summary>
        /// ??????
        /// </summary>
        public MagicaPhysicsManager Manager { get; set; }

        //=========================================================================================
        /// <summary>
        /// ???
        /// </summary>
        /// <param name="manager"></param>
        public void Init(MagicaPhysicsManager manager)
        {
            Manager = manager;
            Create();
        }

        /// <summary>
        /// ?????
        /// </summary>
        public abstract void Create();

        /// <summary>
        /// ???ID???????
        /// </summary>
        /// <param name="teamId"></param>
        public abstract void RemoveTeam(int teamId);

        /// <summary>
        /// ?????
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// ?????????
        /// </summary>
        /// <returns></returns>
        public virtual int GetIterationCount()
        {
            return iteration;
        }

        /// <summary>
        /// ?????(???????????)
        /// </summary>
        /// <param name="dtime">??????</param>
        /// <param name="updatePower">90ups?????????</param>
        /// <param name="iteration">?????????????(0~)</param>
        public abstract JobHandle SolverConstraint(int runCount, float dtime, float updatePower, int iteration, JobHandle jobHandle);
    }
}
