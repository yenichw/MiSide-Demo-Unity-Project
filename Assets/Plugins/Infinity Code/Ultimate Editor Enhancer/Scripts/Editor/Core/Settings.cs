/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class Settings
    {
        // First level
        private const string UEESettingsPath = "Project/Ultimate Editor Enhancer";
        private const string ContextMenuSettingsPath = UEESettingsPath + "/Context Menu";
        private const string CreateBrowserSettingsPath = UEESettingsPath + "/Create Browser";
        private const string GameObjectSettingsPath = UEESettingsPath + "/GameObject";
        private const string HierarchySettingsPath = UEESettingsPath + "/Hierarchy";
        private const string InspectorSettingsPath = UEESettingsPath + "/Inspector";
        private const string ProjectSettingsPath = UEESettingsPath + "/Project";
        private const string SceneViewSettingsPath = UEESettingsPath + "/Scene View";
        private const string SearchWindowsSettingsPath = UEESettingsPath + "/Search";
        private const string ToolbarSettingsPath = UEESettingsPath + "/Toolbar";
        private const string UnsafeSettingsPath = UEESettingsPath + "/Unsafe";
        private const string ViewsSettingsPath = UEESettingsPath + "/Views";
        private const string WindowsSettingsPath = UEESettingsPath + "/Windows";

        // Second level
        private const string EmptyInspectorSettingsPath = InspectorSettingsPath + "/Empty Inspector";
        private const string BackgroundsSettingsPath = HierarchySettingsPath + "/Backgrounds";
        private const string FavoriteWindowsSettingsPath = WindowsSettingsPath + "/Favorite Windows";
        private const string HeadersSettingsPath = HierarchySettingsPath + "/Headers";
        private const string HighlightSettingsPath = SceneViewSettingsPath + "/Highlight";
        private const string MiniLayoutsSettingsPath = WindowsSettingsPath + "/Mini Layouts";
        private const string NavigationSettingsPath = SceneViewSettingsPath + "/Navigation";
        private const string QuickAccessSettingsPath = SceneViewSettingsPath + "/Quick Access Bar";
        private const string ProjectFolderIconsSettingsPath = ProjectSettingsPath + "/Folder Icons";
        private const string WailaSettingsPath = SceneViewSettingsPath + "/WAILA";

        [SettingsProvider]
        public static SettingsProvider GetBackgroundsSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(BackgroundsSettingsPath, SettingsScope.Project)
            {
                label = "Backgrounds",
                guiHandler = Prefs.BackgroundManager.DrawWithToolbar,
                keywords = Prefs.BackgroundManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetContextMenuSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(ContextMenuSettingsPath, SettingsScope.Project)
            {
                label = "Context Menu",
                guiHandler = Prefs.ContextMenuManager.DrawWithToolbar,
                keywords = Prefs.ContextMenuManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetCreateBrowserSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(CreateBrowserSettingsPath, SettingsScope.Project)
            {
                label = "Create Browser",
                guiHandler = Prefs.CreateBrowserManager.DrawWithToolbar,
                keywords = Prefs.CreateBrowserManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetEmptyInspectorSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(EmptyInspectorSettingsPath, SettingsScope.Project)
            {
                label = "Empty Inspector",
                guiHandler = Prefs.EmptyInspectorManager.DrawWithToolbar,
                keywords = Prefs.EmptyInspectorManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetFavoriteWindowsSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(FavoriteWindowsSettingsPath, SettingsScope.Project)
            {
                label = "Favorite Windows",
                guiHandler = Prefs.FavoriteWindowsManager.DrawWithToolbar,
                keywords = Prefs.FavoriteWindowsManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetGameObjectSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(GameObjectSettingsPath, SettingsScope.Project)
            {
                label = "GameObjects",
                guiHandler = Prefs.GameObjectManager.DrawWithToolbar,
                keywords = Prefs.GameObjectManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetHeadersSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(HeadersSettingsPath, SettingsScope.Project)
            {
                label = "Headers",
                guiHandler = Prefs.HeadersManager.DrawWithToolbar,
                keywords = Prefs.HeadersManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetHierarchySettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(HierarchySettingsPath, SettingsScope.Project)
            {
                label = "Hierarchy",
                guiHandler = Prefs.HierarchyManager.DrawWithToolbar,
                keywords = Prefs.HierarchyManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetHighlightSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(HighlightSettingsPath, SettingsScope.Project)
            {
                label = "Highlighter",
                guiHandler = Prefs.HighlightManager.DrawWithToolbar,
                keywords = Prefs.HighlightManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetInspectorSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(InspectorSettingsPath, SettingsScope.Project)
            {
                label = "Inspector",
                guiHandler = Prefs.InspectorManager.DrawWithToolbar,
                keywords = Prefs.InspectorManager.GetKeywords()
            };
            return provider;
        }
        
        [SettingsProvider]
        public static SettingsProvider GetMiniLayoutsSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(MiniLayoutsSettingsPath, SettingsScope.Project)
            {
                label = "Mini Layouts",
                guiHandler = Prefs.MiniLayoutsManager.DrawWithToolbar,
                keywords = Prefs.MiniLayoutsManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetNavigationSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(NavigationSettingsPath, SettingsScope.Project)
            {
                label = "Navigation",
                guiHandler = Prefs.NavigationManager.DrawWithToolbar,
                keywords = Prefs.NavigationManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetQuickAccessSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(QuickAccessSettingsPath, SettingsScope.Project)
            {
                label = "Quick Access Bar",
                guiHandler = Prefs.QuickAccessBarManager.DrawWithToolbar,
                keywords = Prefs.QuickAccessBarManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetProjectSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(ProjectSettingsPath, SettingsScope.Project)
            {
                label = "Project",
                guiHandler = Prefs.ProjectManager.DrawWithToolbar,
                keywords = Prefs.ProjectManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetProjectFolderIconsSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(ProjectFolderIconsSettingsPath, SettingsScope.Project)
            {
                label = "Folder Icons",
                guiHandler = Prefs.ProjectFolderIconManager.DrawWithToolbar,
                keywords = Prefs.ProjectFolderIconManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetSearchSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(SearchWindowsSettingsPath, SettingsScope.Project)
            {
                label = "Search",
                guiHandler = Prefs.SearchManager.DrawWithToolbar,
                keywords = Prefs.SearchManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetSceneViewSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(SceneViewSettingsPath, SettingsScope.Project)
            {
                label = "Scene View",
                guiHandler = Prefs.SceneViewManager.DrawWithToolbar,
                keywords = Prefs.SceneViewManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(UEESettingsPath, SettingsScope.Project)
            {
                label = "Ultimate Editor Enhancer",
                guiHandler = Prefs.DrawGeneralManagers,
                keywords = Prefs.GetGeneralManagersKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetToolbarSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(ToolbarSettingsPath, SettingsScope.Project)
            {
                label = "Toolbar",
                guiHandler = Prefs.ToolbarManager.DrawWithToolbar,
                keywords = Prefs.ToolbarManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetViewsSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(ViewsSettingsPath, SettingsScope.Project)
            {
                label = "Views",
                guiHandler = Prefs.ViewGalleryManager.DrawWithToolbar,
                keywords = Prefs.ViewGalleryManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetWailaSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(WailaSettingsPath, SettingsScope.Project)
            {
                label = "WAILA",
                guiHandler = Prefs.WailaManager.DrawWithToolbar,
                keywords = Prefs.WailaManager.GetKeywords()
            };
            return provider;
        }

        [SettingsProvider]
        public static SettingsProvider GetUnsafeSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider(UnsafeSettingsPath, SettingsScope.Project)
            {
                label = "Unsafe",
                guiHandler = Prefs.UnsafeManager.DrawWithToolbar,
                keywords = Prefs.UnsafeManager.GetKeywords()
            };
            return provider;
        }

        public static void OpenEmptyInspectorSettings()
        {
            SettingsService.OpenProjectSettings(EmptyInspectorSettingsPath);
        }

        public static void OpenFavoriteWindowsSettings()
        {
            SettingsService.OpenProjectSettings(FavoriteWindowsSettingsPath);
        }

        public static void OpenMiniLayoutsSettings()
        {
            SettingsService.OpenProjectSettings(MiniLayoutsSettingsPath);
        }

        public static void OpenQuickAccessSettings()
        {
            SettingsService.OpenProjectSettings(QuickAccessSettingsPath);
        }

        public static void OpenSearchSettings()
        {
            SettingsService.OpenProjectSettings(SearchWindowsSettingsPath);
        }

        [MenuItem(WindowsHelper.MenuPath + "Settings", false, MenuItemOrder.Settings)]
        public static void OpenSettings()
        {
            SettingsService.OpenProjectSettings(UEESettingsPath);
        }

        public static void OpenToolbarSettings()
        {
            SettingsService.OpenProjectSettings(ToolbarSettingsPath);
        }

        public static void OpenViewsSettings()
        {
            SettingsService.OpenProjectSettings(ViewsSettingsPath);
        }
    }
}