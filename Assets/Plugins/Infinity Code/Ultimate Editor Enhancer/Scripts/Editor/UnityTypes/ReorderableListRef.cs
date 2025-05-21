/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ReorderableListRef
    {
        private static MethodInfo _clearCacheMethod;
        private static MethodInfo _doListElementsMethod;
        private static MethodInfo _invalidateCacheMethod;
        private static bool _clearCacheMethodChecked;
        private static bool _invalidateCacheMethodChecked;

        private static MethodInfo clearCacheMethodHidden
        {
            get
            {
                if (_clearCacheMethod == null && !_clearCacheMethodChecked)
                {
                    _clearCacheMethod = type.GetMethod("ClearCache", Reflection.InstanceLookup);
                    _clearCacheMethodChecked = true;
                }
                return _clearCacheMethod;
            }
        }

        public static MethodInfo doListElementsMethod
        {
            get
            {
                if (_doListElementsMethod == null)
                {
                    _doListElementsMethod = type.GetMethod("DoListElements", Reflection.InstanceLookup, null, 
                        new []
                        {
                            typeof(Rect),
#if UNITY_2020_2_OR_NEWER
                            typeof(Rect)
#endif
                        }, null);
                }
                return _doListElementsMethod;
            }
        }

        private static MethodInfo invalidateCacheMethodHidden
        {
            get
            {
                if (_invalidateCacheMethod == null && !_invalidateCacheMethodChecked)
                {
                    _invalidateCacheMethod = type.GetMethod("InvalidateCache", Reflection.InstanceLookup);
                    _invalidateCacheMethodChecked = true;
                }
                return _invalidateCacheMethod;
            }
        }

        public static Type type
        {
            get => typeof(ReorderableList);
        }

        public static void ClearCache(ReorderableList list)
        {
            if (invalidateCacheMethodHidden != null) invalidateCacheMethodHidden.Invoke(list, null);
            else if (clearCacheMethodHidden != null) clearCacheMethodHidden.Invoke(list, null);
        }
    }
}