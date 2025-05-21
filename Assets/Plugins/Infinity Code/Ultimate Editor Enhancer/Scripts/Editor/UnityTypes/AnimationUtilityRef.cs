/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class AnimationUtilityRef
    {
        private static MethodInfo _addInbetweenKeyMethod;
        private static MethodInfo _updateTangentsFromModeSurroundingMethod;

        private static MethodInfo addInbetweenKeyMethod
        {
            get
            {
                if (_addInbetweenKeyMethod == null) _addInbetweenKeyMethod = type.GetMethod("AddInbetweenKey", Reflection.StaticLookup, null, new[] { typeof(AnimationCurve), typeof(float) }, null);
                return _addInbetweenKeyMethod;
            }
        }

        public static Type type
        {
            get => typeof(AnimationUtility);
        }

        public static MethodInfo updateTangentsFromModeSurroundingMethod
        {
            get
            {
                if (_updateTangentsFromModeSurroundingMethod == null) _updateTangentsFromModeSurroundingMethod = type.GetMethod("UpdateTangentsFromModeSurrounding", Reflection.StaticLookup, null, new[] { typeof(AnimationCurve), typeof(int) }, null);
                return _updateTangentsFromModeSurroundingMethod;
            }
        }

        public static int AddInbetweenKey(AnimationCurve curve, float time)
        {
            return (int) addInbetweenKeyMethod.Invoke(null, new object[] {curve, time});
        }

        public static void UpdateTangentsFromModeSurrounding(AnimationCurve curve, int index)
        {
            updateTangentsFromModeSurroundingMethod.Invoke(null, new object[] {curve, index});
        }
    }
}