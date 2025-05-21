/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class IWindowBackendRef
    {
        private static Type _type;
        private static PropertyInfo _visualTreeProp;

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("IWindowBackend");
                return _type;
            }
        }

        public static PropertyInfo visualTreeProp
        {
            get
            {
                if (_visualTreeProp == null) _visualTreeProp = type.GetProperty("visualTree", Reflection.InstanceLookup);
                return _visualTreeProp;
            }
        }
    }
}