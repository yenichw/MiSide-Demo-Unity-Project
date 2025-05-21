/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class PrefabUtilityRef
    {
        private static MethodInfo _getOriginalSourceOrVariantRootMethod;

        private static MethodInfo getOriginalSourceOrVariantRootMethod
        {
            get
            {
                if (_getOriginalSourceOrVariantRootMethod == null)
                {
                    _getOriginalSourceOrVariantRootMethod = Reflection.GetMethod(type, "GetOriginalSourceOrVariantRoot", new[] { typeof(Object) }, Reflection.StaticLookup);
                }

                return _getOriginalSourceOrVariantRootMethod;
            }
        }

        public static Type type
        {
            get => typeof(PrefabUtility);
        }

        public static GameObject GetOriginalSourceOrVariantRoot(Object instanceOrAsset)
        {
            return getOriginalSourceOrVariantRootMethod.Invoke(null, new object[] { instanceOrAsset }) as GameObject;
        }
    }
}