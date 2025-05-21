/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ViewRef
    {
        private static PropertyInfo _childrenProp;
        private static MethodInfo _setMinMaxSizesMethod;
        private static Type _type;
        private static PropertyInfo _windowProp;

        private static PropertyInfo childrenProp
        {
            get
            {
                if (_childrenProp == null) _childrenProp = type.GetProperty("children", Reflection.InstanceLookup);
                return _childrenProp;
            }
        }

        private static MethodInfo setMinMaxSizesMethod
        {
            get
            {
                if (_setMinMaxSizesMethod == null)
                {
                    _setMinMaxSizesMethod = Reflection.GetMethod(type, "SetMinMaxSizes", new[] { typeof(Vector2), typeof(Vector2) }, Reflection.InstanceLookup);
                }

                return _setMinMaxSizesMethod;
            }
        }

        public static PropertyInfo windowProp
        {
            get
            {
                if (_windowProp == null) _windowProp = type.GetProperty("window", Reflection.InstanceLookup);
                return _windowProp;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("View");
                return _type;
            }
        }
        
        public static Object[] GetChildren(object view)
        {
            return childrenProp.GetValue(view, null) as Object[];
        }

        public static ScriptableObject GetWindow(object view)
        {
            return windowProp.GetValue(view, null) as ScriptableObject;
        }

        public static void SetMinMaxSizes(object view, Vector2 min, Vector2 max)
        {
            setMinMaxSizesMethod.Invoke(view, new object[] {min, max});
        }
    }
}