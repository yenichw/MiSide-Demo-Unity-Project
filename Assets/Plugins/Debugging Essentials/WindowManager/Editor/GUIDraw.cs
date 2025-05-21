using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace DebuggingEssentials
{ 
    public class AdvancedKeyEditor
    {
        public SerializedProperty advancedKey;
        public SerializedProperty specialKeys;
        public SerializedProperty key;

        public void Init(SerializedObject serializedObject, string keyWithSpecialKeysName)
        {
            advancedKey = serializedObject.FindProperty(keyWithSpecialKeysName);
            key = advancedKey.FindPropertyRelative("keyCode");
            specialKeys = advancedKey.FindPropertyRelative("specialKeys");
        }
    }

    static public class GUIDraw
    {
        public static float indentSpace = 12;
        public static float editorSkinMulti;
        
        public static void DrawHeader(GUIContent guiContent, Color color)
        {
            GUI.color = color * editorSkinMulti;
            EditorGUILayout.BeginVertical("Box");
            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal();
            LabelWidthUnderline(guiContent, 14);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        static public void DrawSpacer(float spaceBegin = 0, float height = 5, float spaceEnd = 0)
        {
            GUILayout.Space(spaceBegin);
            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(0.5f, 0.5f, 0.5f, 1);
            GUILayout.Button(string.Empty, GUILayout.Height(height));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(spaceEnd + 2);

            GUI.color = Color.white;
        }

        public static void PrefixAndLabel(GUIContent prefix, GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(prefix);
            EditorGUILayout.LabelField(label);
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawAdvancedKey(AdvancedKeyEditor advancedKey, string keyText)
        {
            int specialKeysValue = advancedKey.specialKeys.intValue;

            bool shift = (specialKeysValue & SpecialKeyFlags.shift) != 0;
            bool control = (specialKeysValue & SpecialKeyFlags.control) != 0;
            bool alt = (specialKeysValue & SpecialKeyFlags.alt) != 0;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(keyText);

            EditorGUILayout.LabelField("Shift", GUILayout.Width(30));
            shift = EditorGUILayout.Toggle(shift, GUILayout.Width(20));
            EditorGUILayout.LabelField("Ctrl", GUILayout.Width(30));
            control = EditorGUILayout.Toggle(control, GUILayout.Width(20));
            EditorGUILayout.LabelField("Alt", GUILayout.Width(30));
            alt = EditorGUILayout.Toggle(alt, GUILayout.Width(20));

            EditorGUILayout.PropertyField(advancedKey.key, GUIContent.none);

            EditorGUILayout.EndHorizontal();

            advancedKey.specialKeys.intValue = (shift ? SpecialKeyFlags.shift : 0) | (control ? SpecialKeyFlags.control : 0) | (alt ? SpecialKeyFlags.alt : 0);
        }

        public static void DrawShowKey(SerializedProperty useSameEditorAsBuildShowKey, AdvancedKeyEditor showToggleKeyEditor, AdvancedKeyEditor showToggleKeyBuild)
        {
            EditorGUILayout.PropertyField(useSameEditorAsBuildShowKey, Helper.GetGUIContent("Same Show Key Editor and Build", "Use the same Show Key in the Editor and in the Build"));
            if (useSameEditorAsBuildShowKey.boolValue)
            {
                DrawAdvancedKey(showToggleKeyEditor, "Show Toggle Key");
            }
            else
            {
                DrawAdvancedKey(showToggleKeyEditor, "Show Toggle Key Editor");
                DrawAdvancedKey(showToggleKeyBuild, "Show Toggle Key Build");
            }
        }

        static public void Label(string label, int fontSize)
        {
            int fontSizeOld = EditorStyles.label.fontSize;
            EditorStyles.boldLabel.fontSize = fontSize;
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Height(fontSize + 6));
            EditorStyles.boldLabel.fontSize = fontSizeOld;
        }

        static public void LabelWidthUnderline(GUIContent guiContent, int fontSize, bool boldLabel = true, bool drawUnderline = true)
        {
            int fontSizeOld = EditorStyles.label.fontSize;
            EditorStyles.boldLabel.fontSize = fontSize;
            EditorGUILayout.LabelField(guiContent, boldLabel ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.Height(fontSize + 6));
            EditorStyles.boldLabel.fontSize = fontSizeOld;
            if (drawUnderline) DrawUnderLine();
            GUILayout.Space(5);
        }

        static public void DrawUnderLine(float offsetY = 0)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            if (EditorGUIUtility.isProSkin) GUI.color = Color.grey; else GUI.color = Color.black;
            GUI.DrawTexture(new Rect(rect.x, rect.yMax + offsetY, rect.width, 1), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        static public void PropertyField(SerializedProperty property, GUIContent guiContent, bool indent = false)
        {
            EditorGUILayout.BeginHorizontal();
            if (indent) EditorGUI.indentLevel++;
            EditorGUILayout.PrefixLabel(guiContent);
            if (indent) EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(property, GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }

        static public bool Toggle(bool toggle, GUIContent guiContent, bool indent = false)
        {
            EditorGUILayout.BeginHorizontal();
            if (indent) EditorGUI.indentLevel++;
            EditorGUILayout.PrefixLabel(guiContent);
            if (indent) EditorGUI.indentLevel--;
            toggle = EditorGUILayout.Toggle(GUIContent.none, toggle);
            EditorGUILayout.EndHorizontal();
            return toggle;
        }

        static public Enum EnumPopup(Enum enumValue, GUIContent guiContent, bool indent = false)
        {
            EditorGUILayout.BeginHorizontal();
            if (indent) EditorGUI.indentLevel++;
            EditorGUILayout.PrefixLabel(guiContent);
            if (indent) EditorGUI.indentLevel--;
            enumValue = EditorGUILayout.EnumPopup(enumValue);
            EditorGUILayout.EndHorizontal();
            return enumValue;
        }

        static public void LayerField(SerializedProperty property, GUIContent guiContent, bool indent = false)
        {
            EditorGUILayout.BeginHorizontal();
            if (indent) EditorGUI.indentLevel++;
            EditorGUILayout.PrefixLabel(guiContent);
            if (indent) EditorGUI.indentLevel--;
            property.intValue = EditorGUILayout.LayerField(GUIContent.none, property.intValue);
            EditorGUILayout.EndHorizontal();
        }

        static public void PropertyArray(SerializedProperty property, GUIContent arrayName, bool drawUnderLine = true, bool editArrayLength = true)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            GUILayout.Space(0);
            Rect rect = GUILayoutUtility.GetLastRect();
            property.isExpanded = EditorGUI.Foldout(new Rect(rect.x, rect.y + 3, 30, 30), property.isExpanded, "");
            EditorGUILayout.PrefixLabel(Helper.GetGUIContent(arrayName.text + " Size", arrayName.tooltip));
            if (editArrayLength)
            {
                EditorGUI.indentLevel -= 2;
                property.arraySize = EditorGUILayout.IntField("", property.arraySize);
                EditorGUI.indentLevel += 2;
            }

            if (property.isExpanded)
            {
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < property.arraySize; i++)
                {
                    SerializedProperty elementProperty = property.GetArrayElementAtIndex(i);
                    PropertyField(elementProperty, Helper.GetGUIContent(i + "). Name"), true);
                }
            }
            else EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
    }
}