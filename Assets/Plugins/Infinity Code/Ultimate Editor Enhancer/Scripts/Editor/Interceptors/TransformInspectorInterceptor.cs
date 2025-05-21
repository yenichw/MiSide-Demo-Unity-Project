/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class TransformInspectorInterceptor: StatedInterceptor<TransformInspectorInterceptor>
    {
        public static Func<Editor, bool> OnInspector3DPrefix;
        public static Action<Editor> OnInspector3DPostfix;

        protected override MethodInfo originalMethod
        {
            get => TransformInspectorRef.inspector3DMethod;
        }

        public override bool state
        {
            get => true;
        }

        protected override string prefixMethodName { get => nameof(Inspector3DPrefix); }
        protected override string postfixMethodName { get => nameof(Inspector3DPostfix); }

        private static bool Inspector3DPrefix(Editor __instance)
        {
            if (OnInspector3DPrefix != null) return OnInspector3DPrefix(__instance);
            return true;
        }
        
        private static void Inspector3DPostfix(Editor __instance)
        {
            if (OnInspector3DPostfix != null) OnInspector3DPostfix(__instance);
        }
    }
}