// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ????????????????????
    /// </summary>
    [System.Serializable]
    public class BoneClothTarget : IDataHash, IBoneReplace
    {
        /// <summary>
        /// ???????????
        /// </summary>
        [SerializeField]
        private List<Transform> rootList = new List<Transform>();

        /// <summary>
        /// ?????
        /// </summary>
        public enum ConnectionMode
        {
            Line = 0,
            MeshAutomatic = 1,
            MeshSequentialLoop = 2,
            MeshSequentialNoLoop = 3,
        }
        [SerializeField]
        private ConnectionMode connection = ConnectionMode.Line;

        /// <summary>
        /// ?????????????????(????????)
        /// </summary>
        [SerializeField]
        [Range(10.0f, 90.0f)]
        private float sameSurfaceAngle = 80.0f;

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        private int[] parentIndexList = null;

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public int GetDataHash()
        {
            int hash = 0;
            hash += rootList.GetDataHash();
            return hash;
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        public int RootCount
        {
            get
            {
                return rootList.Count;
            }
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Transform GetRoot(int index)
        {
            if (index < rootList.Count)
                return rootList[index];

            return null;
        }

        /// <summary>
        /// ???????????????????????????(-1)
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public int GetRootIndex(Transform root)
        {
            return rootList.IndexOf(root);
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        public void AddParentTransform()
        {
            if (rootList.Count > 0)
            {
                HashSet<Transform> parentSet = new HashSet<Transform>();
                foreach (var t in rootList)
                {
                    if (t && t.parent)
                        parentSet.Add(t.parent);
                }

                parentIndexList = new int[parentSet.Count];

                int i = 0;
                foreach (var parent in parentSet)
                {
                    int index = -1;
                    if (parent)
                    {
                        index = MagicaPhysicsManager.Instance.Bone.AddBone(parent);
                    }
                    parentIndexList[i] = index;
                    i++;
                }
            }
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        public void RemoveParentTransform()
        {
            if (MagicaPhysicsManager.IsInstance())
            {
                if (parentIndexList != null && parentIndexList.Length > 0)
                {
                    for (int i = 0; i < parentIndexList.Length; i++)
                    {
                        var index = parentIndexList[i];
                        if (index >= 0)
                        {
                            MagicaPhysicsManager.Instance.Bone.RemoveBone(index);
                        }
                    }
                }
            }

            parentIndexList = null;
        }

        /// <summary>
        /// ?????????????????????????
        /// </summary>
        public void ResetFuturePredictionParentTransform()
        {
            if (parentIndexList != null && parentIndexList.Length > 0)
            {
                for (int i = 0; i < parentIndexList.Length; i++)
                {
                    var index = parentIndexList[i];
                    if (index >= 0)
                    {
                        MagicaPhysicsManager.Instance.Bone.ResetFuturePrediction(index);
                    }
                }
            }
        }

        /// <summary>
        /// ????UnityPhysics????????????
        /// </summary>
        /// <param name="sw"></param>
        public void ChangeUnityPhysicsCount(bool sw)
        {
            if (parentIndexList != null && parentIndexList.Length > 0)
            {
                for (int i = 0; i < parentIndexList.Length; i++)
                {
                    var index = parentIndexList[i];
                    if (index >= 0)
                    {
                        MagicaPhysicsManager.Instance.Bone.ChangeUnityPhysicsCount(index, sw);
                    }
                }
            }
        }

        /// <summary>
        /// ???????
        /// </summary>
        public ConnectionMode Connection
        {
            get
            {
                return connection;
            }
        }

        public float SameSurfaceAngle
        {
            get
            {
                return sameSurfaceAngle;
            }
        }

        public bool IsMeshConnection
        {
            get
            {
                return connection == ConnectionMode.MeshAutomatic || connection == ConnectionMode.MeshSequentialLoop || connection == ConnectionMode.MeshSequentialNoLoop;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="boneReplaceDict"></param>
        public void ReplaceBone<T>(Dictionary<T, Transform> boneReplaceDict) where T : class
        {
            for (int i = 0; i < rootList.Count; i++)
            {
                rootList[i] = MeshUtility.GetReplaceBone(rootList[i], boneReplaceDict);
            }
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        public HashSet<Transform> GetUsedBones()
        {
            return new HashSet<Transform>(rootList);
        }
    }
}
