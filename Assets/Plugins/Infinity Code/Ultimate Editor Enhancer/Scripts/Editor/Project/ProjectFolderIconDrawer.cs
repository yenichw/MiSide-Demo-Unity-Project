/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class ProjectFolderIconDrawer
    {
        private const int CheckTwoColumnsTime = 3;
        private const int CheckContentTime = 10;
        
        private static TreeViewState treeViewState;
        private static Dictionary<string, FolderItem> ruleCache = new Dictionary<string, FolderItem>();
        private static double lastCheckTwoColumns;
        private static Object projectWindow;

        static ProjectFolderIconDrawer()
        {
            ProjectItemDrawer.Register("ICON_DRAWER", Draw, ProjectToolOrder.FolderIconDrawer);
        }

        private static void Draw(ProjectItem item)
        {
            if (!Prefs.projectFolderIcons) return;
            if (!item.isFolder) return;
            if (Event.current.type != EventType.Repaint) return;
            if (!item.path.StartsWith("Assets")) return;

            FolderItem folderItem;
            if (!ruleCache.TryGetValue(item.guid, out folderItem))
            {
                folderItem = new FolderItem(item.path);
                folderItem.rule = GetRule(item.path);
                ruleCache.Add(item.guid, folderItem);
            }

            if (folderItem.rule == null) return;
            
            bool expanded = false;
            
            if (!folderItem.isEmpty)
            {
                UpdateTreeViewState();

                if (treeViewState != null)
                {
                    expanded = treeViewState.expandedIDs.Contains(folderItem.instanceID);
                }
            }

            folderItem.rule.Draw(item.rect, expanded, folderItem.isEmpty);
        }

        private static ProjectFolderRule GetRule(string path)
        {
            string folderNameUpper = path.ToUpperInvariant();
            string[] folderParts = folderNameUpper.Split(new []{ '/' }, StringSplitOptions.RemoveEmptyEntries);
            string folder = folderParts[folderParts.Length - 1];

            for (int i = 0; i < ReferenceManager.projectFolderIcons.Count; i++)
            {
                ProjectFolderRule rule = ReferenceManager.projectFolderIcons[i];
                if (rule.rootOnly && folderParts.Length > 2) continue;
                
                string[] parts = rule.parts;
                for (int j = 0; j < parts.Length; j++)
                {
                    if (parts[j] == folder) return rule;
                }
            }

            if (folderParts.Length < 3) return null;

            for (int i = folderParts.Length - 2; i >= 1; i--)
            {
                folder = folderParts[i];
                
                for (int j = 0; j < ReferenceManager.projectFolderIcons.Count; j++)
                {
                    ProjectFolderRule rule = ReferenceManager.projectFolderIcons[j];
                    if (!rule.recursive) continue;
                    
                    string[] parts = rule.parts;
                    for (int k = 0; k < parts.Length; k++)
                    {
                        if (parts[k] == folder) return rule;
                    }
                }
            }

            return null;
        }

        public static void SetDirty()
        {
            foreach (KeyValuePair<string,FolderItem> pair in ruleCache)
            {
                pair.Value.SetDirty();
            }
            
            ruleCache.Clear();

            if (ReferenceManager.projectFolderIcons != null)
            {
                foreach (ProjectFolderRule rule in ReferenceManager.projectFolderIcons)
                {
                    rule.SetDirty();
                }
            }
        }

        private static void UpdateTreeViewState()
        {
            if (treeViewState != null && EditorApplication.timeSinceStartup - lastCheckTwoColumns < CheckTwoColumnsTime) return;
            
            if (projectWindow == null)
            {
                Object[] projects = UnityEngine.Resources.FindObjectsOfTypeAll(ProjectBrowserRef.type);
                if (projects.Length > 0) projectWindow = projects[0];
            }

            if (projectWindow != null)
            {
                bool isTwoColumns = ProjectBrowserRef.IsTwoColumns(projectWindow);

                if (isTwoColumns) treeViewState = ProjectBrowserRef.GetFolderTreeViewState(projectWindow);
                else treeViewState = ProjectBrowserRef.GetAssetTreeViewState(projectWindow);
            }

            lastCheckTwoColumns = EditorApplication.timeSinceStartup;
        }

        internal class FolderItem
        {
            public string path;
            public int instanceID;

            private bool _isEmpty;
            private ProjectFolderRule _rule;
            private double lastCheckContentTime;

            public bool hasRule { get; private set; }
            
            public bool isEmpty
            {
                get
                {
                    if (EditorApplication.timeSinceStartup - lastCheckContentTime > CheckContentTime)
                    {
                        if (!Directory.Exists(path)) path = AssetDatabase.GetAssetPath(instanceID);
                        
                        _isEmpty = Directory.GetDirectories(path).Length == 0 && Directory.GetFiles(path).Length == 0;
                        lastCheckContentTime = EditorApplication.timeSinceStartup;
                    }
                    
                    return _isEmpty;
                }
            }

            public ProjectFolderRule rule
            {
                get
                {
                    return hasRule ? _rule : null;
                }
                set
                {
                    _rule = value;
                    hasRule = true;
                }
            }
            
            public FolderItem(string path)
            {
                this.path = path;

                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                instanceID = obj.GetInstanceID();
            }

            public void SetDirty()
            {
                _rule = null;
                hasRule = false;
            }
        }
    }
}