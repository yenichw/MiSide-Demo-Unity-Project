/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class AutoSave
    {
        private static float lastSaveTime = 0;

        static AutoSave()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            if (!Prefs.saveScenesByTimer) return;
            if (EditorApplication.isPlaying) return;

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isDirty || string.IsNullOrEmpty(scene.path))
            {
                lastSaveTime = Time.realtimeSinceStartup;
                return;
            }

            if (Time.realtimeSinceStartup - lastSaveTime > Prefs.autosaveDelay)
            {
                Save();
                lastSaveTime = Time.realtimeSinceStartup;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (Prefs.saveScenesWhenEnteringPlaymode)
                {
                    Scene scene = SceneManager.GetActiveScene();
                    if (scene.isDirty && !string.IsNullOrEmpty(scene.path)) Save();
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                lastSaveTime = Time.realtimeSinceStartup;
            }
        }

        private static void Save()
        {
            if (!Prefs.autosaveToSeparateFile)
            {
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
            }
            else
            {
                Scene scene = SceneManager.GetActiveScene();
                string path = scene.path;
                path = path.Substring(0, path.Length - 6) + "_autosave.unity";
                EditorSceneManager.SaveScene(scene, path, true);
            }
        }
    }
}