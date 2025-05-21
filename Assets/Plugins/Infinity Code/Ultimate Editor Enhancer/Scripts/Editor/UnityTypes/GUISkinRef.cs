/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class GUISkinRef
    {
        private static FieldInfo _currentField;

        public static FieldInfo currentField
        {
            get
            {
                if (_currentField == null)
                {
                    _currentField = type.GetField("current", Reflection.StaticLookup);
                }

                return _currentField;
            }
        }

        public static Type type
        {
            get => typeof(GUISkin);
        }

        public static GUISkin GetCurrent()
        {
            return (GUISkin)currentField.GetValue(null);
        }
    }
}