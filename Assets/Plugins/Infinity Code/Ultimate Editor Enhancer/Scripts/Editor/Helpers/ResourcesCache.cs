/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class ResourcesCache
    {
        private static Dictionary<string, object> _cache;

        private static Dictionary<string, object> cache
        {
            get
            {
                if (_cache == null) _cache = new Dictionary<string, object>();
                return _cache;
            }
        }

        public static Texture2D GetIcon(string filename, string ext = ".png")
        {
            string id = "Icons/" + filename + ext;
            object item;
            if (cache.TryGetValue(id, out item)) return item as Texture2D;
            Texture2D icon = Resources.LoadIcon(filename, ext);
            if (icon != null) cache.Add(id, icon);
            return icon;
        }
    }
}