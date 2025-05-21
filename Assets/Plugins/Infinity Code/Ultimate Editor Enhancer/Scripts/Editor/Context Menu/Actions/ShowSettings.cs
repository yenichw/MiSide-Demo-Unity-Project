/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    public class ShowSettings : ActionItem
    {
        public override float order
        {
            get { return 1000; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.settings, "Settings");
        }

        public override void Invoke()
        {
            SettingsService.OpenProjectSettings("Project/Ultimate Editor Enhancer");
        }
    }
}