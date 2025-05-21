/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class OddEven
    {
        static OddEven()
        {
            ProjectItemDrawer.Register("ODD_EVEN", Draw, ProjectToolOrder.OddEven);
        }

        private static void Draw(ProjectItem item)
        {
            if (!Prefs.projectOddEven) return;
            
            Rect r = item.rect;
            if (r.height > 32) return;
            OddEvenRowDrawer.Draw(r);
        }
    }
}