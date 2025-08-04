// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ???????
    /// </summary>
    public class MagicaAvatarRuntime : MagicaAvatarAccess
    {
        /// <summary>
        /// ????????????????
        /// </summary>
        private Dictionary<string, Transform> boneDict = new Dictionary<string, Transform>();

        /// <summary>
        /// ??????????????????
        /// </summary>
        private Dictionary<Transform, int> boneReferenceDict = new Dictionary<Transform, int>();

        /// <summary>
        /// ??????????
        /// </summary>
        private List<MagicaAvatarParts> avatarPartsList = new List<MagicaAvatarParts>();

        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <typeparam name="ColliderComponent"></typeparam>
        /// <returns></returns>
        private List<ColliderComponent> colliderList = new List<ColliderComponent>();

        //=========================================================================================
        /// <summary>
        /// ????
        /// </summary>
        public override void Create()
        {
            CreateBoneDict();
            CreateColliderList();
        }

        /// <summary>
        /// ??
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// ???
        /// </summary>
        public override void Active()
        {
        }

        /// <summary>
        /// ???
        /// </summary>
        public override void Inactive()
        {
        }

        //=========================================================================================
        public int AvatarPartsCount
        {
            get
            {
                return avatarPartsList.Count;
            }
        }

        public MagicaAvatarParts GetAvatarParts(int index)
        {
            return avatarPartsList[index];
        }

        //=========================================================================================
        /// <summary>
        /// ???????????????
        /// ???????????????????????????????????
        /// </summary>
        private void CreateBoneDict()
        {
            var tlist = owner.GetComponentsInChildren<Transform>();

            foreach (var t in tlist)
            {
                if (boneDict.ContainsKey(t.name))
                {
                    // Duplication name!
                    Debug.LogWarning(string.Format("{0} [{1}]", Define.GetErrorMessage(Define.Error.OverlappingTransform), t.name));
                }
                else
                {
                    boneDict.Add(t.name, t);
                    boneReferenceDict.Add(t, 1); // ???1????
                }
            }
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        private void CreateColliderList()
        {
            var clist = owner.GetComponentsInChildren<ColliderComponent>();
            if (clist != null && clist.Length > 0)
            {
                colliderList.AddRange(clist);
            }
        }

        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public int GetColliderCount()
        {
            if (Application.isPlaying)
            {
                return colliderList.Count;
            }
            else
            {
                return owner.GetComponentsInChildren<ColliderComponent>().Length;
            }
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        /// <returns></returns>
        public List<Transform> CheckOverlappingTransform()
        {
            var boneHash = new HashSet<string>();
            var overlapList = new List<Transform>();

            var tlist = owner.GetComponentsInChildren<Transform>();
            var root = owner.transform;

            foreach (var t in tlist)
            {
                if (t == root)
                    continue;
                if (boneHash.Contains(t.name))
                {
                    overlapList.Add(t);
                }
                else
                {
                    boneHash.Add(t.name);
                }
            }

            return overlapList;
        }

        //=========================================================================================
        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="parts"></param>
        public int AddAvatarParts(MagicaAvatarParts parts)
        {
            if (parts == null)
                return 0;

            //Debug.Log("AddAvatarParts:" + parts.name);

            // ?????????????????
            if (parts.HasParent)
                return parts.PartsId;

            // ????????
            if (parts.gameObject.activeSelf == false)
                parts.gameObject.SetActive(true);

            // ???(????????????????)
            owner.Init();

            // ???????????????
            var skinRendererList = parts.GetComponentsInChildren<SkinnedMeshRenderer>();
            //Debug.Log("skinRendererList:" + skinRendererList.Length);

            // Magica??????????
            //var magicaComponentList = parts.GetComponentsInChildren<CoreComponent>();
            var magicaComponentList = parts.GetMagicaComponentList();
            //Debug.Log("magicaComponentList:" + magicaComponentList.Length);

            // ????????????
            var root = owner.transform;
            var croot = parts.transform;
            parts.transform.SetParent(root, false);
            parts.transform.localPosition = Vector3.zero;
            parts.transform.localRotation = Quaternion.identity;
            parts.ParentAvatar = owner;
            avatarPartsList.Add(parts);


            // ???????????
            var partsBoneDict = parts.GetBoneDict();
            foreach (var bone in partsBoneDict.Values)
            {
                if (bone != croot)
                    AddBone(root, croot, bone);
            }

            // ???????????????
            foreach (var bone in partsBoneDict.Values)
            {
                if (bone != croot)
                {
                    var t = boneDict[bone.name];
                    boneReferenceDict[t]++;
                    //Debug.Log("reference[" + t.name + "]:" + boneReferenceDict[t]);
                }
            }

            // ??????????
            var boneReplaceDict = new Dictionary<Transform, Transform>();
            foreach (var bone in partsBoneDict.Values)
            {
                if (bone != croot)
                {
                    boneReplaceDict.Add(bone, boneDict[bone.name]);
                }
                else
                {
                    boneReplaceDict.Add(bone, root);
                }
            }

#if false
            foreach (var kv in avatar.Runtime.boneReplaceDict)
            {
                if (kv.Key != kv.Value)
                {
                    Debug.Log("??[" + kv.Key.name + "]->[" + kv.Value.name + "]");
                }
            }
#endif

            // ??????????????
            foreach (var skinRenderer in skinRendererList)
            {
                ReplaceSkinMeshRenderer(skinRenderer, boneReplaceDict);
            }

            // Magica?????????
            foreach (var comp in magicaComponentList)
            {
                ReplaceMagicaComponent(comp, boneReplaceDict);
            }

            // Magica?????????????????????
            if (colliderList.Count > 0)
            {
                foreach (var comp in magicaComponentList)
                {
                    var cloth = comp as BaseCloth;
                    if (cloth && cloth.TeamData.MergeAvatarCollider)
                    {
                        // ???
                        cloth.Init();

                        foreach (var col in colliderList)
                        {
                            cloth.AddCollider(col);
                        }
                    }
                }
            }

            // ????????????
            parts.gameObject.SetActive(false);

            // ????
            owner.OnAttachParts.Invoke(owner, parts);

            return parts.PartsId;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="root"></param>
        /// <param name="croot"></param>
        /// <param name="bone"></param>
        private void AddBone(Transform root, Transform croot, Transform bone)
        {
            if (boneDict.ContainsKey(bone.name))
            {
                // ???????
                return;
            }

            // ?????????????????
            Transform attachBone = root;
            Transform before = bone;
            Transform t = bone.parent;
            while (t && t != croot)
            {
                if (boneDict.ContainsKey(t.name))
                {
                    attachBone = boneDict[t.name];
                    break;
                }

                before = t;
                t = t.parent;
            }

            // ?????
            before.SetParent(attachBone, false);
            //Debug.Log("Add attach:" + attachBone.name + " before:" + before.name);

            // before??????????
            var blist = before.GetComponentsInChildren<Transform>();
            foreach (var b in blist)
            {
                if (boneDict.ContainsKey(b.name))
                {
                    // Duplication name!
                    Debug.LogWarning(string.Format("{0} [{1}]", Define.GetErrorMessage(Define.Error.AddOverlappingTransform), b.name));
                }
                else
                {
                    boneDict.Add(b.name, b);
                    boneReferenceDict.Add(b, 0); // ?????0????
                }
            }
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="skinRenderer"></param>
        private void ReplaceSkinMeshRenderer(SkinnedMeshRenderer skinRenderer, Dictionary<Transform, Transform> boneReplaceDict)
        {
            // ????????
            skinRenderer.rootBone = MeshUtility.GetReplaceBone(skinRenderer.rootBone, boneReplaceDict);

            // ?????
            var bones = skinRenderer.bones;
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = MeshUtility.GetReplaceBone(bones[i], boneReplaceDict);
            }
            skinRenderer.bones = bones;
        }

        /// <summary>
        /// Magica??????????
        /// </summary>
        /// <param name="comp"></param>
        private void ReplaceMagicaComponent(CoreComponent comp, Dictionary<Transform, Transform> boneReplaceDict)
        {
            comp.ChangeAvatar(boneReplaceDict);
        }

        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="parts"></param>
        public void RemoveAvatarParts(MagicaAvatarParts parts)
        {
            //Debug.Log("RemoveAvatarParts:" + parts.name);
            if (parts == null)
                return;
            if (avatarPartsList.Contains(parts) == false)
                return;

            // ?????
            parts.ParentAvatar = null;
            avatarPartsList.Remove(parts);

            // ????1??????????????????
            var removeBoneList = new List<Transform>();
            var croot = parts.transform;
            foreach (var bone in parts.GetBoneDict().Values)
            {
                if (bone == null)
                    continue;

                if (bone != croot)
                {
                    var t = boneDict[bone.name];
                    boneReferenceDict[t]--;
                    if (boneReferenceDict[t] == 0)
                    {
                        boneReferenceDict.Remove(t);
                        boneDict.Remove(t.name);
                        removeBoneList.Add(t);
                    }
                    //Debug.Log("reference[" + t.name + "]:" + boneReferenceDict[t]);
                }
            }

            // ?????
            foreach (var bone in removeBoneList)
            {
                if (bone)
                {
                    GameObject.Destroy(bone.gameObject);
                }
            }

#if false
            foreach (var bone in boneDict.Values)
            {
                if (bone)
                    Debug.Log("? bone:" + bone.name);
            }
            foreach (var kv in boneReferenceDict)
            {
                if (kv.Key)
                    Debug.Log("? reference[" + kv.Key.name + "]:" + kv.Value);
            }
#endif

            // ????????????
            if (colliderList.Count > 0)
            {
                // Magica??????????
                var magicaComponentList = parts.GetMagicaComponentList();

                foreach (var comp in magicaComponentList)
                {
                    var cloth = comp as BaseCloth;
                    if (cloth)
                    {
                        foreach (var col in colliderList)
                        {
                            cloth.RemoveCollider(col);
                        }
                    }
                }
            }

            // ?????
            GameObject.Destroy(parts.gameObject);

            // ????
            owner.OnDetachParts.Invoke(owner);
        }

        /// <summary>
        /// ??????????(???ID)
        /// </summary>
        /// <param name="partsId"></param>
        public void RemoveAvatarParts(int partsId)
        {
            var parts = avatarPartsList.Find((p) => p.PartsId == partsId);
            RemoveAvatarParts(parts);
        }
    }
}
