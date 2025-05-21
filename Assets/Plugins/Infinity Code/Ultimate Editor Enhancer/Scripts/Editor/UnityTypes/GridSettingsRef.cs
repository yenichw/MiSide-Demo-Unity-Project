/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class GridSettingsRef
    {
        private static PropertyInfo _sizeProp;
        private static Type _type;

        private static PropertyInfo sizeProp
        {
            get
            {
                if (_sizeProp == null) _sizeProp = type.GetProperty("size", Reflection.StaticLookup);
                return _sizeProp;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("GridSettings");
                return _type;
            }
        }

        public static Vector3 GetSize() 
        {
            return (Vector3)sizeProp.GetValue(null);
        }

        public static void SetSize(Vector3 value)
        {
            sizeProp.SetValue(null, value);
        }
    }
}