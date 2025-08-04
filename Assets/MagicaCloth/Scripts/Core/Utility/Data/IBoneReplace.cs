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
    public interface IBoneReplace
    {
        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        HashSet<Transform> GetUsedBones();

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="boneReplaceDict"></param>
        void ReplaceBone<T>(Dictionary<T, Transform> boneReplaceDict) where T : class;
    }
}
