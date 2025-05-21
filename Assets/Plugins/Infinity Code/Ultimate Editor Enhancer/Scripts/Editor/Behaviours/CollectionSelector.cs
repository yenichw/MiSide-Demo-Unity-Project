/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class CollectionSelector
    {
        static CollectionSelector()
        {
            SelectionManager.OnChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            if (!Prefs.selectCollectionAsOne) return;
            if (!(EditorWindow.focusedWindow is SceneView)) return;
            if (SelectionManager.currentSelection.Length != 1) return;

            GameObject selected = SelectionManager.currentSelection[0] as GameObject;
            if (selected == null || selected.scene.name == null) return;

            FlattenCollection flattenCollection = selected.GetComponentInParent<FlattenCollection>();
            if (flattenCollection == null) return;

            if (SelectionManager.prevSelection != null && SelectionManager.prevSelection.Length == 1)
            {
                GameObject prev = SelectionManager.prevSelection[0] as GameObject;
                if (flattenCollection.gameObject == prev) return;
                if (selected.transform.IsChildOf(flattenCollection.transform)) return;
            }

            EditorApplication.delayCall += () => Selection.activeGameObject = flattenCollection.gameObject;
        }
    }
}