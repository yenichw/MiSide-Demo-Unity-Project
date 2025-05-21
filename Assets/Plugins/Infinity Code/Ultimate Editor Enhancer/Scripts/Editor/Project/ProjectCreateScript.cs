/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class ProjectCreateScript
    {
        static ProjectCreateScript()
        {
            ProjectItemDrawer.Register("CREATE_SCRIPT", DrawButton, ProjectToolOrder.CreateScript);
        }

        private static void AppendBuiltInTemplates(ProjectItem item, GenericMenuEx menu)
        {
            bool isEditorFolder = item.path.Contains("Editor/") || item.path.EndsWith("Editor");
            if (isEditorFolder) AppendEditorTemplates(item, menu);
            
            menu.Add("C# Script", CreateScript, new object[] { item.asset, "C# Script" });
            menu.Add("C# ScriptableObject", CreateScript, new object[] { item.asset, "C# ScriptableObject" });
            menu.AddSeparator();
            menu.Add("C# Class", CreateScript, new object[] { item.asset, "C# Class" });
            menu.Add("C# Interface", CreateScript, new object[] { item.asset, "C# Interface" });
            menu.Add("C# Abstract Class", CreateScript, new object[] { item.asset, "C# Abstract Class" });
            menu.Add("C# Struct", CreateScript, new object[] { item.asset, "C# Struct" });
            menu.Add("C# Enum", CreateScript, new object[] { item.asset, "C# Enum" });
            menu.AddSeparator();
            
            if (!isEditorFolder) AppendEditorTemplates(item, menu);
            
            menu.Add("C# Test Script", CreateScript, new object[] { item.asset, "C# Test Script" });
            menu.AddSeparator();
            menu.Add("Assembly Definition", CreateScript, new object[] { item.asset, "Assembly Definition" });
            menu.Add("Assembly Definition Reference", CreateScript, new object[] { item.asset, "Assembly Definition Reference" });
        }

        private static void AppendEditorTemplates(ProjectItem item, GenericMenuEx menu)
        {
            menu.Add("C# Custom Editor Script", CreateScript, new object[] { item.asset, "C# Custom Editor" });
            menu.Add("C# Custom Property Drawer", CreateScript, new object[] { item.asset, "C# Custom Property Drawer" });
            menu.Add("C# Editor Window Script", CreateScript, new object[] { item.asset, "C# Editor Window" });
            menu.AddSeparator();
        }

        private static void AppendEntitiesTemplates(ProjectItem item, GenericMenuEx menu)
        {
            if (!PackageLocator.hasEntities) return;
            
            menu.Add("Entities/IComponentData", CreateScript, new object[] { item.asset, "Entities IComponentData" });
            menu.Add("Entities/IJobEntity", CreateScript, new object[] { item.asset, "Entities IJobEntity" });
            menu.Add("Entities/ISystem", CreateScript, new object[] { item.asset, "Entities ISystem" });
            menu.Add("Entities/Baker", CreateScript, new object[] { item.asset, "Entities Baker" });
            menu.Add("Entities/SystemBase", CreateScript, new object[] { item.asset, "Entities SystemBase" });
        }

        private static void AppendJobsTemplates(ProjectItem item, GenericMenuEx menu)
        {
            string extra = PackageLocator.hasBurst? " Burst": "";
            
            menu.Add("Job System/IJob", CreateScript, new object[] { item.asset, $"Jobs IJob{extra}" });
            menu.Add("Job System/IJobParallelFor", CreateScript, new object[] { item.asset, $"Jobs IJobParallelFor{extra}" });
            menu.Add("Job System/IJobParallelForTransform", CreateScript, new object[] { item.asset, $"Jobs IJobParallelForTransform{extra}" });
            menu.Add("Job System/IJobFor", CreateScript, new object[] { item.asset, $"Jobs IJobFor{extra}" });
        }

        private static void AppendUserTemplates(ProjectItem item, GenericMenuEx menu)
        {
            string[] files = Directory.GetFiles("Assets/", "*.txt", SearchOption.AllDirectories);
            if (files.Length == 0) return;

            bool addSeparator = true;
            string scriptTemplatesFolder = Resources.scriptTemplatesFolder.Replace('/', '\\');

            foreach (string file in files)
            {
                if (!file.EndsWith(".cs.txt")) continue;
                
                if (file.Replace('/', '\\').StartsWith(scriptTemplatesFolder)) continue;
                
                if (addSeparator)
                {
                    menu.AddSeparator();
                    addSeparator = false;
                }
                
                string name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));
                menu.Add("User Templates/" + name, CreateUserScript, new object[] { item.asset, file });
            }
        }

        private static void AppendZenjectTemplates(ProjectItem item, GenericMenuEx menu)
        {
            if (!PackageLocator.hasZenject) return;
            menu.Add("Zenject/Mono Installer", CreateScript, new object[] { item.asset, "Zenject MonoInstaller" });
            menu.Add("Zenject/ScriptableObject Installer", CreateScript, new object[] { item.asset, "Zenject ScriptableObjectInstaller" });
        }

        private static void CreateScript(object userdata)
        {
            object[] data = (object[])userdata;
            Object asset = data[0] as Object;
            string name = (string)data[1];
            string path = null;

            string[] files = Directory.GetFiles(Resources.scriptTemplatesFolder, "*.txt", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (Path.GetFileName(file).StartsWith(name + "-"))
                {
                    path = file;
                    break;
                }
            }
            
            if (path == null) return;

            Selection.activeObject = asset;
            string defaultName = Path.GetFileNameWithoutExtension(path).Substring(name.Length + 1);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, defaultName);
        }

        private static void CreateUserScript(object userdata)
        {
            object[] data = (object[])userdata;
            Object asset = data[0] as Object;
            string path = (string)data[1];
            
            Selection.activeObject = asset;
            string defaultName = Path.GetFileNameWithoutExtension(path);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, defaultName);
        }

        private static void DrawButton(ProjectItem item)
        {
            if (!Prefs.projectCreateScript) return;
            if (!item.isFolder) return;
            if (!item.hovered) return;
            if (!item.path.StartsWith("Assets")) return;
            if (!item.path.Contains("Scripts")) return;

            Rect r = item.rect;
            r.xMin = r.xMax - 18;
            r.height = 16;

            item.rect.xMax -= 18;

            ButtonEvent be = GUILayoutUtils.Button(r, TempContent.Get(EditorIconContents.csScript.image, "Create Script\n(Right click to select a template)"), GUIStyle.none);
            if (be == ButtonEvent.click) ProcessClick(item);
        }

        private static void ProcessClick(ProjectItem item)
        {
            Event e = Event.current;
            if (e.button == 0) CreateScript(new object[] { item.asset, "C# Script" });
            else if (e.button == 1) ShowTemplatesMenu(item);
        }

        private static void ShowTemplatesMenu(ProjectItem item)
        {
            GenericMenuEx menu = GenericMenuEx.Start();

            AppendBuiltInTemplates(item, menu);
            
            menu.AddSeparator();

            AppendEntitiesTemplates(item, menu);
            AppendJobsTemplates(item, menu);
            AppendZenjectTemplates(item, menu);
            AppendUserTemplates(item, menu);

            menu.Show();
        }
    }
}