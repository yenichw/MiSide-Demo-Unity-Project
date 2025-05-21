// Copyright (C) 2018 KAMGAM e.U. - All rights reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using kamgam.editor;

namespace kamgam.editor.selectionkeeper
{
    [InitializeOnLoad]
    public class SelectionKeeper
    {
        static int _ignoreNextSceneLoads;
        static bool _ignoreNextSelectionChange;

        static SelectionKeeper()
        {
            if (SelectionKeeper_Settings.instance == null)
            {
                Debug.LogWarning("SelectionKeeper plugin did not find any settings and will do nothing.\nPlease create them in a 'Resources' folder via Assets -> Create -> SelectionKeeper Settings.");
            }
            else
            {
                Selection.selectionChanged += onSelectionChanged;

                // ignore the first scene load, no need the perform autoselect there
                _ignoreNextSceneLoads = EditorSceneManager.loadedSceneCount;
                SceneLoadedFix.sceneLoaded += onSceneLoaded;

                PlayModeStateChangeDetector.playModeStateChanged += onPlayModeStateChanged;

                // initial load or recompile
                if (!EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.timeSinceStartup < 10f)
                {
                    if (SelectionKeeper_Settings.showLogMessages)
                    {
                        Debug.Log("SelectionKeeper: Startup cleaning after " + EditorApplication.timeSinceStartup + " seconds.");
                    }
                    ClearSelectionMemory();
                }
            }
        }

        static void onPlayModeStateChanged(PlayModeStateChange state)
        {
            // handle scene changes in between changes from Edit to Play mode.
            if (state.Equals(PlayModeStateChange.ExitingEditMode))
            {
                EditorPrefs.SetBool("SelectionKeeper._isInBetweenEditAndPlayMode", true);
            }
            else
            {
                EditorPrefs.SetBool("SelectionKeeper._isInBetweenEditAndPlayMode", false);
            }

            // default logic
            if ( state.Equals( PlayModeStateChange.EnteredEditMode ) )
            {
                if (SelectionKeeper_Settings.enablePlugin)
                {
                    // check if restore is necessary (selection empty)
                    if (Selection.gameObjects.Length == 0 || SelectionKeeper_Settings.ignoreSelectionInPlayMode)
                    {
                        LoadSelection();
                    }
                }

                // clear up the "handle scene changes" vars
                EditorPrefs.DeleteKey("SelectionKeeper._isInBetweenEditAndPlayMode");
                EditorPrefs.DeleteKey("SelectionKeeper._loadFinishedInBetweenEnterAndPlay");
            }
            // handle scene changes in between changes from Edit to Play mode.
            else if (state.Equals(PlayModeStateChange.EnteredPlayMode) && EditorPrefs.GetBool("SelectionKeeper._loadFinishedInBetweenEnterAndPlay", false) == true)
            {
                if (SelectionKeeper_Settings.enablePlugin)
                {
                    // check if restore is necessary (selection empty)
                    if (Selection.gameObjects.Length == 0)
                    {
                        LoadSelection();
                    }
                }
            }
        }

        /// <summary>
        /// Tries to restore the selection once a new scene is loaded.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="sceneMode"></param>
        static void onSceneLoaded( Scene scene, LoadSceneMode sceneMode )
        {
            if ( SelectionKeeper_Settings.enablePlugin && _ignoreNextSceneLoads-- <= 0 )
            {
                LoadSelection();
            }
        }

        [MenuItem("Tools/Selection Keeper/Clear Selection Memory", priority = 200)]
        static void ClearSelectionMemory()
        {
            EditorPrefs.DeleteKey("SelectionKeeper.selection");
            EditorPrefs.DeleteKey("SelectionKeeper.openedScenes");
        }

        [MenuItem("Tools/Selection Keeper/Clear Selection Memory", true, priority = 200)]
        public static bool ValidateClearSelectionMemory()
        {
            return SelectionKeeper_Settings.enablePlugin;
        }

        /// <summary>
        /// Tries to find and select the objects stored in EditorPrefs.SelectionKeeper.selection.
        /// </summary>
        // [MenuItem("Tools/Selection Keeper/LoadSelection")] // for testing purposes
        static void LoadSelection()
        {
            List<GameObject> newSelection = new List<GameObject>();

            var selectionPaths = deserializeSavedSelection();
            foreach( var transforms in selectionPaths )
            {
                // is scene loaded
                var scene = getLoadedSceneByGuid(transforms[1]);
                if ( scene.HasValue )
                {
                    // root objects of scene
                    var sameNameObjects = scene.Value.GetRootGameObjects().Where(go => go.name == transforms[2]).ToArray();
                    int sameNameSiblingIndex;
                    if (int.TryParse(transforms[3], out sameNameSiblingIndex)
                        && sameNameObjects.Count() > sameNameSiblingIndex)
                    {
                        var target = sameNameObjects[sameNameSiblingIndex].transform;
                        for (int i = 4; i < transforms.Length; i += 2)
                        {
                            var sameNameTransforms = getEquallyNamedChildren(target, transforms[i]);
                            int sameNameTransformIndex;
                            if ( int.TryParse(transforms[i + 1], out sameNameTransformIndex)
                                && sameNameTransforms.Count() > sameNameTransformIndex)
                            {
                                target = sameNameTransforms[sameNameTransformIndex];
                            }
                            else if( transforms.Length == i+2 )
                            {
                                target = null;
                                if (SelectionKeeper_Settings.showLogMessages)
                                {
                                    Debug.Log("SelectionKeeper: " + transforms[i] + " not found in path '" + string.Join("/", transforms) + "'.");
                                }
                            }
                        }

                        if( target != null )
                        {
                            newSelection.Add(target.gameObject);
                            EditorGUIUtility.PingObject(target.gameObject);
                        }
                    }
                    else
                    {
                        if (SelectionKeeper_Settings.showLogMessages)
                        {
                            Debug.Log("SelectionKeeper: " + transforms[3] + " not found in path '" + string.Join("/", transforms) + "'.");
                        }
                    }
                }
            }

            if( newSelection.Count() > 0 )
            {
                // handle scene changes in between changes from Edit to Play mode.
                EditorPrefs.SetBool("SelectionKeeper._loadFinishedInBetweenEnterAndPlay",
                    EditorPrefs.GetBool("SelectionKeeper._isInBetweenEditAndPlayMode", false)
                    );

                // update selection
                Selection.objects = newSelection.ToArray();

                // remove ping only in edit mode
                if (Application.isPlaying == false && SelectionKeeper_Settings.removePingEffect == true)
                {
                    removePingObjectEffect();
                }
                _ignoreNextSelectionChange = true; // otherwise this would trigger a selection save.
            }
        }

        static void removePingObjectEffect()
        {
            // undocumented feature thus wrapped in try/catch
            try
            {
                EditorGUIUtility.PingObject(-1); // -1 is not documented but seems to work as a silent ping (no highlight), null does not.
            }
            catch (Exception e)
            {
                if (SelectionKeeper_Settings.showLogMessages)
                {
                    Debug.Log("SelectionKeeper EditorGUIUtility.PingObject(-1) caused an error: " + e.Message + "\n" + e.StackTrace);
                }
            };
        }

        static List<string[]> deserializeSavedSelection()
        {
            var results = new List<string[]>();
            string selection = EditorPrefs.GetString("SelectionKeeper.selection", "");
            string[] selectionPaths = selection.Split('\n');
            foreach (var path in selectionPaths)
            {
                var transforms = path.Split('\t'); // sceneName, sceneGuid, rootObjName, rootObjSameNameSiblingIndex, childObjName, childObjSameNameSiblingIndex, ...
                if (transforms.Length > 3) // <= 3 are invalid
                {
                    results.Add(transforms);
                }
            }

            return results;
        }

        static List<Transform> getEquallyNamedChildren( Transform parent, string name )
        {
            var results = new List<Transform>();
            for( int i=0; i<parent.childCount; ++i )
            {
                if( parent.GetChild(i).name == name )
                {
                    results.Add(parent.GetChild(i));
                }
            }
            return results;
        }

        static Scene? getLoadedSceneByGuid(string sceneGuid)
        {
            if (SceneManager.sceneCount > 0)
            {
                for (int i = 0; i < SceneManager.sceneCount; ++i)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.isLoaded)
                    {
                        var guid = AssetDatabase.AssetPathToGUID(scene.path);
                        if (guid == sceneGuid)
                        {
                            return scene;
                        }
                    }
                }
            }

            return null;
        }

        static void onSelectionChanged()
        {
            if (SelectionKeeper_Settings.enablePlugin)
            {
                if (!_ignoreNextSelectionChange)
                {
                    // remember selection to reset on error
                    var selectedObjects = Selection.gameObjects;

                    try
                    {
                        if (EditorApplication.isPlaying == false || SelectionKeeper_Settings.ignoreSelectionInPlayMode == false)
                        {
                            if (Selection.gameObjects.Length > 0)
                            {
                                SaveSelection();
                            }
                            else
                            {
                                if (SelectionKeeper_Settings.clearSelectionMemoryIfEmpty == true)
                                {
                                    // deselect only in edit mode
                                    if (EditorApplication.isPlaying == false)
                                    {
                                        ClearSelectionMemory();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // reset selection in case of an error.
                        Selection.objects = selectedObjects;
                        if (SelectionKeeper_Settings.showLogMessages)
                        {
                            Debug.LogWarning("SelectionKeeper caused an unexpected Error:\n" + e.Message + "\n" + e.StackTrace);
                        }
                    }
                }
                _ignoreNextSelectionChange = false;
            }
        }

        /// <summary>
        /// Saves the current selection in EditorPlayerPrefs.SelectionKeeper.selection.
        /// If the Editor is in EditMode then the selection will be overwritten.
        /// If the Editor is in PlayMode then the selection will only be overwritten if objects from the edited scene are selected.
        /// We use \t and \n as separators because those characters can not be used in game object names in the hierarchy.
        /// Format (ignore the spaces, they are just for readability):
        ///   scene.name \t scene.guid \t rootObjName \t rootObjSameNameSiblingIndex \t childObjName \t childObjSameNameSiblingIndex , ... \n [next path] \n [next path], ...
        ///   Example: MainScene \t as8u2o23rf98a9h3ajsd8hfa \t CharRoot \t 0 \t Torso \t 0 \t UpperArm \t 1 \t LowerArm \t 0 \t Hand \t 0 \t Finger \t 4
        /// </summary>
        static void SaveSelection()
        {
            List<string> allowedSceneGuids = null;
            if (EditorApplication.isPlaying)
            {
                var allowedScenesGuidsString = EditorPrefs.GetString("SelectionKeeper.openedScenes", "");
                if (!string.IsNullOrEmpty(allowedScenesGuidsString))
                {
                    allowedSceneGuids = allowedScenesGuidsString.Split(',').ToList();
                    if (allowedSceneGuids.Count == 0)
                    {
                        allowedSceneGuids = null;
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (var obj in Selection.gameObjects)
            {
                // hierarchy objects only
                if ( !AssetDatabase.Contains(obj) )
                {
                    var path = GetPathAsString(obj.transform, '\t', allowedSceneGuids);
                    if ( !string.IsNullOrEmpty(path) )
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append('\n');
                        }
                        sb.Append(path);
                    }
                }
            }
            if ( !EditorApplication.isPlaying || sb.Length > 0 )
            {
                // save selection
                EditorPrefs.SetString("SelectionKeeper.selection", sb.ToString());

                // Save open scenes in edit mode
                if (!EditorApplication.isPlaying)
                {
                    StringBuilder sceneGuids = new StringBuilder();
                    for (int i = 0; i < EditorSceneManager.loadedSceneCount; ++i)
                    {
                        var sceneGuid = AssetDatabase.AssetPathToGUID(EditorSceneManager.GetSceneAt(i).path);
                        if (sceneGuids.Length > 0)
                        {
                            sceneGuids.Append(',');
                        }
                        sceneGuids.Append(sceneGuid);
                    }
                    if (sceneGuids.Length > 0)
                    {
                        EditorPrefs.SetString("SelectionKeeper.openedScenes", sceneGuids.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Generates a path with scene name and scene guid from a transform (t)
        /// separated by a char (separator).
        /// If allowedSceneGuids is set and the transfrom is not in any of the
        /// given scenes then Null is returned.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="separator"></param>
        /// <param name="allowedSceneGuids">Either null or a list of scene guids. Null means no filtering.</param>
        /// <returns></returns>
        public static string GetPathAsString(Transform t, char separator, List<string> allowedSceneGuids = null)
        {
            string path = null;
            if (t.gameObject != null)
            {
                var sceneGuid = AssetDatabase.AssetPathToGUID(t.gameObject.scene.path);
                if (allowedSceneGuids == null || allowedSceneGuids.Contains(sceneGuid))
                {
                    var scene = t.gameObject.scene.name + separator + sceneGuid + separator;
                    // add path
                    path = t.name + separator + getSameNameSiblingIndex(t);
                    while (t.parent != null)
                    {
                        t = t.parent;
                        path = t.name + separator + getSameNameSiblingIndex(t) + separator + path;
                    }
                    // add scene with guid
                    path = scene + path;
                }
            }
            return path;
        }

        public static int getSameNameSiblingIndex( Transform child )
        {
            int count = 0;
            if (child.parent != null)
            {
                for (int i = 0; i < child.parent.childCount; ++i)
                {
                    if (child.parent.GetChild(i).name == child.name && child.parent.GetChild(i).GetSiblingIndex() < child.GetSiblingIndex())
                    {
                        count++;
                    }
                }
            }
            else
            {
                var rootObjects = child.gameObject.scene.GetRootGameObjects();
                for (int i = 0; i < rootObjects.Length; ++i)
                {
                    if (rootObjects[i].name == child.name && rootObjects[i].transform.GetSiblingIndex() < child.GetSiblingIndex())
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
} 
#endif
