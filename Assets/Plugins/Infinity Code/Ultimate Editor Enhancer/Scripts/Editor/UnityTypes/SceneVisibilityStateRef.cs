/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class SceneVisibilityStateRef
    {
        private static MethodInfo _isGameObjectHiddenMethod;

        private static Type _type;

        private static MethodInfo isGameObjectHiddenMethod
        {
            get
            {
                if (_isGameObjectHiddenMethod == null) _isGameObjectHiddenMethod = type.GetMethod("IsGameObjectHidden", Reflection.StaticLookup, null, new[] {typeof(GameObject)}, null);
                return _isGameObjectHiddenMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("SceneVisibilityState");
                return _type;
            }
        }

        public static bool IsGameObjectHidden(GameObject gameObject)
        {
            if (gameObject == null) return false;
            return (bool)isGameObjectHiddenMethod.Invoke(null, new object[] {gameObject});
        }
    }
}