/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class PopupCallbackInfoRef
    {
        private static MethodInfo _getSelectedValueForControlMethod;
        private static FieldInfo _instanceField;
        private static MethodInfo _setEnumValueDelegateMethod;
        private static Type _type;

        public static MethodInfo getSelectedValueForControlMethod
        {
            get
            {
                if (_getSelectedValueForControlMethod == null) _getSelectedValueForControlMethod = Reflection.GetMethod(type, "GetSelectedValueForControl", new[] {typeof(int), typeof(int)}, Reflection.StaticLookup);
                return _getSelectedValueForControlMethod;
            }
        }

        public static FieldInfo instanceField
        {
            get
            {
                if (_instanceField == null) _instanceField = type.GetField("instance", Reflection.StaticLookup);
                return _instanceField;
            }
        }

        public static MethodInfo setEnumValueDelegateMethod
        {
            get
            {
                if (_setEnumValueDelegateMethod == null) _setEnumValueDelegateMethod = Reflection.GetMethod(type, "SetEnumValueDelegate", new[] {typeof(object), typeof(string[]), typeof(int)}, Reflection.InstanceLookup);
                return _setEnumValueDelegateMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("EditorGUI+PopupCallbackInfo");
                return _type;
            }
        }

        public static object GetInstance()
        {
            return instanceField.GetValue(null);
        }

        public static int GetSelectedValueForControl(int controlID, int selected)
        {
            return (int) getSelectedValueForControlMethod.Invoke(null, new object[] {controlID, selected});
        }

        public static Action<object, string[], int> GetSetEnumValueDelegate(object instance)
        {
            return (Action<object, string[], int>) Delegate.CreateDelegate(typeof(Action<object, string[], int>), instance, setEnumValueDelegateMethod);
        }

        public static void SetInstance(object instance)
        {
            instanceField.SetValue(null, instance);
        }
    }
}