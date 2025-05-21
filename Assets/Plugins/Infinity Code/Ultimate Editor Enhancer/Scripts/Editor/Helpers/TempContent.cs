/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class TempContent
    {
        private static GUIContent _text = new GUIContent();
        private static GUIContent _image = new GUIContent();

        public static GUIContent Get(GUIContent content)
        {
            if (content.image != null)
            {
                _image.text = content.text;
                _image.image = content.image;
                _image.tooltip = content.tooltip;
                return _image;
            }

            _text.image = null;
            _text.text = content.text;
            _text.tooltip = content.tooltip;
            return _text;
        }

        public static GUIContent Get(string text)
        {
            _text.image = null;
            _text.text = text;
            _text.tooltip = null;
            return _text;
        }

        public static GUIContent Get(string text, string tooltip)
        {
            _text.image = null;
            _text.text = text;
            _text.tooltip = tooltip;
            return _text;
        }

        public static GUIContent Get(Texture texture)
        {
            _image.image = texture;
            _image.text = null;
            _image.tooltip = null;
            return _image;
        }

        public static GUIContent Get(Texture texture, string tooltip)
        {
            _image.image = texture;
            _image.text = null;
            _image.tooltip = tooltip;
            return _image;
        }
    }
}