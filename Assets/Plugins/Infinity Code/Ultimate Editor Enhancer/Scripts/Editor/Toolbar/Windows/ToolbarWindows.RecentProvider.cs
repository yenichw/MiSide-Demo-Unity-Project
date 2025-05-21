/*           INFINITY CODE          */
/*     https://infinity-code.com    */

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    public static partial class ToolbarWindows
    {
        public class RecentProvider : Provider
        {
            public override float order => 0;

            public override void GenerateMenu(GenericMenuEx menu, ref bool hasItems)
            {
                if (!Prefs.recentWindowsInToolbar || recent.Count == 0) return;

                foreach (WindowRecord window in recent)
                {
                    menu.Add("Recent/" + window.title, RestoreRecentWindow, window);
                }

                hasItems = true;
            }
        }
    }
}