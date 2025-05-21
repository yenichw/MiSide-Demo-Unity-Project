/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class SearchableEditorWindowRef
    {
        private static MethodInfo _searchFieldGUIMethod;

        public static MethodInfo searchFieldGUIMethod
        {
            get
            {
                if (_searchFieldGUIMethod == null)
                {
                    _searchFieldGUIMethod = type.GetMethod("SearchFieldGUI", Reflection.InstanceLookup, null, new[] { typeof(float) }, null);
                }
                return _searchFieldGUIMethod;
            }
        }

        private static Type type
        {
            get => typeof(SearchableEditorWindow);
        }
    }
}