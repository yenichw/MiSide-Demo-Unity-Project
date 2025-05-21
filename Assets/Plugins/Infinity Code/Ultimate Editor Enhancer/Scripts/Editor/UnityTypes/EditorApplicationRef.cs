/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class EditorApplicationRef
    {
        private static FieldInfo _globalEventsField;

        public static Type type
        {
            get => typeof(EditorApplication);
        }

        private static FieldInfo globalEventsField
        {
            get
            {
                if (_globalEventsField == null) _globalEventsField = type.GetField("globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic);
                return _globalEventsField;
            }
        }

        public static EditorApplication.CallbackFunction GetGlobalEventHandler()
        {
            return (EditorApplication.CallbackFunction)globalEventsField.GetValue(null);
        }

        public static void SetGlobalEventHandler(EditorApplication.CallbackFunction callback)
        {
            globalEventsField.SetValue(null, callback);
        }
    }
}