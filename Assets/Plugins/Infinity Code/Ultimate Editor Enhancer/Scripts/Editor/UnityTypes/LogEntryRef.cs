/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class LogEntryRef
    {
        private static FieldInfo _instanceIDField;
        private static FieldInfo _messageField;
        private static FieldInfo _modeField;
        private static Type _type;

        private static FieldInfo instanceIDField
        {
            get
            {
                if (_instanceIDField == null) _instanceIDField = type.GetField("instanceID", Reflection.InstanceLookup);
                return _instanceIDField;
            }
        }

        private static FieldInfo messageField
        {
            get
            {
                if (_messageField == null) _messageField = type.GetField("message", Reflection.InstanceLookup);
                return _messageField;
            }
        }

        private static FieldInfo modeField
        {
            get
            {
                if (_modeField == null) _modeField = type.GetField("mode", Reflection.InstanceLookup);
                return _modeField;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null)
                {
                    _type = Reflection.GetEditorType("LogEntry", "UnityEditorInternal");
                    if (_type == null) _type = Reflection.GetEditorType("LogEntry");
                }
                return _type;
            }
        }

        public static int GetMode(object instance)
        {
            return (int)modeField.GetValue(instance);
        }

        public static int GetInstanceID(object instance)
        {
            return (int)instanceIDField.GetValue(instance);
        }

        public static string GetMessage(object instance)
        {
            return (string) messageField.GetValue(instance);
        }
    }
}