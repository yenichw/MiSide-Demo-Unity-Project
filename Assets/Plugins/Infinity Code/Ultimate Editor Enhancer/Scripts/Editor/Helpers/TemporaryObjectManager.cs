/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class TemporaryObjectManager
    {
        [MenuItem(WindowsHelper.MenuPath + "Temporary Objects/Create Container", false, MenuItemOrder.TemporaryCreateContainer)]
        public static void CreateTemporaryContainer()
        {
            GameObject go = TemporaryContainer.GetContainer();
            Undo.RegisterCreatedObjectUndo(go, "Create Temporary Container");
        }

        [MenuItem(WindowsHelper.MenuPath + "Temporary Objects/Destroy Container", false, MenuItemOrder.TemporaryRemoveContainer)]
        public static void DestroyTemporaryContainer()
        {
            TemporaryContainer temporaryContainer = ObjectHelper.FindObjectOfType<TemporaryContainer>();
            if (temporaryContainer == null) return;

            Undo.DestroyObjectImmediate(temporaryContainer.gameObject);
        }
    }
}