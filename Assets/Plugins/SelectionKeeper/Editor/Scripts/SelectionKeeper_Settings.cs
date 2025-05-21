// Copyright (C) 2018 KAMGAM e.U. - All rights reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#if UNITY_EDITOR
#define KAMGAM_SELECTION_KEEPER
using UnityEngine;
using UnityEditor;

namespace kamgam.editor.selectionkeeper
{
    [System.Serializable]
    [ExecuteInEditMode]
    public class SelectionKeeper_Settings : ScriptableObject
    {
        private static SelectionKeeper_Settings _instance;

        /// <summary>
        /// Gets or creates a single instance of the settings.
        /// </summary>
        public static SelectionKeeper_Settings instance
        {
            get
            {
                if (SelectionKeeper_Settings._instance == null)
                {
                    SelectionKeeper_Settings._instance = LoadOrCreateSettingsInstance( false );
#if UNITY_5_5_OR_NEWER
                    AssetDatabase.importPackageCompleted += OnPackageImported;
#endif
                }

                return SelectionKeeper_Settings._instance;
            }
        }

        public static SelectionKeeper_Settings LoadOrCreateSettingsInstance( bool suppressWarning )
        {
            SelectionKeeper_Settings settings = getSettingsFromFile();

            // no settings from file, create an instance
            if ( settings == null )
            {
                settings = createSettingsInstance();
            }

            if( settings == null && !suppressWarning)
            {
                Debug.LogWarning("Selection Keeper: no settings file found. Call Tools > Selection Keeper > Settings to create one. Falling back to default settings.");
            }

            return settings;
        }

        public static SelectionKeeper_Settings CreateSettingsFileIfNotExisting()
        {
            // no settings file found, try to create one from the settings instance
            SelectionKeeper_Settings settings = getSettingsFromFile(); 
            if ( settings == null )
            {
                // fetch or create instance
                settings = LoadOrCreateSettingsInstance( true );

                // select asset file location
                string path = "Assets";
                if( AssetDatabase.IsValidFolder("Assets/SelectionKeeper/Editor") )
                {
                    path = "Assets/SelectionKeeper/Editor";
                }
                else if( AssetDatabase.IsValidFolder("Assets/Plugins/SelectionKeeper/Editor") )
                {
                    path = "Assets/Plugins/SelectionKeeper/Editor";
                }
                else
                {
                    if( !AssetDatabase.IsValidFolder("Assets/Editor") )
                    {
                        AssetDatabase.CreateFolder("Assets", "Editor");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    path = "Assets/Editor";
                }
                path = path + "/SelectionKeeper Settings.asset";

                // create asset file
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if( settings != null )
                {
                    _instance = settings;
                }

                // notify user
                EditorUtility.DisplayDialog("SelectionKeeper settings created.", "The 'SelectionKeeper Settings' file has been created in:\n'" + path + "'\n\nYou can also find it through the menu:\nTools > Selection Keeper > Settings", "Ok");
                        
                // select settings file
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }

            return settings;
        }

        static SelectionKeeper_Settings getSettingsFromFile()
        {
            SelectionKeeper_Settings settings = null;

            string[] foundPathGuids = AssetDatabase.FindAssets("t:SelectionKeeper_Settings");
            if (foundPathGuids.Length > 0)
            {
                settings = AssetDatabase.LoadAssetAtPath<SelectionKeeper_Settings>(AssetDatabase.GUIDToAssetPath(foundPathGuids[0]));
            }

            return settings;
        }

        static SelectionKeeper_Settings createSettingsInstance()
        {
            SelectionKeeper_Settings settings = ScriptableObject.CreateInstance<SelectionKeeper_Settings>();
            return settings;
        }

        public static void OnPackageImported(string packageName)
        {
            if( packageName.IndexOf("SelectionKeeper", System.StringComparison.CurrentCultureIgnoreCase) >= 0 )
            {
                if (_instance != null)
                {
                    EditorUtility.DisplayDialog("SelectionKeeper", "Selection Keeper imported.\nYou can open the settings through the menu:\n\nTools > Selection Keeper > Settings\n\nPlease read the manual.", "Ok");
                        
                    // select settings file
                    Selection.activeObject = _instance;
                    EditorGUIUtility.PingObject(_instance);
                }
            }
        }

        // Plugin on/off
        [MenuItem("Tools/Selection Keeper/Turn Plugin On", priority = 1)]
        public static void TurnPluginOn()
        {
            instance._enablePlugin = true;
        }

        [MenuItem("Tools/Selection Keeper/Turn Plugin On", true, priority = 1)]
        public static bool ValidateTurnPluginOn()
        {
            return !instance._enablePlugin;
        }

        [MenuItem("Tools/Selection Keeper/Turn Plugin Off", priority = 1)]
        public static void TurnPluginOff()
        {
            instance._enablePlugin = false;
        }

        [MenuItem("Tools/Selection Keeper/Turn Plugin Off", true, priority = 1)]
        public static bool ValidateTurnPluginOff()
        {
            return instance._enablePlugin;
        }

        // Plugin on/off
        [MenuItem("Tools/Selection Keeper/Turn ignore selection in PLAY mode On", priority = 100)]
        public static void turnIgnoreSelectionInPlayModeOn()
        {
            instance._ignoreSelectionInPlayMode = true;
        }

        [MenuItem("Tools/Selection Keeper/Turn ignore selection in PLAY mode On", true, priority = 100)]
        public static bool ValidateTurnIgnoreSelectionInPlayModeOn()
        {
            return !instance._ignoreSelectionInPlayMode && instance._enablePlugin;
        }

        [MenuItem("Tools/Selection Keeper/Turn ignore selection in PLAY mode Off", priority = 100)]
        public static void turnIgnoreSelectionInPlayModeOff()
        {
            instance._ignoreSelectionInPlayMode = false;
        }

        [MenuItem("Tools/Selection Keeper/Turn ignore selection in PLAY mode Off", true, priority = 100)]
        public static bool ValidateTurnIgnoreSelectionInPlayModeOff()
        {
            return instance._ignoreSelectionInPlayMode && instance._enablePlugin;
        }

        // settings
        [MenuItem("Tools/Selection Keeper/Settings", priority = 300)]
        public static void SelectSettingsFile()
        {
            var settings = CreateSettingsFileIfNotExisting();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("SelectionKeeper settings could not be found.", "Settings file not found.\nPlease create it in Assets/Editor/Resources with Right-Click > Create > Selection Keeper > Settings.", "Ok");
            }
        }

        // manual
        [MenuItem("Tools/Selection Keeper/Manual", priority = 300)]
        public static void SelectManualFile()
        {
            string[] foundPathGuids = AssetDatabase.FindAssets("SelectionKeeper-manual");
            if (foundPathGuids.Length > 0)
            {
                var manual = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(foundPathGuids[0]));
                Selection.activeObject = manual;
                EditorGUIUtility.PingObject(manual);
            }
            else
            {
                Debug.Log("SelectionKeeper: manual not found.");
            }
        }

        public static string Version
        {
            get { return instance._version; }
        }
        private string _version = "1.2.1"; // semver of course

        // GENERNAL SETTINGS

        // enabled
        public static bool enablePlugin
        {
            get { return instance != null && Application.isEditor && instance._enablePlugin; }
        }
        [Header("Selection Keeper - v1.2.0")]
        [Tooltip("Enables or disables the whole plugin.")]
        [SerializeField]
        private bool _enablePlugin = true;

        // removePingEffect
        public static bool removePingEffect
        {
            get { return instance != null && Application.isEditor && instance._removePingEffect; }
        }
        [Tooltip("Disable this if you are having trouble with the hierarchy not being folded out to the selected object in Edit Mode.")]
        [SerializeField]
        private bool _removePingEffect = true;

        // clearSelectionMemoryIfEmpty
        public static bool clearSelectionMemoryIfEmpty
        {
            get { return instance != null && Application.isEditor && instance._clearSelectionMemoryIfEmpty; }
        }
        [Tooltip("Should the selection memory be cleared if everything is deselected (edit mode only)? If disabled then you may have to clear the selection memory by hand (see 'Tools' menu).")]
        [SerializeField]
        private bool _clearSelectionMemoryIfEmpty = true;

        // ignore selections in play mode
        public static bool ignoreSelectionInPlayMode
        {
            get { return instance != null && Application.isEditor && instance._ignoreSelectionInPlayMode; }
        }
        [Tooltip("Ignore selection changes during play mode? Turn on if you want the last selection from Edit mode to be restored once you exit the Play mode.")]
        [SerializeField]
        private bool _ignoreSelectionInPlayMode = false;

        // showLogMessages
        public static bool showLogMessages
        {
            get { return instance != null && Application.isEditor && instance._showLogMessages; }
        }
        [Tooltip("Show log messages if an error happened or if a selection failed unexpectedly?")]
        [SerializeField]
        private bool _showLogMessages = false;
    }
}
#endif
