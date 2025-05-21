/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    public class CloseComponentWindows : ActionItem, IValidatableLayoutItem
    {
        public override float order
        {
            get { return 850; }
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.closeWindows, "Close All Component Windows");
        }

        public override void Invoke()
        {
            ComponentWindow[] windows = UnityEngine.Resources.FindObjectsOfTypeAll<ComponentWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                try
                {
                    windows[i].Close();
                }
                catch
                {
                    Object.DestroyImmediate(windows[i]);
                }
            }
        }

        public bool Validate()
        {
            ComponentWindow[] windows = UnityEngine.Resources.FindObjectsOfTypeAll<ComponentWindow>();
            return windows.Length > 0;
        }
    }
}