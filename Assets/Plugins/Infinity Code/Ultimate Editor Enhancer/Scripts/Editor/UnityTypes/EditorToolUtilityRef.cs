/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class EditorToolUtilityRef
    {
        private static MethodInfo _getCustomEditorToolsForTypeMethod;
        private static Type _type;

        private static List<Type> customTypes;

        private static MethodInfo getCustomEditorToolsForTypeMethod
        {
            get
            {
                if (_getCustomEditorToolsForTypeMethod == null) _getCustomEditorToolsForTypeMethod = type.GetMethod("GetCustomEditorToolsForType", Reflection.StaticLookup);
                return _getCustomEditorToolsForTypeMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("EditorToolUtility", "UnityEditor.EditorTools");
                return _type;
            }
        }

        public static List<Type> GetCustomEditorToolsForType(Type type)
        {
            if (customTypes != null) return customTypes;

#if UNITY_2021_2_OR_NEWER
            customTypes = new List<Type>();

            IEnumerable types = getCustomEditorToolsForTypeMethod.Invoke(null, new object[] { type }) as IEnumerable;
            if (types == null) return customTypes;

            foreach (object t in types)
            {
                Type value = Reflection.GetPropertyValue<Type>(t, "editor");
                customTypes.Add(value);
            }
#else
            customTypes = getCustomEditorToolsForTypeMethod.Invoke(null, new object[] { type }) as List<Type>;
#endif
            return customTypes;
        }
    }
}