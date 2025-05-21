/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.TransformEditorTools
{
    public abstract class TransformEditorTool
    {
        protected GUIContent _content;

        public GUIContent content
        {
            get { return _content; }
        }

        public virtual float order
        {
            get { return 0; }
        }

        public virtual void OnDisable()
        {

        }

        public virtual void OnEnable()
        {

        }

        public abstract void Draw();

        public abstract void Init();

        public virtual bool Validate()
        {
            return true;
        }

        public virtual void Dispose()
        {
            _content = null;
        }
    }
}