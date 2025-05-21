/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class SoloVisibility
    {
        private static int phase = 0;

        static SoloVisibility()
        {
            HierarchyItemDrawer.Register("SoloVisibility", WaitRightClickOnEye, HierarchyToolOrder.SoloVisibility);
        }

        private static bool GetSoloVisibilityState(GameObject go)
        {
            if (go == null) return false;
            if (SceneVisibilityStateRef.IsGameObjectHidden(go)) return true;

            Transform current = go.transform;
            Transform parent = current.parent;
            while (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform t = parent.GetChild(i);
                    if (t == current) continue;

                    GameObject g = t.gameObject;
                    if (!SceneVisibilityStateRef.IsGameObjectHidden(g))
                    {
                        return true;
                    }
                }

                current = parent;
                parent = parent.parent;
            }

            GameObject[] rootObjects = go.scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                GameObject g = rootObjects[i];
                if (!SceneVisibilityStateRef.IsGameObjectHidden(g) && g.transform != current && g.hideFlags != HideFlags.HideInHierarchy && g.hideFlags != HideFlags.HideAndDontSave)
                {
                    return true;
                }
            }

            return false;
        }

        private static void HideOther(GameObject go)
        {
            if (go == null) return;
            object instance = SceneVisibilityManagerRef.GetInstance();
            SceneVisibilityManagerRef.Show(instance, go, true);

            Transform current = go.transform;
            Transform parent = current.parent;
            while (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform t = parent.GetChild(i);
                    if (current == t) continue;

                    GameObject g = t.gameObject;
                    SceneVisibilityManagerRef.Hide(instance, g, true);
                }

                current = parent;
                parent = parent.parent;
            }

            GameObject[] rootObjects = go.scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                GameObject g = rootObjects[i];
                if (g.transform != current) SceneVisibilityManagerRef.Hide(instance, g, true);
            }
        }

        private static void ShowEverything(GameObject go)
        {
            if (go == null) return;
            object instance = SceneVisibilityManagerRef.GetInstance();
            SceneVisibilityManagerRef.Show(instance, go, true);

            Transform current = go.transform;
            Transform parent = current.parent;
            while (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform t = parent.GetChild(i);
                    if (current == t) continue;
                    
                    GameObject g = t.gameObject;
                    SceneVisibilityManagerRef.Show(instance, g, true);
                }

                current = parent;
                parent = parent.parent;
            }

            GameObject[] rootObjects = go.scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                GameObject g = rootObjects[i];
                if (g.transform != current) SceneVisibilityManagerRef.Show(instance, g, true);
            }
        }

        private static void ToggleSoloVisibility(GameObject go)
        {
            bool state = GetSoloVisibilityState(go);
            if (state) HideOther(go);
            else ShowEverything(go);
        }

        private static void WaitRightClickOnEye(HierarchyItem item)
        {
            if (!Prefs.hierarchySoloVisibility) return;

            Event e = Event.current;
            if (phase == 0)
            {
                if (e.type != EventType.MouseDown) return;
                if (e.button != 1) return;
                Vector2 pos = e.mousePosition;
                if (pos.x > 16) return;

                phase = 1;
                e.Use();
                EditorApplication.RepaintHierarchyWindow();
            }
            else if (phase == 1)
            {
                if (e.type == EventType.Used || e.type == EventType.Layout) return;

                if (e.type == EventType.Repaint)
                {
                    Vector2 pos = e.mousePosition;
                    Rect rect = item.rect;
                    if (pos.y < rect.y || pos.y > rect.yMax) return;

                    ToggleSoloVisibility(item.gameObject);
                    phase = 2;
                }
                else
                {
                    if (e.type == EventType.MouseUp)
                    {
                        e.Use();
                        phase = 0;
                    }
                    else phase = 2;
                }
            }
            else if (phase == 2)
            {
                if (e.type == EventType.MouseUp)
                {
                    e.Use();
                    phase = 0;
                }
            }
        }
    }
}