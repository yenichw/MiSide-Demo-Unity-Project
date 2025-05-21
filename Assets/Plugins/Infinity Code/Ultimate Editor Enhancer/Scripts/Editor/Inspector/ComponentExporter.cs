/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.JSON;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.InspectorTools
{
    public static class ComponentExporter
    {
        [MenuItem("CONTEXT/Component/Advanced Copy and Paste/Copy Component As JSON", false, 504)]
        private static void CopyAsJson(MenuCommand command)
        {
            Component c = command.context as Component;
            string json = GetJsonString(c);
            if (!string.IsNullOrEmpty(json)) EditorGUIUtility.systemCopyBuffer = json;
        }

        [MenuItem("CONTEXT/Component/Advanced Copy and Paste/Export Component To JSON")]
        private static void ExportAsJson(MenuCommand command)
        {
            Component c = command.context as Component;
            string json = GetJsonString(c);
            if (string.IsNullOrEmpty(json)) return;
            
            string path = EditorUtility.SaveFilePanel("Export Component", "Assets", c.GetType().Name + ".json", "json");
            if (string.IsNullOrEmpty(path)) return;
            
            System.IO.File.WriteAllText(path, json);
        }

        public static string GetJsonString(Component c)
        {
            if (c == null) return null;
            
            SerializedObject so = new SerializedObject(c);
            SerializedProperty p = so.GetIterator();

            JsonObject json = new JsonObject();

            if (!p.Next(true)) return json.ToString();

            do 
            {
                if (p.name == "m_Script" || p.name == "m_ObjectHideFlags") continue;
                json.Add(p.name, SerializedPropertyHelper.ToJson(p));
            } while (p.NextVisible(false));

            return json.ToString();
        }
        
        [MenuItem("CONTEXT/Component/Advanced Copy and Paste/Import Component From JSON")]
        private static void ImportJson(MenuCommand command)
        {
            Component c = command.context as Component;
            if (c == null) return;

            string path = EditorUtility.OpenFilePanel("Import Component", "Assets", "json");
            if (string.IsNullOrEmpty(path)) return;
            
            string json = System.IO.File.ReadAllText(path);
            SetComponentJson(c, json);
        }

        [MenuItem("CONTEXT/Component/Advanced Copy and Paste/Paste Component From JSON", false, 504)]
        private static void PasteJson(MenuCommand command)
        {
            Component c = command.context as Component;
            SetComponentJson(c, EditorGUIUtility.systemCopyBuffer);
        }

        [MenuItem("CONTEXT/Component/Advanced Copy and Paste/Paste JSON", true)]
        private static bool PasteJsonValidate(MenuCommand command)
        {
            if (!(command.context is Component)) return false;
            return ValidateJson(EditorGUIUtility.systemCopyBuffer);
        }

        public static void SetComponentJson(Component c, string jsonString)
        {
            if (!ValidateJson(jsonString)) return;
            if (c == null) return;
            
            Undo.RecordObject(c, "Paste JSON");
            EditorUtility.SetDirty(c);

            SerializedObject so = new SerializedObject(c);

            JsonObject json = Json.Parse(jsonString) as JsonObject;
            if (json == null) return;

            foreach (KeyValuePair<string, JsonItem> pair in json.table)
            {
                SerializedProperty p = so.FindProperty(pair.Key);
                if (p == null) continue;

                SerializedPropertyHelper.FromJson(p, pair.Value);
            }

            so.ApplyModifiedProperties();
        }

        public static bool ValidateJson(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            value = value.Trim();
            if (value.Length < 3) return false;

            char first = value[0];
            char last = value[value.Length - 1];

            return ((first == '{' && last == '}') || (first == '[' && last == ']'));
        }
    }
}