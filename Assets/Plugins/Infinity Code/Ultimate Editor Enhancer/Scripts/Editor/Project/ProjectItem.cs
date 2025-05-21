/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    public class ProjectItem
    {
        private const double CACHE_LIFETIME = 10;
        private static Dictionary<string, CacheItem> cache = new Dictionary<string, CacheItem>();
        
        public string guid;
        public string path;
        public Rect rect;
        public bool hovered;
        public bool isFolder;

        public Object asset
        {
            get
            {
                return ProjectAssetCache.Get<Object>(path);
            }
        }

        public void Set(string guid, Rect rect)
        {
            this.guid = guid;
            this.rect = rect;
            
            CacheItem item;
            Vector2 p = Event.current.mousePosition;
            hovered = rect.Contains(p);
            
            if (cache.TryGetValue(guid, out item))
            {
                path = item.path;
                isFolder = item.isFolder;
                return;
            }

            path = AssetDatabase.GUIDToAssetPath(guid);

            if (!string.IsNullOrEmpty(path))
            {
                FileAttributes attributes = File.GetAttributes(path);
                isFolder = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
            }
            else isFolder = false;
            
            item.path = path;
            item.isFolder = isFolder;
            cache[guid] = item;
        }

        private struct CacheItem
        {
            public string path;
            public bool isFolder;
        }
    }
}