/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools.QuickAccessActions
{
    public abstract class QuickAccessAction
    {
        public abstract GUIContent content { get; }

        public virtual void Draw()
        {
            string tooltip = content.tooltip;
            GUIContent currentContent = TempContent.Get(content);
            currentContent.tooltip = null;

            ButtonEvent buttonEvent = GUILayoutUtils.Button(currentContent, QuickAccess.contentStyle, GUILayout.Width(QuickAccess.width), GUILayout.Height(QuickAccess.width));
            if (buttonEvent == ButtonEvent.click)
            {
                OnClick();
            }
            else if (buttonEvent == ButtonEvent.hover)
            {
                QuickAccess.SetTooltip(tooltip, GUILayoutUtils.lastRect);
            }
        }

        public abstract void OnClick();

        public virtual void ResetContent()
        {
            
        }

        public virtual bool Validate()
        {
            return true;
        }
    }
}
