using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DebuggingEssentials
{
    [CustomEditor(typeof(RuntimeConsole))]
    public class RuntimeConsoleEditor : Editor
    {
        SerializedProperty showConsoleOnStart, showConsoleOnWarning, showConsoleOnError, showConsoleOnException, showConsoleWhenInvokingMethod, disableUnityDevelopmentConsole;
        SerializedProperty useSameEditorAsBuildShowKey;
        AdvancedKeyEditor showToggleKeyEditor = new AdvancedKeyEditor();
        AdvancedKeyEditor showToggleKeyBuild = new AdvancedKeyEditor();
        SerializedProperty ignoreCasesInCommands;
        SerializedProperty ignoreCasesInAutoCompleteInput;
        SerializedProperty adminModeInBuild, adminModeConsoleCommand, specialModeInBuild, specialModeConsoleCommand, testOnlyFreeConsoleCommands;
        SerializedProperty searchCommandPrefix, ignoreCasesInSearch;
        SerializedProperty executeOnAllPrefix, executeOnlyOnRemotePrefix;
        SerializedProperty titleFontSize, frameFontSize, logFontSize, stackFontSize;


        void OnEnable()
        {
            GUIDraw.editorSkinMulti = EditorGUIUtility.isProSkin ? 1 : 0.35f;

            showConsoleOnStart = serializedObject.FindProperty("showConsoleOnStart");
            showConsoleOnWarning = serializedObject.FindProperty("showConsoleOnWarning");
            showConsoleOnError = serializedObject.FindProperty("showConsoleOnError");
            showConsoleOnException = serializedObject.FindProperty("showConsoleOnException");
            showConsoleWhenInvokingMethod = serializedObject.FindProperty("showConsoleWhenInvokingMethod");
            disableUnityDevelopmentConsole = serializedObject.FindProperty("disableUnityDevelopmentConsole");

            useSameEditorAsBuildShowKey = serializedObject.FindProperty("useSameEditorAsBuildShowKey");
            showToggleKeyEditor.Init(serializedObject, "showToggleKeyEditor");
            showToggleKeyBuild.Init(serializedObject, "showToggleKeyBuild");

            ignoreCasesInCommands = serializedObject.FindProperty("ignoreCasesInCommands");
            ignoreCasesInAutoCompleteInput = serializedObject.FindProperty("ignoreCasesInAutoCompleteInput");

            adminModeInBuild = serializedObject.FindProperty("adminModeInBuild");
            adminModeConsoleCommand = serializedObject.FindProperty("adminModeConsoleCommand");

            specialModeInBuild = serializedObject.FindProperty("specialModeInBuild");
            specialModeConsoleCommand = serializedObject.FindProperty("specialModeConsoleCommand");

            testOnlyFreeConsoleCommands = serializedObject.FindProperty("testOnlyFreeConsoleCommands");

            searchCommandPrefix = serializedObject.FindProperty("searchCommandPrefix");
            ignoreCasesInSearch = serializedObject.FindProperty("ignoreCasesInSearch");

            executeOnAllPrefix = serializedObject.FindProperty("executeOnAllPrefix");
            executeOnlyOnRemotePrefix = serializedObject.FindProperty("executeOnlyOnRemotePrefix");

            titleFontSize = serializedObject.FindProperty("titleFontSize");
            frameFontSize = serializedObject.FindProperty("frameFontSize");
            logFontSize = serializedObject.FindProperty("logFontSize");
            stackFontSize = serializedObject.FindProperty("stackFontSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIDraw.DrawSpacer();
            GUIDraw.DrawHeader(Helper.GetGUIContent("VISIBLE"), Color.green);
            EditorGUILayout.LabelField("Show Console");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(showConsoleOnStart, Helper.GetGUIContent("On Start"));
            EditorGUILayout.PropertyField(showConsoleOnWarning, Helper.GetGUIContent("On Warning"));
            EditorGUILayout.PropertyField(showConsoleOnError, Helper.GetGUIContent("On Error"));
            EditorGUILayout.PropertyField(showConsoleOnException, Helper.GetGUIContent("On Exception"));
            EditorGUILayout.PropertyField(showConsoleWhenInvokingMethod, Helper.GetGUIContent("When Invoking Method", "Show the Console when invoking a method from the Runtime Inspector"));
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(disableUnityDevelopmentConsole);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("CONTROLS"), Color.cyan);
            GUIDraw.DrawShowKey(useSameEditorAsBuildShowKey, showToggleKeyEditor, showToggleKeyBuild);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("SETTINGS"), Color.blue);
            EditorGUILayout.PropertyField(ignoreCasesInCommands);
            EditorGUILayout.PropertyField(ignoreCasesInAutoCompleteInput);
            EditorGUILayout.PropertyField(adminModeInBuild);
            EditorGUILayout.PropertyField(adminModeConsoleCommand);
            EditorGUILayout.PropertyField(specialModeInBuild);
            EditorGUILayout.PropertyField(specialModeConsoleCommand);
            EditorGUILayout.PropertyField(testOnlyFreeConsoleCommands);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("SEARCH CONSOLE"), Color.yellow);
            EditorGUILayout.PropertyField(searchCommandPrefix);
            EditorGUILayout.PropertyField(ignoreCasesInSearch);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("NETWORK COMMAND PREFIX"), Color.red);
            EditorGUILayout.PropertyField(executeOnAllPrefix);
            EditorGUILayout.PropertyField(executeOnlyOnRemotePrefix);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("FONT SIZES"), Color.magenta);
            EditorGUILayout.PropertyField(titleFontSize);
            EditorGUILayout.PropertyField(frameFontSize);
            EditorGUILayout.PropertyField(logFontSize);
            EditorGUILayout.PropertyField(stackFontSize);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("EVENTS"), Color.grey);

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("onSetActive", "This event will execute when the Runtime Console will show/hide.\n\nYou can subscribe/unsubscribe to RuntimeConsole.onSetActive"));
            EditorGUILayout.LabelField(new GUIContent("(bool active)", ""));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("onRemoteCommand", "This event will execute when using the `Network command prefixes`.\n\nYou can subscribe/unsubscribe to RuntimeConsole.onRemoteCommand"));
            EditorGUILayout.LabelField(new GUIContent("(string command)", ""));
            EditorGUILayout.EndHorizontal();
            GUIDraw.DrawSpacer();


        serializedObject.ApplyModifiedProperties();
        }
    }
}
