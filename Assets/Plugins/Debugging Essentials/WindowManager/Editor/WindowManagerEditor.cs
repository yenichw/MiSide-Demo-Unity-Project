using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;

namespace DebuggingEssentials
{
    [CustomEditor(typeof(WindowManager))]
    public class WindowManagerEditor : Editor
    {
        SerializedProperty guiScale, guiScaleValue, guiScaleText;
        SerializedProperty useDontDestroyOnLoadScene, onlyUseViewToggleKeys, useCanvas;

        SerializedObject serializedWindowData;

        DEProducts products = new DEProducts();

        void OnEnable() 
        {
            GUIDraw.editorSkinMulti = EditorGUIUtility.isProSkin ? 1 : 0.55f;

            SerializedProperty windowData = serializedObject.FindProperty("windowData");
            serializedWindowData = new SerializedObject(windowData.objectReferenceValue);

            guiScale = serializedWindowData.FindProperty("guiScale");
            guiScaleValue = guiScale.FindPropertyRelative("value");
            guiScaleText = guiScale.FindPropertyRelative("text");
            useDontDestroyOnLoadScene = serializedObject.FindProperty("useDontDestroyOnLoadScene");
            onlyUseViewToggleKeys = serializedObject.FindProperty("onlyUseViewToggleKeys");
            useCanvas = serializedObject.FindProperty("useCanvas");
        }

        public override void OnInspectorGUI()
        {
            var windowManager = (WindowManager)target;

            serializedObject.Update();
            serializedWindowData.Update();

            DrawSpacer(5, 5, 0);
            products.Draw((MonoBehaviour)target);

            // base.OnInspectorGUI();
            // EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green * GUIDraw.editorSkinMulti;
            if (GUILayout.Button("Open Documentation"))
            {
                Application.OpenURL("http://www.terraincomposer.com/de-documentation/");
            }
            GUI.backgroundColor = Color.white;
            DrawSpacer(2, 5, 1);
            GUI.backgroundColor = Color.magenta * GUIDraw.editorSkinMulti;
            if (GUILayout.Button(new GUIContent("Quick Discord Support", "With instant chatting I'm able to give very fast support.\nWhen I'm online I try to answer right away.")))
            {
                Application.OpenURL("https://discord.gg/HhepjD9");
            }
            GUI.backgroundColor = Color.white;
            // EditorGUILayout.EndHorizontal();
            DrawSpacer(2, 5, 1);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Editor Logs"))
            {
                EditorUtility.RevealInFinder(Helper.GetConsoleLogPath());
                // Application.OpenURL("file://" + Application.consoleLogPath);
            }
            if (GUILayout.Button("Open Build Logs"))
            {
                // EditorUtility.RevealInFinder(Application.persistentDataPath + "/Player.log");
                Application.OpenURL("file://" + Application.persistentDataPath);
            }
            GUILayout.EndHorizontal();
            DrawSpacer(2, 5, 2);

            GUIDraw.DrawHeader(Helper.GetGUIContent("SETTINGS"), Color.green);
            GUI.changed = false;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Only Use View Toggle Keys", "This disables all other keys except the ones to display the Runtime Console and Runtime Inspector."));
            EditorGUILayout.PropertyField(onlyUseViewToggleKeys, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Use DontDestroyOnLoad Scene", "This will put this Debugging Essentials GameObject as don't destroy on load and will display the 'DontDestroyOnLoad' Scane."));
            EditorGUILayout.PropertyField(useDontDestroyOnLoadScene, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                if (Application.isPlaying && useDontDestroyOnLoadScene.boolValue)
                {
                    DontDestroyOnLoad(windowManager.gameObject);
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Use Canvas", "This will draw the OnGUI on a canvas so that it can be used in VR and Hololens."));
            GUI.changed = false;
            EditorGUILayout.PropertyField(useCanvas, GUIContent.none);
            if (GUI.changed)
            {
                windowManager.UpdateUseCanvas(useCanvas.boolValue); 
            }
            EditorGUILayout.EndHorizontal();

            GUI.changed = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("GUI Scale", "GUI scale of the Runtime Windows"));
            EditorGUILayout.Slider(guiScaleValue, 0.55f, 10f, GUIContent.none);
            if (GUI.changed)
            {
                guiScaleText.stringValue = guiScaleValue.floatValue.ToString();
            }
            if (GUILayout.Button(new GUIContent("R", "Reset the scale to 1"), GUILayout.Width(25)))
            {
                guiScaleValue.floatValue = 1;
                guiScaleText.stringValue = "1";
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            DrawSpacer(2, 5, 2);

            serializedWindowData.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }

        static public void DrawSpacer(float spaceBegin = 5, float height = 5, float spaceEnd = 5)
        {
            GUILayout.Space(spaceBegin - 1);
            EditorGUILayout.BeginHorizontal();
            GUI.color = new Color(0.5f, 0.5f, 0.5f, 1);
            GUILayout.Button("", GUILayout.Height(height));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(spaceEnd);

            GUI.color = Color.white;
        }
    }
}