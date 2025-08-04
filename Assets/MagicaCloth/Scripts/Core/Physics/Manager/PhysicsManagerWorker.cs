// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using Unity.Jobs;

namespace MagicaCloth
{
    /// <summary>
    /// ???????????
    /// </summary>
    public abstract class PhysicsManagerWorker
    {
        /// <summary>
        /// ??????
        /// </summary>
        public MagicaPhysicsManager Manager { get; set; }

        //=========================================================================================
        protected virtual void Start()
        {
        }

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
        /// ????ID???????
        /// </summary>
        /// <param name="group"></param>
        public abstract void RemoveGroup(int group);

        /// <summary>
        /// ?????
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// ??????????????????????????
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public abstract void Warmup();

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        public abstract JobHandle PreUpdate(JobHandle jobHandle);

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="jobHandle"></param>
        /// <returns></returns>
        public abstract JobHandle PostUpdate(JobHandle jobHandle);
    }
}
