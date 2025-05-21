/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.Interceptors;
using InfinityCode.UltimateEditorEnhancer.PropertyDrawers;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.InspectorTools
{
    [InitializeOnLoad]
    public static class NestedEditor
    {
        private static readonly int NestedEditorHash = "NestedEditor".GetHashCode();
        public static bool disallowNestedEditors;

        private static Dictionary<int, bool> disallowCache;

        private static GUIContent content;
        private static Rect? contentArea;
        private static Object target;

        static NestedEditor()
        {
            disallowCache = new Dictionary<int, bool>();
            ObjectFieldDrawer.OnGUIBefore += OnGUIBefore;
            ObjectFieldDrawer.OnGUIAfter += OnGUIAfter;
        }

        private static void OnGUIBefore(Rect area, SerializedProperty property, GUIContent label)
        {
            contentArea = null;
            target = null;

            if (!Prefs.nestedEditors || disallowNestedEditors) return;

            Object obj = property.objectReferenceValue;
            if (obj == null) return;

            if (!ReorderableListInterceptor.insideList)
            {
                bool disallow;
                int id = property.serializedObject.targetObject.GetInstanceID() ^ property.propertyPath.GetHashCode();

                if (!disallowCache.TryGetValue(id, out disallow))
                {
                    Type type = property.serializedObject.targetObject.GetType();
                    FieldInfo field = type.GetField(property.name, Reflection.InstanceLookup);
                    if (field != null)
                    {
                        disallowCache[id] = disallow = field.GetCustomAttribute<DisallowNestedEditor>() != null;
                        if (disallow) return;
                    }
                    else disallowCache[id] = false;
                }
                else if (disallow) return;
            }

            if (Prefs.nestedEditorsSide == NestedEditorSide.left)
            {
                area.xMin += EditorGUI.indentLevel * 15 - 16;
            }
            else
            {
                area.xMin = area.xMax - 36;
                area.y += 1;
            }

            area.width = 16;

            contentArea = area;
            target = obj;

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && area.Contains(e.mousePosition))
            {
                if (target is Component) ComponentWindow.Show(target as Component, false).closeOnLossFocus = false;
                else if (target is GameObject) PropertyEditorRef.OpenPropertyEditor(target);
                else ObjectWindow.Show(new[] { target }, false).closeOnLossFocus = false;

                e.Use();
            }
        }

        private static void OnGUIAfter(Rect area, SerializedProperty property, GUIContent label)
        {
            if (!contentArea.HasValue) return;

            area = contentArea.Value;

            Color color = GUI.color;

            Event e = Event.current;
            Vector2 mousePosition = e.mousePosition;
            if (area.Contains(mousePosition))
            {
                GUI.color = Color.gray;
            }

            if (content == null)
            {
                content = new GUIContent(EditorIconContents.editIcon);
            }

            StringBuilder builder = StaticStringBuilder.Start();
            builder.Append("Open ");
            builder.Append(target.name);

            if (target is Component)
            {
                builder.Append(" (");
                builder.Append(ObjectNames.NicifyVariableName(target.GetType().Name));
                builder.Append(")");
            }

            builder.Append(" in window");

            content.tooltip = builder.ToString();

            int controlId = GUIUtility.GetControlID(NestedEditorHash, FocusType.Passive, area);
            if (e.type == EventType.Repaint) GUIStyle.none.Draw(area, content, controlId, false, area.Contains(e.mousePosition));

            GUI.color = color;
        }
    }
}