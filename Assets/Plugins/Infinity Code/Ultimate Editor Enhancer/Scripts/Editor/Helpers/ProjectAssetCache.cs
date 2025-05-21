/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class ProjectAssetCache
    {
        private static Dictionary<string, Object> assets = new Dictionary<string, Object>();
        
        public static T Get<T>(string path) where T : Object
        {
            Object asset;
            if (assets.TryGetValue(path, out asset)) return (T) asset;
            
            asset = AssetDatabase.LoadAssetAtPath<T>(path);
            assets[path] = asset;
            return (T) asset;
        }
    }
}