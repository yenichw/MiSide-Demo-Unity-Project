// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// MonoBehaviour???????????????????
    /// </summary>
    public abstract partial class BaseComponent : MonoBehaviour
    {
        //=========================================================================================
        /// <summary>
        /// ????????????
        /// </summary>
        /// <returns></returns>
        public abstract ComponentType GetComponentType();
    }
}
