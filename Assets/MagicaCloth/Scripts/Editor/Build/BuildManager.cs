// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicaCloth
{
    public static partial class BuildManager
    {
        //=========================================================================================
        /// <summary>
        /// MagicaCloth?????????????????????????
        /// ???????????????
        /// Upgrading old formats of MagicaCloth components to the latest.
        /// If it is already up-to-date, do nothing.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static Define.Error UpgradeComponent(CoreComponent core)
        {
            Define.Error result = Define.Error.None;
            if (core == null)
                result = Define.Error.BuildInvalidComponent;

            if (core)
            {
                // ???????????????????????????????
                string savePrefabPath = GetAssetSavePath(core);
                bool isPrefab = string.IsNullOrEmpty(savePrefabPath) == false;

                if (Define.IsNormal(result))
                {
                    var serializedObject = new SerializedObject(core);
                    serializedObject.Update();

                    if (core.UpgradeFormat())
                    {
                        // ????
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(core);

                        // ????
                        if (isPrefab)
                            AssetDatabase.SaveAssets();

                        if (Define.IsNormal(result))
                            Debug.Log($"<color=yellow>[Upgrade]</color> {core.name}");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// MagicaCloth?????????????[Create]?????.
        /// Execute the MagicaCloth component's data creation [Create].
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static Define.Error CreateComponent(CoreComponent core)
        {
            Define.Error result = Define.Error.None;
            if (core == null)
                result = Define.Error.BuildInvalidComponent;

            if (core)
            {
                // ???????????????????????????????
                string savePrefabPath = GetAssetSavePath(core);
                bool isPrefab = string.IsNullOrEmpty(savePrefabPath) == false;

                if (Define.IsNormal(result))
                {
                    //Debug.Log($"Started creating. [{core.name}] isPrefab:{isPrefab} path:{savePrefabPath}");
                    var serializedObject = new SerializedObject(core);
                    serializedObject.Update();

                    // ?????????????
                    if (core is MagicaBoneCloth)
                        result = CreateBoneCloth(core, serializedObject, savePrefabPath);
                    else if (core is MagicaBoneSpring)
                        result = CreateBoneSpring(core, serializedObject, savePrefabPath);
                    else if (core is MagicaMeshCloth)
                        result = CreateMeshCloth(core, serializedObject, savePrefabPath);
                    else if (core is MagicaMeshSpring)
                        result = CreateMeshSpring(core, serializedObject, savePrefabPath);
                    else if (core is MagicaRenderDeformer)
                        result = CreateRenderDeformer(core, serializedObject, savePrefabPath);
                    else if (core is MagicaVirtualDeformer)
                        result = CreateVirtualDeformer(core, serializedObject, savePrefabPath);

                    // ??????
                    if (Define.IsNormal(result))
                        result = core.VerifyData();

                    // ????
                    if (isPrefab)
                    {
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            // ??
            if (result == Define.Error.None)
                Debug.Log($"<color=cyan>[Creation]</color> {core.name}");
            else
                Debug.LogError($"<color=cyan>[Creation]</color> <color=red>Failed!</color> {core.name}\n{Define.GetErrorMessage(result)}");

            return result;
        }

        /// <summary>
        /// ??????????????????????????
        /// Execute data creation for the specified component list.
        /// </summary>
        /// <param name="coreComponents"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BuildResult BuildFromComponents(List<CoreComponent> coreComponents, BuildOptions options)
        {
            if (coreComponents.Count == 0)
                return new BuildResult(Define.Error.Cancel);

            // ??????????(??????????????????)
            SortCoreComponents(coreComponents);

            // ??????????
            var result = new BuildResult();
            foreach (var core in coreComponents)
            {
                //Debug.Log(core.name);
                var err = Define.Error.None;

                if (options.verificationOnly)
                {
                    // ????
                    if (IsOldFormat(core))
                        Debug.Log($"<color=yellow>[Old Format]</color> {core.name}");
                    if (IsOldAlgorithm(core))
                        Debug.Log($"<color=yellow>[Old Algorithm]</color> {core.name}");
                    var e = core.VerifyData();
                    if (e == Define.Error.EmptyData)
                        Debug.Log($"<color=cyan>[Not Created]</color> {core.name}");
                    else if (Define.IsError(e))
                        Debug.Log($"<color=red>[In Error]</color> {core.name}\n{Define.GetErrorMessage(e)}");
                    //if (IsNotCreated(core))
                    //    Debug.Log($"<color=cyan>[Not created or in error]</color> {core.name}");
                }
                else
                {
                    // ??
                    // ???????
                    if (options.upgradeFormatAndAlgorithm && (IsOldFormat(core) || IsOldAlgorithm(core)))
                    {
                        err = UpgradeComponent(core);
                        if (Define.IsError(err))
                        {
                            result.SetError(err);
                            //Debug.LogError(Define.GetErrorMessage(err));

                            // ???????
                            if (options.errorStop)
                                break;
                        }
                    }

                    // ??
                    err = CreateComponent(core);
                    if (Define.IsError(err))
                    {
                        result.SetError(err);
                        //Debug.LogError(Define.GetErrorMessage(err));

                        // ???????
                        if (options.errorStop)
                            break;
                    }

                    if (Define.IsNormal(err))
                        result.SetSuccess();
                }
            }

            return result;
        }

        //=========================================================================================
        /// <summary>
        /// ????????????????????????
        /// Perform data creation on objects in the scene.
        /// </summary>
        /// <param name="gobj"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static BuildResult BuildFromSceneObject(GameObject gobj, BuildOptions options)
        {
            if (gobj == null)
                return new BuildResult(Define.Error.BuildInvalidGameObject);
            if (gobj.scene.IsValid() == false)
                return new BuildResult(Define.Error.BuildNotSceneObject);

            var result = new BuildResult();

            // ??????????
            var coreComponents = new List<CoreComponent>();
            GetBuildComponents(gobj, options, coreComponents);

            if (coreComponents.Count > 0)
            {
                Debug.Log($"<color=#f39800>[GameObject]</color> {gobj.name}");

                // ???
                result = BuildFromComponents(coreComponents, options);
            }

            return result;
        }

        /// <summary>
        /// ??????????????????????????
        /// Perform all data creation for prefab assets.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BuildResult BuildFromAssetPath(string path, BuildOptions options)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                return new BuildResult(Define.Error.BuildInvalidPrefab);

            // ???????????????????????????
            if (PrefabUtility.IsPartOfImmutablePrefab(prefab))
                return new BuildResult(Define.Error.Cancel);

            var result = new BuildResult();

            // ??????????
            var coreComponents = new List<CoreComponent>();
            GetBuildComponents(prefab, options, coreComponents);

            if (coreComponents.Count > 0)
            {
                Debug.Log($"<color=#f39800>[Prefab]</color> {path}");

                // ????????(missing)?????????????????
                if (options.verificationOnly == false && CheckMissingScripts(prefab))
                    return new BuildResult(Define.Error.BuildMissingScriptOnPrefab);

                // ???
                result.Merge(BuildFromComponents(coreComponents, options));

                // ?????????????
                if (result.SuccessCount > 0 && options.verificationOnly == false)
                    ShareDataPrefabExtension.CleanUpSubAssets(prefab, log: false);
            }

            return result;
        }

        /// <summary>
        /// ??????????????????????????????
        /// Perform all data construction for the scene's internal objects.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BuildResult BuildFromScenePath(string path, BuildOptions options)
        {
            Scene targetScene = new Scene();
            bool isOpened = false;

            // ???????????????
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.path == path)
                {
                    targetScene = scene;
                    isOpened = true;
                }
            }

            if (isOpened == false)
            {
                targetScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }
            if (targetScene.IsValid() == false)
                return new BuildResult(Define.Error.BuildInvalidScene);

            Debug.Log($"<color=#BFFF00>[Scene]</color> {path}");

            // ????????????????
            var coreComponents = new List<CoreComponent>();
            foreach (var go in targetScene.GetRootGameObjects())
                GetBuildComponents(go, options, coreComponents);

            var result = new BuildResult();
            if (coreComponents.Count > 0)
            {
                // ???
                result.Merge(BuildFromComponents(coreComponents, options));

                // 1??????????????????????
                if (result.SuccessCount > 0 && options.verificationOnly == false)
                {
                    EditorSceneManager.SaveScene(targetScene);
                }
            }

            if (isOpened == false)
                EditorSceneManager.CloseScene(targetScene, true);

            return result;
        }
    }
}
