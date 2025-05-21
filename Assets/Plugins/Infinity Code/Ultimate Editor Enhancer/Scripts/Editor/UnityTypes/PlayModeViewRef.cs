/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class PlayModeViewRef
    {
        private static Type _type;
        private static FieldInfo _playModeViewsField;

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("PlayModeView");
                return _type;
            }
        }

        private static FieldInfo playModeViewsField
        {
            get
            {
                if (_playModeViewsField == null) _playModeViewsField = type.GetField("s_PlayModeViews", Reflection.StaticLookup);
                return _playModeViewsField;
            }
        }
        public static IList GetPlayModeViews()
        {
            return (IList)playModeViewsField.GetValue(null);
        }
    }
}