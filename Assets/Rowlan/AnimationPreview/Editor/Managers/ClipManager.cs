using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.AnimationPreview
{
    public class ClipManager
    {
        private AnimationPreviewEditor editor;
        private AnimationPreview editorTarget;

        private AnimationClip previewClip;
        private bool isPlaying = false;

        public ClipManager(AnimationPreviewEditor editor)
        {
            this.editor = editor;
            this.editorTarget = (AnimationPreview) editor.target;

            UpdateClipName();
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("Clip List", GUIStyles.BoxTitleStyle);

                if (editor.GetAnimatorManager().HasAnimator() && editor.GetAnimatorManager().HasAnimatorController())
                {
                    AnimationClip[] clips = editor.GetAnimatorManager().GetAnimatorController().animationClips;
                    for (int i = 0; i < clips.Length; i++)
                    {
                        AnimationClip clip = clips[i];

                        bool isCurrentClip = i == editorTarget.clipIndex;

                        GUI.backgroundColor = isCurrentClip ? GUIStyles.SelectedClipBackgroundColor : GUIStyles.DefaultBackgroundColor;
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.PrefixLabel("Clip: " + i);

                                if (GUILayout.Button(clip.name))
                                {
                                    SetClip(i);
                                    ClipAction();
                                }

                                if (GUILayout.Button(EditorGUIUtility.IconContent("AnimationClip Icon", "Open Clip in Project"), GUIStyles.ToolbarButtonStyle))
                                {
                                    OpenClip(i);
                                }

                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        GUI.backgroundColor = GUIStyles.DefaultBackgroundColor;
                    }
                }

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Log Clips"))
                    {
                        LogClips();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        public bool IsPlaying()
        {
            return isPlaying;
        }

        public void OnDisable()
        {
            EditorApplication.update -= DoPreview;

        }

        #region Clip Navigation
        public void PreviousClip()
        {
            editorTarget.clipIndex--;
            editorTarget.clipIndex = GetValidClipIndex(editorTarget.clipIndex);

            ClipChanged();
        }

        public void NextClip()
        {
            editorTarget.clipIndex++;
            editorTarget.clipIndex = GetValidClipIndex(editorTarget.clipIndex);
            ClipChanged();
        }

        private void SetClip(int clipIndex)
        {
            editorTarget.clipIndex = GetValidClipIndex(clipIndex);
            ClipChanged();

        }

        /// <summary>
        /// Open the clip file in project view
        /// </summary>
        /// <param name="clipIndex"></param>
        private void OpenClip(int clipIndex)
        {

            AnimationClip clip = GetClip(clipIndex);

            if (!clip)
                return;

            Selection.activeObject = clip;

        }

        private void ClipChanged()
        {
            if (isPlaying)
                PlayClip();
            else
                ResetClip();

            UpdateClipName();
        }

        public void UpdateClipName()
        {
            AnimationClip clip = GetClipToPreview();

            editorTarget.clipName = clip == null ? "" : clip.name;

        }

        private int GetValidClipIndex(int clipIndex)
        {
            if (!editor.GetAnimatorManager().HasAnimator())
                return -1;

            int clipCount = editor.GetAnimatorManager().GetAnimatorController().animationClips.Length;

            // check if there are clips at all
            if (clipCount == 0)
            {
                return -1;
            }

            if (clipIndex < 0)
            {
                return clipCount - 1;
            }

            if (clipIndex >= clipCount)
            {
                return 0;
            }

            return clipIndex;

        }

        private AnimationClip GetClipToPreview()
        {
            int clipIndex = editorTarget.clipIndex;
            if (clipIndex == -1)
                return null;

            return GetClip(clipIndex);
        }

        private AnimationClip GetClip(int clipIndex)
        {
            if (!editor.GetAnimatorManager().HasAnimatorController())
                return null;

            AnimationClip[] clips = editor.GetAnimatorManager().GetAnimatorController().animationClips;

            if (clipIndex >= clips.Length)
                return null;

            AnimationClip clip = clips[clipIndex];

            return clip;
        }

        #endregion Clip Navigation


        #region Clip Control
        public void ClipAction()
        {
            switch(editorTarget.mode)
            {
                case AnimationPreview.Mode.PlayClip:
                    PlayClip();
                    break;

                default:
                    throw new System.Exception("Unsupported mode: " + editorTarget.mode);

            }
        }

        public void PlayClip()
        {
            isPlaying = true;

            previewClip = GetClipToPreview();
            ResetClip();

            EditorApplication.update -= DoPreview;
            EditorApplication.update += DoPreview;
        }

        void DoPreview()
        {
            if (!previewClip)
                return;

            if (!editor.GetAnimatorManager().HasAnimator())
                return;

            if (!isPlaying)
                return;

            previewClip.SampleAnimation(editorTarget.gameObject, Time.deltaTime);

            editor.GetAnimatorManager().GetAnimator().Update(Time.deltaTime);
        }

        public void ResetClip()
        {
            if (!previewClip)
                return;

            previewClip.SampleAnimation(editorTarget.gameObject, 0);

            Animator animator = editor.GetAnimatorManager().GetAnimator();
            animator.Play(previewClip.name, 0, 0f);
            animator.Update(0);

        }

        public void StopClip()
        {
            isPlaying = false;

            EditorApplication.update -= DoPreview;

            ResetClip();
        }

        public void SetFrame( int frame)
        {
            previewClip = GetClipToPreview();

            if (!previewClip)
                return;

            if (!editor.GetAnimatorManager().HasAnimator())
                return;

            if (IsPlaying())
                StopClip();

            Animator animator = editor.GetAnimatorManager().GetAnimator();
            AnimatorClipInfo[] animationClip = animator.GetCurrentAnimatorClipInfo(0);

            if (animationClip.Length == 0)
                return;

            int frameCount = (int)(animationClip[0].clip.length * animationClip[0].clip.frameRate);
            if (frame >= frameCount)
                frame = frameCount;

            if (frame < 0)
                frame = 0;

            // int currentFrameInClip = (int)(editor.GetAnimatorManager().GetAnimator().GetCurrentAnimatorStateInfo(0).normalizedTime * (animationClip[0].clip.length * animationClip[0].clip.frameRate));
            previewClip.SampleAnimation(editorTarget.gameObject, 0);

            float frameTime = frame / (float)frameCount;

            animator.speed = 1f;
            animator.Play(previewClip.name, 0, frameTime);
            animator.Update(0);

            // Debug.Log($"frameCount: {frameCount}, currentFrame: {frame}, frameTime: {frameTime}");

        }

        /// <summary>
        /// Number of frames in current clip
        /// </summary>
        /// <returns></returns>
        public int GetFrameCount()
        {
            previewClip = GetClipToPreview();

            if (!previewClip)
                return 0;

            AnimatorClipInfo[] animationClip = editor.GetAnimatorManager().GetAnimator().GetCurrentAnimatorClipInfo(0);

            if (animationClip.Length == 0)
                return 0;

            int frameCount = (int)(animationClip[0].clip.length * animationClip[0].clip.frameRate);

            return frameCount;
        }

        #endregion Clip Control

        #region Logging
        private void LogClips()
        {
            if (!editor.GetAnimatorManager().HasAnimator() || !editor.GetAnimatorManager().HasAnimatorController())
                return;

            AnimationClip[] clips = editor.GetAnimatorManager().GetAnimatorController().animationClips;

            string text = "Clips of " + editor.GetAnimatorManager().GetAnimator().name + ": " + clips.Length + "\n";

            for (int i = 0; i < clips.Length; i++)
            {
                AnimationClip clip = clips[i];

                text += string.Format("{0}: {1}\n", i, clip.name);
            }

            Debug.Log(text);

        }
        #endregion Logging
    }
}