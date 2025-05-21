/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ToolbarRef
    {
        private static Type _type;

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("Toolbar");
                return _type;
            }
        }

#if UNITY_2021_1_OR_NEWER

        private static FieldInfo _rootField;

        private static FieldInfo rootField
        {
            get
            {
                if (_rootField == null) _rootField = type.GetField("m_Root", Reflection.InstanceLookup);
                return _rootField;
            }
        }

        public static VisualElement GetRoot(object toolbar)
        {
            return rootField.GetValue(toolbar) as VisualElement;
        }
#endif 

        public static int GetToolCount()
        {
            return 7;
        }
    }
}