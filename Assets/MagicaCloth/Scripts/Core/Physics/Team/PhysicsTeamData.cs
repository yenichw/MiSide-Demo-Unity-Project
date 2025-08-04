// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????????
    /// </summary>
    [System.Serializable]
    public class PhysicsTeamData : IDataHash
    {
        // ??????????????
        [SerializeField]
        private List<ColliderComponent> colliderList = new List<ColliderComponent>();

        /// <summary>
        /// ?????????????????
        /// </summary>
        [SerializeField]
        private List<ColliderComponent> penetrationIgnoreColliderList = new List<ColliderComponent>();

        /// <summary>
        /// ????????????
        /// </summary>
        //[SerializeField]
        //private List<Transform> skinningBoneList = new List<Transform>();

        /// <summary>
        /// ????????????????????
        /// </summary>
        [SerializeField]
        private bool mergeAvatarCollider = true;

        //=========================================================================================
        /// <summary>
        /// ????????????????
        /// </summary>
        private List<ColliderComponent> addColliderList = new List<ColliderComponent>();

        //=========================================================================================
        /// <summary>
        /// ???????????
        /// </summary>
        /// <returns></returns>
        public int GetDataHash()
        {
            return colliderList.GetDataHash();
        }

        //=========================================================================================
        public void Init(int teamId)
        {
            // ???????????????
            foreach (var collider in colliderList)
            {
                if (collider)
                {
                    collider.CreateColliderParticle(teamId);
                }
            }
        }

        public void Dispose(int teamId)
        {
            if (MagicaPhysicsManager.IsInstance())
            {
                // ???????????????
                foreach (var collider in colliderList)
                {
                    if (collider)
                    {
                        collider.RemoveColliderParticle(teamId);
                    }
                }

                // ????????????????????
                foreach (var collider in addColliderList)
                {
                    if (collider)
                    {
                        collider.RemoveColliderParticle(teamId);
                    }
                }
                addColliderList.Clear();
            }
        }

        /// <summary>
        /// ??????????????????????????
        /// </summary>
        /// <param name="collider"></param>
        public void AddCollider(ColliderComponent collider)
        {
            if (collider && addColliderList.Contains(collider) == false)
                addColliderList.Add(collider);
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <param name="collider"></param>
        public void RemoveCollider(ColliderComponent collider)
        {
            if (collider)
            {
                if (addColliderList.Contains(collider))
                    addColliderList.Remove(collider);
                // ?????????????????????????????(v1.12.7)
                if (colliderList.Contains(collider))
                    colliderList[colliderList.IndexOf(collider)] = null;
            }
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        internal void UpdateStatus()
        {
            foreach (var collider in colliderList)
                if (collider != null)
                    collider.UpdateStatus();
        }

        //=========================================================================================
        public int ColliderCount
        {
            get
            {
                return colliderList.Count;
            }
        }

        public List<ColliderComponent> ColliderList
        {
            get
            {
                return colliderList;
            }
        }

        public List<ColliderComponent> PenetrationIgnoreColliderList
        {
            get
            {
                return penetrationIgnoreColliderList;
            }
        }

        //public List<Transform> SkinningBoneList => skinningBoneList;

        public bool MergeAvatarCollider
        {
            get
            {
                return mergeAvatarCollider;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        public void ValidateColliderList()
        {
            // ??????null????????
            ShareDataObject.RemoveNullAndDuplication(colliderList);
        }
    }
}
