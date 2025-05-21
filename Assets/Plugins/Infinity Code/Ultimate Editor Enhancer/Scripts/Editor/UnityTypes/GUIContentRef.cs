/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class GUIContentRef
    {
        private static MethodInfo _tempContentMethodTexture;

        private static MethodInfo tempContentMethodTexture
        {
            get
            {
                if (_tempContentMethodTexture == null) _tempContentMethodTexture = Reflection.GetMethod(type, "Temp", new[] { typeof(Texture) });
                return _tempContentMethodTexture;
            }
        }

        private static MethodInfo _tempContentMethodString;

        private static MethodInfo tempContentMethodString
        {
            get
            {
                if (_tempContentMethodString == null) _tempContentMethodString = Reflection.GetMethod(type, "Temp", new[] { typeof(string) });
                return _tempContentMethodString;
            }
        }

        public static Type type
        {
            get => typeof(GUIContent);
        }

        public static GUIContent TempContent(string label)
        {
            return (GUIContent)tempContentMethodString.Invoke(null, new object[] { label });
        }

        public static GUIContent TempContent(Texture texture)
        {
            return (GUIContent)tempContentMethodTexture.Invoke(null, new object[] {texture});
        }
    }
}