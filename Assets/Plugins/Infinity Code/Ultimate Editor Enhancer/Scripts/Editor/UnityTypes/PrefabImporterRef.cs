/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class PrefabImporterRef
    {
        private static Type _type;

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("PrefabImporter");
                return _type;
            }
        }
    }
}