/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Interceptors;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.InspectorTools
{
    [InitializeOnLoad]
    public static class AnimatorInspectorClips
    {
        private static GUIContent pauseContent;
        private static GUIContent playContent;
        private static GUIContent resumeContent;
        private static GUIContent selectContent;
        private static GUIContent stopContent;
        
        private static bool showClips = false;
        private static bool inited;
        private static int playIndex = -1;
        private static Animator animator;
        private static AnimationClip clip;
        private static double startTime;
        private static int frame;
        private static bool isPaused;
        private static Vector3 position;
        private static Quaternion rotation;
        private static Vector3 scale;

        static AnimatorInspectorClips()
        {
            AnimatorInspectorInterceptor.OnInspectorGUI += OnInspectorGUI;
        }

        private static void DrawClip(AnimationClip clip, int i, Animator animator)
        {
            if (clip == null) return;
            EditorGUILayout.BeginHorizontal();

            bool isPlayed = playIndex == i;
            if (isPlayed)
            {
                if (GUILayout.Button(stopContent, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    Stop();
                }
            }
            else
            {
                if (GUILayout.Button(playContent, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    Play(animator, clip, i);
                }
            }

            if (GUILayout.Button(selectContent, GUILayout.Width(30), GUILayout.Height(20)))
            {
                Selection.activeObject = clip;
            }

            EditorGUILayout.LabelField(clip.name, EditorStyles.textField);

            EditorGUILayout.EndHorizontal();

            if (isPlayed)
            {
                EditorGUILayout.BeginHorizontal();

                GUIContent content = isPaused ? resumeContent : pauseContent;

                if (GUILayout.Button(content, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    isPaused = !isPaused;
                }

                int total = Mathf.RoundToInt(clip.length * clip.frameRate);
                EditorGUI.BeginChangeCheck();
                frame = EditorGUILayout.IntSlider($"Frame ({frame}/{total})", frame, 0, total);
                if (EditorGUI.EndChangeCheck())
                {
                    isPaused = true;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void EditorUpdate()
        {
            float current;
            if (!isPaused)
            {
                current = (float) (EditorApplication.timeSinceStartup - startTime) % clip.length;
                frame = Mathf.RoundToInt(current * clip.frameRate);
            }
            else
            {
                startTime = EditorApplication.timeSinceStartup - frame / clip.frameRate;
                current = frame / clip.frameRate;
            }
            clip.SampleAnimation(animator.gameObject, current);

            Transform t = animator.transform;
            t.position = rotation * t.position + position;
            t.Rotate(rotation.eulerAngles);
        }

        private static void Init()
        {
            inited = true;
            pauseContent = new GUIContent(EditorIconContents.pauseButtonOn.image, "Pause");
            playContent = new GUIContent(EditorIconContents.playButtonOn.image, "Play");
            resumeContent = new GUIContent(EditorIconContents.playButtonOn.image, "Resume");
            stopContent = new GUIContent(Resources.LoadIcon("Stop"), "Stop");
            selectContent = new GUIContent(EditorIconContents.rectTransformBlueprint.image, "Select");
        }

        private static void OnInspectorGUI(Editor editor)
        {
            if (!Prefs.animatorInspectorClips) return;
            if (EditorApplication.isPlaying) return;
            if (editor.targets.Length != 1) return;

            Animator animator = editor.target as Animator;
            if (animator.runtimeAnimatorController == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;
            showClips = EditorGUILayout.Foldout(showClips, "Clips");
            EditorGUI.indentLevel--;
            if (!showClips)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            if (!inited) Init();

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

            for (int i = 0; i < clips.Length; i++)
            {
                DrawClip(clips[i], i, animator);
            }

            EditorGUILayout.EndVertical();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            Stop();
        }

        private static void Play(Animator _animator, AnimationClip _clip, int index)
        {
            playIndex = index;
            animator = _animator;
            clip = _clip;
            position = _animator.transform.position;
            rotation = _animator.transform.rotation;
            scale = _animator.transform.localScale;

            startTime = EditorApplication.timeSinceStartup;
            frame = 0;
            isPaused = false;

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            Selection.selectionChanged -= Stop;
            Selection.selectionChanged += Stop;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void Stop()
        {
            if (clip == null) return;
            if (animator == null) return;

            animator.Rebind();
            animator.transform.position = position;
            animator.transform.rotation = rotation;
            animator.transform.localScale = scale;

            playIndex = -1;
            clip = null;
            animator = null;
            isPaused = false;

            Selection.selectionChanged -= Stop;
            EditorApplication.update -= EditorUpdate;
        }
    }
}