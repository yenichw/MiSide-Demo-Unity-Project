/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class PresetSelectorRef
    {
        private static MethodInfo _drawPresetButtonMethod;

        public static MethodInfo drawPresetButtonMethod
        {
            get
            {
                if (_drawPresetButtonMethod == null)
                {
                    Type[] parameters = {
                        typeof(Rect),
                        typeof(Object[])
                    };
                    _drawPresetButtonMethod = type.GetMethod("DrawPresetButton", Reflection.StaticLookup, null, parameters, null);
                }

                return _drawPresetButtonMethod;
            }
        }

        public static Type type
        {
            get => typeof(PresetSelector);
        }
    }
}