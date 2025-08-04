// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp


using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// MagicaAvatar API
    /// </summary>
    public partial class MagicaAvatar : CoreComponent
    {
        /// <summary>
        /// ?????????????????????
        /// ??????????????????????????????
        /// ?????????????ID?????
        /// Attach avatar parts.
        /// Avatar parts to be attached are instantiated.
        //// Returns the attached avatar part ID.
        /// </summary>
        /// <param name="avatarPartsPrefab"></param>
        /// <param name="instanceAction">Action called after instantiation.</param>
        /// <returns></returns>
        public int AttachAvatarParts(GameObject avatarPartsPrefab, System.Action<GameObject> instanceAction = null)
        {
            var avatarPartsObject = Instantiate(avatarPartsPrefab);

            if (instanceAction != null)
                instanceAction(avatarPartsObject);

            return Runtime.AddAvatarParts(avatarPartsObject.GetComponent<MagicaAvatarParts>());
        }

        /// <summary>
        /// ??????????????
        /// ????????????????????
        /// Remove avatar parts.
        /// Removed avatar parts will be deleted.
        /// </summary>
        /// <param name="partsId"></param>
        public void DetachAvatarParts(int partsId)
        {
            Runtime.RemoveAvatarParts(partsId);
        }

        /// <summary>
        /// ??????????????
        /// ???????????????????
        /// Remove avatar parts.
        /// Removed avatar parts will be deleted.
        /// </summary>
        /// <param name="avatarObject"></param>
        public void DetachAvatarParts(GameObject avatarPartsObject)
        {
            Runtime.RemoveAvatarParts(avatarPartsObject.GetComponent<MagicaAvatarParts>());
        }

        /// <summary>
        /// ??????????????
        /// ???????????????????
        /// Remove avatar parts.
        /// Removed avatar parts will be deleted.
        /// </summary>
        /// <param name="avatarObject"></param>
        public void DetachAvatarParts(MagicaAvatarParts parts)
        {
            Runtime.RemoveAvatarParts(parts);
        }
    }
}
