// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????????
    /// </summary>
    public class DressUpControl : MonoBehaviour
    {
        [SerializeField]
        private GameObject partsItemPrefab;

        [SerializeField]
        private VerticalLayoutGroup verticalLayoutGroup;

        [Space]

        [SerializeField]
        private MagicaAvatar avatar;

        /// <summary>
        /// ????????
        /// </summary>
        [System.Serializable]
        public class AvatarPartsGroup
        {
            /// <summary>
            /// ?????
            /// </summary>
            public string groupName;

            /// <summary>
            /// AvatarParts????????
            /// </summary>
            public List<GameObject> partsPrefabList = new List<GameObject>();

            /// <summary>
            /// ??ID
            /// </summary>
            [System.NonSerialized]
            public int id;

            /// <summary>
            /// ??????????
            /// </summary>
            [System.NonSerialized]
            public int handle;

            /// <summary>
            /// ????????????
            /// </summary>
            [System.NonSerialized]
            public int index;
        }

        [SerializeField]
        public List<AvatarPartsGroup> avatarPartsGroupList = new List<AvatarPartsGroup>();


        void Start()
        {
            Init();
        }

        void Update()
        {
        }

        private void OnDestroy()
        {
            avatar = null;
            partsItemPrefab = null;
            verticalLayoutGroup = null;
        }

        private void Init()
        {
            for (int i = 0; i < avatarPartsGroupList.Count; i++)
            {
                // UI??????????
                var group = avatarPartsGroupList[i];
                group.id = i;
                var item = Instantiate(partsItemPrefab);
                var ui = item.GetComponent<UIPartsItem>();
                ui.Init(group.groupName, i, (id, dir) =>
                {
                    ChangeParts(id, dir);
                });
                item.transform.SetParent(verticalLayoutGroup.transform);
                item.transform.localScale = Vector3.one;

                // ????
                ChangeParts(i, 0);
            }
        }

        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dir"></param>
        private void ChangeParts(int id, int dir)
        {
            //Debug.Log("id:" + id + " dir:" + dir);

            var group = avatarPartsGroupList[id];

            if (group.handle != 0)
            {
                avatar.DetachAvatarParts(group.handle);
                group.handle = 0;
            }

            var index = group.index + dir;
            int cnt = group.partsPrefabList.Count;
            if (index < 0)
                index += cnt;
            else if (index >= cnt)
                index -= cnt;
            group.index = index;

            group.handle = avatar.AttachAvatarParts(group.partsPrefabList[index]);
        }

        /// <summary>
        /// ??????
        /// </summary>
        public void Clear()
        {
            foreach (var group in avatarPartsGroupList)
            {
                if (group.handle != 0)
                {
                    avatar.DetachAvatarParts(group.handle);
                    group.handle = 0;
                }
            }
        }
    }
}
