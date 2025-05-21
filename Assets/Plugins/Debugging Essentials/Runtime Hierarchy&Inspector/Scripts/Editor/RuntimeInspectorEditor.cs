using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DebuggingEssentials
{
    [CustomEditor(typeof(RuntimeInspector))]
    public class RuntimeInspectorEditor : Editor
    {
        SerializedProperty showOnStart;
        SerializedProperty enableCameraOnStart;
        SerializedProperty useSameEditorAsBuildShowKey;

        AdvancedKeyEditor gamePauseKey = new AdvancedKeyEditor();
        AdvancedKeyEditor showToggleKeyEditor = new AdvancedKeyEditor();
        AdvancedKeyEditor showToggleKeyBuild = new AdvancedKeyEditor();
        AdvancedKeyEditor alignWithViewKey = new AdvancedKeyEditor();
        AdvancedKeyEditor moveToViewKey = new AdvancedKeyEditor();
        AdvancedKeyEditor focusCameraKey = new AdvancedKeyEditor();
        AdvancedKeyEditor followCameraKey = new AdvancedKeyEditor();

        SerializedProperty selectLayerMask;
        SerializedProperty unfoldMultipleScenesOnStart;

        void OnEnable()
        {
            GUIDraw.editorSkinMulti = EditorGUIUtility.isProSkin ? 1 : 0.35f;

            showOnStart = serializedObject.FindProperty("showOnStart"); 
            enableCameraOnStart = serializedObject.FindProperty("enableCameraOnStart");

            useSameEditorAsBuildShowKey = serializedObject.FindProperty("useSameEditorAsBuildShowKey");
            showToggleKeyEditor.Init(serializedObject, "showToggleKeyEditor");
            showToggleKeyBuild.Init(serializedObject, "showToggleKeyBuild");

            gamePauseKey.Init(serializedObject, "gamePauseKey");
            alignWithViewKey.Init(serializedObject, "alignWithViewKey");
            moveToViewKey.Init(serializedObject, "moveToViewKey");
            focusCameraKey.Init(serializedObject, "focusCameraKey");  
            followCameraKey.Init(serializedObject, "followCameraKey");

            selectLayerMask = serializedObject.FindProperty("selectLayerMask");

            unfoldMultipleScenesOnStart = serializedObject.FindProperty("unfoldMultipleScenesOnStart");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIDraw.DrawSpacer();
            GUIDraw.DrawHeader(Helper.GetGUIContent("VISIBLE"), Color.green);
            EditorGUILayout.PropertyField(showOnStart);
            EditorGUILayout.PropertyField(enableCameraOnStart);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("CONTROLS"), Color.blue);
            GUIDraw.DrawShowKey(useSameEditorAsBuildShowKey, showToggleKeyEditor, showToggleKeyBuild);
            GUIDraw.DrawAdvancedKey(gamePauseKey, "Game Pause Key");
            GUIDraw.DrawAdvancedKey(followCameraKey, "Follow Camera Key");
            GUIDraw.DrawAdvancedKey(focusCameraKey, "Focus Camera Key");
            GUIDraw.DrawAdvancedKey(alignWithViewKey, "Align With View Key");
            GUIDraw.DrawAdvancedKey(moveToViewKey, "Move To View Key");
            
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("CAMERA"), Color.yellow);
            EditorGUILayout.PropertyField(selectLayerMask);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("SETTINGS"), Color.red);
            EditorGUILayout.PropertyField(unfoldMultipleScenesOnStart);
            EditorGUILayout.EndVertical();
            GUIDraw.DrawSpacer();

            GUIDraw.DrawHeader(Helper.GetGUIContent("EVENTS"), Color.grey);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("onSetActive", "This event will execute when the Runtime Inspector will show/hide.\n\nYou can subscribe/unsubscribe to RuntimeInspector.onSetActive"));
            EditorGUILayout.LabelField(new GUIContent("(bool active)", ""));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("onTimeScaleChanged", "This event will execute when changing the time-scale slider.\n\nYou can subscribe/unsubscribe to RuntimeInspector.onTimeScaleChanged"));
            EditorGUILayout.LabelField(new GUIContent("(float timeScale)", ""));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUIDraw.DrawSpacer();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
