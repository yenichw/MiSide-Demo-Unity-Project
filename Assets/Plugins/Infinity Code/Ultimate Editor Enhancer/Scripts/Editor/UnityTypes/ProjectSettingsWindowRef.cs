/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ProjectSettingsWindowRef
    {
        private static Type _type;

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("ProjectSettingsWindow");
                return _type;
            }
        }
    }
}