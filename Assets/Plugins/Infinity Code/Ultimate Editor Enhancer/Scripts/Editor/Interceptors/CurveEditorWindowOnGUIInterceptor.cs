/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class CurveEditorWindowOnGUIInterceptor : StatedInterceptor<CurveEditorWindowOnGUIInterceptor>
    {
        public static Action<EditorWindow> OnGUIAfter;
        public static Action<EditorWindow> OnGUIBefore;

        protected override MethodInfo originalMethod
        {
            get => CurveEditorWindowRef.onGUIMethod;
        }

        public override bool state
        {
            get => true;
        }

        protected override string prefixMethodName
        {
            get => nameof(OnGUIPrefix);
        }

        protected override string postfixMethodName
        {
            get => nameof(OnGUIPostfix);
        }

        private static void OnGUIPrefix(EditorWindow __instance)
        {
            if (OnGUIBefore != null) OnGUIBefore(__instance);
        }

        private static void OnGUIPostfix(EditorWindow __instance)
        {
            if (OnGUIAfter != null) OnGUIAfter(__instance);
        }
    }
}