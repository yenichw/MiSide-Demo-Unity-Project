/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class SpriteUtilityRef
    {
        private static MethodInfo _cleanUpMethod;
        private static Type _type;

        private static MethodInfo cleanUpMethod
        {
            get
            {
                if (_cleanUpMethod == null) _cleanUpMethod = type.GetMethod("CleanUp", Reflection.StaticLookup, null, new[] {typeof(bool)}, null);
                return _cleanUpMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("SpriteUtility");
                return _type;
            }
        }

        public static void CleanUp(bool deleteTempSceneObject)
        {
            cleanUpMethod.Invoke(null, new object[] {deleteTempSceneObject});
        }
    }
}