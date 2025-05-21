/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static partial class Prefs
    {
        private const string PrefsKey = "UEE";
        public const string Prefix = PrefsKey + ".";
        private const string SettingsFilename = "UEE-Settings.uee";

        private static Action AfterFirstLoad;

        private static bool migrationReplace;

        private static PrefManager[] _managers;
        private static string[] escapeChars = { "%", "%25", ";", "%3B", "(", "%28", ")", "%29" };
        private static bool forceSave;
        private static Vector2 scrollPosition;
        private static bool loaded;

        internal static PrefManager[] managers
        {
            get
            {
                if (_managers == null)
                {
                    List<PrefManager> items = Reflection.GetInheritedItems<PrefManager>();
                    _managers = items.OrderBy(d => d.order).ToArray();
                }

                return _managers;
            }
        }

        static Prefs()
        {
            Load();
        }
        
        private static void DrawToggleField(string label, ref bool value, Action OnChange)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.ToggleLeft(label, value);
            if (EditorGUI.EndChangeCheck() && OnChange != null) OnChange();
        }

        public static PrefManager GetManager<T>() where T : PrefManager
        {
            return managers.FirstOrDefault(m => m.GetType() == typeof(T));
        }

        private static string GetSettings()
        {
            FieldInfo[] fields = typeof(Prefs).GetFields(BindingFlags.Static | BindingFlags.Public);
            StringBuilder builder = StaticStringBuilder.Start();

            try
            {
                SaveFields(fields, null, builder);
                return builder.ToString();
            }
            catch (Exception e)
            {
                Log.Add(e);
            }

            return string.Empty;
        }

        public static void InvokeAfterFirstLoad(Action action)
        {
            if (loaded) action();
            else AfterFirstLoad += action;
        }

        private static void Load()
        {
            string prefStr;

            if (File.Exists(SettingsFilename)) prefStr = File.ReadAllText(SettingsFilename, Encoding.UTF8);
            else prefStr = EditorPrefs.GetString(PrefsKey, String.Empty);
            
            LoadSettings(prefStr);
            loaded = true;
            OnLoadComplete();
        }

        private static void LoadSettings(string str)
        {
            if (string.IsNullOrEmpty(str)) return;

            Type prefType = typeof(Prefs);
            FieldInfo[] fields = prefType.GetFields(BindingFlags.Static | BindingFlags.Public);
            Dictionary<string, FieldInfo> fieldsDict = fields.ToDictionary(f => f.Name);

            int i = 0;
            try
            {
                LoadFields(str, fieldsDict, ref i, null);
            }
            catch (Exception e)
            {
                Log.Add(e);
            }
        }

        private static void LoadFields(string prefStr, Dictionary<string, FieldInfo> fields, ref int i, object target)
        {
            StringBuilder builder = new StringBuilder();
            bool isKey = true;
            string key = null;

            while (i < prefStr.Length)
            {
                char c = prefStr[i];
                i++;
                if (c == ':' && isKey)
                {
                    key = builder.ToString();
                    builder.Clear();
                    isKey = false;
                }
                else if (c == ';')
                {
                    string value = builder.ToString();
                    builder.Clear();
                    isKey = true;
                    SetValue(target, fields, key, value);
                }
                else if (c == '(')
                {
                    FieldInfo field;
                    fields.TryGetValue(key, out field);
                    if (field == null || (field.FieldType.IsValueType && field.FieldType.IsPrimitive) || field.FieldType == typeof(string))
                    {
                        int indent = 1;
                        i++;
                        while (indent > 0 && i < prefStr.Length)
                        {
                            c = prefStr[i];
                            if (c == ')') indent--;
                            else if (c == '(') indent++;
                            i++;
                        }

                        isKey = true;
                    }
                    else
                    {
                        Type type = field.FieldType;
                        object newTarget = Activator.CreateInstance(type, false);

                        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                        if (type == typeof(Vector2Int)) bindingFlags |= BindingFlags.NonPublic;

                        FieldInfo[] objFields = type.GetFields(bindingFlags);
                        Dictionary<string, FieldInfo> objFieldsDict = objFields.ToDictionary(f => f.Name);

                        LoadFields(prefStr, objFieldsDict, ref i, newTarget);
                        field.SetValue(target, newTarget);
                        i++;
                        isKey = true;
                    }
                }
                else if (c == ')')
                {
                    string value = builder.ToString();
                    builder.Clear();
                    SetValue(target, fields, key, value);
                    return;
                }
                else builder.Append(c);
            }
        }

        public static string ModifierToString(EventModifiers modifiers)
        {
            StringBuilder builder = StaticStringBuilder.Start();
            if ((modifiers & EventModifiers.Control) == EventModifiers.Control) builder.Append("CTRL");
            if ((modifiers & EventModifiers.Command) == EventModifiers.Command)
            {
                if (builder.Length > 0) builder.Append(" + ");
                builder.Append("CMD");
            }

            if ((modifiers & EventModifiers.Shift) == EventModifiers.Shift)
            {
                if (builder.Length > 0) builder.Append(" + ");
                builder.Append("SHIFT");
            }

            if ((modifiers & EventModifiers.Alt) == EventModifiers.Alt)
            {
                if (builder.Length > 0) builder.Append(" + ");
                builder.Append("ALT");
            }

            if ((modifiers & EventModifiers.FunctionKey) == EventModifiers.FunctionKey)
            {
                if (builder.Length > 0) builder.Append(" + ");
                builder.Append("FN");
            }

            return builder.ToString();
        }

        public static string ModifierToString(EventModifiers modifiers, string extra)
        {
            string v = ModifierToString(modifiers);
            if (!string.IsNullOrEmpty(v)) v += " + ";
            v += extra;
            return v;
        }

        public static string ModifierToString(EventModifiers modifiers, KeyCode keycode)
        {
            return ModifierToString(modifiers, keycode.ToString());
        }

        private static void OnLoadComplete()
        {
            if (AfterFirstLoad == null) return;
            
            Delegate[] invocationList = AfterFirstLoad.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
            {
                try
                {
                    Delegate d = invocationList[i];
                    d.DynamicInvoke(null);
                }
                catch
                {
                }
            }

            AfterFirstLoad = null;
        }

        public static void Save()
        {
            string value = GetSettings();
            EditorPrefs.SetString(PrefsKey, value);
            if (File.Exists(SettingsFilename)) File.WriteAllText(SettingsFilename, value, Encoding.UTF8);
        }

        private static void SaveFields(FieldInfo[] fields, object target, StringBuilder builder)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field.IsLiteral || field.IsInitOnly) continue;
                object value = field.GetValue(target);

                if (value == null) continue;

                if (i > 0) builder.Append(";");
                builder.Append(field.Name).Append(":");

                Type type = value.GetType();
                if (type == typeof(string)) StaticStringBuilder.AppendEscaped(builder, value as string, escapeChars);
                else if (type.IsEnum) builder.Append(value);
                else if (type.IsValueType && type.IsPrimitive) builder.Append(value);
                else
                {
                    BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
                    if (type == typeof(Vector2Int)) bindingFlags |= BindingFlags.NonPublic;
                    FieldInfo[] objFields = type.GetFields(bindingFlags);
                    if (objFields.Length == 0) continue;

                    builder.Append("(");

                    SaveFields(objFields, value, builder);

                    builder.Append(")");
                }
            }
        }

        private static void SetValue(object target, Dictionary<string, FieldInfo> fields, string key, object value)
        {
            FieldInfo field;
            if (!fields.TryGetValue(key, out field)) return;

            Type type = field.FieldType;
            if (type == typeof(string))
            {
                field.SetValue(target, Unescape(value as string, escapeChars));
            }
            else if (field.FieldType.IsEnum)
            {
                string strVal = value as string;
                if (strVal != null)
                {
                    try
                    {
                        value = Enum.Parse(type, strVal);
                        field.SetValue(target, value);
                    }
                    catch
                    {
                        Debug.Log("Some exception");
                    }
                }
            }
            else if (type.IsValueType)
            {
                try
                {
                    MethodInfo method = type.GetMethod("Parse", new[] { typeof(string) });
                    if (method == null)
                    {
                        Debug.Log("No parse for " + key);
                        return;
                    }

                    value = method.Invoke(null, new[] { value });
                    if (value != null) field.SetValue(target, value);
                }
                catch
                {
                }
            }
        }

        private static string Unescape(string value, string[] escapeCodes)
        {
            if (escapeChars == null || escapeChars.Length % 2 != 0) throw new Exception("Length of escapeCodes must be N * 2");

            StringBuilder builder = StaticStringBuilder.Start();

            for (int i = 0; i < value.Length; i++)
            {
                bool success = false;
                for (int j = 0; j < escapeCodes.Length; j += 2)
                {
                    string code = escapeCodes[j + 1];
                    if (value.Length - i - code.Length <= 0) continue;

                    success = true;

                    for (int k = 0; k < code.Length; k++)
                    {
                        if (code[k] != value[i + k])
                        {
                            success = false;
                            break;
                        }
                    }

                    if (success)
                    {
                        builder.Append(escapeCodes[j]);
                        i += code.Length - 1;
                        break;
                    }
                }

                if (!success) builder.Append(value[i]);
            }

            return builder.ToString();
        }

        public enum ExportItemIndex
        {
            everything = -1,
            bookmarks = 0,
            favoriteWindows = 1,
            quickAccessBar = 2,
            hierarchyHeaders = 3,
            emptyInspector = 4,
            projectIcons = 5,
            miniLayouts = 6
        }
    }
}