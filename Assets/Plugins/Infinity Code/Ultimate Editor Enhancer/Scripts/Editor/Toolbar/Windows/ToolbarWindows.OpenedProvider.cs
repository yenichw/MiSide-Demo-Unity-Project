/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    public static partial class ToolbarWindows
    {
        public class OpenedProvider : Provider
        {
            public override float order
            {
                get { return 1000; }
            }

            public override void GenerateMenu(GenericMenuEx menu, ref bool hasItems)
            {
                if (hasItems) menu.AddSeparator();

                foreach (WindowRecord window in windows.Values.OrderBy(w => w.title))
                {
                    menu.Add(window.title, lastFocusedWindowType == window.type, FocusWindow, window);
                }
            }
        }
    }
}

