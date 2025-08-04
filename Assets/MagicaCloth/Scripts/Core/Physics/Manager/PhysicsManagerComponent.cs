// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MagicaCloth
{
    /// <summary>
    /// ????????????
    /// </summary>
    public class PhysicsManagerComponent : PhysicsManagerAccess
    {
        /// <summary>
        /// ???????????????
        /// ???????????????????????????!
        /// ????????? comp.Status.IsInitSuccess ?????
        /// </summary>
        private readonly HashSet<CoreComponent> componentSet = new HashSet<CoreComponent>();

        /// <summary>
        /// ?????????????????????????
        /// </summary>
        private HashSet<ParticleComponent> dataUpdateParticleSet = new HashSet<ParticleComponent>();

        //=========================================================================================
        /// <summary>
        /// ????
        /// </summary>
        public override void Create()
        {
        }

        /// <summary>
        /// ??
        /// </summary>
        public override void Dispose()
        {
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        public int ComponentCount
        {
            get
            {
                return componentSet.Count;
            }
        }

        /// <summary>
        /// ????????????????????.
        /// Returns a list of copies of the registration component.
        /// </summary>
        /// <returns></returns>
        public List<CoreComponent> GetComponentList()
        {
            return new List<CoreComponent>(componentSet);
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        /// <param name="act"></param>
        public void ComponentAction(System.Action<CoreComponent> act)
        {
            foreach (var comp in componentSet)
            {
                if (comp != null)
                    act(comp);
            }
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        public void UpdateComponentStatus()
        {
            foreach (var comp in componentSet)
            {
                if (comp == null)
                    continue;

                if (comp.Status.IsInitSuccess == false)
                    continue;

                comp.Status.UpdateStatus();
            }
        }

        //=========================================================================================
        public void AddComponent(CoreComponent comp)
        {
            //Debug.Log($"AddComponent:{comp.name}");
            componentSet.Add(comp);
        }

        public void RemoveComponent(CoreComponent comp)
        {
            //Debug.Log($"RemoveComponent:{comp.name}");
            if (componentSet.Contains(comp))
                componentSet.Remove(comp);
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????????????????
        /// </summary>
        /// <param name="comp"></param>
        internal void ReserveDataUpdateParticleComponent(ParticleComponent comp)
        {
            dataUpdateParticleSet.Add(comp);
        }

        /// <summary>
        /// ?????????????????????????????????????
        /// </summary>
        internal void DataUpdateParticleComponent()
        {
            if (dataUpdateParticleSet.Count > 0)
            {
                foreach (var comp in dataUpdateParticleSet)
                {
                    comp?.DataUpdate();
                }
                dataUpdateParticleSet.Clear();
            }
        }
    }
}
