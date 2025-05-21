using UnityEditor;
using UnityEngine;

namespace Rowlan.AnimationPreview
{
    /// <summary>
    /// Animation preview editor which allows you to select clips of an Animator and play them inside the Unity Editor.
    /// </summary>
    [ExecuteInEditMode]
    [CustomEditor(typeof(AnimationPreview))]
    public class AnimationPreviewEditor : Editor
    {
        private AnimationPreviewEditor editor;
        private AnimationPreview editorTarget;

        private SerializedProperty mode;
        private SerializedProperty clipIndex;
        private SerializedProperty clipName;
        private SerializedProperty animator;
        private SerializedProperty controller;

        private AnimatorManager animatorManager;
        private ClipManager clipManager;

        private PlayClipController playClipController;

        public void OnEnable()
        {
            editor = this;
            editorTarget = (AnimationPreview)target;

            mode = serializedObject.FindProperty("mode");
            clipIndex = serializedObject.FindProperty("clipIndex");
            clipName = serializedObject.FindProperty("clipName");
            animator = serializedObject.FindProperty("animator");
            controller = serializedObject.FindProperty("controller");

            // managers
            animatorManager = new AnimatorManager(editor);
            clipManager = new ClipManager(editor);

            // controllers
            playClipController = new PlayClipController(editor);

            // try to get the animator from the gameobject if none is specified
            if ( !animatorManager.HasAnimator())
            {
                editorTarget.animator = editorTarget.GetComponent<Animator>();
            }
            
            if(animatorManager.HasAnimator() && !animatorManager.HasAnimatorController())
            {
                Debug.LogWarning( $"Runtime animator controller not found for animator {editorTarget.animator.name}");
            }

        }

        public void OnDisable()
        {
            clipManager.OnDisable();
        }

        #region Inspector
        public override void OnInspectorGUI()
        {
            editor.serializedObject.Update();

            /// 
            /// Info & Help
            /// 
            GUILayout.BeginVertical(GUIStyles.HelpBoxStyle);
            {
                EditorGUILayout.BeginHorizontal();


                if (GUILayout.Button("Asset Store", EditorStyles.miniButton, GUILayout.Width(120)))
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/tools/animation/animation-preview-224321");
                }

                if (GUILayout.Button("Documentation", EditorStyles.miniButton))
                {
                    Application.OpenURL("https://bit.ly/animationpreview-doc");
                }

                if (GUILayout.Button("Forum", EditorStyles.miniButton, GUILayout.Width(120)))
                {
                    Application.OpenURL("https://forum.unity.com/threads/released-animation-preview.1297551");
                }

                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUIStyles.AppTitleBoxStyle);
            {
                EditorGUILayout.LabelField("Animation Preview", GUIStyles.AppTitleBoxStyle, GUILayout.Height(30));

            }
            GUILayout.EndVertical();

            bool animatorChanged = false;

            // help
            EditorGUILayout.HelpBox( 
                "Play animator clips inside the Unity editor. Press Play or the clip button to play the selected animation. Press Stop to stop continuous playing."
                + "\n\n"
                + "Setup: Create an animator controller, drag animations into the controller, assign the controller to an animator of a gameobject and drag the gameobject into the Animator slot."
                , MessageType.Info);

            // data
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("Clip Data", GUIStyles.BoxTitleStyle);

                EditorGUI.BeginChangeCheck();
                {
                    GUI.backgroundColor = animatorManager.HasAnimator() && animatorManager.HasAnimatorController() ? GUIStyles.DefaultBackgroundColor : GUIStyles.ErrorBackgroundColor;
                    {
                        EditorGUILayout.PropertyField(animator);

                        // visualize the controller
                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(controller);
                        GUI.enabled = true;

                        if (!animatorManager.HasAnimator() || !animatorManager.HasAnimatorController())
                        {
                            EditorGUILayout.HelpBox("The animator must have a controller. Use a gameobject with an attached Animator and Controller.", MessageType.Error);
                        }

                    }
                    GUI.backgroundColor = GUIStyles.DefaultBackgroundColor;


                    if (!animatorManager.HasAnimatorController())
                    {
                        EditorGUILayout.HelpBox("Quick solution to get a preview of all animations:\n1. Create > Animator Controller\n2. Drag all animations into the controller\n3. Add controller to your animator ", MessageType.Info);
                    }

                }

                // stop clip in case the animator changes
                if (EditorGUI.EndChangeCheck())
                {
                    animatorChanged = true;

                    // update the controller
                    controller.objectReferenceValue = animatorManager.GetAnimatorController();
                }

                GUI.enabled = false;
                EditorGUILayout.PropertyField(clipIndex);
                EditorGUILayout.PropertyField(clipName);
                GUI.enabled = true;

            }
            EditorGUILayout.EndVertical();

            // control
            switch( editorTarget.mode)
            {
                case AnimationPreview.Mode.PlayClip:
                    playClipController.OnInspectorGUI();
                    break;

                default:
                    throw new System.Exception("Unsupported mode: " + editorTarget.mode);
            }

            // clip list
            clipManager.OnInspectorGUI();

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            editor.serializedObject.ApplyModifiedProperties();

            if (animatorChanged)
            {
                clipManager.StopClip();

                // reset the 
                // set index to either -1 or the first index depending on the number of animations
                editorTarget.clipIndex = !animatorManager.HasAnimator() || !animatorManager.HasAnimatorController() || animatorManager.GetAnimatorController().animationClips.Length == 0 ? -1 : 0;

                clipManager.UpdateClipName();

                EditorUtility.SetDirty(target);

            }
        }
        #endregion Inspector

        /// <summary>
        /// Get the animator
        /// </summary>
        /// <returns></returns>
        public Animator GetAnimator()
        {
            if (animator == null)
                return null;

            return animator.objectReferenceValue as Animator;
        }

        public AnimatorManager GetAnimatorManager()
        {
            return animatorManager;
        }

        public ClipManager GetClipManager()
        {
            return clipManager;
        }

        private void ModeChanged(AnimationPreview.Mode mode)
        {
            clipManager.StopClip();
        }


    }
}

