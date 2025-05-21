/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class CurveEditorWindowGetCurveEditorInterceptor : StatedInterceptor<CurveEditorWindowGetCurveEditorInterceptor>
    {
        public static Func<EditorWindow, Rect> OnGetCurveEditorRect;

        protected override MethodInfo originalMethod
        {
            get => CurveEditorWindowRef.getCurveEditorRectMethod;
        }

        public override bool state
        {
            get => true;
        }

        protected override string prefixMethodName
        {
            get => nameof(GetCurveEditorRectPrefix);
        }

        private static bool GetCurveEditorRectPrefix(EditorWindow __instance, ref Rect __result)
        {
            if (OnGetCurveEditorRect != null)
            {
                Rect rect = OnGetCurveEditorRect(__instance);
                if (rect != default)
                {
                    __result = rect;
                    return false;
                }
            }

            return true;
        }
    }
}