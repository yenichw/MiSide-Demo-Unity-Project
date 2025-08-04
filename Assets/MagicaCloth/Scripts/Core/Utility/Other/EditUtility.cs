// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????????
    /// </summary>
    public static class EditUtility
    {
        /// <summary>
        /// ????????????????????
        /// (Define.OptimizeMesh???)
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static int GetOptimizeMesh(Mesh mesh)
        {
            if (mesh == null)
                return 0;

            // ??????
            var path = AssetDatabase.GetAssetPath(mesh);
            if (string.IsNullOrEmpty(path))
                return 0;

            int flag = 0;

            // ????????????
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer)
            {
                if (importer.optimizeMeshPolygons)
                    flag |= Define.OptimizeMesh.Unity2019_PolygonOrder;
                if (importer.optimizeMeshVertices)
                    flag |= Define.OptimizeMesh.Unity2019_VertexOrder;
                if (flag == 0)
                    flag = Define.OptimizeMesh.Nothing;
            }
            else
            {
                // ??????????????????????????
                flag |= Define.OptimizeMesh.Unity2019_PolygonOrder;
                flag |= Define.OptimizeMesh.Unity2019_VertexOrder;
            }

            return flag;
        }

        /// <summary>
        /// ?????KeepQuads????????????????
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        internal static bool IsKeepQuadsMesh(Mesh mesh)
        {
            if (mesh == null)
                return false;

            // ??????
            var path = AssetDatabase.GetAssetPath(mesh);
            if (string.IsNullOrEmpty(path))
                return false;

            // ????????????
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer)
                return importer.keepQuads;
            else
                return false;
        }
    }
}
#endif
