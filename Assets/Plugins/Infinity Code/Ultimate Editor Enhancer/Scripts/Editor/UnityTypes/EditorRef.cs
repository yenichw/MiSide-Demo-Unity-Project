/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class EditorRef
    {
        private static MethodInfo _drawFoldoutInspectorMethod;
        private static MethodInfo _drawHeaderGUIMethod;
        private static PropertyInfo _targetTitleProp;

        public static MethodInfo drawFoldoutInspectorMethod
        {
            get
            {
                if (_drawFoldoutInspectorMethod == null) _drawFoldoutInspectorMethod = type.GetMethod("DrawFoldoutInspector", Reflection.StaticLookup, null, new []{typeof(UnityEngine.Object), typeof(Editor).MakeByRefType()}, null);
                return _drawFoldoutInspectorMethod;
            }
        }

        private static MethodInfo drawHeaderGUIMethod
        {
            get
            {
                if (_drawHeaderGUIMethod == null) _drawHeaderGUIMethod = type.GetMethod("DrawHeaderGUI", Reflection.StaticLookup, null, new[] { typeof(Editor), typeof(string), typeof(float) }, null);
                return _drawHeaderGUIMethod;
            }
        }

        private static PropertyInfo targetTitleProp
        {
            get
            {
                if (_targetTitleProp == null) _targetTitleProp = type.GetProperty("targetTitle", Reflection.InstanceLookup);
                return _targetTitleProp;
            }
        }

        private static Type type
        {
            get => typeof(Editor);
        }

        public static Rect DrawHeaderGUI(Editor editor)
        {
            return (Rect)drawHeaderGUIMethod.Invoke(null, new object[]{editor, GetTargetTitle(editor), 10});
            //return (Rect) Editor.DrawHeaderGUI(editor, editor.targetTitle, 10f);
        }

        private static string GetTargetTitle(Editor editor)
        {
            return (string) targetTitleProp.GetValue(editor);
        }
    }
}