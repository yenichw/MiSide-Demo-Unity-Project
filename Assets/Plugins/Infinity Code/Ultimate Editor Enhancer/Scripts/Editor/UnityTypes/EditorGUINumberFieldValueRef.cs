/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
#if UNITY_2021_2_OR_NEWER
    public static class EditorGUINumberFieldValueRef
    {
        private static Type _type;

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("EditorGUI+NumberFieldValue");
                return _type;
            }
        }
    }
#endif
}