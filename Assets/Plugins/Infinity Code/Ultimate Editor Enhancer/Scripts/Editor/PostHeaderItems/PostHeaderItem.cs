/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.PostHeader
{
    public abstract class PostHeaderItem
    {
        protected GUIStyle _style;
        
        public virtual int rowOrder
        {
            get => 0;
        }

        public virtual int blockOrder
        {
            get => 0;
        }

        protected virtual GUIStyle style
        {
            get
            {
                if (_style == null)
                {
                    _style = new GUIStyle(EditorStyles.toolbarButton);
                }
                
                return _style;
            }
        }

        public virtual bool Button(GUIContent content)
        {
            return GUILayout.Button(content, style, GUILayout.Width(20), GUILayout.Height(20));
        }

        public virtual void OnRowGUI(Object target)
        {

        }

        public virtual void OnBlockGUI(Object target)
        {

        }

        public virtual bool Toggle(GUIContent content, bool value)
        {
            return GUILayout.Toggle(value, content, EditorStyles.toolbarButton, GUILayout.Width(20), GUILayout.Height(20));
        }
    }
}