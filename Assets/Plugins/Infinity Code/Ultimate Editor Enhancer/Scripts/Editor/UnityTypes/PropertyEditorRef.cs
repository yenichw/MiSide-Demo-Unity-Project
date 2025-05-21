/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class PropertyEditorRef
    {
        private static Type _type;
        
#if UNITY_2021_2_OR_NEWER
        private static MethodInfo _openPropertyEditor2Method;
#endif
        
        private static MethodInfo _addMethod;
        private static MethodInfo _getInspectedObjectMethod;
        private static MethodInfo _loadPersistedObjectMethod;
        private static MethodInfo _openPropertyEditorMethod;
        private static PropertyInfo _trackerProp;

        private static MethodInfo getInspectedObjectMethod
        {
            get
            {
                if (_getInspectedObjectMethod == null) _getInspectedObjectMethod = type.GetMethod("GetInspectedObject", Reflection.InstanceLookup, null, Type.EmptyTypes, null);
                return _getInspectedObjectMethod;
            }
        }
        
        private static MethodInfo loadPersistedObjectMethod
        {
            get
            {
                if (_loadPersistedObjectMethod == null) _loadPersistedObjectMethod = type.GetMethod("LoadPersistedObject", Reflection.InstanceLookup, null, Type.EmptyTypes, null);
                return _loadPersistedObjectMethod;
            }
        }

        private static MethodInfo openPropertyEditorMethod
        {
            get
            {
                if (_openPropertyEditorMethod == null) _openPropertyEditorMethod = type.GetMethod("OpenPropertyEditor", Reflection.StaticLookup, null, new[] { typeof(UnityEngine.Object), typeof(bool) }, null);
                return _openPropertyEditorMethod;
            }
        }
        
#if UNITY_2021_2_OR_NEWER

        private static MethodInfo openPropertyEditor2Method
        {
            get
            {
                if (_openPropertyEditor2Method == null) _openPropertyEditor2Method = type.GetMethod("OpenPropertyEditor", Reflection.StaticLookup, null, new[] { typeof(IList<UnityEngine.Object>) }, null);
                return _openPropertyEditor2Method;
            }
        }
#endif

        private static PropertyInfo trackerProp
        {
            get
            {
                if (_trackerProp == null) _trackerProp = type.GetProperty("tracker", Reflection.InstanceLookup);
                return _trackerProp;
            }
        }


        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("PropertyEditor");
                return _type;
            }
        }

        public static UnityEngine.Object GetInspectedObject(EditorWindow wnd)
        {
            return getInspectedObjectMethod.Invoke(wnd, new object[0]) as UnityEngine.Object;
        }

        public static ActiveEditorTracker GetTracker(object propertyEditor)
        {
            return trackerProp.GetValue(propertyEditor) as ActiveEditorTracker;
        }
        
        public static void LoadPersistedObject(EditorWindow wnd)
        {
            loadPersistedObjectMethod.Invoke(wnd, null);
        }

        public static EditorWindow OpenPropertyEditor(Object obj, bool showWindow = true)
        {
            return openPropertyEditorMethod.Invoke(null, new []{obj, showWindow}) as EditorWindow;
        }

#if UNITY_2021_2_OR_NEWER

        public static EditorWindow OpenPropertyEditor(IList<UnityEngine.Object> objects)
        {
            return openPropertyEditor2Method.Invoke(null, new []{ objects }) as EditorWindow;
        }
#endif
    }
}