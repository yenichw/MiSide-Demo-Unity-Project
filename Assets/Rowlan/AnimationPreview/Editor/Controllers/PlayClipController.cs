using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.AnimationPreview
{
    public class PlayClipController
    {
        private AnimationPreviewEditor editor;
        private AnimationPreview editorTarget;

        public PlayClipController(AnimationPreviewEditor editor)
        {
            this.editor = editor;
            this.editorTarget = (AnimationPreview)editor.target;
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Play Clip Controls", GUIStyles.BoxTitleStyle);

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Previous"))
                    {
                        editor.GetClipManager().PreviousClip();
                    }
                    if (GUILayout.Button("Next"))
                    {
                        editor.GetClipManager().NextClip();
                    }

                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    {   // Play button has special background color handling
                        GUI.backgroundColor = editor.GetClipManager().IsPlaying() ? GUIStyles.PlayBackgroundColor : GUIStyles.DefaultBackgroundColor;
                        if (GUILayout.Button("Play"))
                        {
                            editor.GetClipManager().PlayClip();
                        }
                        GUI.backgroundColor = GUIStyles.DefaultBackgroundColor;
                    }

                    if (GUILayout.Button("Reset"))
                    {
                        editor.GetClipManager().ResetClip();
                    }
                    if (GUILayout.Button("Stop"))
                    {
                        editor.GetClipManager().StopClip();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}