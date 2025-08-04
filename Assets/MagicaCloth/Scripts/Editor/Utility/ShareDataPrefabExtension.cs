// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace MagicaCloth
{
    /// <summary>
    /// ???????????????????
    /// ?????Apply???????????????????????????????????????????
    /// ????????????IShareDataObject?????GetAllShareDataObject()?????????????????????
    /// </summary>
    [InitializeOnLoad]
    internal class ShareDataPrefabExtension
    {
        private enum Mode
        {
            Saving = 1,
            Update = 2,
        }
        static List<GameObject> prefabInstanceList = new List<GameObject>();
        static List<Mode> prefabModeList = new List<Mode>();

        /// <summary>
        /// ??????????????
        /// </summary>
        static ShareDataPrefabExtension()
        {
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
            PrefabStage.prefabSaving += OnPrefabSaving;
        }

        /// <summary>
        /// ?????????????
        /// </summary>
        /// <param name="obj"></param>
        static void OnPrefabStageClosing(PrefabStage pstage)
        {
            //#if UNITY_2020_1_OR_NEWER
            //            Debug.Log($"OnPrefabStageClosing() root:[{pstage.prefabContentsRoot.name}] id:{pstage.prefabContentsRoot.GetInstanceID()} path:{pstage.assetPath}");
            //#else
            //            Debug.Log($"OnPrefabStageClosing() root:[{pstage.prefabContentsRoot.name}] id:{pstage.prefabContentsRoot.GetInstanceID()} path:{pstage.prefabAssetPath}");
            //#endif
            if (prefabInstanceList.Count > 0)
            {
                DelayAnalyze();
            }
        }


        /// <summary>
        /// ????????????????????
        /// </summary>
        /// <param name="instance"></param>
        static void OnPrefabSaving(GameObject instance)
        {
            //Debug.Log($"OnPrefabSaving() instance:[{instance.name}] id:{instance.GetInstanceID()}");
            if (prefabInstanceList.Contains(instance) == false)
            {
                prefabInstanceList.Add(instance);
                prefabModeList.Add(Mode.Saving);
                DelayAnalyze();
            }
        }

        /// <summary>
        /// ?????Apply??????????
        /// instance???????????????????
        /// ??????????????????????????????????????????????
        /// </summary>
        /// <param name="instance"></param>
        static void OnPrefabInstanceUpdate(GameObject instance)
        {
            //Debug.Log($"OnPrefabInstanceUpdate() instance:{instance.name} id:{instance.GetInstanceID()}");
            if (prefabInstanceList.Contains(instance))
                return;
            prefabInstanceList.Add(instance);
            prefabModeList.Add(Mode.Update);
            EditorApplication.delayCall += DelayAnalyze;
        }

        static void DelayAnalyze()
        {
            //Debug.Log($"DelayAnalyze.start:{prefabInstanceList.Count}");

            EditorApplication.delayCall -= DelayAnalyze;
            for (int i = 0; i < prefabInstanceList.Count; i++)
            {
                var instance = prefabInstanceList[i];
                var mode = prefabModeList[i];

                if (instance)
                {
                    Analyze(instance, mode);
                }
            }

            prefabInstanceList.Clear();
            prefabModeList.Clear();

            //Debug.Log("DelayAnalyze.end.");
        }

        static void Analyze(GameObject instance, Mode mode)
        {
            var pstage = PrefabStageUtility.GetCurrentPrefabStage();
            bool isVariant = PrefabUtility.IsPartOfVariantPrefab(instance);
            bool onStage = pstage != null ? pstage.IsPartOfPrefabContents(instance) : false;
            //Debug.Log($"Analyze instance:{instance.name} id:{instance.GetInstanceID()} IsVariant:{isVariant} Mode:{mode} PStage:{pstage != null} OnStage:{onStage}");

            string pstageAssetPath = string.Empty;
            if (pstage != null)
            {
#if UNITY_2020_1_OR_NEWER
                pstageAssetPath = pstage.assetPath;
#else
                pstageAssetPath = pstage.prefabAssetPath;
#endif
                //Debug.Log($"pstage root:{pstage.prefabContentsRoot.name} id:{pstage.prefabContentsRoot.GetInstanceID()} path:{pstageAssetPath}");
            }
            else
            {
                //Debug.Log($"pstage = (null)");
            }

            string prefabAssetPath = string.Empty;
            string baseAssetPath = string.Empty;

            if (mode == Mode.Saving)
            {
                // ???????????
                prefabAssetPath = pstageAssetPath;
                var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);

                // ????????????
                var baseAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefabAsset);
                baseAssetPath = AssetDatabase.GetAssetPath(baseAsset);
            }
            else
            {
                if (pstage != null)
                {
                    if (pstage.prefabContentsRoot == instance)
                    {
                        // ???????????
                        prefabAssetPath = pstageAssetPath;
                        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);

                        // ????????????
                        var baseAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefabAsset);
                        baseAssetPath = AssetDatabase.GetAssetPath(baseAsset);
                    }
                    else
                    {
                        // ???????????
                        var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(instance);
                        prefabAssetPath = AssetDatabase.GetAssetPath(prefabAsset);

                        // ????????????
                        var baseAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefabAsset);
                        baseAssetPath = AssetDatabase.GetAssetPath(baseAsset);
                    }
                }
                else
                {
                    // ???????????
                    var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(instance);
                    prefabAssetPath = AssetDatabase.GetAssetPath(prefabAsset);

                    // ????????????
                    if (prefabAsset)
                    {
                        var baseAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefabAsset);
                        baseAssetPath = AssetDatabase.GetAssetPath(baseAsset);
                    }
                }
            }
            //Debug.Log($"prefabPath:{prefabAssetPath}");
            //Debug.Log($"basePath:{baseAssetPath}");

            // ??
            string saveAssetPath = prefabAssetPath;
            if (pstage == null && isVariant)
            {
                //Debug.Log($"instance1[{instance.name}]????->[{prefabAssetPath}]?????.");
            }
            else if (string.IsNullOrEmpty(prefabAssetPath))
            {
                //Debug.Log($"Skip1");
                return;
            }
            else if (mode == Mode.Saving)
            {
                //Debug.Log($"instance2[{instance.name}]????->[{prefabAssetPath}]?????.");
            }
            else
            {
                if (isVariant)
                {
                    //Debug.Log("Skip2");
                    return;
                }
                else if (string.IsNullOrEmpty(baseAssetPath))
                {
                    //Debug.Log($"instance4[{instance.name}]????->[{prefabAssetPath}]?????.");
                }
                else
                {
                    //Debug.Log($"instance5[{instance.name}]????->[{baseAssetPath}]?????.");
                    saveAssetPath = baseAssetPath;
                }
            }

            // ??????
            bool forceCopy = false;
            if (isVariant && pstage != null && instance != pstage.prefabContentsRoot)
                forceCopy = true;
            if (pstage != null && prefabAssetPath == saveAssetPath && saveAssetPath != pstageAssetPath && onStage)
                forceCopy = true;

            // ????
            SavePrefab(instance, prefabAssetPath, saveAssetPath, isVariant, forceCopy, mode);
        }

        static void SavePrefab(GameObject instance, string prefabPath, string savePrefabPath, bool isVariant, bool forceCopy, Mode mode)
        {
            //Debug.Log($"SavePrefab instance:{instance.name} forceCopy:{forceCopy} isVariant:{isVariant}\npath:{prefabPath}\nsavePath:{savePrefabPath} ");

            // ???????????
            var savePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(savePrefabPath);

            // ???????????????????????????
            if (PrefabUtility.IsPartOfImmutablePrefab(savePrefab))
            {
                //Debug.Log("Skip3");
                return;
            }

            // ???????????????????
            bool change = false;
            List<ShareDataObject> removeDatas = new List<ShareDataObject>();

            // ????????????????????ShareDataObject????????????????????
            List<Object> subassets = new List<Object>(AssetDatabase.LoadAllAssetRepresentationsAtPath(savePrefabPath));
            if (subassets != null)
            {
                foreach (var obj in subassets)
                {
                    // ShareDataObject??
                    ShareDataObject sdata = obj as ShareDataObject;
                    if (sdata && removeDatas.Contains(sdata) == false)
                    {
                        //Debug.Log("remove reserve sub asset:" + obj.name + " type:" + obj + " test:" + AssetDatabase.IsSubAsset(sdata));
                        // ???????????
                        removeDatas.Add(sdata);
                    }
                }
            }

            // ????????????
            var coreList = instance.GetComponentsInChildren<CoreComponent>(true);
            if (coreList != null)
            {
                foreach (var core in coreList)
                {
                    // ???????
                    var shareDataInterfaces = core.GetComponentsInChildren<IShareDataObject>(true);
                    if (shareDataInterfaces != null)
                    {
                        foreach (var sdataInterface in shareDataInterfaces)
                        {
                            List<ShareDataObject> shareDatas = sdataInterface.GetAllShareDataObject();
                            if (shareDatas != null)
                            {
                                foreach (var sdata in shareDatas)
                                {
                                    if (sdata)
                                    {
                                        //Debug.Log($"target shareData:{sdata.name}");

                                        if (removeDatas.Contains(sdata))
                                        {
                                            //Debug.Log($"Ignore:{sdata.name}");
                                            removeDatas.Remove(sdata);
                                        }
                                        else if (AssetDatabase.Contains(sdata))
                                        {
                                            // ??????????????
                                            var sdataPrefabPath = AssetDatabase.GetAssetPath(sdata);
                                            //Debug.Log($"sdataPrefabPath:{sdataPrefabPath}");

                                            if (forceCopy || prefabPath != savePrefabPath)
                                            {
                                                var newdata = sdataInterface.DuplicateShareDataObject(sdata);
                                                if (newdata != null)
                                                {
                                                    //Debug.Log($"+Duplicate sub asset:{newdata.name} -> [{savePrefab.name}]");
                                                    AssetDatabase.AddObjectToAsset(newdata, savePrefab);
                                                    change = true;
                                                }
                                            }
                                            else
                                            {
                                                removeDatas.Remove(sdata);
                                            }
                                        }
                                        else
                                        {
                                            //Debug.Log($"+Add sub asset:{sdata.name} -> [{savePrefab.name}]");
                                            AssetDatabase.AddObjectToAsset(sdata, savePrefab);
                                            change = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // ?????????????
            foreach (var sdata in removeDatas)
            {
                //Debug.Log($"-Remove sub asset:{sdata.name} path:{AssetDatabase.GetAssetPath(sdata)}");
                UnityEngine.Object.DestroyImmediate(sdata, true);
                change = true;
            }

            // ???????
            if (change)
            {
                //Debug.Log("save!");

                // ??????????????????????????????????
                if (mode == Mode.Saving)
                {
                    PrefabUtility.SaveAsPrefabAsset(instance, savePrefabPath);
                }
                else
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(instance, savePrefabPath, InteractionMode.AutomatedAction);
                }
            }
        }

        //=========================================================================================
        public static bool CleanUpSubAssets(GameObject savePrefab, bool log = true)
        {
            // ???????????????????????????
            if (PrefabUtility.IsPartOfImmutablePrefab(savePrefab))
            {
                return false;
            }

            string savePrefabPath = AssetDatabase.GetAssetPath(savePrefab);
            //Debug.Log($"PrefabPath:{savePrefabPath}");
            if (string.IsNullOrEmpty(savePrefabPath))
                return false;

            // ???????????????????
            List<ShareDataObject> removeDatas = new List<ShareDataObject>();

            // ????????????????????ShareDataObject????????????????????
            List<Object> subassets = new List<Object>(AssetDatabase.LoadAllAssetRepresentationsAtPath(savePrefabPath));
            if (subassets != null)
            {
                foreach (var obj in subassets)
                {
                    // ShareDataObject??
                    ShareDataObject sdata = obj as ShareDataObject;
                    if (sdata && removeDatas.Contains(sdata) == false)
                    {
                        //Debug.Log("remove reserve sub asset:" + obj.name + " type:" + obj + " test:" + AssetDatabase.IsSubAsset(sdata));
                        // ???????????
                        removeDatas.Add(sdata);
                    }
                }
            }

            // ????????????
            var coreList = savePrefab.GetComponentsInChildren<CoreComponent>(true);
            if (coreList != null)
            {
                foreach (var core in coreList)
                {
                    // ???????
                    var shareDataInterfaces = core.GetComponentsInChildren<IShareDataObject>(true);
                    if (shareDataInterfaces != null)
                    {
                        foreach (var sdataInterface in shareDataInterfaces)
                        {
                            List<ShareDataObject> shareDatas = sdataInterface.GetAllShareDataObject();
                            if (shareDatas != null)
                            {
                                foreach (var sdata in shareDatas)
                                {
                                    if (sdata)
                                    {
                                        //Debug.Log($"target shareData:{sdata.name}");
                                        if (removeDatas.Contains(sdata))
                                        {
                                            //Debug.Log($"Ignore:{sdata.name}");
                                            removeDatas.Remove(sdata);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // ?????????????
            if (removeDatas.Count > 0)
            {
                foreach (var sdata in removeDatas)
                {
                    //Debug.Log($"-Remove sub asset:{sdata.name} path:{AssetDatabase.GetAssetPath(sdata)}");
                    if (log)
                        Debug.Log($"Remove sub-asset : {sdata.name}");
                    UnityEngine.Object.DestroyImmediate(sdata, true);
                }
                AssetDatabase.SaveAssets();
            }
            if (log)
                Debug.Log($"Remove Count : {removeDatas.Count}");

            return true;
        }
    }
}
