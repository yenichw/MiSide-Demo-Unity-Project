using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DebuggingEssentials
{
    [CustomEditor(typeof(HtmlDebug))]
    public class HtmlDebugEditor : Editor
    {
        SerializedProperty deleteBuildLogs, deleteBuildLogsAfterDays, openLogManually;
        SerializedProperty normalLogOnlyFirstLineStackTrace;

        SerializedProperty titleFontSize, frameFontSize, logFontSize, stackFontSize;

        void OnEnable()
        {
            GUIDraw.editorSkinMulti = EditorGUIUtility.isProSkin ? 1 : 0.35f;

            deleteBuildLogs = serializedObject.FindProperty("deleteBuildLogs");
            deleteBuildLogsAfterDays = serializedObject.FindProperty("deleteBuildLogsAfterDays");
            openLogManually = serializedObject.FindProperty("openLogManually");
            normalLogOnlyFirstLineStackTrace = serializedObject.FindProperty("normalLogOnlyFirstLineStackTrace");

            titleFontSize = serializedObject.FindProperty("titleFontSize");
            frameFontSize = serializedObject.FindProperty("frameFontSize");
            logFontSize = serializedObject.FindProperty("logFontSize");
            stackFontSize = serializedObject.FindProperty("stackFontSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(5);
            GUIDraw.DrawSpacer();
            GUIDraw.DrawHeader(Helper.GetGUIContent("SETTINGS"), Color.yellow);
            EditorGUILayout.PropertyField(deleteBuildLogs);
            if (deleteBuildLogs.boolValue)
            {
                GUIDraw.PropertyField(deleteBuildLogsAfterDays, new GUIContent("After Days"), true);
            }
            EditorGUILayout.PropertyField(openLogManually);
            EditorGUILayout.PropertyField(normalLogOnlyFirstLineStackTrace);

            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();
            GUIDraw.DrawHeader(Helper.GetGUIContent("FONT SIZES"), Color.green);
            EditorGUILayout.PropertyField(titleFontSize);
            EditorGUILayout.PropertyField(frameFontSize);
            EditorGUILayout.PropertyField(logFontSize);
            EditorGUILayout.PropertyField(stackFontSize);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
