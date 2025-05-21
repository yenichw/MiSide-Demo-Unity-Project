/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class EventRef
    {
        private static FieldInfo _s_CurrentField;

        private static Type type
        {
            get
            {
                return typeof(Event);
            }
        }

        private static FieldInfo s_CurrentField
        {
            get
            {
                if (_s_CurrentField == null) _s_CurrentField = type.GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
                return _s_CurrentField;
            }
        }

        public static Event s_Current
        {
            get => s_CurrentField.GetValue(null) as Event;
        }
    }
}