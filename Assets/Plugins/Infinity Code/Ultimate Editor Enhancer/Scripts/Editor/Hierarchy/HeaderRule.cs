/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.JSON;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [Serializable]
    public class HeaderRule
    {
        private static Color HOVERED_TINT = Color.white;
        private static Color SELECTED_TINT = new Color(0.172f, 0.364f, 0.529f);
        private static Color HOVERED_SELECTED_TINT = new Color(0.294f, 0.42f, 0.53f);

        public bool enabled = true;
        public HeaderCondition condition = HeaderCondition.nameStarts;
        public string value;
        public string trimChars = "-=";
        public Color backgroundColor = Color.gray;
        public Color textColor = Color.white;
        public TextAlignment textAlign = TextAlignment.Center;
        public FontStyle textStyle = FontStyle.Bold;

        private Color _backgroundColor;
        private Color _textColor;
        private TextAlignment _textAlign = TextAlignment.Center;
        private FontStyle _textStyle = FontStyle.Bold;

        [NonSerialized]
        private GUIStyle _headerStyle;
        [NonSerialized]
        private GUIStyle _headerStyleHovered;
        [NonSerialized]
        private GUIStyle _headerStyleSelected;
        [NonSerialized]
        private GUIStyle _headerStyleHoveredSelected;

        private GUIStyle headerStyle
        {
            get
            {
                if (_headerStyle == null || CheckStyleChanges())
                {
                    _headerStyle = new GUIStyle();
                    Color color = backgroundColor;
                    color.a = 1;
                    _headerStyle.normal.background = Resources.CreateSinglePixelTexture(color);
                    _textColor = new Color(textColor.r, textColor.g, textColor.b, 1);
                    _headerStyle.normal.textColor = _textColor;

                    if (textAlign == TextAlignment.Center) _headerStyle.alignment = TextAnchor.MiddleCenter;
                    else if (textAlign == TextAlignment.Left) _headerStyle.alignment = TextAnchor.MiddleLeft;
                    else _headerStyle.alignment = TextAnchor.MiddleRight;

                    _headerStyle.fontStyle = textStyle;
                    _headerStyle.padding = new RectOffset(8, 8, 0, 0);

                    _backgroundColor = backgroundColor;
                    _textAlign = textAlign;
                    _textStyle = textStyle;

                    _headerStyleHovered = null;
                    _headerStyleSelected = null;
                    _headerStyleHoveredSelected = null;
                }
                else if (_headerStyle.normal.background == null)
                {
                    Color color = backgroundColor;
                    color.a = 1;
                    _headerStyle.normal.background = Resources.CreateSinglePixelTexture(color);
                }

                return _headerStyle;
            }
        }

        private GUIStyle headerStyleHovered
        {
            get
            {
                if (_headerStyleHovered == null)
                {
                    _headerStyleHovered = new GUIStyle(headerStyle);
                    Color color = Color.Lerp(backgroundColor, HOVERED_TINT, 0.3f);
                    color.a = 1;
                    _headerStyleHovered.normal.background = Resources.CreateSinglePixelTexture(color);
                }
                else if (_headerStyleHovered.normal.background == null)
                {
                    Color color = Color.Lerp(backgroundColor, HOVERED_TINT, 0.3f);
                    color.a = 1;
                    _headerStyleHovered.normal.background = Resources.CreateSinglePixelTexture(color);
                }

                return _headerStyleHovered;
            }
        }

        private GUIStyle headerStyleSelected
        {
            get
            {
                if (_headerStyleSelected == null)
                {
                    _headerStyleSelected = new GUIStyle(headerStyle);
                    Color color = Color.Lerp(backgroundColor, SELECTED_TINT, 0.4f);
                    color.a = 1;
                    _headerStyleSelected.normal.background = Resources.CreateSinglePixelTexture(color);
                }
                else if (_headerStyleSelected.normal.background == null)
                {
                    Color color = Color.Lerp(backgroundColor, SELECTED_TINT, 0.4f);
                    color.a = 1;
                    _headerStyleSelected.normal.background = Resources.CreateSinglePixelTexture(color);
                }

                return _headerStyleSelected;
            }
        }

        private GUIStyle headerStyleHoveredSelected
        {
            get
            { 
                if (_headerStyleHoveredSelected == null)
                {
                    _headerStyleHoveredSelected = new GUIStyle(headerStyle);
                    Color color = Color.Lerp(backgroundColor, HOVERED_SELECTED_TINT, 0.4f);
                    color.a = 1;
                    _headerStyleHoveredSelected.normal.background = Resources.CreateSinglePixelTexture(color);
                }
                else if (_headerStyleHoveredSelected.normal.background == null)
                {
                    Color color = Color.Lerp(backgroundColor, HOVERED_SELECTED_TINT, 0.4f);
                    color.a = 1;
                    _headerStyleHoveredSelected.normal.background = Resources.CreateSinglePixelTexture(color);
                }

                return _headerStyleHoveredSelected;
            }
        }

        public JsonObject json
        {
            get
            {
                return Json.Serialize(this) as JsonObject;
            }
        }

        private bool CheckStyleChanges()
        {
            return backgroundColor != _backgroundColor ||
                   textAlign != _textAlign ||
                   textStyle != _textStyle ||
                   !MathHelper.ColorsEqualWithoutAlpha(textColor, _textColor);
        }

        public void Draw(HierarchyItem item, int textPadding = 0)
        {
            if (Event.current.type != EventType.Repaint) return;

            string name = item.gameObject.name;

            Rect rect = item.rect;
            Rect r = new Rect(32, rect.y, rect.xMax - 16, rect.height);

            int start = 0;
            int end = name.Length;

            for (int i = start; i < name.Length; i++)
            {
                char c = name[i];
                int j;
                for (j = 0; j < trimChars.Length; j++)
                {
                    if (trimChars[j] == c)
                    {
                        start++;
                        break;
                    }
                }
                if (j == trimChars.Length) break;
            }

            for (int i = end - 1; i > start; i--)
            {
                char c = name[i];
                int j;
                for (j = 0; j < trimChars.Length; j++)
                {
                    if (trimChars[j] == c)
                    {
                        end--;
                        break;
                    }
                }
                if (j == trimChars.Length) break;
            }

            name = name.Substring(start, end - start);

            GUIStyle style = headerStyle;
            if (item.selected)
            {
                if (item.hovered) style = headerStyleHoveredSelected;
                else style = headerStyleSelected;
            }
            else if (item.hovered) style = headerStyleHovered;
            RectOffset padding = style.padding;
            padding.left = textPadding;
            style.padding = padding;
            style.Draw(r, TempContent.Get(name), 0, false, false);
        }

        public bool Validate(GameObject go)
        {
            if (!enabled) return false;

            string name = go.name;
            switch (condition)
            {
                case HeaderCondition.nameStarts:
                    return StringHelper.StartsWith(name, value);
                case HeaderCondition.nameContains:
                    return StringHelper.Contains(name, value);
                case HeaderCondition.nameEqual:
                    return name == value;
                default:
                    return false;
            }
        }
    }
}