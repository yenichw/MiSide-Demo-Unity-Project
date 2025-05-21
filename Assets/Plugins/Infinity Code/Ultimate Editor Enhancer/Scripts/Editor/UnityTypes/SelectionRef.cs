/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class SelectionRef
    {
        private static MethodInfo _addMethod;

        private static MethodInfo addMethod
        {
            get
            {
                if (_addMethod == null) _addMethod = type.GetMethod("Add", Reflection.StaticLookup, null, new[] { typeof(Object) }, null);
                return _addMethod;
            }
        }

        public static Type type
        {
            get => typeof(Selection);
        }

        public static void Add(GameObject go)
        {
            addMethod.Invoke(null, new object[] { go });
        }
    }
}