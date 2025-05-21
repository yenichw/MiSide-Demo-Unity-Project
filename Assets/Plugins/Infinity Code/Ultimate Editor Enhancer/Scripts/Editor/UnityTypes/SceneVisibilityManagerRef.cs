/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class SceneVisibilityManagerRef
    {
        private static MethodInfo _disablePickingMethod;
        private static MethodInfo _enablePickingMethod;
        private static MethodInfo _hideMethod;
        private static PropertyInfo _instanceProp;
        private static MethodInfo _isSelectableMethod;
        private static MethodInfo _showMethod;
        private static MethodInfo _toggleVisibilityMethod;
        private static Type _type;

        private static MethodInfo disablePickingMethod
        {
            get
            {
                if (_disablePickingMethod == null) _disablePickingMethod = type.GetMethod("DisablePicking", Reflection.InstanceLookup, null, new[] { typeof(GameObject), typeof(bool) }, null);
                return _disablePickingMethod;
            }
        }

        private static MethodInfo enablePickingMethod
        {
            get
            {
                if (_enablePickingMethod == null) _enablePickingMethod = type.GetMethod("EnablePicking", Reflection.InstanceLookup, null, new[] { typeof(GameObject), typeof(bool) }, null);
                return _enablePickingMethod;
            }
        }

        private static MethodInfo hideMethod
        {
            get
            {
                if (_hideMethod == null) _hideMethod = type.GetMethod("Hide", Reflection.InstanceLookup, null, new[] { typeof(GameObject), typeof(bool) }, null);
                return _hideMethod;
            }
        }

        private static MethodInfo isSelectableMethod
        {
            get
            {
                if (_isSelectableMethod == null) _isSelectableMethod = type.GetMethod("IsSelectable", Reflection.InstanceLookup, null, new[] { typeof(GameObject) }, null);
                return _isSelectableMethod;
            }
        }

        private static PropertyInfo instanceProp
        {
            get
            {
                if (_instanceProp == null)
                {
                    Type ssType = typeof(ScriptableSingleton<>);
                    Type[] typeArgs = { type };
                    Type t = ssType.MakeGenericType(typeArgs);
                    _instanceProp = t.GetProperty("instance", Reflection.StaticLookup);
                }

                return _instanceProp;
            }
        }

        private static MethodInfo showMethod
        {
            get
            {
                if (_showMethod == null) _showMethod = type.GetMethod("Show", Reflection.InstanceLookup, null, new[] {typeof(GameObject), typeof(bool) }, null);
                return _showMethod;
            }
        }

        private static MethodInfo toggleVisibilityMethod
        {
            get
            {
                if (_toggleVisibilityMethod == null) _toggleVisibilityMethod = type.GetMethod("ToggleVisibility", Reflection.InstanceLookup, null, new[] {typeof(GameObject), typeof(bool)}, null);
                return _toggleVisibilityMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("SceneVisibilityManager");
                return _type;
            }
        }
        
        public static void DisablePicking(object instance, GameObject gameObject, bool includeChildren)
        {
            if (gameObject == null) return;
            disablePickingMethod.Invoke(instance, new object[] {gameObject, includeChildren});
        }
        
        public static void EnablePicking(object instance, GameObject gameObject, bool includeChildren)
        {
            if (gameObject == null) return;
            enablePickingMethod.Invoke(instance, new object[] {gameObject, includeChildren});
        }

        public static object GetInstance()
        {
            return instanceProp.GetValue(null);
        }

        public static void Hide(object instance, GameObject gameObject, bool includeChildren)
        {
            if (gameObject == null) return;
            hideMethod.Invoke(instance, new object[] { gameObject, includeChildren });
        }
        
        public static bool IsSelectable(object instance, GameObject gameObject)
        {
            if (gameObject == null) return false;
            return (bool)isSelectableMethod.Invoke(instance, new object[] {gameObject});
        }

        public static void Show(object instance, GameObject gameObject, bool includeChildren)
        {
            if (gameObject == null) return;
            showMethod.Invoke(instance, new object[] {gameObject, includeChildren});
        }

        public static void ToggleVisibility(object instance, GameObject target, bool visible)
        {
            if (target == null) return;
            toggleVisibilityMethod.Invoke(instance, new object[] {target, visible});
        }
    }
}