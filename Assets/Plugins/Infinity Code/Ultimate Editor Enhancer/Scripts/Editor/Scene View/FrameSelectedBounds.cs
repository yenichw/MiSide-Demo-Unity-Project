/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    [InitializeOnLoad]
    public static class FrameSelectedBounds
    {
        static FrameSelectedBounds()
        {
            KeyManager.KeyBinding binding = KeyManager.AddBinding();
            binding.OnValidate += OnValidate;
            binding.OnPress += OnInvoke;
        }

        private static void OnInvoke()
        {
            GameObject[] gameObjects = Selection.gameObjects;

            bool isFirst = true;
            bool is2D = false;
            Bounds bounds = new Bounds();

            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject go = gameObjects[i];
                if (go.scene.name == null) continue;

                Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                for (int j = 0; j < renderers.Length; j++)
                {
                    Renderer renderer = renderers[j];
                    if (renderer is ParticleSystemRenderer || renderer is TrailRenderer) continue;

                    if (isFirst)
                    {
                        bounds = renderer.bounds;
                        isFirst = false;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }

                Vector3[] fourCorners = new Vector3[4];
                RectTransform[] rectTransforms = go.GetComponentsInChildren<RectTransform>();
                for (int j = 0; j < rectTransforms.Length; j++)
                {
                    RectTransform rt = rectTransforms[j];
                    rt.GetWorldCorners(fourCorners);

                    if (isFirst)
                    {
                        is2D = true;
                        bounds.center = fourCorners[0];
                        for (int k = 1; k < 4; k++)
                        {
                            bounds.Encapsulate(fourCorners[k]);
                        }
                        isFirst = false;
                    }
                    else
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            bounds.Encapsulate(fourCorners[k]);
                        }
                    }
                }

                Collider[] colliders = go.GetComponentsInChildren<Collider>();

                for (int j = 0; j < colliders.Length; j++)
                {
                    Collider collider = colliders[j];

                    if (isFirst)
                    {
                        bounds = collider.bounds;
                        isFirst = false;
                    }
                    else
                    {
                        bounds.Encapsulate(collider.bounds);
                    }
                }
            }

            if (isFirst) return;

            SceneView.lastActiveSceneView.in2DMode = is2D;

            SceneView.lastActiveSceneView.Frame(bounds, false);

            Event.current.Use();
        }

        private static bool OnValidate()
        {
            if (!Prefs.frameSelectedBounds) return false;

            Event e = Event.current;
            if (e.type != EventType.KeyDown || e.keyCode != KeyCode.F || e.modifiers != EventModifiers.Shift) return false;
            if (SceneView.lastActiveSceneView == null) return false;
            return true;
        }
    }
}