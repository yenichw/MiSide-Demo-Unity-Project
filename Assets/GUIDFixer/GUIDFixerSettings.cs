using UnityEditor;
using UnityEngine;

namespace Kamgam.GF
{
    public enum LogLevel
    {
        Log = 0,
        Warning = 1,
        Error = 2,
        Message = 3,
        NoLogs = 99
    }

    public delegate void LogCallback(string text, LogLevel logLevel = LogLevel.Log);

    public static class Logger
    {
        public static void Log(string text)
        {
            Log(text, LogLevel.Log);
        }

        public static void Warning(string text)
        {
            Log(text, LogLevel.Warning);
        }

        public static void Error(string text)
        {
            Log(text, LogLevel.Error);
        }

        public static void Message(string text)
        {
            Log(text, LogLevel.Message);
        }

        public static void Log(string text, LogLevel logLevel = LogLevel.Log)
        {
            if (!IsLogging(logLevel))
                return;

            switch (logLevel)
            {
                case LogLevel.Log:
                    Debug.Log("GUID Fixer: " + text);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning("GUID Fixer: " + text);
                    break;
                case LogLevel.Error:
                    Debug.LogError("GUID Fixer: " + text);
                    break;
                case LogLevel.Message:
                    Debug.Log("GUID Fixer: " + text);
                    break;
                default:
                    break;
            }
        }

        public static bool IsLogging(LogLevel logLevel)
        {
            var settingsLogLevel = GUIDFixerSettings.GetOrCreateSettings().LogLevel;
            return logLevel >= settingsLogLevel;
        }
    }

    public class GUIDFixerSettings : ScriptableObject
    {
        public const string Version = "1.4.0";
        public const string SettingsFilePath = "Assets/GUIDFixerSettings.asset";
        protected static GUIDFixerSettings cachedSettings;

        [SerializeField]
        public LogLevel LogLevel = LogLevel.Log;

        public static GUIDFixerSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                cachedSettings = AssetDatabase.LoadAssetAtPath<GUIDFixerSettings>(SettingsFilePath);

                // Not found? Then search for it.
                if (cachedSettings == null)
                {
                    string[] results = AssetDatabase.FindAssets("t:GUIDFixerSettings");
                    if (results.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(results[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<GUIDFixerSettings>(path);
                    }
                }

                // Still not found? Then create settings.
                if (cachedSettings == null)
                {
                    cachedSettings = ScriptableObject.CreateInstance<GUIDFixerSettings>();
                    cachedSettings.LogLevel = LogLevel.Warning;

                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                    AssetDatabase.SaveAssets();
                }

                if (cachedSettings == null)
                {
                    EditorUtility.DisplayDialog("Error", "LineBreakFixer settings could not be found or created.", "Ok");
                }
            }
            return cachedSettings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        // settings
        public static void SelectSettings()
        {
            var settings = GUIDFixerSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "GUID Fixer settings could not be found or created.", "Ok");
            }
        }
    }

    static class GUIDFixerSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateGUIDFixerSettingsProvider()
        {
            var provider = new SettingsProvider("Project/GUID Fixer", SettingsScope.Project)
            {
                label = "GUID Fixer",
                guiHandler = (searchContext) =>
                {
                    var style = new GUIStyle(GUI.skin.label);
                    style.wordWrap = true;

                    var settings = GUIDFixerSettings.GetSerializedSettings();

                    EditorGUILayout.LabelField("Version: " + GUIDFixerSettings.Version);

                    drawField("LogLevel", "Log level:", null, settings, style);

                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting.
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "GUID fixer", "fix", "GUID", "broken", "connect", "reconnect" })
            };

            return provider;
        }

        static void drawField(string propertyName, string label, string tooltip, SerializedObject settings, GUIStyle style)
        {
            EditorGUILayout.PropertyField(settings.FindProperty(propertyName), new GUIContent(label));
            if (!string.IsNullOrEmpty(tooltip))
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(tooltip, style);
                GUILayout.EndVertical();
            }
        }
    }
}