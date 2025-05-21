/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [InitializeOnLoad]
    public class SceneBackups : EditorWindow
    {
        private const int CheckInterval = 10;
        private const string BackupFolder = "SceneBackups/";
        
        private static SceneBackups instance;
        private static SceneItem[] items;
        private static double lastCheckTime;
        private static GUIContent openContent;
        private static GUIContent selectContent;

        private string filter;
        private Vector2 scrollPosition;
        private SceneItem[] filteredItems;

        static SceneBackups()
        {
            EditorApplication.update += CheckSceneChanges;
        }

        private static void Backup(string assetPath, string guid)
        {
            string folder = BackupFolder + guid;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string ticks = File.GetLastWriteTime(assetPath).Ticks.ToString();
            string path = folder + "/" + ticks + ".backup";
            try
            {
                File.Copy(assetPath, path);
            }
            catch
            {
            }

            if (instance != null) instance.Load();
        }

        private static void CheckSceneChanges()
        {
            if (!Prefs.backupScene) return;
            if (EditorApplication.timeSinceStartup - lastCheckTime < CheckInterval) return;
            
            lastCheckTime = EditorApplication.timeSinceStartup;
            
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.name == null || string.IsNullOrEmpty(scene.path)) return;
            
            string guid = AssetDatabase.AssetPathToGUID(scene.path);
            if (!Directory.Exists(BackupFolder))
            {
                Directory.CreateDirectory(BackupFolder);
                Backup(scene.path, guid);
                return;
            }
            
            string folder = BackupFolder + guid;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                File.WriteAllText(folder + "/!path.txt", scene.path, Encoding.UTF8);
                
                Backup(scene.path, guid);
                return;
            }
            
            string[] files = Directory.GetFiles(folder);
            DateTime lastWriteTime = File.GetLastWriteTime(scene.path);
            
            long maxTicks = 0;
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                if (info.Extension != ".backup") continue;
                
                string ticks = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                long t = long.Parse(ticks); 
                if (t > maxTicks) maxTicks = t;
            }
            
            if (maxTicks == 0)
            {
                Backup(scene.path, guid);
                return;
            }
            
            DateTime lastBackupTime = new DateTime(maxTicks);
            if (lastWriteTime - lastBackupTime < TimeSpan.FromHours(Prefs.backupSceneIntervalHours)) return;
            
            Backup(scene.path, guid);
        }

        private void Load()
        {
            if (!Directory.Exists(BackupFolder)) return;
            
            string[] folders = Directory.GetDirectories(BackupFolder);
            if (folders.Length == 0) return;
            
            List<SceneItem> temp = new List<SceneItem>();

            foreach (string folder in folders)
            {
                DirectoryInfo info = new DirectoryInfo(folder);
                string assetPath = AssetDatabase.GUIDToAssetPath(info.Name);
                if (string.IsNullOrEmpty(assetPath))
                {
                    string path = folder + "/!path.txt";
                    if (!File.Exists(path)) continue;
                    assetPath = File.ReadAllText(path, Encoding.UTF8);
                }

                SceneItem item = new SceneItem(assetPath, folder);
                temp.Add(item);
            }
            
            items = temp.OrderBy(i => i.name).ToArray();
            UpdateFilteredItems();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void OnEnable()
        {
            instance = this;
            
            openContent = new GUIContent(Styles.isProSkin ? Icons.openNewWhite : Icons.openNewBlack, "Open Scene");
            selectContent = new GUIContent(EditorIconContents.rectTransformBlueprint);
            selectContent.tooltip = "Select Scene";
            
            Load();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            filter = EditorGUILayout.TextField(filter);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateFilteredItems();
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            SceneItem[] currentItems = filteredItems ?? items;
            foreach (SceneItem item in currentItems) item.Draw();
            EditorGUILayout.EndScrollView();
        }

        [MenuItem(WindowsHelper.MenuPath + "Scene Backups", false, MenuItemOrder.SceneBackup)]
        public static void OpenWindow()
        {
            GetWindow<SceneBackups>( "Scene Backups", true);
        }

        private void UpdateFilteredItems()
        {
            if (string.IsNullOrEmpty(filter))
            {
                filteredItems = null;
                return;
            }

            string pattern = SearchableItem.GetPattern(filter);

            filteredItems = items.Where(i => i.Match(pattern)).ToArray();
        }

        internal class SceneItem : SearchableItem
        {
            public string assetPath;
            public string backupPath;
            public bool exists;
            public bool isExpanded;
            public string label;
            public string name;
            public Record[] Records;

            public SceneItem(string assetPath, string backupPath)
            {
                this.assetPath = assetPath;
                this.backupPath = backupPath;
                
                exists = File.Exists(assetPath);
                
                FileInfo info = new FileInfo(assetPath);
                name = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                
                if (exists) label = name;
                else label = name + " (missing)";
                
                string[] files = Directory.GetFiles(backupPath);
                if (files.Length == 0) return;
                
                List<Record> temp = new List<Record>();
                foreach (string file in files)
                {
                    info = new FileInfo(file);
                    
                    if (info.Extension != ".backup") continue;

                    Record record = new Record(this, info); 
                    temp.Add(record);
                }
                
                temp.Sort((a, b) => b.time.CompareTo(a.time));
                Records = temp.ToArray();
            }

            public void Draw()
            {
                GUIContent content = TempContent.Get(EditorIconContents.unityLogo);
                content.text = label;
                content.tooltip = assetPath;

                EditorGUILayout.BeginHorizontal();
                isExpanded = EditorGUILayout.Foldout(isExpanded, content);

                if (exists)
                {
                    if (GUILayout.Button(selectContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                    }

                    if (GUILayout.Button(openContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                    {
                        EditorSceneManager.OpenScene(assetPath);
                    }
                }
                
                EditorGUILayout.EndHorizontal();

                if (!isExpanded) return;

                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 40;
                EditorGUILayout.LabelField("Path", assetPath, EditorStyles.textField);
                EditorGUIUtility.labelWidth = labelWidth;

                foreach (Record record in Records)
                {
                    record.Draw();
                }
            }

            protected override int GetSearchCount()
            {
                return 1;
            }

            protected override string GetSearchString(int index)
            {
                return name;
            }
        }
        
        internal class Record
        {
            public SceneItem sceneItem;
            public DateTime time;
            public string label;
            public string path;

            public Record(SceneItem sceneItem, FileInfo info)
            {
                this.sceneItem = sceneItem;
                
                path = info.FullName;
                string ticks = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                time = new DateTime(long.Parse(ticks));
                label = time.ToString("yyyy-MM-dd HH:mm:ss");
            }

            public void Draw()
            {
                if (GUILayout.Button(label))
                {
                    // restore to current scene or show save dialog
                    int result = EditorUtility.DisplayDialogComplex("Restore scene", "Restore scene to " + label + "?", "Restore", "Cancel", "Save As");
                    if (result == 1) return;

                    if (result == 0)
                    {
                        File.Copy(path, sceneItem.assetPath, true);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        string destFileName = EditorUtility.SaveFilePanel("Save scene", "Assets/", sceneItem.name, "unity");
                        if (string.IsNullOrEmpty(destFileName)) return;

                        File.Copy(path, destFileName, true);
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
}