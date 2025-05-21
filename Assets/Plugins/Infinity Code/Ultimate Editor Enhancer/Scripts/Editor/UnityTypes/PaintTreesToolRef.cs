/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class PaintTreesToolRef
    {
        private static PropertyInfo _brushSizeProp;
        private static Type _type;
        private static PropertyInfo _instanceProp;

        private static PropertyInfo brushSizeProp
        {
            get
            {
                if (_brushSizeProp == null) _brushSizeProp = type.GetProperty("brushSize", Reflection.InstanceLookup);
                return _brushSizeProp;
            }
        }

        private static PropertyInfo instanceProp
        {
            get
            {
                if (_instanceProp == null)
                {
                    Type ssType = typeof(ScriptableSingleton<>);
                    Type[] typeArgs = { type };
                    Type t = ssType.MakeGenericType(typeArgs);
                    _instanceProp = t.GetProperty("instance", Reflection.StaticLookup);
                }

                return _instanceProp;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null)
                {
#if UNITY_2021_2_OR_NEWER
                    _type = Reflection.GetEditorType("TerrainTools.PaintTreesTool"); 
#else
                    _type = Reflection.GetEditorType("Experimental.TerrainAPI.PaintTreesTool");
#endif
                }
                return _type;
            }
        }

        public static float GetBrushSize(object instance)
        {
            return (float)brushSizeProp.GetValue(instance);
        }

        public static object GetInstance()
        {
            return instanceProp.GetValue(null);
        }

        public static void SetBrushSize(object instance, float size)
        {
            brushSizeProp.SetValue(instance, size);
        }
    }
}