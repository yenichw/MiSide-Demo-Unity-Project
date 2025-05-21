/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    public static class SelectPrefabInstances
    {
        private const string MenuItem = "Edit/Select Prefab Instances";

        [MenuItem(MenuItem, false, 85)]
        public static void Select()
        {
            List<string> prefabPaths = Selection.gameObjects.Where(g => g.scene.name != null && PrefabUtility.IsPartOfAnyPrefab(g)).Select(g => PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(g)).Distinct().ToList();
            GameObject[] allObjects = ObjectHelper.FindObjectsOfType<GameObject>(true);
            List<GameObject> result = new List<GameObject>();

            foreach (GameObject go in allObjects)
            {
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                if (string.IsNullOrEmpty(path)) continue;
                if (prefabPaths.Contains(path)) result.Add(go);
            }

            Selection.objects = result.ToArray();
        }

        [MenuItem(MenuItem, true, 85)]
        public static bool Validate()
        {
            return Selection.gameObjects.Any(g => g.scene.name != null && PrefabUtility.IsPartOfAnyPrefab(g));
        }
    }
}