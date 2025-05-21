/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ObjectListAreaStateRef
    {
        private static FieldInfo _selectedInstanceIDsField;
        private static Type _type;

        private static FieldInfo selectedInstanceIDsField
        {
            get
            {
                if (_selectedInstanceIDsField == null) _selectedInstanceIDsField = type.GetField("m_SelectedInstanceIDs", Reflection.InstanceLookup);
                return _selectedInstanceIDsField;
            }
        }

        private static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("ObjectListAreaState");
                return _type;
            }
        }

        public static void SetSelectedInstanceIDs(object instance, List<int> ids)
        {
            selectedInstanceIDsField.SetValue(instance, ids);
        }
    }
}