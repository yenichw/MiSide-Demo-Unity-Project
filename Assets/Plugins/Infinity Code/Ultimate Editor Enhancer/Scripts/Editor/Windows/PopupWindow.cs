/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    [Serializable]
    public abstract class PopupWindow : EditorWindow
    {
        private static Texture2D _background; 

        protected static Texture2D background
        {
            get
            {
                if (_background == null) _background = Resources.CreateSinglePixelTexture(0.2f, 0.2f, 0.2f, 1);
                return _background;
            }
        }

        protected virtual void OnGUI()
        {
            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), background, ScaleMode.StretchToFill);
        }
    }
}