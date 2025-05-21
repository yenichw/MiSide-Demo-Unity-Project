/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class EditorGUIDoTextFieldInterceptor : StatedInterceptor<EditorGUIDoTextFieldInterceptor>
    {
        public delegate void DoTextFieldDelegate(TextEditor editor,
            int id,
            ref Rect position,
            ref string text,
            GUIStyle style,
            string allowedletters,
            ref bool changed,
            bool reset,
            bool multiline,
            bool passwordField);

        public static DoTextFieldDelegate OnPrefix;
        public static DoTextFieldDelegate OnPostfix;

        protected override MethodInfo originalMethod => EditorGUIRef.doTextFieldMethod;
        protected override string prefixMethodName => nameof(DoTextFieldPrefix);
        protected override string postfixMethodName => nameof(DoTextFieldPostfix);
        public override bool state => true;

        private static void DoTextFieldPrefix(
            TextEditor editor,
            int id,
            ref Rect position,
            string text,
            GUIStyle style,
            string allowedletters,
            ref bool changed,
            bool reset,
            bool multiline,
            bool passwordField
        )
        {
            if (OnPrefix != null) OnPrefix(editor, id, ref position, ref text, style, allowedletters, ref changed, reset, multiline, passwordField);
        }

        private static void DoTextFieldPostfix(
            ref string __result,
            TextEditor editor,
            int id,
            ref Rect position,
            string text,
            GUIStyle style,
            string allowedletters,
            ref bool changed,
            bool reset,
            bool multiline,
            bool passwordField)
        {
            if (OnPostfix != null)
            {
                OnPostfix(editor, id, ref position, ref __result, style, allowedletters, ref changed, reset, multiline, passwordField);
            }
        }
    }
}