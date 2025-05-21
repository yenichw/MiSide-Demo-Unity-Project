using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace EPOOutline
{
    public static class RendererFilteringUtility
    {
        private static List<Outlinable> filteredOutlinables = new List<Outlinable>();

        public static void Filter(Camera camera, OutlineParameters parameters)
        {
            filteredOutlinables.Clear();

            var mask = parameters.Mask.value & camera.cullingMask;

            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                var layer = 1L << outlinable.OutlineLayer;
                if ((parameters.OutlineLayerMask & layer) == 0)
                    continue;

                var go = outlinable.gameObject;

                if (!go.activeInHierarchy)
                    continue;

                if (((1 << go.layer) & mask) == 0)
                    continue;

#if UNITY_EDITOR
                var stage = PrefabStageUtility.GetCurrentPrefabStage();

                if (stage != null && !stage.IsPartOfPrefabContents(outlinable.gameObject))
                    continue;
#endif

                filteredOutlinables.Add(outlinable);
            }

            parameters.OutlinablesToRender.Clear();
            parameters.OutlinablesToRender.AddRange(filteredOutlinables);
        }
    }
}