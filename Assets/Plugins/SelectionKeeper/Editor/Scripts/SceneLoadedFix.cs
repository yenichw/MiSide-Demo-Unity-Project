// Copyright (C) 2018 KAMGAM e.U. - All rights reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEngine.SceneManagement;


namespace kamgam.editor
{
    /// <summary>
    /// Fixes a bug in Unity 5.5 and older which causes SceneManager.sceneLoaded
    /// to execute even if the scene is not yet fully loaded.
    /// Bug: https://issuetracker.unity3d.com/issues/onsceneloaded-always-called-with-scene-dot-isloaded-equals-equals-false
    /// </summary>
    [InitializeOnLoad]
    public class SceneLoadedFix
    {
        public static Action<Scene, LoadSceneMode> sceneLoaded;

        static SceneLoadedFix()
        {
            // We've got to wait for the scene to actually load (bug in unity < 5.6.0)
            SceneManager.sceneLoaded += onSceneLoaded;
        }

#if UNITY_5_6_OR_NEWER
        static void onSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (sceneLoaded != null)
            {
                sceneLoaded(scene, sceneMode);
            }
        }
#else
        /// <summary>
        /// Tries to restore the selection once a new scene is loaded.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="sceneMode"></param>
        static void onSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            Coroutines.StartCoroutine(WaitForSceneToLoad(scene, sceneMode));
        }

        static IEnumerator WaitForSceneToLoad(Scene scene, LoadSceneMode sceneMode)
        {
            yield return new WaitUntil(() => scene.isLoaded);
            if (sceneLoaded != null)
            {
                sceneLoaded(scene, sceneMode);
            }
        }
#endif
    }
}
#endif
