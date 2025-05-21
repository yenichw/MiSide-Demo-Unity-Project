/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class GUILayoutUtils
    {
        public static Rect lastRect;
        public static int buttonHash = "Button".GetHashCode();
        public static int toggleHash = "Toggle".GetHashCode();
        public static int hoveredButtonID;
        public static float nestedEditorMargin = 0;
        
        public static void BeginDisabledStyle(bool disabled)
        {
            if (Event.current.type == EventType.Repaint) EditorGUI.BeginDisabledGroup(disabled);
        }

        public static ButtonEvent Button(GUIContent content)
        {
            return Button(content, GUI.skin.button);
        }

        public static ButtonEvent Button(Texture texture, GUIStyle style, params GUILayoutOption[] options)
        {
            return Button(TempContent.Get(texture), style, options);
        }

        public static ButtonEvent Button(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            Rect rect = GUILayoutUtility.GetRect(content, style, options);
            lastRect = rect;
            return Button(rect, content, style);
        }

        public static ButtonEvent Button(Rect rect, GUIContent content, GUIStyle style)
        {
            int id = GUIUtility.GetControlID(buttonHash, FocusType.Passive, rect);

            Event e = Event.current;
            bool isHover = rect.Contains(e.mousePosition);
            bool hasMouseControl = GUIUtility.hotControl == id;

            if (e.type == EventType.Repaint)
            {
                style.Draw(rect, content, id, false, isHover);
                if (isHover) return ButtonEvent.hover;
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (hasMouseControl)
                {
                    GUIUtility.hotControl = 0;
                    return ButtonEvent.drag;
                }
            }
            else if (e.type == EventType.MouseMove)
            {
                if (isHover)
                {
                    hoveredButtonID = id;
                    return ButtonEvent.hover;
                }
            }
            else if (e.type == EventType.MouseDown)
            {
                if (isHover)
                {
                    Debug.unityLogger.logEnabled = false;
                    try
                    {
                        GUIUtility.hotControl = id;
                    }
                    catch
                    {
                    }

                    Debug.unityLogger.logEnabled = true;
                    e.Use();
                    return ButtonEvent.press;
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (hasMouseControl)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();

                    if (isHover)
                    {
                        GUI.changed = true;
                        return ButtonEvent.click;
                    }
                }

                return ButtonEvent.release;
            }

            return ButtonEvent.none;
        }

        public static void EndDisabledStyle()
        {
            if (Event.current.type == EventType.Repaint) EditorGUI.EndDisabledGroup();
        }

        public static void Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            Event e = Event.current;
            if (e.type != EventType.Repaint) return;
            Rect rect = GUILayoutUtility.GetRect(content, style, options);
            lastRect = rect;
            style.Draw(rect, content, false, false, false, false);
        }

        public static ButtonEvent ToggleButton(GUIContent content, GUIStyle style, bool isActive, params GUILayoutOption[] options)
        {
            Event e = Event.current;

            Rect rect = GUILayoutUtility.GetRect(content, style, options);
            int id = GUIUtility.GetControlID(buttonHash, FocusType.Passive, rect);
            bool isHover = rect.Contains(e.mousePosition);
            bool hasMouseControl = GUIUtility.hotControl == id;

            if (e.type == EventType.Repaint)
            {
                style.Draw(rect, content, id, false);
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (hasMouseControl)
                {
                    GUIUtility.hotControl = 0;
                    return ButtonEvent.drag;
                }
            }
            else if (e.type == EventType.MouseMove)
            {
                if (isHover) hoveredButtonID = id;
            }
            else if (e.type == EventType.MouseDown)
            {
                if (isHover)
                {
                    GUIUtility.hotControl = id;
                    e.Use();
                    return ButtonEvent.press;
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (hasMouseControl)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();

                    if (isHover)
                    {
                        GUI.changed = true;
                        return ButtonEvent.click;
                    }
                }

                return ButtonEvent.release;
            }

            return ButtonEvent.none;
        }

        public static bool ToggleButton(ref bool toggled, GUIContent content, GUIStyle toggleButtonStyle, params GUILayoutOption[] options)
        {
            Rect rect = GUILayoutUtility.GetRect(content, toggleButtonStyle, options);
            EditorGUI.BeginChangeCheck();
            toggled = GUI.Toggle(rect, toggled, content, toggleButtonStyle);
            return EditorGUI.EndChangeCheck();
        }

        public static bool ToolbarButton(string text)
        {
            return GUILayout.Button(text, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        }

        public static bool ToolbarButton(GUIContent content)
        {
            return GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
        }

        public static string ToolbarSearchField(string text)
        {
            return GUILayout.TextField(text, EditorStyles.toolbarSearchField);
        }
    }
}