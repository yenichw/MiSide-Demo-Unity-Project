/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class GUIViewRef
    {
        private static Type _type;

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("GUIView");
                return _type;
            }
        }

        private static PropertyInfo _currentProp;
        private static MethodInfo _sendEventMethod;

        public static PropertyInfo currentProp
        {
            get
            {
                if (_currentProp == null) _currentProp = type.GetProperty("current", Reflection.StaticLookup);
                return _currentProp;
            }
        }

        public static MethodInfo sendEventMethod
        {
            get
            {
                if (_sendEventMethod == null) _sendEventMethod = type.GetMethod("SendEvent", Reflection.InstanceLookup, null, new []{typeof(Event)}, null);
                return _sendEventMethod;
            }
        }

        private static PropertyInfo _windowBackendProp;

        public static PropertyInfo windowBackendProp
        {
            get
            {
                if (_windowBackendProp == null) _windowBackendProp = type.GetProperty("windowBackend", Reflection.InstanceLookup);
                return _windowBackendProp;
            }
        }

        public static object GetCurrent()
        {
            return currentProp.GetValue(null);
        }

        public static void SendEvent(object view, Event e)
        {
            sendEventMethod.Invoke(view, new object[] {e});
        }
    }
}