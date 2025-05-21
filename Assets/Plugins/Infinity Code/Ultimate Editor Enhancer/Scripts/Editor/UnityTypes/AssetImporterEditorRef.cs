/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class AssetImporterEditorRef
    {
        private static Type _type;

        public static Type type
        {
            get
            {
                if (_type == null)
                {
#if UNITY_2020_2_OR_NEWER
                    string name = "AssetImporters.AssetImporterEditor";
#else
                    string name = "Experimental.AssetImporters.AssetImporterEditor";
#endif
                    _type = Reflection.GetEditorType(name);
                }
                return _type;
            }
        }
    }
}