/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class Styles
    {
        public const float ProLabel = 0.7f;
        public static Color ProLabelColor = new Color(ProLabel, ProLabel, ProLabel);
        public const float ProButton = 0.2f;
        public const float ProButtonHover = 0.25f;

        private static GUIStyle _appToolbarButtonLeft;
        private static GUIStyle _buttonAlignLeft;
        private static GUIStyle _buttonWithToggleAlignLeft;
        private static GUIStyle _centeredHelpbox;
        private static GUIStyle _centeredLabel;
        private static GUIStyle _colorField;
        private static GUIStyle _dropdown;
        private static GUIStyle _grayRow;
        private static GUIStyle _hierarchyIcon;
        private static GUIStyle _iconButton;
        private static GUIStyle _inspectorTitlebar;
        private static GUIStyle _multilineLabel;
        private static GUIStyle _normalRow;
        private static GUIStyle _proRow;
        private static GUIStyle _selectedRow;
        private static Texture2D _selectedRowTexture;
        private static GUIStyle _smallLabel;
        private static GUIStyle _toolbarSelectedButton;
        private static GUIStyle _tooltip;
        private static GUIStyle _transparentButton;
        private static GUIStyle _transparentRow;
        private static GUIStyle _whiteLabel;
        private static GUIStyle _whiteRow;

        public static GUIStyle appToolbarButtonLeft
        {
            get
            {
                if (_appToolbarButtonLeft == null)
                {
                    _appToolbarButtonLeft = "AppToolbarButtonLeft";
                    _appToolbarButtonLeft.fixedHeight = 20;
                    _appToolbarButtonLeft.margin = new RectOffset(0, 0, 0, 0);
                }
                return _appToolbarButtonLeft;
            }
        }

        public static GUIStyle buttonAlignLeft
        {
            get
            {
                if (_buttonAlignLeft == null)
                {
                    _buttonAlignLeft = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(isProSkin? EditorSkin.Scene: EditorSkin.Inspector).button)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fixedHeight = 20,
                    };
                }
                return _buttonAlignLeft;
            }
        }

        public static GUIStyle buttonWithToggleAlignLeft
        {
            get
            {
                if (_buttonWithToggleAlignLeft == null)
                {
                    _buttonWithToggleAlignLeft = new GUIStyle(buttonAlignLeft)
                    {
                        padding = new RectOffset(22, 6, 2, 3)
                    };
                }
                return _buttonWithToggleAlignLeft;
            }
        }

        public static GUIStyle centeredHelpbox
        {
            get
            {
                if (_centeredHelpbox == null)
                {
                    _centeredHelpbox = new GUIStyle(EditorStyles.helpBox)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return _centeredHelpbox;
            }
        }

        public static GUIStyle centeredLabel
        {
            get
            {
                if (_centeredLabel == null)
                {
                    _centeredLabel = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                return _centeredLabel;
            }
        }

        public static GUIStyle colorField
        {
            get
            {
                if (_colorField == null)
                {
                    _colorField = new GUIStyle(EditorStyles.colorField)
                    {
                        margin = new RectOffset(4, 4, 4, 4)
                    };
                }

                return _colorField; 
            }
        }

        public static GUIStyle dropdown
        {
            get
            {
                if (_dropdown == null)
                {
                    _dropdown = "Dropdown";
                    _dropdown.fixedHeight = 20;
                    _dropdown.margin = new RectOffset(0, 0, 0, 0);
                }
                return _dropdown;
            }
        }

        public static GUIStyle grayRow
        {
            get
            {
                if (_grayRow == null || _grayRow.normal.background == null)
                {
                    _grayRow = new GUIStyle(EditorStyles.label)
                    {
                        normal =
                        {
                            background = Resources.CreateSinglePixelTexture(82, 82, 82, 255)
                        },
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return _grayRow;
            }
        }

        public static GUIStyle hierarchyIcon
        {
            get
            {
                if (_hierarchyIcon == null)
                {
                    _hierarchyIcon = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 10,
                        margin = new RectOffset(),
                        padding = new RectOffset(),
                        clipping = TextClipping.Overflow,
                    };
                }

                return _hierarchyIcon;
            }
        }

        public static GUIStyle iconButton
        {
            get
            {
                if (_iconButton == null) _iconButton = GetStyle("IconButton");
                return _iconButton;
            }
        }

        public static GUIStyle inspectorTitlebar
        {
            get
            {
                if (_inspectorTitlebar == null) _inspectorTitlebar = GetStyle("IN Title");
                return _inspectorTitlebar;
            }
        }

        public static bool isProSkin
        {
            get
            {
                return EditorGUIUtility.isProSkin;
            }
        }

        public static GUIStyle multilineLabel
        {
            get
            {
                if (_multilineLabel == null)
                {
                    _multilineLabel = new GUIStyle("Label")
                    {
                        wordWrap = true
                    };
                }

                return _multilineLabel;
            }
        }

        public static GUIStyle normalRow
        {
            get
            {
                if (_normalRow == null || _normalRow.normal.background == null)
                {
                    _normalRow = new GUIStyle(EditorStyles.label)
                    {
                        normal =
                        {
                            background = Resources.CreateSinglePixelTexture(isProSkin? (Color)new Color32(45, 45, 48, 255) : Color.white)
                        },
                        margin = new RectOffset(0, 0, 0, 0),
                        overflow = new RectOffset(0, 0, 0, 0),
                        fixedHeight = 20,
                    };
                }

                return _normalRow;
            }
        }

        public static GUIStyle proRow
        {
            get
            {
                if (_proRow == null || _proRow.normal.background == null)
                {
                    _proRow = new GUIStyle(EditorStyles.label)
                    {
                        normal =
                        {
                            background = Resources.CreateSinglePixelTexture(63, 63, 63, 255)
                        },
                        margin = new RectOffset(0, 0, 0, 0),
                    };
                }

                return _proRow;
            }
        }

        public static GUIStyle selectedRow
        {
            get
            {
                if (_selectedRow == null || _selectedRow.normal.background == null)
                {
                    _selectedRow = new GUIStyle(EditorStyles.label)
                    {
                        normal =
                        {
                            background = selectedRowTexture
                        },
                        margin = new RectOffset(0, 0, 0, 0),
                        overflow = new RectOffset(0, 0, 0, 0),
                        fixedHeight = 20,
                    };
                }

                return _selectedRow;
            }
        }

        public static Texture2D selectedRowTexture
        {
            get
            {
                if (_selectedRowTexture == null) _selectedRowTexture = Resources.CreateSinglePixelTexture(62, 125, 231, 255);

                return _selectedRowTexture;
            }
        }

        public static GUIStyle smallLabel
        {
            get
            {
                if (_smallLabel == null)
                {
                    _smallLabel = new GUIStyle(EditorStyles.label)
                    {
                        fontSize = 9
                    };
                }

                return _smallLabel;
            }
        }

        public static GUIStyle toolbarSelectedButton
        {
            get
            {
                if (_toolbarSelectedButton == null)
                {
                    _toolbarSelectedButton = new GUIStyle(EditorStyles.toolbarButton)
                    {
                        normal = EditorStyles.toolbarButton.active
                    };
                }

                return _toolbarSelectedButton;
            }
        }

        public static GUIStyle tooltip
        {
            get
            {
                if (_tooltip == null)
                {
                    _tooltip = GetStyle("Tooltip");
                }

                return _tooltip;
            }
        }

        public static GUIStyle transparentButton
        {
            get
            {
                if (_transparentButton == null)
                {
                    _transparentButton = new GUIStyle
                    {
                        margin = new RectOffset(4, 4, 4, 4)
                    };
                }

                return _transparentButton;
            }
        }

        public static GUIStyle transparentRow
        {
            get
            {
                if (_transparentRow == null || _transparentRow.normal.background == null)
                {
                    _transparentRow = new GUIStyle(EditorStyles.label)
                    {
                        normal =
                        {
                            background = Resources.CreateSinglePixelTexture(255, 255, 255, 0)
                        },
                        margin = new RectOffset(0, 0, 0, 0),
                        overflow = new RectOffset(0, 0, 0, 0),
                        fixedHeight = 20,
                    };
                }

                return _transparentRow;
            }
        }


        public static GUIStyle whiteLabel
        {
            get
            {
                if (_whiteLabel == null)
                {
                    _whiteLabel = new GUIStyle(EditorStyles.whiteLabel)
                    {
                        margin = new RectOffset(4, 4, 4, 4)
                    };
                }

                return _whiteLabel;
            }
        }

        public static GUIStyle whiteRow
        {
            get
            {
                if (_whiteRow == null || _whiteRow.normal.background == null)
                {
                    _whiteRow = new GUIStyle(EditorStyles.label)
                    {
                        normal =
                        {
                            background = Resources.CreateSinglePixelTexture(255, 255, 255, 255)
                        }, margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return _whiteRow;
            }
        }

        internal static GUIStyle GetStyle(string styleName)
        {
            return GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        }
    }
}