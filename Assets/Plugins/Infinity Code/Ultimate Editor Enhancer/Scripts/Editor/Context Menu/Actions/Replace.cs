/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    [RequireSelected]
    public class Replace : ActionItem
    {
        public override float order
        {
            get { return -905; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.replace, "Replace");
        }

        public override void Invoke()
        {
            EditorMenu.Close();
            Behaviors.Replace.Show(targets);
        }
    }
}