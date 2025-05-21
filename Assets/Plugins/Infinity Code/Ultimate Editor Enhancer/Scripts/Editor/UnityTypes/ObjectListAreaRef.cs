/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ObjectListAreaRef
    {
        private static PropertyInfo _gridSizeProp;
        private static MethodInfo _onGUIMethod;
        private static Type _type;

        public static PropertyInfo gridSizeProp
        {
            get
            {
                if (_gridSizeProp == null) _gridSizeProp = type.GetProperty("gridSize", Reflection.InstanceLookup);
                return _gridSizeProp;
            }
        }
        
        private static MethodInfo onGUIMethod
        {
            get
            {
                if (_onGUIMethod == null) _onGUIMethod = type.GetMethod("OnGUI", Reflection.InstanceLookup, null, new[] { typeof(Rect), typeof(int) }, null);
                return _onGUIMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("ObjectListArea");
                return _type;
            }
        }

        public static int GetGridSize(object instance)
        {
            return (int)gridSizeProp.GetValue(instance, null);
        }
        
        public static void SetGridSize(object instance, int value)
        {
            gridSizeProp.SetValue(instance, value, null);
        }

        public static void OnGUI(object instance, Rect area, int keyboardControlID)
        {
            onGUIMethod.Invoke(instance, new object[] {area, keyboardControlID});
        }
    }
}