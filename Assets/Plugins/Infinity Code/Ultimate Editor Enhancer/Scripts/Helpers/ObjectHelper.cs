/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class ObjectHelper
    {
        public static T FindObjectOfType<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        public static T[] FindObjectsOfType<T>(bool includeInactive = false) where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            FindObjectsInactive inactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            return Object.FindObjectsByType<T>(inactive, FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<T>(includeInactive);
#endif
        }

        public static Object[] FindObjectsOfType(System.Type type, bool includeInactive = false)
        {
#if UNITY_2023_1_OR_NEWER
            FindObjectsInactive inactive = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            return Object.FindObjectsByType(type, inactive, FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType(type, includeInactive);
#endif
        }
    }
}