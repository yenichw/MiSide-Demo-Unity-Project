/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class ComponentHeaderContextMenuInjection
    {
        [MenuItem("CONTEXT/Component/Add Bookmark", false, 41)]
        private static void AddToBookmark(MenuCommand command)
        {
            Bookmarks.Add(command.context as Component);
        }

        [MenuItem("CONTEXT/Component/Open As Tab Window", false, 21)]
        private static void OpenInWindow(MenuCommand command)
        {
            ComponentWindow.Show(command.context as Component, false);
        }

        [MenuItem("CONTEXT/Component/Open As DropDown Window", false, 22)]
        private static void OpenAsWindow(MenuCommand command)
        {
            ComponentWindow.ShowPopup(command.context as Component);
        }

        [MenuItem("CONTEXT/Component/Open As Utility Window", false, 23)] 
        private static void OpenAsUtilityWindow(MenuCommand command)
        {
            ComponentWindow.ShowUtility(command.context as Component, false);
        }
    }
}