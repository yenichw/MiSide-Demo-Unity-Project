// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????????????????
    /// ???????????????
    /// </summary>
    public interface IEditorMesh
    {
        /// <summary>
        /// ???????????/??/?????
        /// </summary>
        /// <param name="wposList"></param>
        /// <param name="wnorList"></param>
        /// <param name="wtanList"></param>
        /// <returns>???</returns>
        int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList);

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        List<int> GetEditorTriangleList();

        /// <summary>
        /// ??????????????
        /// </summary>
        /// <returns></returns>
        List<int> GetEditorLineList();
    }
}
