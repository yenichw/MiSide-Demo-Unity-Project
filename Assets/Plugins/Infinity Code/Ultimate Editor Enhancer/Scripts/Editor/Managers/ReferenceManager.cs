/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using InfinityCode.UltimateEditorEnhancer.HierarchyTools;
using InfinityCode.UltimateEditorEnhancer.InspectorTools;
using InfinityCode.UltimateEditorEnhancer.PostHeader;
using InfinityCode.UltimateEditorEnhancer.ProjectTools;
using InfinityCode.UltimateEditorEnhancer.SceneTools;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    [Serializable]
    [PreferBinarySerialization]
    public class ReferenceManager : ScriptableObject
    {
        [SerializeField]
        private List<BackgroundRule> _backgroundRules = new List<BackgroundRule>();
        
        [SerializeField]
        private List<ProjectBookmark> _bookmarks = new List<ProjectBookmark>();

        [SerializeField]
        private List<EmptyInspector.Group> _emptyInspectorItems = new List<EmptyInspector.Group>();

        [SerializeField]
        private List<FavoriteWindowItem> _favoriteWindows = new List<FavoriteWindowItem>();

        [SerializeField]
        private List<HeaderRule> _headerRules = new List<HeaderRule>();

        [SerializeField]
        private List<QuickAccessItem> _quickAccessItems = new List<QuickAccessItem>();

        [SerializeField]
        private List<ProjectFolderRule> _projectFolderIcons = new List<ProjectFolderRule>();

        [SerializeField]
        private List<SceneHistoryItem> _sceneHistory = new List<SceneHistoryItem>();

        [SerializeField]
        private List<NoteItem> _notes = new List<NoteItem>();
        
        [SerializeField]
        private List<MiniLayout> _miniLayouts = new List<MiniLayout>();

        private static ReferenceManager _instance;

        public static ReferenceManager instance
        {
            get
            {
                if (_instance == null) Load();
                return _instance;
            }
        }
        
        public static List<BackgroundRule> backgroundRules
        {
            get { return instance._backgroundRules; }
        }

        public static List<ProjectBookmark> bookmarks
        {
            get { return instance._bookmarks; }
        }

        public static List<EmptyInspector.Group> emptyInspectorItems
        {
            get { return instance._emptyInspectorItems; }
        }

        public static List<FavoriteWindowItem> favoriteWindows
        {
            get { return instance._favoriteWindows; }
        }

        public static List<HeaderRule> headerRules
        {
            get { return instance._headerRules; }
        }
        
        public static List<MiniLayout> miniLayouts
        {
            get { return instance._miniLayouts; }
        }

        public static List<NoteItem> notes
        {
            get { return instance._notes; }
        }

        public static List<QuickAccessItem> quickAccessItems
        {
            get { return instance._quickAccessItems; }
        }

        public static List<ProjectFolderRule> projectFolderIcons
        {
            get { return instance._projectFolderIcons; }
        }

        public static List<SceneHistoryItem> sceneHistory
        {
            get { return instance._sceneHistory; }
            set { instance._sceneHistory = value; }
        }

        private static void Load()
        {
            string path = Resources.settingsFolder + "References.asset";
            try
            {
                if (File.Exists(path))
                {
                    try
                    {
                        _instance = AssetDatabase.LoadAssetAtPath<ReferenceManager>(path);
                    }
                    catch (Exception e)
                    {
                        Log.Add(e);
                    }

                }

                if (_instance == null)
                {
                    _instance = CreateInstance<ReferenceManager>();

#if !UEE_IGNORE_SETTINGS
                    FileInfo info = new FileInfo(path);
                    if (!info.Directory.Exists) info.Directory.Create();

                    AssetDatabase.CreateAsset(_instance, path);
                    AssetDatabase.SaveAssets();
#endif
                }
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
        }

        public static void ResetContent()
        {
            backgroundRules.Clear();
            bookmarks.Clear();
            emptyInspectorItems.Clear();
            favoriteWindows.Clear();
            headerRules.Clear();
            miniLayouts.Clear();
            projectFolderIcons.Clear();
            quickAccessItems.Clear();
            RecordUpgrader.InitDefaultEmptyInspectorItems();
            RecordUpgrader.InitDefaultQuickAccessItems();
            Save();
        }

        public static void Save()
        {
            try
            {
                EditorUtility.SetDirty(_instance); 
            }
            catch
            {

            }

        }
    }
}