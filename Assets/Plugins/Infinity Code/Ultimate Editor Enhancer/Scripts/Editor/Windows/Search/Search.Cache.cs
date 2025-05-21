/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class Search
    {
        private static void CachePrefabWithComponents(Dictionary<int, Record> tempRecords, GameObject go)
        {
            tempRecords.Add(go.GetInstanceID(), new GameObjectRecord(go));
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component c = components[i];
                tempRecords.Add(c.GetInstanceID(), new ComponentRecord(c));
            }

            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++) CachePrefabWithComponents(tempRecords, t.GetChild(i).gameObject);
        }

        private static void CachePrefabWithoutComponents(Dictionary<int, Record> tempRecords, GameObject go)
        {
            tempRecords.Add(go.GetInstanceID(), new GameObjectRecord(go));
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++) CachePrefabWithoutComponents(tempRecords, t.GetChild(i).gameObject);
        }

        private static void CacheProject()
        {
            Dictionary<string, Record> tempRecords = new Dictionary<string, Record>();
            string[] assets = AssetDatabase.GetAllAssetPaths();

            if (projectRecords != null)
            {
                foreach (KeyValuePair<string, Record> pair in projectRecords) pair.Value.used = false;
            }

            char[] validPrefix = {'A', 's', 's', 'e', 't', 's', '/'};

            for (int i = 0; i < assets.Length; i++)
            {
                string assetPath = assets[i];
                try
                {
                    if (string.IsNullOrEmpty(assetPath)) continue;
                    if (assetPath.Length < 8) continue;

                    bool hasValidPrefix = true;

                    for (int j = 0; j < validPrefix.Length; j++)
                    {
                        if (validPrefix[j] != assetPath[j])
                        {
                            hasValidPrefix = false;
                            break;
                        }
                    }

                    if (!hasValidPrefix) continue;

                    if (projectRecords != null)
                    {
                        Record r;

                        if (projectRecords.TryGetValue(assetPath, out r))
                        {
                            if (!tempRecords.ContainsKey(assetPath))
                            {
                                tempRecords.Add(assetPath, r);
                                r.used = true;
                            }

                            continue;
                        }
                    }

                    if (!tempRecords.ContainsKey(assetPath))
                    {
                        tempRecords.Add(assetPath, new ProjectRecord(assetPath));
                    }
                }
                catch
                {
                }
            }

            if (projectRecords != null)
            {
                foreach (var pair in projectRecords.Where(p => !p.Value.used)) pair.Value.Dispose();
            }

            projectRecords = tempRecords;
        }

        private static void CacheScene()
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (sceneRecords != null)
            {
                foreach (var record in sceneRecords) record.Value.used = false;
            }

            if (prefabStage != null)
            {
                Dictionary<int, Record> tempRecords = new Dictionary<int, Record>();
                try
                {
                    if (Prefs.searchByComponents) CachePrefabWithComponents(tempRecords, prefabStage.prefabContentsRoot);
                    else CachePrefabWithoutComponents(tempRecords, prefabStage.prefabContentsRoot);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }

                if (sceneRecords != null)
                {
                    foreach (var record in sceneRecords)
                    {
                        if (!record.Value.used) record.Value.Dispose();
                    }
                }

                sceneRecords = tempRecords;
            }
            else
            {
                if (Prefs.searchByComponents) CacheSceneItems();
                else CacheSceneGameObjects();
            }
        }

        private static void CacheSceneGameObjects()
        {
            Transform[] transforms = ObjectHelper.FindObjectsOfType<Transform>(true);
            Dictionary<int, Record> tempRecords = new Dictionary<int, Record>(transforms.Length);

            for (int i = 0; i < transforms.Length; i++)
            {
                GameObject go = transforms[i].gameObject;
                int key = go.GetInstanceID();

                Record r;

                if (sceneRecords == null || !sceneRecords.TryGetValue(key, out r)) r = new GameObjectRecord(go);
                else r.UpdateGameObjectName(go);

                r.used = true;
                tempRecords.Add(key, r);
            }

            if (sceneRecords != null)
            {
                foreach (var record in sceneRecords)
                {
                    if (!record.Value.used) record.Value.Dispose();
                }
            }

            sceneRecords = tempRecords;
        }

        private static void CacheSceneItems()
        {
            Component[] components = ObjectHelper.FindObjectsOfType<Component>(true);
            Dictionary<int, Record> tempRecords = new Dictionary<int, Record>(Mathf.NextPowerOfTwo(components.Length));

            for (int i = 0; i < components.Length; i++)
            {
                Component c = components[i];
                int key = c.GetInstanceID();
                Record r = null;
                if (sceneRecords == null || !sceneRecords.TryGetValue(key, out r)) r = new ComponentRecord(c);
                else r.UpdateGameObjectName(c.gameObject);

                r.used = true;
                tempRecords.Add(key, r);

                GameObject go = c.gameObject;
                key = go.GetInstanceID();

                if (!tempRecords.ContainsKey(key))
                {
                    if (sceneRecords == null || !sceneRecords.TryGetValue(key, out r)) r = new GameObjectRecord(go);
                    else r.UpdateGameObjectName(go);

                    r.used = true;
                    tempRecords.Add(key, r);
                }
            }

            if (sceneRecords != null)
            {
                foreach (var record in sceneRecords)
                {
                    if (!record.Value.used) record.Value.Dispose();
                }
            }

            sceneRecords = tempRecords;
        }

        private static void CacheWindows()
        {
            if (windowRecords != null) return;

            windowRecords = new Dictionary<int, Record>();

            foreach (string submenu in Unsupported.GetSubmenus("Window"))
            {
                string upper = Culture.textInfo.ToUpper(submenu);
                if (upper == "WINDOW/NEXT WINDOW") continue;
                if (upper == "WINDOW/PREVIOUS WINDOW") continue;
                if (Culture.compareInfo.IsPrefix(upper, "WINDOW/LAYOUTS", CompareOptions.None)) continue;

                int lastSlash = 7;

                for (int i = submenu.Length - 1; i >= 8; i--)
                {
                    if (submenu[i] == '/')
                    {
                        lastSlash = i;
                        break;
                    }
                }

                windowRecords.Add(submenu.GetHashCode(), new WindowRecord(submenu, submenu.Substring(lastSlash + 1)));
            }

            foreach (string submenu in Unsupported.GetSubmenus("Tools"))
            {
                int lastSlash = 6;

                for (int i = submenu.Length - 1; i >= 7; i--)
                {
                    if (submenu[i] == '/')
                    {
                        lastSlash = i;
                        break;
                    }
                }

                windowRecords.Add(submenu.GetHashCode(), new WindowRecord(submenu, submenu.Substring(lastSlash + 1)));
            }

            windowRecords.Add("Project Settings...".GetHashCode(), new WindowRecord("Edit/Project Settings...", "Project Settings"));
            windowRecords.Add("Preferences...".GetHashCode(), new WindowRecord("Edit/Preferences...", "Preferences"));
        }
    }
}