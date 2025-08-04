// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ?????????????
    /// </summary>
    [HelpURL("https://magicasoft.jp/avatar/")]
    [AddComponentMenu("MagicaCloth/MagicaAvatar")]
    public partial class MagicaAvatar : CoreComponent
    {
        /// <summary>
        /// ????????
        /// </summary>
        private const int DATA_VERSION = 1;

        /// <summary>
        /// ??????????
        /// ???????????????????????????????
        /// </summary>
        [SerializeField]
        private bool dataReset;

        /// <summary>
        /// ???????
        /// </summary>
        MagicaAvatarRuntime runtime = new MagicaAvatarRuntime();

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// Avatar parts attach event.
        /// </summary>
        public AvatarPartsAttachEvent OnAttachParts = new AvatarPartsAttachEvent();

        /// <summary>
        /// ?????????????
        /// Avatar parts detach event.
        /// </summary>
        public AvatarPartsDetachEvent OnDetachParts = new AvatarPartsDetachEvent();

        //=========================================================================================
        public override ComponentType GetComponentType()
        {
            return ComponentType.Avatar;
        }

        //=========================================================================================
        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <returns></returns>
        public override int GetDataHash()
        {
            int hash = 0;
            return hash;
        }

        //=========================================================================================
        public bool DataReset
        {
            set
            {
                dataReset = value;
            }
            get
            {
                return dataReset;
            }
        }

        public MagicaAvatarRuntime Runtime
        {
            get
            {
                runtime.SetParent(this);
                return runtime;
            }
        }

        //=========================================================================================
        void Reset()
        {
            // ??????????????
            DataReset = true;
        }

        void OnValidate()
        {
        }

        protected override void OnInit()
        {
            Runtime.Create();
        }

        protected override void OnDispose()
        {
            Runtime.Dispose();
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnActive()
        {
            Runtime.Active();
        }

        protected override void OnInactive()
        {
            Runtime.Inactive();
        }

        //=========================================================================================
        public override int GetVersion()
        {
            return DATA_VERSION;
        }

        /// <summary>
        /// ???????????????????
        /// </summary>
        /// <returns></returns>
        public override int GetErrorVersion()
        {
            return 0;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns></returns>
        public override void CreateVerifyData()
        {
            base.CreateVerifyData();
        }

        /// <summary>
        /// ?????????(???????)???
        /// </summary>
        /// <returns></returns>
        public override Define.Error VerifyData()
        {
            if (Application.isPlaying)
            {
                // ???
                return Define.Error.None;
            }
            else
            {
                // ??????
                // ??????????????
                var olist = Runtime.CheckOverlappingTransform();
                if (olist.Count > 0)
                    return Define.Error.OverlappingTransform;

                return Define.Error.None;
            }
        }

        public override string GetInformation()
        {
            StaticStringBuilder.Clear();

            if (Application.isPlaying)
            {
                // ???
                if (Runtime.AvatarPartsCount > 0)
                {
                    StaticStringBuilder.Append("Connection avatar parts:");
                    int cnt = Runtime.AvatarPartsCount;
                    for (int i = 0; i < cnt; i++)
                    {
                        StaticStringBuilder.AppendLine();
                        StaticStringBuilder.Append("    [", Runtime.GetAvatarParts(i).name, "]");
                    }
                }
                else
                {
                    StaticStringBuilder.Append("No avatar parts connected.");
                }
            }
            else
            {
                // ??????
                // ??????????????
                var olist = Runtime.CheckOverlappingTransform();
                if (olist.Count > 0)
                {
                    StaticStringBuilder.Append("There are duplicate game object names.");
                    foreach (var t in olist)
                    {
                        StaticStringBuilder.AppendLine();
                        StaticStringBuilder.Append("* ", t.name);
                    }
                }
                else
                {
                    StaticStringBuilder.Append("No problem.");
                }

                StaticStringBuilder.AppendLine();
                StaticStringBuilder.Append("Collider : ", Runtime.GetColliderCount());
            }

            return StaticStringBuilder.ToString();
        }

        //=========================================================================================
        /// <summary>
        /// ?????????????
        /// </summary>
        /// <returns></returns>
        public override List<ShareDataObject> GetAllShareDataObject()
        {
            var slist = base.GetAllShareDataObject();
            return slist;
        }

        /// <summary>
        /// source?????????????????
        /// ??????????????
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
        {
            return null;
        }
    }
}
