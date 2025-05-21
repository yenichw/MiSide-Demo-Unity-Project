using UnityEditor;
using UnityEngine;

namespace Rowlan.AnimationPreview
{
    /// <summary>
    /// Editor styles
    /// </summary>
    public class GUIStyles
    {
        private static GUIStyle _appTitleBoxStyle;
        public static GUIStyle AppTitleBoxStyle
        {
            get
            {
                if (_appTitleBoxStyle == null)
                {
                    _appTitleBoxStyle = new GUIStyle("helpBox");
                    _appTitleBoxStyle.fontStyle = FontStyle.Bold;
                    _appTitleBoxStyle.fontSize = 16;
                    _appTitleBoxStyle.alignment = TextAnchor.MiddleCenter;
                }
                return _appTitleBoxStyle;
            }
        }

        private static GUIStyle _boxTitleStyle;
        public static GUIStyle BoxTitleStyle
        {
            get
            {
                if (_boxTitleStyle == null)
                {
                    _boxTitleStyle = new GUIStyle("Label");
                    _boxTitleStyle.fontStyle = FontStyle.BoldAndItalic;
                }
                return _boxTitleStyle;
            }
        }

        private static GUIStyle _helpBoxStyle;
        public static GUIStyle HelpBoxStyle
        {
            get
            {
                if (_helpBoxStyle == null)
                {
                    _helpBoxStyle = new GUIStyle("helpBox");
                    _helpBoxStyle.fontStyle = FontStyle.Bold;
                }
                return _helpBoxStyle;
            }
        }

        private static GUIStyle _groupTitleStyle;
        public static GUIStyle GroupTitleStyle
        {
            get
            {
                if (_groupTitleStyle == null)
                {
                    _groupTitleStyle = new GUIStyle("Label");
                    _groupTitleStyle.fontStyle = FontStyle.Bold;
                }
                return _groupTitleStyle;
            }
        }

        private static GUIStyle _toolbarButtonStyle;
        public static GUIStyle ToolbarButtonStyle
        {
            get
            {
                if (_toolbarButtonStyle == null)
                {
                    _toolbarButtonStyle = new GUIStyle("Button");
                    _toolbarButtonStyle.fixedWidth = 32f;
                    _toolbarButtonStyle.fixedHeight = EditorGUIUtility.singleLineHeight + 1;

                }
                return _toolbarButtonStyle;
            }
        }

        private static GUIContent _previousIcon;
        public static GUIContent PreviousIcon
        {
            get
            {
                if (_previousIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_Animation.PrevKey" : "Animation.PrevKey";
                    _previousIcon = EditorGUIUtility.IconContent(iconName, "Previous");
                    _previousIcon.tooltip = "Previous Frame";
                }

                return _previousIcon;
            }
        }

        private static GUIContent _nextIcon;
        public static GUIContent NextIcon
        {
            get
            {
                if (_nextIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_Animation.NextKey" : "Animation.NextKey";
                    _nextIcon = EditorGUIUtility.IconContent(iconName, "Next");
                    _nextIcon.tooltip = "Next Frame";
                }

                return _nextIcon;
            }
        }

        private static GUIContent _firstIcon;
        public static GUIContent FirstIcon
        {
            get
            {
                if (_firstIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_Animation.FirstKey" : "Animation.FirstKey";
                    _firstIcon = EditorGUIUtility.IconContent(iconName, "First");
                    _firstIcon.tooltip = "First Frame";
                }

                return _firstIcon;
            }
        }

        private static GUIContent _lastIcon;
        public static GUIContent LastIcon
        {
            get
            {
                if (_lastIcon == null)
                {
                    string iconName = EditorGUIUtility.isProSkin ? "d_Animation.LastKey" : "Animation.LastKey";
                    _lastIcon = EditorGUIUtility.IconContent(iconName, "Last");
                    _lastIcon.tooltip = "Last Frame";
                }

                return _lastIcon;
            }
        }

        public static Color DefaultBackgroundColor = GUI.backgroundColor;
        public static Color ErrorBackgroundColor = new Color(1f, 0f, 0f, 1f); // red
        public static Color PlayBackgroundColor = new Color(0f, 1f, 0f, 1f); // green
        public static Color SelectedClipBackgroundColor = new Color(1f, 1f, 0f, 1f); // yellow

    }
}