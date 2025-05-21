/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class LODGroupSelector
    {
        static LODGroupSelector()
        {
            SelectionManager.OnChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            if (!Prefs.selectLODGroupAsOne) return;
            if (!(EditorWindow.focusedWindow is SceneView)) return;
            if (SelectionManager.currentSelection.Length != 1) return;

            GameObject selected = SelectionManager.currentSelection[0] as GameObject;
            if (selected == null || selected.scene.name == null) return;

            LODGroup lodGroup = selected.GetComponentInParent<LODGroup>();
            if (lodGroup == null) return;

            if (SelectionManager.prevSelection != null && SelectionManager.prevSelection.Length == 1)
            {
                GameObject prev = SelectionManager.prevSelection[0] as GameObject;
                if (lodGroup.gameObject == prev) return;
                if (selected.transform.IsChildOf(lodGroup.transform)) return;
            }

            EditorApplication.delayCall += () => Selection.activeGameObject = lodGroup.gameObject;
        }
    }
}