/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class TransformInspectorRef
    {
        private static Type _type;
        private static MethodInfo _inspector3DMethod;

        public static MethodInfo inspector3DMethod
        {
            get
            {
                if (_inspector3DMethod == null) _inspector3DMethod = type.GetMethod("Inspector3D", Reflection.InstanceLookup);
                return _inspector3DMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("TransformInspector");
                return _type;
            }
        }
    }
}