/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.UltimateEditorEnhancer
{
    [AddComponentMenu("")]
    public class SceneReferences : MonoBehaviour
    {
        public static Action<SceneReferences> OnCreate;
        public static Action OnUpdateInstances;

        public static List<SceneReferences> instances = new List<SceneReferences>();

        public List<SceneBookmark> bookmarks = new List<SceneBookmark>();
        public List<HierarchyBackground> hierarchyBackgrounds = new List<HierarchyBackground>();
        
        [NonSerialized]
        private HashSet<int> _bookmarkIDs;
        
        public HashSet<int> bookmarkIDs
        {
            get
            {
                if (_bookmarkIDs == null)
                {
                    IEnumerable<int> ids = bookmarks.Where(b => b.target != null).Select(b => b.target.GetInstanceID());
                    _bookmarkIDs = new HashSet<int>(ids);
                }

                return _bookmarkIDs;
            }
        }

        public static SceneReferences Get(Scene scene, bool createIfMissed = true)
        {
            if (instances == null) return null;
            SceneReferences r = instances.FirstOrDefault(i => i != null && i.gameObject.scene == scene);
            if (r != null) return r;
            if (!createIfMissed) return null;

            Scene activeScene = SceneManager.GetActiveScene();
            bool sceneChanged = false;
            if (activeScene != scene)
            {
                SceneManager.SetActiveScene(scene);
                sceneChanged = true;
            }
            GameObject go = new GameObject("Ultimate Editor Enhancer References");
            go.tag = "EditorOnly";
            r = go.AddComponent<SceneReferences>();
            if (OnCreate != null) OnCreate(r);
            instances.Add(r);
            if (sceneChanged) SceneManager.SetActiveScene(activeScene);

            return r;
        }

        public HierarchyBackground GetBackground(GameObject target, bool useNonRecursive = false)
        {
            foreach (HierarchyBackground b in hierarchyBackgrounds)
            {
                if (b.gameObject == target) return b;
            }

            Transform t = target.transform.parent;
            if (useNonRecursive)
            {
                while (t != null)
                {
                    GameObject go = t.gameObject;

                    foreach (HierarchyBackground b in hierarchyBackgrounds)
                    {
                        if (b.gameObject == go) return b;
                    }

                    t = t.parent;
                }
            }
            else
            {
                while (t != null)
                {
                    GameObject go = t.gameObject;

                    foreach (HierarchyBackground b in hierarchyBackgrounds)
                    {
                        if (b.gameObject == go)
                        {
                            if (!b.recursive) return null;
                            return b;
                        }
                    }

                    t = t.parent;
                }
            }

            return null;
        }

        public void ResetIDs()
        {
            _bookmarkIDs = null;
        }

        public static void UpdateInstances()
        {
            instances = ObjectHelper.FindObjectsOfType<SceneReferences>().ToList();
            if (OnUpdateInstances != null) OnUpdateInstances();
        }

        [Serializable]
        public class HierarchyBackground
        {
            public GameObject gameObject;
            public Color color;
            public bool recursive = false;
        }
    }
}