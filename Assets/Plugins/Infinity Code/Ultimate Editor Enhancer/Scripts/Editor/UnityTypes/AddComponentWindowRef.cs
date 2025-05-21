/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class AddComponentWindowRef
    {
        private static MethodInfo _showMethod;
        private static Type _type;

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("AddComponent.AddComponentWindow");
                return _type;
            }
        }

        private static MethodInfo showMethod
        {
            get
            {
                if (_showMethod == null) _showMethod = type.GetMethod("Show", Reflection.StaticLookup, null, new[] {typeof(Rect), typeof(GameObject[])}, null);

                return _showMethod;
            }
        }

        public static bool Show(Rect rect, GameObject[] gameObjects)
        {
            return (bool)showMethod.Invoke(null, new object [] {rect, gameObjects});
        }
    }
}