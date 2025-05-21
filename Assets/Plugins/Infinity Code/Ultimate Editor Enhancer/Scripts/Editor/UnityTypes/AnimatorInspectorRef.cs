/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class AnimatorInspectorRef
    {
        private static Type _type;
        private static MethodInfo _onInspectorGUIMethod;

        public static MethodInfo onInspectorGUIMethod
        {
            get
            {
                if (_onInspectorGUIMethod == null) _onInspectorGUIMethod = type.GetMethod("OnInspectorGUI", Reflection.InstanceLookup);
                return _onInspectorGUIMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("AnimatorInspector");
                return _type;
            }
        }
    }
}