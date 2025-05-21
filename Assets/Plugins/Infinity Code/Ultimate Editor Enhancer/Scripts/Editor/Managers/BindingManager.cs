/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;

namespace InfinityCode.UltimateEditorEnhancer
{
    public abstract class BindingManager<T>
    {
        private static List<T> _bindings;

        protected static List<T> bindings
        {
            get
            {
                if (_bindings == null) _bindings = new List<T>();
                return _bindings;
            }
        }

        protected static T Add(T item)
        {
            bindings.Add(item);
            return item;
        }
    }
}