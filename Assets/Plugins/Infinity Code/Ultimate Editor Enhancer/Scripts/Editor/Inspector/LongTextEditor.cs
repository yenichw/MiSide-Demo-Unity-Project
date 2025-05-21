/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Diagnostics;
using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.Interceptors;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InfinityCode.UltimateEditorEnhancer.InspectorTools
{
    [InitializeOnLoad]
    public static class LongTextEditor
    {
        private const int ButtonWidth = 20;

        private static int targetID = int.MinValue;
        private static string restoreText;
        private static bool drawButton;
        private static Rect buttonRect;

        static LongTextEditor()
        {
            EditorGUIDoTextFieldInterceptor.OnPrefix += OnPrefix;
            EditorGUIDoTextFieldInterceptor.OnPostfix += OnPostfix;
        }

        private static void OnPrefix(TextEditor editor, int id, ref Rect position, ref string text, GUIStyle style, string allowedletters, ref bool changed, bool reset, bool multiline, bool passwordfield)
        {
            if (!Prefs.expandLongTextFields) return;

            drawButton = false;
            if (multiline || passwordfield || allowedletters != null || string.IsNullOrEmpty(text)) return;
            if (EditorStyles.textField.CalcSize(TempContent.Get(text)).x < position.width - 10) return;
            
            if (Prefs.longTextFieldsInVisualScripting)
            {
                StackTrace stackTrace = new StackTrace();
                for (int i = 3; i < stackTrace.FrameCount; i++)
                {
                    StackFrame frame = stackTrace.GetFrame(i);
                    MethodBase method = frame.GetMethod();
                    if (method == null) break;
                    if (method.DeclaringType.AssemblyQualifiedName.StartsWith("Unity.VisualScripting.")) return;
                }
            }

            drawButton = true;
            buttonRect = new Rect(position.xMax - ButtonWidth, position.y, ButtonWidth, position.height);
            position.width -= buttonRect.width + 2;
        }

        private static void OnPostfix(TextEditor editor, int id, ref Rect position, ref string text, GUIStyle style, string allowedletters, ref bool changed, bool reset, bool multiline, bool passwordfield)
        {
            if (!Prefs.expandLongTextFields) return;

            if (targetID == id && restoreText != null)
            {
                text = restoreText;
                editor.text = text;
                changed = true;
                targetID = int.MinValue;
                restoreText = null;
                GUI.changed = true;
            }

            if (!drawButton) return;
            if (GUI.Button(buttonRect, TempContent.Get(Icons.upDown, "Expand Text Field")))
            {
                targetID = id;
                LongTextEditorWindow.OpenWindow(text, new Rect(position.x - 3, position.y - 2, position.width + ButtonWidth + 5, position.height + 2)).OnClose += s => { restoreText = s; };
            }
        }
    }
}