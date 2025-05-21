/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class SoloPickability
    {
        private static int phase;

        static SoloPickability()
        {
            HierarchyItemDrawer.Register("SoloPickability", WaitRightClickOnPickable, HierarchyToolOrder.SoloPickability);
        }

        private static void WaitRightClickOnPickable(HierarchyItem item)
        {
            if (!Prefs.hierarchySoloPickability) return;

            Event e = Event.current;
            if (phase == 0)
            {
                if (e.type != EventType.MouseDown) return;
                if (e.button != 1) return;
                Vector2 pos = e.mousePosition;
                if (pos.x < 16 || pos.x > 32) return;

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

                    ToggleSoloPickability(item.gameObject);
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
        
        private static bool GetSoloPickabilityState(GameObject go)
        {
            if (go == null) return false;

            object instance = SceneVisibilityManagerRef.GetInstance();
            if (!SceneVisibilityManagerRef.IsSelectable(instance, go)) return true;

            Transform current = go.transform;
            Transform parent = current.parent;
            while (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform t = parent.GetChild(i);
                    if (t == current) continue;

                    GameObject g = t.gameObject;
                    if (SceneVisibilityManagerRef.IsSelectable(instance, g))
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
                if (SceneVisibilityManagerRef.IsSelectable(instance, g) && g.transform != current && g.hideFlags != HideFlags.HideInHierarchy && g.hideFlags != HideFlags.HideAndDontSave)
                {
                    return true;
                }
            }

            return false;
        }

        private static void DisableOther(GameObject go)
        {
            if (go == null) return;
            object instance = SceneVisibilityManagerRef.GetInstance();
            SceneVisibilityManagerRef.EnablePicking(instance, go, true);

            Transform current = go.transform;
            Transform parent = current.parent;
            while (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform t = parent.GetChild(i);
                    if (current == t) continue;

                    GameObject g = t.gameObject;
                    SceneVisibilityManagerRef.DisablePicking(instance, g, true);
                }

                current = parent;
                parent = parent.parent;
            }

            GameObject[] rootObjects = go.scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                GameObject g = rootObjects[i];
                if (g.transform != current) SceneVisibilityManagerRef.DisablePicking(instance, g, true);
            }
        }

        private static void EnableEverything(GameObject go)
        {
            if (go == null) return;
            object instance = SceneVisibilityManagerRef.GetInstance();
            SceneVisibilityManagerRef.EnablePicking(instance, go, true);

            Transform current = go.transform;
            Transform parent = current.parent;
            while (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform t = parent.GetChild(i);
                    if (current == t) continue;
                    
                    GameObject g = t.gameObject;
                    SceneVisibilityManagerRef.EnablePicking(instance, g, true);
                }

                current = parent;
                parent = parent.parent;
            }

            GameObject[] rootObjects = go.scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                GameObject g = rootObjects[i];
                if (g.transform != current) SceneVisibilityManagerRef.EnablePicking(instance, g, true);
            }
        }

        private static void ToggleSoloPickability(GameObject go)
        {
            bool state = GetSoloPickabilityState(go);
            if (state) DisableOther(go);
            else EnableEverything(go);
        }
    }
}