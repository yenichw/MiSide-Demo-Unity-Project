/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class ProjectCreateFolder
    {
        private static string[] defaultNames = new[]
        {
            "Animations",
            "Audio",
            "Editor",
            "Materials",
            "Models",
            "Plugins",
            "Prefabs",
            "Resources",
            "Scenes",
            "Scripts",
            "Settings",
            "Shaders",
            "StreamingAssets",
            "Textures",
            "UI",
        };

        static ProjectCreateFolder()
        {
            ProjectItemDrawer.Register("CREATE_FOLDER", DrawButton, ProjectToolOrder.CreateFolder);
        }

        private static void CreateFolder(Object asset, string folderName)
        {
            Selection.activeObject = asset;
            ProjectWindowUtilsRef.CreateFolderWithTemplates(folderName, null);
        }

        private static void DrawButton(ProjectItem item)
        {
            if (!Prefs.projectCreateFolder) return;
            if (!item.isFolder) return;
            if (!item.hovered) return;
            if (!item.path.StartsWith("Assets")) return;

            Rect r = item.rect;
            r.xMin = r.xMax - 18;
            r.height = 16;

            item.rect.xMax -= 18;
            
            ButtonEvent be = GUILayoutUtils.Button(r, TempContent.Get(Icons.addFolder, "Create Subfolder\n(Right click to select default names)"), GUIStyle.none);

            if (be == ButtonEvent.click) ProcessClick(item);
        }

        private static void ProcessClick(ProjectItem item)
        {
            Event e = Event.current;
            Object asset = item.asset;
            if (asset == null) return;
            
            if (e.button == 0)
            {
                CreateFolder(asset, "New Folder");
            }
            else if (e.button == 1)
            {
                GenericMenuEx menu = GenericMenuEx.Start();

                if (item.path.EndsWith("/Scripts"))
                {
                    menu.Add("Editor", () => CreateFolder(asset, "Editor"));
                    menu.AddSeparator();
                }

                for (int i = 0; i < defaultNames.Length; i++)
                {
                    string folderName = defaultNames[i];
                    menu.Add(folderName, () => CreateFolder(asset, folderName));
                }

                menu.AddSeparator();
                menu.Add("New Folder", () => CreateFolder(asset, "New Folder"));

                menu.Show();
            }
        }
    }
}