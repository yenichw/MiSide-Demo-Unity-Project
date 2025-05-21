/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class PropertyHandlerRef
    {
        private static Type _type;

        private static MethodInfo _addMenuItemsMethod;

        public static MethodInfo addMenuItemsMethod
        {
            get
            {
                if (_addMenuItemsMethod == null)
                {
                    _addMenuItemsMethod = type.GetMethod("AddMenuItems", Reflection.InstanceLookup, null, new[] { typeof(SerializedProperty), typeof(GenericMenu) }, null);
                }
                return _addMenuItemsMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("PropertyHandler");
                return _type;
            }
        }
    }
}