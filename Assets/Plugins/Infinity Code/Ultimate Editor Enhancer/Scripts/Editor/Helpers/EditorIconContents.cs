/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class EditorIconContents
    {
        private static GUIContent _alignHorizontallyCenter;
        private static GUIContent _alignHorizontallyCenterActive;
        private static GUIContent _alignHorizontallyLeft;
        private static GUIContent _alignHorizontallyLeftActive;
        private static GUIContent _alignHorizontallyRight;
        private static GUIContent _alignHorizontallyRightActive;
        private static GUIContent _alignVerticallyCenter;
        private static GUIContent _animationFirstKey;
        private static GUIContent _animationPrevKey;
        private static GUIContent _avatarPivot;
        private static GUIContent _consoleErrorIconSmall;
        private static GUIContent _csScript;
        private static GUIContent _dropdown;
        private static GUIContent _editIconSmall;
        private static GUIContent _folderIcon;
        private static GUIContent _folderOpenedIcon;
        private static GUIContent _gameObject;
        private static GUIContent _guiText;
        private static GUIContent _hierarchyWindow;
        private static GUIContent _inspectorWindow;
        private static GUIContent _linked;
        private static GUIContent _material;
        private static GUIContent _pauseButtonOn;
        private static GUIContent _playButtonOn;
        private static GUIContent _popup;
        private static GUIContent _preAudioPlayOn;
        private static GUIContent _prefabIcon;
        private static GUIContent _prefabVariantIcon;
        private static GUIContent _project;
        private static GUIContent _rectTransformBlueprint;
        private static GUIContent _refresh;
        private static GUIContent _saveActive;
        private static GUIContent _saveFromPlay;
        private static GUIContent _sceneAsset;
        private static GUIContent _sceneLoadIn;
        private static GUIContent _sceneVisibilityHiddenHover;
        private static GUIContent _sceneVisibilityVisibleHover;
        private static GUIContent _search;
        private static GUIContent _settings;
        private static GUIContent _shader;
        private static GUIContent _stepButton;
        private static GUIContent _textAsset;
        private static GUIContent _toolHandleGlobal;
        private static GUIContent _toolHandleLocal;
        private static GUIContent _toolbarPlus;
        private static GUIContent _treeEditorTrash;
        private static GUIContent _unityLogo;
        private static GUIContent _unlinked;
        private static GUIContent _verticalLayoutGroup;


        public static GUIContent alignHorizontallyCenterActive
        {
            get
            {
                if (_alignHorizontallyCenterActive == null)
                {
                    _alignHorizontallyCenterActive = EditorGUIUtility.IconContent("align_horizontally_center_active");
                }
                return _alignHorizontallyCenterActive;
            }
        }

        public static GUIContent alignHorizontallyCenter
        {
            get
            {
                if (_alignHorizontallyCenter == null)
                {
                    _alignHorizontallyCenter = EditorGUIUtility.IconContent("align_horizontally_center");
                }
                return _alignHorizontallyCenter;
            }
        }

        public static GUIContent alignHorizontallyLeftActive
        {
            get
            {
                if (_alignHorizontallyLeftActive == null)
                {
                    _alignHorizontallyLeftActive = EditorGUIUtility.IconContent("align_horizontally_left_active");
                }
                return _alignHorizontallyLeftActive;
            }
        }

        public static GUIContent alignHorizontallyLeft
        {
            get
            {
                if (_alignHorizontallyLeft == null)
                {
                    _alignHorizontallyLeft = EditorGUIUtility.IconContent("align_horizontally_left");
                }
                return _alignHorizontallyLeft;
            }
        }

        public static GUIContent alignHorizontallyRight
        {
            get
            {
                if (_alignHorizontallyRight == null)
                {
                    _alignHorizontallyRight = EditorGUIUtility.IconContent("align_horizontally_right");
                }
                return _alignHorizontallyRight;
            }
        }

        public static GUIContent alignHorizontallyRightActive
        {
            get
            {
                if (_alignHorizontallyRightActive == null)
                {
                    _alignHorizontallyRightActive = EditorGUIUtility.IconContent("align_horizontally_right_active");
                }
                return _alignHorizontallyRightActive;
            }
        }

        public static GUIContent alignVerticallyCenter
        {
            get
            {
                if (_alignVerticallyCenter == null)
                {
                    _alignVerticallyCenter = EditorGUIUtility.IconContent("align_vertically_center");
                }
                return _alignVerticallyCenter;
            }
        }

        public static GUIContent animationFirstKey
        {
            get
            {
                if (_animationFirstKey == null)
                {
                    _animationFirstKey = EditorGUIUtility.IconContent("Animation.FirstKey");
                }
                return _animationFirstKey;
            }
        }

        public static GUIContent animationPrevKey
        {
            get
            {
                if (_animationPrevKey == null)
                {
                    _animationPrevKey = EditorGUIUtility.IconContent("Animation.PrevKey");
                }
                return _animationPrevKey;
            }
        }

        public static GUIContent avatarPivot
        {
            get
            {
                if (_avatarPivot == null)
                {
                    _avatarPivot = EditorGUIUtility.IconContent("AvatarPivot");
                }
                return _avatarPivot;
            }
        }

        public static GUIContent consoleErrorIconSmall
        {
            get
            {
                if (_consoleErrorIconSmall == null)
                {
                    _consoleErrorIconSmall = EditorGUIUtility.IconContent("console.erroricon.sml");
                }
                return _consoleErrorIconSmall;
            }
        }

        public static GUIContent csScript
        {
            get
            {
                if (_csScript == null)
                {
                    _csScript = EditorGUIUtility.IconContent("cs Script Icon");
                }
                return _csScript;
            }
        }

        public static GUIContent dropdown
        {
            get
            {
                if (_dropdown == null)
                {
                    _dropdown = EditorGUIUtility.IconContent("icon dropdown");
                }
                return _dropdown;
            }
        }

        public static GUIContent editIcon
        {
            get
            {
                if (_editIconSmall == null)
                {
                    _editIconSmall = EditorGUIUtility.IconContent("editicon.sml");
                }
                return _editIconSmall;
            }
        }

        public static GUIContent folder
        {
            get
            {
                if (_folderIcon == null)
                {
                    _folderIcon = EditorGUIUtility.IconContent("Folder Icon");
                }
                return _folderIcon;
            }
        }

        public static GUIContent folderOpened
        {
            get
            {
                if (_folderOpenedIcon == null)
                {
                    _folderOpenedIcon = EditorGUIUtility.IconContent("FolderOpened Icon");
                }
                return _folderOpenedIcon;
            }
        }

        public static GUIContent gameObject
        {
            get
            {
                if (_gameObject == null)
                {
                    _gameObject = EditorGUIUtility.IconContent("GameObject Icon");
                }
                return _gameObject;
                
            }
        }

        public static GUIContent guiText
        {
            get
            {
                if (_guiText == null)
                {
                    _guiText = EditorGUIUtility.IconContent("GUIText Icon");
                }

                return _guiText;
            }
        }

        public static GUIContent hierarchyWindow
        {
            get
            {
                if (_hierarchyWindow == null)
                {
                    _hierarchyWindow = EditorGUIUtility.IconContent("UnityEditor.HierarchyWindow");
                }
                return _hierarchyWindow;
            }
        }

        public static GUIContent inspectorWindow
        {
            get
            {
                if (_inspectorWindow == null)
                {
                    _inspectorWindow = EditorGUIUtility.IconContent("UnityEditor.InspectorWindow");
                }
                return _inspectorWindow;
            }
        }

        public static GUIContent linked
        {
            get
            {
                if (_linked == null)
                {
                    _linked = EditorGUIUtility.IconContent("Linked");
                }
                return _linked;
            }
        }

        public static GUIContent material
        {
            get
            {
                if (_material == null)
                {
                    _material = EditorGUIUtility.IconContent("Material Icon");
                }
                return _material;
            }
        }

        public static GUIContent pauseButtonOn
        {
            get
            {
                if (_pauseButtonOn == null)
                {
                    _pauseButtonOn = EditorGUIUtility.IconContent("d_PauseButton On");
                }
                return _pauseButtonOn;
            }
        }

        public static GUIContent playButtonOn
        {
            get
            {
                if (_playButtonOn == null)
                {
                    _playButtonOn = EditorGUIUtility.IconContent("d_PlayButton On");
                }
                return _playButtonOn;
            }
        }

        public static GUIContent popup
        {
            get
            {
                if (_popup == null)
                {
                    _popup = EditorGUIUtility.IconContent("_Popup");
                }
                return _popup;
            }
        }

        public static GUIContent preAudioPlayOn
        {
            get
            {
                if (_preAudioPlayOn == null)
                {
                    _preAudioPlayOn = EditorGUIUtility.IconContent("preAudioPlayOn");
                }
                return _preAudioPlayOn;
            }
        }

        public static GUIContent prefab
        {
            get
            {
                if (_prefabIcon == null)
                {
                    _prefabIcon = EditorGUIUtility.IconContent("Prefab Icon");
                }
                return _prefabIcon;
            }
        }

        public static GUIContent prefabVariant
        {
            get
            {
                if (_prefabVariantIcon == null)
                {
                    _prefabVariantIcon = EditorGUIUtility.IconContent("PrefabVariant Icon");
                }
                return _prefabVariantIcon;
            }
        }

        public static GUIContent project
        {
            get
            {
                if (_project == null)
                {
                    _project = EditorGUIUtility.IconContent("Project");
                }
                return _project;
            }
        }

        public static GUIContent rectTransformBlueprint
        {
            get
            {
                if (_rectTransformBlueprint == null)
                {
                    _rectTransformBlueprint = EditorGUIUtility.IconContent("RectTransformBlueprint");
                }
                return _rectTransformBlueprint;
            }
        }

        public static GUIContent refresh
        {
            get
            {
                if (_refresh == null)
                {
                    _refresh = EditorGUIUtility.IconContent("Refresh");
                }
                return _refresh;
            }
        }

        public static GUIContent saveActive
        {
            get
            {
                if (_saveActive == null) _saveActive = EditorGUIUtility.IconContent("SaveActive");
                return _saveActive;
            }
        }

        public static GUIContent saveFromPlay
        {
            get
            {
                if (_saveFromPlay == null) _saveFromPlay = EditorGUIUtility.IconContent("SaveFromPlay");
                return _saveFromPlay;
            }
        }

        public static GUIContent sceneAsset
        {
            get
            {
                if (_sceneAsset == null) _sceneAsset = EditorGUIUtility.IconContent("SceneAsset Icon");
                return _sceneAsset;
            }
        }

        public static GUIContent sceneLoadIn
        {
            get
            {
                if (_sceneLoadIn == null)
                {
                    _sceneLoadIn = EditorGUIUtility.IconContent("SceneLoadIn");
                }
                return _sceneLoadIn;
            }
        }

        public static GUIContent sceneVisibilityHiddenHover
        {
            get
            {
                if (_sceneVisibilityHiddenHover == null)
                {
                    _sceneVisibilityHiddenHover = EditorGUIUtility.IconContent("scenevis_hidden_hover");
                }
                return _sceneVisibilityHiddenHover;
            }
        }

        public static GUIContent sceneVisibilityVisibleHover
        {
            get
            {
                if (_sceneVisibilityVisibleHover == null)
                {
                    _sceneVisibilityVisibleHover = EditorGUIUtility.IconContent("scenevis_visible_hover");
                }
                return _sceneVisibilityVisibleHover;
            }
        }

        public static GUIContent search
        {
            get
            {
                if (_search == null)
                {
                    _search = EditorGUIUtility.IconContent("Search Icon");
                }
                return _search;
            }
        }

        public static GUIContent settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = EditorGUIUtility.IconContent("Settings");
                }
                return _settings;
            }
        }

        public static GUIContent shader
        {
            get
            {
                if (_shader == null)
                {
                    _shader = EditorGUIUtility.IconContent("Shader Icon");
                }
                return _shader;
            }
        }

        public static GUIContent stepButton
        {
            get
            {
                if (_stepButton == null)
                {
                    _stepButton = EditorGUIUtility.IconContent("StepButton");
                }
                return _stepButton;
            }
        }

        public static GUIContent textAsset
        {
            get
            {
                if (_textAsset == null)
                {
                    _textAsset = EditorGUIUtility.IconContent("TextAsset Icon");
                }

                return _textAsset;
            }
        }

        public static GUIContent toolHandleGlobal 
        {
            get
            {
                if (_toolHandleGlobal == null)
                {
                    _toolHandleGlobal = EditorGUIUtility.IconContent("ToolHandleGlobal");
                }
                return _toolHandleGlobal;
            }
        }

        public static GUIContent toolHandleLocal
        {
            get
            {
                if (_toolHandleLocal == null)
                {
                    _toolHandleLocal = EditorGUIUtility.IconContent("ToolHandleLocal");
                }
                return _toolHandleLocal;
            }
        }

        public static GUIContent toolbarPlus
        {
            get
            {
                if (_toolbarPlus == null)
                {
                    _toolbarPlus = EditorGUIUtility.IconContent("Toolbar Plus");
                }
                return _toolbarPlus;
            }
        }

        public static GUIContent treeEditorTrash
        {
            get
            {
                if (_treeEditorTrash == null)
                {
                    _treeEditorTrash = EditorGUIUtility.IconContent("TreeEditor.Trash");
                }
                return _treeEditorTrash;
            }
        }

        public static GUIContent unlinked
        {
            get
            {
                if (_unlinked == null)
                {
                    _unlinked = EditorGUIUtility.IconContent("Unlinked");
                }
                return _unlinked;
            }
        }

        public static GUIContent unityLogo
        {
            get
            {
                if (_unityLogo == null)
                {
                    _unityLogo = EditorGUIUtility.IconContent("UnityLogo");
                }
                return _unityLogo;
            }
        }

        public static GUIContent verticalLayoutGroup
        {
            get
            {
                if (_verticalLayoutGroup == null)
                {
                    _verticalLayoutGroup = EditorGUIUtility.IconContent("VerticalLayoutGroup Icon");
                }
                return _verticalLayoutGroup;
            }
        }
    }
}