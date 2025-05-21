/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class AssetPreviewRef
    {
        private static Type _type;
        private static MethodInfo _getMiniTypeThumbnailFromClassIDMethod;

        public static Type type
        {
            get => typeof(AssetPreview);
        }

        private static MethodInfo getMiniTypeThumbnailFromClassIDMethod
        {
            get
            {
                if (_getMiniTypeThumbnailFromClassIDMethod == null)
                {
                    _getMiniTypeThumbnailFromClassIDMethod = Reflection.GetMethod(type, "GetMiniTypeThumbnailFromClassID", new[] { typeof(int) }, Reflection.StaticLookup);
                }

                return _getMiniTypeThumbnailFromClassIDMethod;
            }
        }

        public static Texture2D GetMiniTypeThumbnailFromClassID(int classID)
        {
            return getMiniTypeThumbnailFromClassIDMethod.Invoke(null, new object[]
            {
                classID
            }) as Texture2D;
        }
    }
}