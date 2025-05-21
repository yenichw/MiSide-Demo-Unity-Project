/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class SceneHierarchyWindowRef
    {
        private static PropertyInfo _hasSearchFilterFocusField;
        private static FieldInfo _lastInteractedHierarchyField;
        private static FieldInfo _searchFilterField;
        private static MethodInfo _searchFieldGUIMethod;
        private static MethodInfo _setExpandedRecursiveMethod;
        private static FieldInfo _sceneHierarchyField;
        private static Type _type;
        private static MethodInfo _setSearchFilterMethod;
        private static FieldInfo _searchModeField;

        private static PropertyInfo hasSearchFilterFocusProp
        {
            get
            {
                if (_hasSearchFilterFocusField == null) _hasSearchFilterFocusField = type.GetProperty("hasSearchFilterFocus", Reflection.InstanceLookup);
                return _hasSearchFilterFocusField;
            }
        }

        private static FieldInfo lastInteractedHierarchyField
        {
            get
            {
                if (_lastInteractedHierarchyField == null) _lastInteractedHierarchyField = type.GetField("s_LastInteractedHierarchy", Reflection.StaticLookup);
                return _lastInteractedHierarchyField;
            }
        }

        private static FieldInfo searchFilterField
        {
            get
            {
                if (_searchFilterField == null) _searchFilterField = type.GetField("m_SearchFilter", Reflection.InstanceLookup);
                return _searchFilterField;
            }
        }

        private static MethodInfo searchFieldGUIMethod
        {
            get
            {
                if (_searchFieldGUIMethod == null) _searchFieldGUIMethod = type.GetMethod("SearchFieldGUI", Reflection.InstanceLookup, null, new Type[0], null);
                return _searchFieldGUIMethod;
            }
        }

        private static MethodInfo setExpandedRecursiveMethod
        {
            get
            {
                if (_setExpandedRecursiveMethod == null) _setExpandedRecursiveMethod = type.GetMethod("SetExpandedRecursive", Reflection.InstanceLookup, null, new[] { typeof(int), typeof(bool) }, null);
                return _setExpandedRecursiveMethod;
            }
        }

        private static MethodInfo setSearchFilterMethod
        {
            get
            {
                if (_setSearchFilterMethod == null) _setSearchFilterMethod = type.GetMethod("SetSearchFilter", Reflection.InstanceLookup);
                return _setSearchFilterMethod;
            }
        }

        private static FieldInfo sceneHierarchyField
        {
            get
            {
                if (_sceneHierarchyField == null) _sceneHierarchyField = type.GetField("m_SceneHierarchy", Reflection.InstanceLookup);
                return _sceneHierarchyField;
            }
        }

        private static FieldInfo searchModeField
        {
            get
            {
                if (_searchModeField == null) _searchModeField = type.GetField("m_SearchMode", Reflection.InstanceLookup);
                return _searchModeField;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("SceneHierarchyWindow");
                return _type;
            }
        }

        public static EditorWindow GetLastInteractedHierarchy()
        {
            return lastInteractedHierarchyField.GetValue(null) as EditorWindow;
        }

        public static object GetSceneHierarchy(object instance)
        {
            return sceneHierarchyField.GetValue(instance);
        }

        public static string GetSearchFilter(EditorWindow instance)
        {
            return searchFilterField.GetValue(instance) as string;
        }

        public static int GetSearchMode(EditorWindow instance)
        {
            return (int) searchModeField.GetValue(instance);
        }

        public static bool HasSearchFilterFocus(EditorWindow instance)
        {
            return (bool) hasSearchFilterFocusProp.GetValue(instance);
        }

        public static void SearchFieldGUI(EditorWindow instance)
        {
            searchFieldGUIMethod.Invoke(instance, null);
        }

        public static void SetExpandedRecursive(object instance, int id, bool expand)
        {
            setExpandedRecursiveMethod.Invoke(instance, new object[] {id, expand});
        }

        public static void SetSearchFilter(EditorWindow instance, string value, int mode = -1)
        {
            if (mode == -1) mode = GetSearchMode(instance);
            try
            {
                setSearchFilterMethod.Invoke(instance, new object[] { value, mode, true, true });
            }
            catch
            {
            }
        }
    }
}