/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class EditorResources
    {
        private static Texture _prefabTexture;

        public static Texture prefabTexture
        {
            get
            {
                if (_prefabTexture == null) _prefabTexture = EditorIconContents.prefab.image;
                return _prefabTexture;
            }
        }
    }
}