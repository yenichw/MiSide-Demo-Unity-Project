﻿using System;
using UnityEngine;
using UnityEditor;

namespace Bewildered.SmartLibrary
{
    public enum ChooseItemAction { Open, Ping }

    [FilePath("SmartLibrary/LibraryPreferences.settings", FilePathAttribute.Location.PreferencesFolder)]
    public class LibraryPreferences : ScriptableSingleton<LibraryPreferences>
    {
        private static readonly string Version = "2.5.0";
            
        [SerializeField] private string _lastSavedVersion = Version;
        [SerializeField] private TextTruncationPosition _gridTruncationPosition = TextTruncationPosition.Middle;
        [SerializeField] private ChooseItemAction _chooseItemAction = ChooseItemAction.Ping;
        [SerializeField] private bool _showPathInListView = true;
        [SerializeField] private float _particleSystemProgress = 0.25f;
        [SerializeField] private PreviewResolution _previewResolution = PreviewResolution.x128;
        [SerializeField] private bool _useDefaultAssetPreviews = false;
        [SerializeField] private bool _showTypeIcons = true;
        [SerializeField] private float _minItemSizeDisplayTypeIcon = 70.0f;
        [SerializeField] private bool _showTextureTypeIcon = true;
        [SerializeField] private bool _showAudioTypeIcon = true;
        [SerializeField] private bool _showNamesInGridView = true;
        [SerializeField] private bool _showLivePreviews = true;
        [SerializeField] private float _livePreviewDelay = 1;

#if HDRP_1_OR_NEWER
        [SerializeField] private bool _didShowHDRPPrompt = false;
#endif

        internal static string LastSavedVersion
        {
            get { return instance._lastSavedVersion; }
        }
        
        public static TextTruncationPosition GridTruncationPosition
        {
            get { return instance._gridTruncationPosition; }
            set 
            {
                if (value == instance._gridTruncationPosition)
                    return;

                instance._gridTruncationPosition = value;
                Modified();
            }
        }

        public static ChooseItemAction ChooseAction
        {
            get { return instance._chooseItemAction; }
            set 
            {
                if (value == instance._chooseItemAction)
                    return;
                instance._chooseItemAction = value;
                Modified();
            }
        }

        public static bool ShowPathInListView
        {
            get { return instance._showPathInListView; }
            set
            {
                if (value == instance._showPathInListView)
                    return;
                instance._showPathInListView = value;
                Modified();
            }
        }
        
        public static float ParticleSystemProgress
        {
            get { return instance._particleSystemProgress; }
            set
            {
                if (value == instance._particleSystemProgress)
                    return;
                instance._particleSystemProgress = value;
                Modified();
            }
        }

        public static PreviewResolution PreviewResolution
        {
            get { return instance._previewResolution; }
            set
            {
                if (value == instance._previewResolution)
                    return;

                instance._previewResolution = value;
                Previewer.Resolution = value;
                Modified();
            }
        }
        
        public static bool UseDefaultAssetPreviews
        {
            get { return instance._useDefaultAssetPreviews; }
            set
            {
                if (value == instance._useDefaultAssetPreviews)
                    return;

                instance._useDefaultAssetPreviews = value;
                Modified();
            }
        }

        public static bool ShowItemTypeIcon
        {
            get { return instance._showTypeIcons; }
            set
            {
                if (value == instance._showTypeIcons)
                    return;

                instance._showTypeIcons = value;
                Modified();
            }
        }

        public static float MinItemSizeDisplayTypeIcon
        {
            get { return instance._minItemSizeDisplayTypeIcon; }
            set
            {
                if (value == instance._minItemSizeDisplayTypeIcon)
                    return;

                instance._minItemSizeDisplayTypeIcon = value;
                Modified();
            }
        }

        public static bool ShowTextureTypeIcon
        {
            get { return instance._showTextureTypeIcon; }
            set
            {
                if (value == instance._showTextureTypeIcon)
                    return;

                instance._showTextureTypeIcon = value;
                Modified();
            }
        }
        
        public static bool ShowAudioTypeIcon
        {
            get { return instance._showAudioTypeIcon; }
            set
            {
                if (value == instance._showAudioTypeIcon)
                    return;

                instance._showAudioTypeIcon = value;
                Modified();
            }
        }

        public static bool ShowNamesInGridView
        {
            get { return instance._showNamesInGridView; }
            set
            {
                if (value == instance._showNamesInGridView)
                    return;

                instance._showNamesInGridView = value;
                Modified();
            }
        }
        
        public static bool ShowLivePreviews
        {
            get { return instance._showLivePreviews; }
            set
            {
                if (value == instance._showLivePreviews)
                    return;

                instance._showLivePreviews = value;
                Modified();
            }
        }
        
        public static float LivePreviewDelay
        {
            get { return instance._livePreviewDelay; }
            set
            {
                if (Math.Abs(value - instance._livePreviewDelay) < 0.001f)
                    return;

                instance._livePreviewDelay = value;
                Modified();
            }
        }

#if HDRP_1_OR_NEWER
        internal static bool DidShowHDRPPrompt
        {
            get { return instance._didShowHDRPPrompt; }
            set
            {
                if (value == instance._didShowHDRPPrompt)
                    return;
                
                instance._didShowHDRPPrompt = value;
                Modified();
            }
        }
#endif

        private static void Modified()
        {
            instance.Save(true);
        }

        [SettingsProvider]
        internal static SettingsProvider CreateLibraryPreferences()
        {
            var provider = new SettingsProvider("Preferences/Smart Library", SettingsScope.User)
            {
                label = "Smart Library",
                guiHandler = OnGUI
            };

            return provider;
        }
        
        private static readonly GUIContent _gridTruncationContent = new GUIContent("Grid Truncation Position", "The position to put the ellips when truncating text in the GridView.");
        private static readonly GUIContent _chooseActionContent = new GUIContent("Item Chosen Action", "The action to take when an item in the library window is chosen (Double-clicked, pressed enter/return)");
        private static readonly GUIContent _previewResolutionContent =  new GUIContent("Preview Resolution", "The resolution of the previews generated for assets in the Library. Higher resolutions increase memory usage of Unity, each size uses 4x the memory of the previous size, so X256 uses 4x the memory of X128.");

        private static readonly GUIContent _showLivePreviewsContent = new GUIContent("Show Live Previews", "Whether to show live animated previews of objects when they are hovered over.");
        private static readonly GUIContent _livePreviewDelayContent = new GUIContent("Live Preview Hover Delay", "The amount of time in seconds that a item is hovered over before the live preview starts playing.");

        private static void OnGUI(string text)
        {
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth = 251;
            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            HeaderLabel("General");
            
            GridTruncationPosition = (TextTruncationPosition) EditorGUILayout.EnumPopup(_gridTruncationContent, GridTruncationPosition);
            
            ChooseAction = (ChooseItemAction) EditorGUILayout.EnumPopup(_chooseActionContent, ChooseAction);

            ShowPathInListView = EditorGUILayout.Toggle(new GUIContent("Show Path In List View"), ShowPathInListView);

            ShowNamesInGridView = EditorGUILayout.Toggle(new GUIContent("Show Names In Grid View"), ShowNamesInGridView);

            GUILayout.Space(10);
            
            HeaderLabel("Previews");

            var useDefaultAssetPreviewsContent = new GUIContent("Use Default Unity Asset Previews", "Whether to use the default Unity previews instead of the custom rendered ones.");
            UseDefaultAssetPreviews = EditorGUILayout.Toggle(useDefaultAssetPreviewsContent, UseDefaultAssetPreviews);

            if (!UseDefaultAssetPreviews)
            {
                PreviewResolution = (PreviewResolution) EditorGUILayout.EnumPopup(_previewResolutionContent, PreviewResolution);
            
                ParticleSystemProgress =
                    EditorGUILayout.Slider(new GUIContent("Particle System Play Progress"), ParticleSystemProgress, 0, 1);
                
                GUILayout.Space(8);
                ShowLivePreviews = EditorGUILayout.Toggle(_showLivePreviewsContent, ShowLivePreviews);
                if (ShowLivePreviews)
                {
                    EditorGUI.indentLevel++;
                    LivePreviewDelay = EditorGUILayout.Slider(_livePreviewDelayContent, LivePreviewDelay, 0, 10);
                    EditorGUI.indentLevel--;
                }
                
                GUILayout.Space(8);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(18);
                    if (GUILayout.Button("Force Regenerate All Previews", GUILayout.ExpandWidth(false)))
                    {
                        AssetPreviewManager.DeleteAllPreviewTextures();
                    }
                }
            }

            GUILayout.Space(10);
            HeaderLabel("Type Icon");

            var showItemTypeIconContent = new GUIContent("Show Item Type Icon", "Whether to show a mini icon in the corner of item preview that indicates its asset type.");
            ShowItemTypeIcon = EditorGUILayout.Toggle(showItemTypeIconContent, ShowItemTypeIcon);

            if (ShowItemTypeIcon)
            {
                EditorGUI.indentLevel++;
                var minItemSizeDisplayTypeIconContent = new GUIContent("Min Item Size Display Type Icon", "The minimum size of an item where the type icon will display at.");
                MinItemSizeDisplayTypeIcon = EditorGUILayout.Slider(minItemSizeDisplayTypeIconContent, MinItemSizeDisplayTypeIcon, 40, 256);

                var showTextureTypeIconContent = new GUIContent("Show Texture Type Icon", "Whether to show the type icon for texture type items.");
                ShowTextureTypeIcon = EditorGUILayout.Toggle(showTextureTypeIconContent, ShowTextureTypeIcon);
                var showAudioTypeIconContent = new GUIContent("Show Audio Type Icon", "Whether to show the type icon for audio type items.");
                ShowAudioTypeIcon = EditorGUILayout.Toggle(showAudioTypeIconContent, ShowAudioTypeIcon);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        private static void HeaderLabel(string text)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.IndentedRect(rect);

            GUI.Label(rect, text, EditorStyles.boldLabel);
        }
    } 
}
