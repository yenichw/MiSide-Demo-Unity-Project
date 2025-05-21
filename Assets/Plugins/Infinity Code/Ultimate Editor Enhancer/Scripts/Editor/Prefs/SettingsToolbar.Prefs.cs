/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.HierarchyTools;
using InfinityCode.UltimateEditorEnhancer.JSON;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public partial class Prefs
    {
        private static void CreateIgnore(string filename, bool entireAsset)
        {
            string path = new DirectoryInfo(Resources.assetFolder).Parent.FullName + "/." + filename;
            string content = "";
            if (entireAsset)
            {
                content = @"
/Ultimate Editor Enhancer/*
!/Ultimate Editor Enhancer/Scripts
/Ultimate Editor Enhancer/Scripts/Editor/
";
            }

            content += "/Ultimate Editor Enhancer Settings/";

            File.WriteAllText(path, content, Encoding.UTF8);
        }
        
        private static void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayoutUtils.ToolbarButton("File")) ShowFileMenu();
            if (GUILayoutUtils.ToolbarButton("Bulk Operations")) ShowBulkMenu();
            if (GUILayoutUtils.ToolbarButton("Version Control")) ShowVersionControlMenu();
            GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
            if (GUILayoutUtils.ToolbarButton("Help")) ShowHelpMenu();

            EditorGUILayout.EndHorizontal();
        }
        
        private static void DisableEverything()
        {
            SetGeneralManagersState(false);
            
            foreach (PrefManager m in managers)
            {
                IStateablePref p = m as IStateablePref;
                if (p != null) p.SetState(false);
            }

            Save();
        }
        
        private static void EnableEverything()
        {
            SetGeneralManagersState(true);
            
            foreach (PrefManager m in managers)
            {
                IStateablePref p = m as IStateablePref;
                if (p != null) p.SetState(true);
            }

            Save();
        }
        
        private static void ExportItems(object data)
        {
            ExportItemIndex target = (ExportItemIndex)(int)data;
            string name = "UEE-Items-";
            if (target == ExportItemIndex.everything) name += "Everything";
            else if (target == ExportItemIndex.bookmarks) name += "Bookmarks";
            else if (target == ExportItemIndex.favoriteWindows) name += "Favorite-Windows";
            else if (target == ExportItemIndex.miniLayouts) name += "Mini-Layouts";
            else if (target == ExportItemIndex.quickAccessBar) name += "Quick-Access-Bar";
            else if (target == ExportItemIndex.hierarchyHeaders) name += "Hierarchy-Headers";
            else if (target == ExportItemIndex.emptyInspector) name += "Empty-Inspector";
            else if (target == ExportItemIndex.projectIcons) name += "Project-Icons";

            string filename = EditorUtility.SaveFilePanel("Export Items", EditorApplication.applicationPath, name, "json");
            if (string.IsNullOrEmpty(filename)) return;

            JsonObject obj = GetExportJson(target);

            File.WriteAllText(filename, obj.ToString(), Encoding.UTF8);
        }
        
        private static void ExportSettings()
        {
            string filename = EditorUtility.SaveFilePanel("Export Settings", EditorApplication.applicationPath, "UEE-Settings", "ucs");
            if (string.IsNullOrEmpty(filename)) return;

            File.WriteAllText(filename, GetSettings(), Encoding.UTF8);
        }
        
        private static JsonObject GetExportJson(ExportItemIndex target)
        {
            JsonObject obj = new JsonObject();

            if (target == ExportItemIndex.everything || target == ExportItemIndex.bookmarks) obj.Add("bookmarks", Bookmarks.json);
            if (target == ExportItemIndex.everything || target == ExportItemIndex.favoriteWindows) obj.Add("favorite-windows", FavoriteWindowsManager.json);
            if (target == ExportItemIndex.everything || target == ExportItemIndex.miniLayouts) obj.Add("mini-layouts", MiniLayoutsManager.json);
            if (target == ExportItemIndex.everything || target == ExportItemIndex.quickAccessBar) obj.Add("quick-access-bar", QuickAccessBarManager.json);
            if (target == ExportItemIndex.everything || target == ExportItemIndex.hierarchyHeaders) obj.Add("hierarchy-headers", Header.json);
            if (target == ExportItemIndex.everything || target == ExportItemIndex.emptyInspector) obj.Add("empty-inspector", EmptyInspectorManager.json);
            if (target == ExportItemIndex.everything || target == ExportItemIndex.projectIcons) obj.Add("project-icons", ProjectFolderIconManager.json);

            return obj;
        }
        
        private static void ImportItems()
        {
            string filename = EditorUtility.OpenFilePanel("Import Items", EditorApplication.applicationPath, "json");
            if (string.IsNullOrEmpty(filename)) return;

            string text = File.ReadAllText(filename, Encoding.UTF8);
            JsonItem json = Json.Parse(text);
            JsonItem bookmarksItem = json["bookmarks"];

            migrationReplace = true;

            if (bookmarksItem != null) Bookmarks.json = bookmarksItem as JsonArray;

            JsonItem fwItem = json["favorite-windows"];
            if (fwItem != null) FavoriteWindowsManager.json = fwItem as JsonArray;

            JsonItem qabItem = json["quick-access-bar"];
            if (qabItem != null) QuickAccessBarManager.json = qabItem as JsonArray;

            JsonItem hrItem = json["hierarchy-headers"];
            if (hrItem != null) Header.json = hrItem as JsonArray;

            JsonItem mlItem = json["mini-layouts"];
            if (mlItem != null) MiniLayoutsManager.json = mlItem as JsonArray;

            JsonItem eiItem = json["empty-inspector"];
            if (eiItem != null) EmptyInspectorManager.json = eiItem as JsonArray;

            JsonItem piItem = json["project-icons"];
            if (piItem != null) ProjectFolderIconManager.json = piItem as JsonArray;

            migrationReplace = false;

            ReferenceManager.Save();
        }

        private static void ImportSettings()
        {
            string filename = EditorUtility.OpenFilePanel("Import Settings", EditorApplication.applicationPath, "ucs");
            if (string.IsNullOrEmpty(filename)) return;

            string prefs = File.ReadAllText(filename, Encoding.UTF8);
            LoadSettings(prefs);
            Save();
        }

        private static void MoveSettingsToFile()
        {
            string prefStr = EditorPrefs.GetString(PrefsKey, String.Empty);
            File.WriteAllText(SettingsFilename, prefStr, Encoding.UTF8);
            
            string message = $"Settings have been moved to {SettingsFilename} file.\nTo return to the settings in EditorPrefs delete this file.";
            EditorUtility.DisplayDialog("Settings Moved", message, "OK");
        }

        private static void RestoreDefaultSettings()
        {
            if (!EditorUtility.DisplayDialog(
                    "Restore default settings",
                    "Are you sure you want to restore the default settings?",
                    "Restore", "Cancel"))
            {
                return;
            }
            
            if (EditorPrefs.HasKey(PrefsKey)) EditorPrefs.DeleteKey(PrefsKey);

            ReferenceManager.ResetContent();
            LocalSettings.ResetContent();

            AssetDatabase.ImportAsset(Resources.assetFolder + "Scripts/Editor/Prefs/Methods.Prefs.cs", ImportAssetOptions.ForceUpdate);
        }

        private static void ShowBulkMenu()
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add("Restore Default Settings", RestoreDefaultSettings);
            menu.AddSeparator();
            menu.Add("Disable/Everything", DisableEverything);
            menu.AddSeparator("Disable/");

            menu.Add("Enable/Everything", EnableEverything);
            menu.AddSeparator("Enable/");

            Dictionary<string, Action<bool>> actions = new Dictionary<string, Action<bool>>();

            foreach (PrefManager m in managers)
            {
                IStateablePref p = m as IStateablePref;
                if (p == null) continue;

                actions.Add(p.GetMenuName(), p.SetState);
            }

            foreach (KeyValuePair<string, Action<bool>> pair in actions.OrderBy(d => d.Key))
            {
                menu.Add("Enable/" + pair.Key, () => pair.Value(true));
                menu.Add("Disable/" + pair.Key, () => pair.Value(false));
            }

            menu.Show();
        }

        private static void ShowFileMenu()
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add("Export/Settings", ExportSettings);
            menu.Add("Export/Items/Everything", ExportItems, (int)ExportItemIndex.everything);
            menu.AddSeparator("Export/Items/");
            menu.Add("Export/Items/Bookmarks", ExportItems, (int)ExportItemIndex.bookmarks);
            menu.Add("Export/Items/Empty Inspector", ExportItems, (int)ExportItemIndex.emptyInspector);
            menu.Add("Export/Items/Favorite Windows", ExportItems, (int)ExportItemIndex.favoriteWindows);
            menu.Add("Export/Items/Hierarchy Headers", ExportItems, (int)ExportItemIndex.hierarchyHeaders);
            menu.Add("Export/Items/Quick Access Bar", ExportItems, (int)ExportItemIndex.quickAccessBar);
            menu.Add("Export/Items/Project Icons", ExportItems, (int)ExportItemIndex.projectIcons);

            menu.Add("Import/Settings", ImportSettings);
            menu.Add("Import/Items", ImportItems);
            
            menu.AddSeparator();
            if (!File.Exists(SettingsFilename)) menu.Add("Move Settings To File", MoveSettingsToFile);

            menu.Show();
        }

        private static void ShowHelpMenu()
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add("Welcome", Welcome.OpenWindow);
            menu.Add("Getting Started", GettingStarted.OpenWindow);
            menu.Add("Shortcuts", Shortcuts.OpenWindow);
            menu.AddSeparator();
            menu.Add("Product Page", Links.OpenHomepage);
            menu.Add("Documentation", Links.OpenDocumentation);
            menu.Add("Videos", Links.OpenYouTube);
            menu.AddSeparator();
            menu.Add("Support", Links.OpenSupport);
            menu.Add("Forum", Links.OpenForum);
            menu.Add("Check Updates", Updater.OpenWindow);
            menu.AddSeparator();
            menu.Add("Rate and Review", Welcome.RateAndReview);
            menu.Add("About", About.OpenWindow);

            menu.Show();
        }

        private static void ShowVersionControlMenu()
        {
            GenericMenuEx menu = GenericMenuEx.Start();
            menu.Add(".gitignore/Exclude Settings", () => { CreateIgnore("gitignore", false); });
            menu.Add(".gitignore/Exclude Entire Asset", () => { CreateIgnore("gitignore", true); });
            menu.Add(".collabignore/Exclude Settings", () => { CreateIgnore("collabignore", false); });
            menu.Add(".collabignore/Exclude Entire Asset", () => { CreateIgnore("collabignore", true); });
            menu.Show();
        }
    }
}