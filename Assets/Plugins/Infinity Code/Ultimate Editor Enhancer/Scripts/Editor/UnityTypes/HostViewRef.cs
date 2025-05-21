/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class HostViewRef
    {
        private static MethodInfo _createDelegateMethod;
        private static Type _editorWindowDelegate;
        private static FieldInfo _onGUIField;
        private static PropertyInfo _positionProp;
        private static MethodInfo _setPositionMethod;
        private static Type _type;

        private static MethodInfo createDelegateMethod
        {
            get
            {
                if (_createDelegateMethod == null)
                {
                    _createDelegateMethod = Reflection.GetMethod(type, "CreateDelegate", new[] { typeof(string) }, Reflection.InstanceLookup);
                }

                return _createDelegateMethod;
            }
        }

        public static Type editorWindowDelegate
        {
            get
            {
                if (_editorWindowDelegate == null) _editorWindowDelegate = type.GetNestedType("EditorWindowDelegate", Reflection.InstanceLookup);
                return _editorWindowDelegate;
            }
        }

        private static FieldInfo onGUIField
        {
            get
            {
                if (_onGUIField == null) _onGUIField = type.GetField("m_OnGUI", Reflection.InstanceLookup);
                return _onGUIField;
            }
        }

        private static PropertyInfo positionProp
        {
            get
            {
                if (_positionProp == null) _positionProp = type.GetProperty("position", Reflection.InstanceLookup);
                return _positionProp;
            }
        }

        private static MethodInfo setPositionMethod
        {
            get
            {
                if (_setPositionMethod == null)
                {
                    _setPositionMethod = Reflection.GetMethod(type, "SetPosition", new[] { typeof(Rect) }, Reflection.InstanceLookup);
                }

                return _setPositionMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("HostView");
                return _type;
            }
        }

        public static Delegate GetOnGUI(object view)
        {
            return (Delegate)onGUIField.GetValue(view);
        }

        public static Rect GetPosition(object view)
        {
            return (Rect)positionProp.GetValue(view);
        }

        public static void SetPosition(object view, Rect position)
        {
            setPositionMethod.Invoke(view, new object[] {position});
        }

        public static void SetOnGUI(object view, Delegate action)
        {
            onGUIField.SetValue(view, action);
        }

        public static Delegate CreateDelegate(object view, string methodName)
        {
            return (Delegate)createDelegateMethod.Invoke(view, new object[] {methodName});
        }
    }
}