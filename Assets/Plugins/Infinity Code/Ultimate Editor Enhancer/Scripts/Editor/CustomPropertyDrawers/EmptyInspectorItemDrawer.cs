/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Text;
using InfinityCode.UltimateEditorEnhancer.InspectorTools;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(EmptyInspector.Item))]
    public class EmptyInspectorItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect r = new Rect(position.position, new Vector2(position.width, 18));

            SerializedProperty enabledProp = property.FindPropertyRelative("enabled");
            Rect r2 = new Rect(r) {width = 18};
            enabledProp.boolValue = EditorGUI.ToggleLeft(r2, TempContent.Get(String.Empty, "Enabled"), enabledProp.boolValue);

            r.xMin += 18;

            SerializedProperty titleProp = property.FindPropertyRelative("title");
            titleProp.stringValue = EditorGUI.TextField(r, "Title", titleProp.stringValue);

            r.y += 21;
            r2 = new Rect(r);
            const int buttonWidth = 20;
            r2.xMax -= buttonWidth;
            SerializedProperty pathProp = property.FindPropertyRelative("menuPath");
            pathProp.stringValue = EditorGUI.TextField(r2, "Path", pathProp.stringValue);

            r2 = new Rect(r);
            r2.xMin = r2.xMax - buttonWidth;
            if (GUI.Button(r2, "..."))
            {
                GenericMenuEx menu = GenericMenuEx.Start();

                string[] menuToString = EditorGUIUtility.SerializeMainMenuToString().Split('\n');
                string[] groups = new string[64];
                int prevLevel = -1;
                SerializedObject serializedObject = property.serializedObject;
                string propertyPath = property.propertyPath;

                StringBuilder builder = StaticStringBuilder.Start();

                for (int i = 0; i < menuToString.Length; i++)
                {
                    string s = menuToString[i];
                    string s2 = s.Trim();
                    if (string.IsNullOrEmpty(s2)) continue;

                    int lDiff = s.Length - s2.Length;
                    int level = lDiff / 4;

                    if (prevLevel >= level)
                    {
                        string menuItem = builder.ToString();
                        menu.Add(menuItem, () =>
                        {
                            serializedObject.Update();
                            SerializedProperty prop = serializedObject.FindProperty(propertyPath);
                            if (prop != null)
                            {
                                pathProp.stringValue = menuItem;
                                serializedObject.ApplyModifiedProperties();
                            }
                        });
                    }

                    prevLevel = level;
                    groups[level] = s2;

                    if (groups[0] == "CONTEXT") continue;

                    builder.Clear();

                    for (int j = 0; j <= level; j++)
                    {
                        if (j > 0) builder.Append("/");
                        builder.Append(groups[j]);
                    }
                }

                menu.Show();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 40;
        }
    }
}