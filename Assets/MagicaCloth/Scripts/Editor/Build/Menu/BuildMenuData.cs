// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

using UnityEditor;
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ?????
    /// </summary>
    [System.Serializable]
    public class BuildMenuData
    {
        const string SettingDataSaveName = "MagicaClothBuildSettings";

        //=========================================================================================
        // Target Object
        public int TargetMode = 1; // 0 = all, 1 = selected
        public bool Prefab = true;
        public bool Scene = true;

        //=========================================================================================
        // Target Components
        public bool BoneCloth = true;
        public bool BoneSpring = true;
        public bool MeshCloth = true;
        public bool MeshSpring = true;
        public bool RenderDeformer = true;
        public bool VirtualDeformer = true;

        //=========================================================================================
        // Build Conditions
        public bool ForceBuild = false;
        public bool NotCreated = true;
        public bool UpgradeFormatAndAlgorithm = true;
        public bool VerificationOnly = false;

        //=========================================================================================
        // Options
        public bool ErrorStop = false;

        //=========================================================================================
        /// <summary>
        /// ?????????
        /// </summary>
        public void Load()
        {
            if (EditorPrefs.HasKey(SettingDataSaveName))
            {
                var data = EditorPrefs.GetString(SettingDataSaveName, JsonUtility.ToJson(this, false));
                JsonUtility.FromJsonOverwrite(data, this);
            }
        }

        /// <summary>
        /// ????????
        /// ??????PC????????(Windows?????????)
        /// </summary>
        public void Save()
        {
            var data = JsonUtility.ToJson(this, false);
            EditorPrefs.SetString(SettingDataSaveName, data);
        }
    }
}
