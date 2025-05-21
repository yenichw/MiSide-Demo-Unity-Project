/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    public abstract class ActionItem : LayoutItem, ISortableLayoutItem, IInvokableLayoutItem
    {
        protected GUIContent _guiContent;

        protected virtual bool closeOnSelect
        {
            get { return true; }
        }

        public GUIContent guiContent
        {
            get { return _guiContent; }
            set { _guiContent = value; }
        }

        public virtual float order
        {
            get { return 0; }
        }

        protected override void CalcSize()
        {
            GUIStyle style = Styles.transparentButton;
            _size = style.CalcSize(guiContent);
            _size.x += style.margin.horizontal;
            _size.y += style.margin.bottom;
        }

        public override void Dispose()
        {
            base.Dispose();

            _guiContent = null;
        }

        public override void Draw()
        {
            if (_guiContent == null) return;

            if (GUILayout.Button(_guiContent, Styles.transparentButton, GUILayout.ExpandWidth(false)))
            {
                try
                {
                    Invoke();
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
                
                if (closeOnSelect) EditorMenu.Close();
            }
        }

        public abstract void Invoke();
    }
}