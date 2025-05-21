/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class LongTextEditorWindow : EditorWindow
    {
        private string _text;
        private string originalText;

        public Action<string> OnClose;
        private GUIStyle style;
        private EventManager.EventBinding binding;

        private void OnDestroy()
        {
            EventManager.RemoveBinding(binding);
            if (OnClose != null) OnClose(_text);
        }

        private void OnEnable()
        {
            binding = EventManager.AddBinding(EventManager.ClosePopupEvent);
            binding.OnInvoke += b => Close();
        }

        private void OnGUI()
        {
            if (focusedWindow != this)
            {
                Close();
                return;
            }

            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    Close();
                    return;
                }

                if (e.keyCode == KeyCode.Escape)
                {
                    _text = originalText;
                    Close();
                    return;
                }
            }

            if (style == null)
            {
                style = new GUIStyle(EditorStyles.textField);
                style.wordWrap = true;
            }

            float textfieldWidth = position.width - 38;
            if (e.type == EventType.Repaint)
            {
                float height = style.CalcHeight(TempContent.Get(_text), textfieldWidth);
                Rect r = position;
                r.height = height + 4;
                position = r;
            }

            EditorGUILayout.BeginHorizontal();
            _text = EditorGUILayout.TextArea(_text, style, GUILayout.Width(textfieldWidth));
            if (GUILayout.Button("OK", GUILayout.Width(30)))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        public static LongTextEditorWindow OpenWindow(string text, Rect rect)
        {
            rect.position = GUIUtility.GUIToScreenPoint(rect.position);

            LongTextEditorWindow wnd = CreateInstance<LongTextEditorWindow>();
            wnd._text = wnd.originalText = text;
            wnd.ShowPopup();
            wnd.minSize = Vector2.one;
            wnd.position = rect;

            return wnd;
        }
    }
}