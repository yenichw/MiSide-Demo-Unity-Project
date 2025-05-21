/*           INFINITY CODE          */
/*     https://infinity-code.com    */

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    public static partial class ToolbarWindows
    {
        public class FavoriteProvider : Provider
        {
            public override float order
            {
                get { return -1; }
            }

            public override void GenerateMenu(GenericMenuEx menu, ref bool hasItems)
            {
                if (!Prefs.favoriteWindowsInToolbar) return;

                foreach (var window in ReferenceManager.favoriteWindows)
                {
                    menu.Add("Favorites/" + window.title, window.Open);
                }
                if (ReferenceManager.favoriteWindows.Count > 0) menu.AddSeparator("Favorites/");
                menu.Add("Favorites/Edit", Settings.OpenFavoriteWindowsSettings);
                hasItems = true;
            }
        }
    }
}