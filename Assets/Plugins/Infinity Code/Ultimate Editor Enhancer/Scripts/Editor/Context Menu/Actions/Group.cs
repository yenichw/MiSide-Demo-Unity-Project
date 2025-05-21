/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    [RequireMultipleGameObjects]
    public class Group : ActionItem
    {
        public override float order
        {
            get { return -895; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.group, "Group");
        }

        public override void Invoke()
        {
            Behaviors.Group.GroupSelection();
        }
    }
}