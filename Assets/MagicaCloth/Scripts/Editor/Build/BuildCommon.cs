// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MagicaCloth
{
    public static partial class BuildManager
    {
        /// <summary>
        /// ????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataName"></param>
        /// <param name="savePrefabPath">??????????????.null=?????</param>
        /// <returns></returns>
        static T CreateShareData<T>(string dataName, string savePrefabPath) where T : ShareDataObject
        {
            // ???????
            var sdata = ShareDataObject.CreateShareData<T>(dataName);

            // ???????????????????
            // (???????????????????????)
            if (string.IsNullOrEmpty(savePrefabPath) == false)
            {
                SaveShareDataSubAsset(sdata, savePrefabPath);
            }

            return sdata;
        }

        /// <summary>
        /// ??????????????????????????
        /// </summary>
        /// <param name="sdata"></param>
        /// <param name="savePrefabPath"></param>
        /// <returns></returns>
        static bool SaveShareDataSubAsset(ShareDataObject sdata, string savePrefabPath)
        {
            // ???????????
            var savePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(savePrefabPath);
            if (savePrefab == null)
                return false;

            // ???????????????????????????
            if (PrefabUtility.IsPartOfImmutablePrefab(savePrefab))
            {
                return false;
            }

            // ??????????????????
            AssetDatabase.AddObjectToAsset(sdata, savePrefab);

            return true;
        }

        /// <summary>
        /// ??????????????????????????????????????
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        static bool IsExternalShareDataObject(CoreComponent core)
        {
            Debug.Assert(core);

            bool ret = false;
            try
            {
                if (core is BaseCloth)
                {
                    var cloth = core as BaseCloth;

                    if (cloth.ClothData != null)
                        ret = AssetDatabase.IsForeignAsset(cloth.ClothData) ? true : ret;
                    if (cloth is MagicaMeshSpring)
                        ret = AssetDatabase.IsForeignAsset((cloth as MagicaMeshSpring).SpringData) ? true : ret;
                }
                else if (core is MagicaRenderDeformer)
                {
                    ret = AssetDatabase.IsForeignAsset((core as MagicaRenderDeformer).Deformer.MeshData) ? true : ret;
                }
                else if (core is MagicaVirtualDeformer)
                {
                    ret = AssetDatabase.IsForeignAsset((core as MagicaVirtualDeformer).Deformer.MeshData) ? true : ret;
                }
            }
            catch (Exception)
            {
                // Reference is missing!
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        static bool IsNotCreated(CoreComponent core)
        {
            Debug.Assert(core);
            return Define.IsError(core.VerifyData());
        }

        /// <summary>
        /// ?????????????????
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        static bool IsOldFormat(CoreComponent core)
        {
            Debug.Assert(core);
            return core.IsOldDataVertion();
        }

        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        static bool IsOldAlgorithm(CoreComponent core)
        {
            Debug.Assert(core);
            if (core is BaseCloth)
            {
                var cloth = core as BaseCloth;

                // ???????????????????????
                if (cloth.Params.AlgorithmType != ClothParams.Algorithm.Algorithm_2)
                    return true;

                // ?????????????????????
                if (cloth.ClothData != null && Define.IsError(cloth.VerifyAlgorithmVersion()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// ???????????????????????????????????????
        /// </summary>
        /// <param name="gobj"></param>
        /// <param name="options"></param>
        /// <param name="coreComponents"></param>
        /// <returns></returns>
        static void GetBuildComponents(GameObject gobj, BuildOptions options, List<CoreComponent> coreComponents)
        {
            if (gobj == null)
                return;

            // ????????????
            bool isScene = gobj.scene.IsValid();

            // ??????????
            var components = new List<CoreComponent>(
                gobj.GetComponentsInChildren<CoreComponent>(options.includeInactive)
                // ???????????????
                .Where(x =>
                    x is MagicaBoneCloth && options.buildBoneCloth
                    || x is MagicaBoneSpring && options.buildBoneSpring
                    || x is MagicaMeshCloth && options.buildMeshCloth
                    || x is MagicaMeshSpring && options.buildMeshSpring
                    || x is MagicaRenderDeformer && options.buildRenderDeformer
                    || x is MagicaVirtualDeformer && options.buildVirtualDeformer
                )
                // ????????????????????????????????
                .Where(x => isScene == false || IsExternalShareDataObject(x) == false)
                // ?????
                .Where(x =>
                    options.forceBuild
                    || options.verificationOnly
                    || options.notCreated && IsNotCreated(x)
                    || options.upgradeFormatAndAlgorithm && (IsOldFormat(x) || IsOldAlgorithm(x))
                )
                );

            coreComponents.AddRange(components);
        }

        /// <summary>
        /// ??????????????????????
        /// </summary>
        /// <param name="coreComponents"></param>
        static void SortCoreComponents(List<CoreComponent> coreComponents)
        {
            // RenderDeformer > VirtualDeformer > ClothComponent ????????
            // ????????????????!
            coreComponents.Sort((a, b) => a.GetComponentType() < b.GetComponentType() ? -1 : 1);
        }

        /// <summary>
        /// ???????????????????
        /// ??????????????????????????????
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        static string GetAssetSavePath(CoreComponent core)
        {
            if (core == null)
                return null;

            return EditorUtility.IsPersistent(core) ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(core) : null;
        }

        /// <summary>
        /// ????????(????)?Missing???????????????????
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        static bool CheckMissingScripts(GameObject go)
        {
            return go.GetComponentsInChildren<Component>().Contains(null);
        }
    }
}
