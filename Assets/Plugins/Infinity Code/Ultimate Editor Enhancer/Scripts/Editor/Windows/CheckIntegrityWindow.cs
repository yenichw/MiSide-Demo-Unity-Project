/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class CheckIntegrityWindow : EditorWindow
    {
        private static bool hasProblems;
        private static string message;
        private TypeRecord[] types;
        private Vector2 scrollPosition;

        private void OnEnable()
        {
            string nspace = "InfinityCode.UltimateEditorEnhancer.UnityTypes";
            StringBuilder builder = StaticStringBuilder.Start();
            types = typeof(CheckIntegrityWindow).Assembly.GetTypes()
                .Where(t => t.IsClass && t.Namespace == nspace && t.IsAbstract && t.IsSealed && t.GetCustomAttribute<HideInIntegrityAttribute>() == null)
                .Select(t => new TypeRecord(t, builder)).ToArray();
            string problems = builder.ToString();
            if (string.IsNullOrEmpty(problems))
            {
                message = "No problems were found.";
                hasProblems = false;
            }
            else
            {
                message = "Found the following problems:\n" + problems;
                hasProblems = true;
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUIStyle normalStyle = EditorStyles.label;
            GUIStyle missedStyle = new GUIStyle(normalStyle)
            {
                normal =
                {
                    textColor = Color.red
                },
                fontStyle = FontStyle.Bold
            };

            foreach (TypeRecord type in types)
            {
                string typeName = type.name;
                GUIStyle style = normalStyle;
                if (!type.exist)
                {
                    typeName = "[Missed] " + typeName;
                    style = missedStyle;
                }
                EditorGUILayout.LabelField(typeName, style);
                EditorGUI.indentLevel++;

                foreach (PropRecord prop in type.properties)
                {
                    string propName = prop.name;
                    style = normalStyle;
                    if (!prop.exist)
                    {
                        propName = "[Missed] " + propName;
                        style = missedStyle;
                    }
                    EditorGUILayout.LabelField(propName, style);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();

            if (!hasProblems) EditorGUILayout.HelpBox(message, MessageType.Info);
            else
            {
                EditorGUILayout.HelpBox(message, MessageType.Error);
                if (GUILayout.Button("Report To Infinity Code"))
                {
                    string subject = "Ultimate Editor Enhancer Integrity Check";
                    StringBuilder builder = StaticStringBuilder.Start();
                    builder.Append("OS: ")
                        .Append(SystemInfo.operatingSystem)
                        .Append("\nUnity version: ")
                        .Append(Application.unityVersion)
                        .Append("\nUltimate Editor Enhancer version: ")
                        .Append(Version.version)
                        .Append("\n")
                        .Append(message);
                    Process.Start("mailto:support@infinity-code.com?subject=" + Uri.EscapeUriString(subject) + "&body=" + Uri.EscapeUriString(builder.ToString()));
                }
            }

        }

        [MenuItem(WindowsHelper.MenuPath + "Check Integrity", false, MenuItemOrder.CheckIntegrity)]
        public static void OpenWindow()
        {
            GetWindow<CheckIntegrityWindow>("Check Integrity");
        }

        public class TypeRecord
        {
            public string name;
            public PropRecord[] properties;
            public bool exist;

            public TypeRecord(Type type, StringBuilder builder)
            {
                name = type.Name;
                if (name.EndsWith("Ref")) name = name.Substring(0, name.Length - 3);

                PropertyInfo[] allProps = type.GetProperties(Reflection.StaticLookup);

                PropertyInfo typeProp = allProps.FirstOrDefault(p => p.Name == "type");
                if (typeProp != null) exist = typeProp.GetValue(null) != null;
                else builder.Append("Missed Type: ").Append(name).Append("\n");

                properties = allProps.Where(p => p.Name.EndsWith("Field") || p.Name.EndsWith("Prop") || p.Name.EndsWith("Method")).Select(p => new PropRecord(p, name, builder)).ToArray();
            }
        }

        public class PropRecord
        {
            public string name;
            public bool exist;

            public PropRecord(PropertyInfo prop, string typeName, StringBuilder builder)
            {
                name = prop.Name;

                try
                {
                    exist = prop.GetValue(null) != null;
                }
                catch
                {
                    exist = false;
                }
                

                if (name[name.Length - 4] == 'P') name = name.Substring(0, name.Length - 4) + " (Property)";
                else if (name[name.Length - 5] == 'F') name = name.Substring(0, name.Length - 5) + " (Field)";
                else if (name[name.Length - 6] == 'M') name = char.ToUpperInvariant(name[0]) + name.Substring(1, name.Length - 7) + " (Method)";

                if (!exist) builder.Append("Missed ").Append(typeName).Append(" - ").Append(name).Append("\n");
            }
        }
    }
}