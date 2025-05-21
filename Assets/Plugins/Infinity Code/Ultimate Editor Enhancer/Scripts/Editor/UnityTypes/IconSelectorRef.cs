/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class IconSelectorRef
    {
        private static Type _type;
        private static MethodInfo _showAtPositionMethod;

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("IconSelector");
                return _type;
            }
        }

        private static MethodInfo showAtPositionMethod
        {
            get
            {
                if (_showAtPositionMethod == null) _showAtPositionMethod = type.GetMethod("ShowAtPosition", Reflection.StaticLookup, null, new[] { typeof(Object), typeof(Rect), typeof(bool) }, null);
                return _showAtPositionMethod;
            }
        }

        public static bool ShowAtPosition(Object target, Rect rect, bool showLabelIcons)
        {
            return (bool)showAtPositionMethod.Invoke(null, new object[] {target, rect, showLabelIcons});
        }
    }
}