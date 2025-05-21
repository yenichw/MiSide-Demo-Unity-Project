/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class TreeViewControllerUserInputChangedExpandedState : StatedInterceptor<TreeViewControllerUserInputChangedExpandedState>
    {
        private static bool useAnimation;

        protected override MethodInfo originalMethod
        {
            get => TreeViewControllerRef.userInputChangedExpandedStateMethod;
        }

        public override bool state
        {
            get => Prefs.treeControllerCollapse;
        }
        
        protected override string prefixMethodName { get => nameof(UserInputChangedExpandedStatePrefix); }
        
        private static bool UserInputChangedExpandedStatePrefix(object __instance, TreeViewItem item, int row, bool expand)
        {
            Event e = Event.current;
            if (e.type != EventType.Used || e.button != 1 || expand) return true;

            useAnimation = TreeViewControllerRef.GetUseExpansionAnimation(__instance);
            TreeViewControllerRef.SetUseExpansionAnimation(__instance, false);
            TreeViewControllerRef.ChangeExpandedState(__instance, item, expand, true);
            TreeViewControllerRef.ChangeExpandedState(__instance, item, !expand, false);
            TreeViewControllerRef.SetUseExpansionAnimation(__instance, useAnimation);
            
            return false;
        }
    }
}