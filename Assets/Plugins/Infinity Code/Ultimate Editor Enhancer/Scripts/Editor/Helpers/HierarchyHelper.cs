/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class HierarchyHelper
    {
        public static void ExpandHierarchy(EditorWindow window, GameObject selection)
        {
            if (selection != null)
            {
                SceneHierarchyWindowRef.SetExpandedRecursive(window, selection.GetInstanceID(), true);
            }
            else
            {
                object sceneHierarchy = SceneHierarchyWindowRef.GetSceneHierarchy(window);
                SceneHierarchyRef.SetScenesExpanded(sceneHierarchy, new List<string> { SceneManager.GetActiveScene().name });
            }
        }

        public static bool IsExpanded(int id)
        {
            EditorWindow hierarchyWindow = SceneHierarchyWindowRef.GetLastInteractedHierarchy();
            if (hierarchyWindow == null) return false;
            
            object sceneHierarchy = SceneHierarchyWindowRef.GetSceneHierarchy(hierarchyWindow);
            if (sceneHierarchy == null) return false;

            object treeView = SceneHierarchyRef.GetTreeView(sceneHierarchy);
            if (treeView == null) return false;

            object data = TreeViewControllerRef.GetData(treeView);
            if (data == null) return false;

            return ITreeViewDataSourceRef.IsExpanded(data, id);
        }

        public static void SetDefaultIconsSize(EditorWindow hierarchyWindow, int size = 0)
        {
            object sceneHierarchy = SceneHierarchyWindowRef.GetSceneHierarchy(hierarchyWindow);
            if (sceneHierarchy == null) return;

            object treeView = SceneHierarchyRef.GetTreeView(sceneHierarchy);
            if (treeView == null) return;

            object gui = TreeViewControllerRef.GetGUI(treeView);
            if (gui == null) return;

            TreeViewGUIRef.SetIconWidth(gui, size);
            TreeViewGUIRef.SetSpaceBetweenIconAndText(gui, 18 - size);
        }
    }
}