using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace Rowlan.AnimationPreview
{
    /// <summary>
    /// Preview animations of an Animator inside the Unity Editor
    /// 
    /// Usage:
    /// 
    ///   + create empty gameobject and attach this script to it
    ///   + drag a gameobject with an animator from the scene into the Animator slot
    ///   + use the control functions to play animations
    /// 
    /// </summary>
    public class AnimationPreview : MonoBehaviour
    {
#if UNITY_EDITOR

        public enum Mode
        {
            PlayClip,
        }

        [SerializeField]
        public Mode mode = Mode.PlayClip;

        [SerializeField]
        public int clipIndex = -1;

        [SerializeField]
        public string clipName = "";

        [SerializeField]
        public Animator animator;

        [SerializeField]
        public RuntimeAnimatorController controller;

#endif
    }
}