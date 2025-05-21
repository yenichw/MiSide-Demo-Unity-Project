/*           INFINITY CODE          */
/*     https://infinity-code.com    */

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class FloatToolbarManager
    {
        private static List<FloatToolbar> toolbars = new List<FloatToolbar>();
        private static Dictionary<int, Rect> sizes;
        private static PrefabStage lastPrefabStage;

        static FloatToolbarManager()
        {
            sizes = new Dictionary<int, Rect>();
            SceneViewManager.AddListener(OnSceneViewGUI, SceneViewOrder.FloatToolbar, true);
        }

        public static void Add(FloatToolbar toolbar)
        {
            toolbars.Add(toolbar);
        }

        private static void OnSceneViewGUI(SceneView sceneView)
        {
            if (toolbars == null || toolbars.Count == 0) return;

            bool sizeChanged = false;
            if (Event.current.type == EventType.Layout && DragAndDrop.objectReferences.Length == 0)
            {
                int id = sceneView.GetInstanceID();
                Rect viewRect = SceneViewManager.GetRect(sceneView);

                Rect rect;
                if (sizes.TryGetValue(id, out rect))
                {
                    if (viewRect != rect)
                    {
                        sizeChanged = true;
                        sizes[id] = viewRect;
                    }
                }
                else
                {
                    sizeChanged = true;
                    sizes[id] = viewRect;
                }

                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != lastPrefabStage)
                {
                    sizeChanged = true;
                    lastPrefabStage = prefabStage;
                }
            }

            for (int i = toolbars.Count - 1; i >= 0; i--)
            {
                try
                {
                    FloatToolbar toolbar = toolbars[i];
                    if (toolbar == null) continue;

                    toolbar.isDirty = sizeChanged;
                    toolbar.OnSceneViewGUI(sceneView);
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }

        public static void Remove(FloatToolbar toolbar)
        {
            toolbars.Remove(toolbar);
        }
    }
}