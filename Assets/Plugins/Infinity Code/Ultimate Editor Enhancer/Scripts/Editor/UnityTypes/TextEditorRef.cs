/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class TextEditorRef
    {
        public static Type type
        {
            get => typeof(TextEditor);
        }

        public static void SetText(string text)
        {
            SetText(EditorGUIRef.GetRecycledEditor(), text);
        }

        public static void SetText(object recycledEditor, string text)
        {
            (recycledEditor as TextEditor).text = text;
        }
    }
}