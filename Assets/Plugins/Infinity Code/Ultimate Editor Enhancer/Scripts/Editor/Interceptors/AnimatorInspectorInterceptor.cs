/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class AnimatorInspectorInterceptor : StatedInterceptor<AnimatorInspectorInterceptor>
    {
        public static Action<Editor> OnInspectorGUI;

        protected override MethodInfo originalMethod
        {
            get => AnimatorInspectorRef.onInspectorGUIMethod;
        }

        public override bool state
        {
            get => Prefs.animatorInspectorClips;
        }

        protected override string postfixMethodName { get => nameof(OnInspectorGUIPostfix); }

        private static void OnInspectorGUIPostfix(Editor __instance)
        {
            if (OnInspectorGUI != null) OnInspectorGUI(__instance);
        }
    }
}