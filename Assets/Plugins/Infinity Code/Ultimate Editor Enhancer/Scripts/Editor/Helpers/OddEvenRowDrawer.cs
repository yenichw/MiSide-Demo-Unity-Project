/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class OddEvenRowDrawer
    {
        private static Color darkColor = new Color(0, 0, 0, 0.1f);
        private static Color lightColor = new Color(1, 1, 1, 0.1f);

        private static Color rowColor
        {
            get
            {
                return EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }
        }

        public static void Draw(Rect rect, float minX = 0)
        {
            if (((int)(rect.y / rect.height) & 1) == 0) return;
            rect.x = minX;
            rect.width = EditorGUIUtility.currentViewWidth;
            EditorGUI.DrawRect(rect, rowColor);
        }
    }
}