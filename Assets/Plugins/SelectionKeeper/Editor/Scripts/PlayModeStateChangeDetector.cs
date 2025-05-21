// Copyright (C) 2018 KAMGAM e.U. - All rights reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace kamgam.editor
{
#if !UNITY_2017_2_OR_NEWER
    public enum PlayModeStateChange
    {
        EnteredEditMode = 0,
        ExitingEditMode = 1,
        EnteredPlayMode = 2,
        ExitingPlayMode = 3
    }
#endif

    /// <summary>
    /// PlayModeStateChange polyfill for old Unity versions.
    /// Uses EditorApplication.playModeStateChanged in Unity 2017.2+
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeStateChangeDetector
    {
#if UNITY_2017_2_OR_NEWER
        public static Action<PlayModeStateChange> playModeStateChanged;

        static PlayModeStateChangeDetector()
        {
            StartDetecting();
        }

        public static void StartDetecting()
        {
            EditorApplication.playModeStateChanged += onStateChanged;
        }

        public static void StopDetecting()
        {
            EditorApplication.playModeStateChanged -= onStateChanged;
        }

        private static void onStateChanged(PlayModeStateChange state)
        {
            if( playModeStateChanged != null )
            {
                playModeStateChanged(state);
            }
        }
#else
        public static Action<PlayModeStateChange> playModeStateChanged;

        protected enum State
        {
            EditMode = 1,
            PlayMode = 2
        }

        private static State? _currentState = null;

        static PlayModeStateChangeDetector()
        {
            StartDetecting();
        }

        public static void StartDetecting()
        {
            _currentState = getState();
            EditorApplication.update += update;
        }

        public static void StopDetecting()
        {
            EditorApplication.update -= update;
        }

        static void update()
        {
            State newState = getState();
            if(newState != _currentState.Value )
            {
                onStateChanged(_currentState.Value, newState);
                _currentState = getState();
            }
        }

        static State getState()
        {
            if( EditorApplication.isPlaying )
            {
                return State.PlayMode;
            }
            else
            {
                return State.EditMode;
            }
        }

        static void onStateChanged( State from, State to )
        {
            if (playModeStateChanged != null)
            {

                if (from == State.EditMode && to == State.PlayMode)
                {
                    playModeStateChanged(PlayModeStateChange.ExitingEditMode);
                    playModeStateChanged(PlayModeStateChange.EnteredPlayMode);
                }
                else if (from == State.PlayMode && to == State.EditMode)
                {
                    playModeStateChanged(PlayModeStateChange.ExitingPlayMode);
                    playModeStateChanged(PlayModeStateChange.EnteredEditMode);
                }
                else
                {
                    // what the heck?
                    Debug.Log("kamgam.editor.PlayModeStateChangeDetector detected unknow state change.");
                }
            }
        }
#endif
    }
}
#endif