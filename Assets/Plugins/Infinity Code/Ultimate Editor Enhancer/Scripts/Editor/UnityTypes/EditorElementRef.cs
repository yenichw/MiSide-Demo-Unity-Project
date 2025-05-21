/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class EditorElementRef
    {
        private static Type _type;
        private static PropertyInfo _m_EditorsProp;
        private static MethodInfo _setElementVisibleMethod;

        public static Type type
        {
            get
            {
                if (_type == null)
                {
#if UNITY_2020_3_OR_NEWER && !UNITY_2022_2_OR_NEWER
                    string assembly = "UnityEditor.UIElementsModule";
#else
                    string assembly = "UnityEditor";
#endif
                    _type = Reflection.GetEditorTypeFromAssembly("UIElements.EditorElement", assembly);
                }
                return _type;
            }
        }

        private static PropertyInfo m_EditorsProp
        {
            get
            {
                if (_m_EditorsProp == null) _m_EditorsProp = type.GetProperty("m_Editors", Reflection.InstanceLookup);
                return _m_EditorsProp;
            }
        }

#if UNITY_2022_1_OR_NEWER
        private static FieldInfo _m_InspectorElementField;

        private static FieldInfo m_InspectorElementField
        {
            get
            {
                if (_m_InspectorElementField == null) _m_InspectorElementField = type.GetField("m_InspectorElement", Reflection.InstanceLookup);
                return _m_InspectorElementField;
            }
        }

#else
        private static PropertyInfo _m_InspectorElementProp;

        private static PropertyInfo m_InspectorElementProp
        {
            get
            {
                if (_m_InspectorElementProp == null) _m_InspectorElementProp = type.GetProperty("m_InspectorElement", Reflection.InstanceLookup);
                return _m_InspectorElementProp;
            }
        }
#endif

        private static MethodInfo setElementVisibleMethod
        {
            get
            {
                if (_setElementVisibleMethod == null) _setElementVisibleMethod = type.GetMethod("SetElementVisible", Reflection.StaticLookup);
                return _setElementVisibleMethod;
            }
        }

        public static Editor[] GetEditors(object instance)
        {
            return m_EditorsProp.GetValue(instance) as Editor[];
        }

        public static object GetInspectorElement(object instance)
        {
#if UNITY_2022_1_OR_NEWER
            return m_InspectorElementField.GetValue(instance);
#else
            return m_InspectorElementProp.GetValue(instance);
#endif
        }

        public static void SetElementVisible(object inspectorElement, bool visible)
        {
            setElementVisibleMethod.Invoke(null, new []{inspectorElement, visible});
        }
    }
}