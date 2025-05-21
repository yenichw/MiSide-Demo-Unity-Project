/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class ProjectCreateCustomEditor
    {
        static ProjectCreateCustomEditor()
        {
            ProjectItemDrawer.Register("CREATE_CUSTOM_EDITOR", DrawButton, ProjectToolOrder.CreateCustomEditor);
        }

        private static void DrawButton(ProjectItem item)
        {
            if (!Prefs.projectCreateCustomEditor) return;
            if (!item.hovered) return;
            if (!item.path.StartsWith("Assets")) return;
            MonoScript monoScript = item.asset as MonoScript;
            if (monoScript == null) return;
            Type @class = monoScript.GetClass();
            if (@class == null) return;
            if (!ValidateBaseType(@class)) return;
            
            Rect r = item.rect;
            r.xMin = r.xMax - 18;
            r.height = 16;

            item.rect.xMax -= 18;

            ButtonEvent be = GUILayoutUtils.Button(r, TempContent.Get(EditorIconContents.csScript.image, "Create Custom Editor"), GUIStyle.none);
            if (be == ButtonEvent.click)
            {
                CreateCustomEditor(item, monoScript);
            }
        }

        private static void CreateCustomEditor(ProjectItem item, MonoScript monoScript)
        {
            string path = item.path;
            string name = Path.GetFileNameWithoutExtension(path);
            string editorFolder = Path.Combine(Path.GetDirectoryName(path), "Editor");
            
            if (!Directory.Exists(editorFolder))
            {
                Directory.CreateDirectory(editorFolder);
            }
            
            string editorPath = Path.Combine(editorFolder, name + "Editor.cs");
            if (File.Exists(editorPath))
            {
                if (!EditorUtility.DisplayDialog("Create Custom Editor", "File already exists", "Replace", "Cancel")) return;
            }

            string content = GetContent(monoScript);
            
            File.WriteAllText(editorPath, content, Encoding.UTF8);
            AssetDatabase.Refresh();

            Object obj = AssetDatabase.LoadAssetAtPath(editorPath, typeof(MonoScript));
            Selection.activeObject = obj;
        }

        private static string GetContent(MonoScript monoScript)
        {
            StringBuilder sb = StaticStringBuilder.Start();

            Type @class = monoScript.GetClass();
            string className = @class.Name;
            string classFullName = @class.FullName;
            string ns = @class.Namespace;

            if (string.IsNullOrEmpty(ns)) ns = EditorSettings.projectGenerationRootNamespace;
            else ns += ".Editors";

            int spaces = 0;
            sb.Append("using UnityEditor;\n\n");
            
            if (!string.IsNullOrEmpty(ns))
            {
                sb.Append("namespace ");
                sb.Append(ns);
                sb.Append("\n{\n");
                spaces += 4;
            }
            
            sb.Append(' ', spaces);
            sb.Append("[CustomEditor(typeof(");
            sb.Append(classFullName);
            sb.Append("))]\n");
            sb.Append(' ', spaces);
            sb.Append("public class ");
            sb.Append(className);
            sb.Append("Editor : Editor\n");
            sb.Append(' ', spaces);
            sb.Append("{\n");
            spaces += 4;
            sb.Append(' ', spaces);
            sb.Append("// Implement this function to make a custom inspector.\n");
            sb.Append(' ', spaces);
            sb.Append("// Inside this function you can add your own custom IMGUI based GUI for the inspector of a specific object class.\n");
            sb.Append(' ', spaces);
            sb.Append("public override void OnInspectorGUI()\n");
            sb.Append(' ', spaces);
            sb.Append("{\n");
            spaces += 4;
            sb.Append(' ', spaces);
            sb.Append("base.OnInspectorGUI();\n");
            spaces -= 4;
            sb.Append(' ', spaces);
            sb.Append("}\n");
            spaces -= 4;
            sb.Append(' ', spaces);
            sb.Append("}\n");
            
            if (!string.IsNullOrEmpty(ns)) sb.Append("}\n");

            return sb.ToString();
        }

        private static bool ValidateBaseType(Type @class)
        {
            if (@class.IsSubclassOf(typeof(MonoBehaviour))) return true;
            if (!@class.IsSubclassOf(typeof(ScriptableObject))) return false;
            
            Type type = @class.BaseType;
            while (type != null)
            {
                string ns = type.Namespace;
                if (ns != null && ns.StartsWith("UnityEditor")) return false;
                type = type.BaseType;
            }

            return true;

        }
    }
}