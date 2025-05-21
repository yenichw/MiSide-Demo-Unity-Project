/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public abstract class LayoutItem
    {
        protected bool _isActive = true;
        protected Vector2 _size;
        protected GameObject[] targets;

        public bool isActive
        {
            get { return _isActive; }
        }

        public virtual Vector2 size
        {
            get { return _size; }
            set { _size = value; }
        }

        protected abstract void CalcSize();

        public virtual void Dispose()
        {
            targets = null;
        }

        public abstract void Draw();

        protected abstract void Init();

        public virtual void Prepare(GameObject[] targets)
        {
            this.targets = targets;
            if (this is IValidatableLayoutItem)
            {
                _isActive = (this as IValidatableLayoutItem).Validate();
            }
            else
            {
                object[] attributes = GetType().GetCustomAttributes(typeof(ValidateAttribute), true);
                if (attributes.Length > 0)
                {
                    _isActive = true;
                    foreach (object a in attributes)
                    {
                        ValidateAttribute attribute = a as ValidateAttribute;
                        _isActive = attribute.Validate();
                        if (!_isActive) break;
                    }
                }
            }

            if (_isActive)
            {
                try
                {
                    Init();
                    CalcSize();
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }
    }
}