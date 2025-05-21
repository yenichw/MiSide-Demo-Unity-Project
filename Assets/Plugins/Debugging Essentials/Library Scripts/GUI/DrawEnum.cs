using System;
using System.Reflection;
using UnityEngine;

namespace DebuggingEssentials
{
    public class DrawEnum
    {
        static public bool draw;

        const float heightPadding = 4;
        const float addHeight = 2;

        float boxAlpha, backgroundAlpha;

        Enum enumField;
        GameObject go;
        object obj;
        MemberInfo member;
        GUISkin skin;
        Rect rect;
        string[] names;

        public static void ResetStatic()
        {
            draw = false;
        }

        public DrawEnum(GUISkin skin)
        {
            this.skin = skin;
        }

        public void DrawButton(object obj, string fieldName)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, RuntimeInspector.bindingFlags);

            object value = field.GetValue(obj);

            if (GUILayout.Button(value.ToString()))
            {
                // InitEnumWindow(obj, field, (Enum)value, rect);
            }

            if (Event.current.type == EventType.Repaint)
            {
                if (!draw) rect = GUILayoutUtility.GetLastRect();
            }
        }

        public void InitEnumWindow(object obj, MemberInfo member, Enum enumField, Vector2 pos)
        {
            go = null;
            this.obj = obj;
            this.member = member;
            this.enumField = enumField;

            names = Enum.GetNames(enumField.GetType());

            rect.position = pos;
            rect.height = 0;

            float maxWidth = 0;

            for (int i = 0; i < names.Length; i++)
            {
                Vector2 size = GUI.skin.button.CalcSize(new GUIContent(names[i]));
                if (size.x > maxWidth) maxWidth = size.x;
                rect.height += size.y + heightPadding;
            }

            rect.height += addHeight;

            rect.width = maxWidth + 10;

            if (names != null) draw = true;
        }

        public void InitEnumWindow(GameObject go, Vector2 pos)
        {
            this.go = go;

            rect.position = pos;
            rect.height = 0;

            float maxWidth = 0;

            FastList<string> list = new FastList<string>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != string.Empty)
                {
                    Vector2 size = GUI.skin.button.CalcSize(new GUIContent(layerName));
                    list.Add(layerName);
                    if (size.x > maxWidth) maxWidth = size.x;
                    rect.height += size.y + heightPadding;
                }
            }

            rect.height += addHeight;
            rect.width = maxWidth + 10;

            names = list.ToArray();
            draw = true;
        }

        public void Draw(float boxAlpha, float backgroundAlpha)
        {
            if (!draw) return;

            this.boxAlpha = boxAlpha;
            this.backgroundAlpha = backgroundAlpha;

            GUI.skin = skin;
            GUILayout.Window(3443, rect, DrawWindow, GUIContent.none);
            if (Event.current.type == EventType.MouseDown) draw = false;
        }

        void DrawWindow(int windowId)
        {
            GUI.BringWindowToFront(windowId);
            Color color = new Color(0, 0, 0, boxAlpha);
            GUI.backgroundColor = color;

            GUILayout.BeginVertical("Box");
            GUI.backgroundColor = new Color(1, 1, 1, backgroundAlpha);

            GUILayout.Space(-1);

            for (int i = 0; i < names.Length; i++)
            {
                if (GUILayout.Button(names[i]))
                {
                    draw = false;

                    if (go != null)
                    {
                        go.layer = LayerMask.NameToLayer(names[i]);
                        go = null;
                    }
                    else
                    {
                        FieldInfo field = member as FieldInfo;
                        if (field != null) { field.SetValue(obj, i); return; }

                        PropertyInfo prop = member as PropertyInfo;
                        if (prop != null) prop.SetValue(obj, Enum.Parse(enumField.GetType(), names[i]), null);

                        obj = null;
                        member = null;
                        enumField = null;
                    }
                }
            }
        }
    }
}