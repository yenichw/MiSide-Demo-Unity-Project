/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class OddEven
    {
        static OddEven()
        {
            HierarchyItemDrawer.Register("OddEven", DrawItem, HierarchyToolOrder.OddEven);
        }

        private static void DrawItem(HierarchyItem item)
        {
            if (!Prefs.hierarchyOddEven) return;
            
            OddEvenRowDrawer.Draw(item.rect, 32);
        }
    }
}