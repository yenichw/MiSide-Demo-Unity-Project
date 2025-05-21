/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class Highlighter
    {
        private static Texture2D _hoveredTexture;
        private static Color lastColor;

        private static Texture2D hoveredTexture
        {
            get
            {
                if (lastColor != Prefs.highlightHierarchyRowColor)
                {
                    if (_hoveredTexture != null)
                    {
                        Object.DestroyImmediate(_hoveredTexture);
                        _hoveredTexture = null;
                    }
                }

                if (_hoveredTexture == null)
                {
                    _hoveredTexture = Resources.CreateSinglePixelTexture(Prefs.highlightHierarchyRowColor);
                    lastColor = Prefs.highlightHierarchyRowColor;
                }
                return _hoveredTexture;
            }
        }

        static Highlighter()
        {
            HierarchyItemDrawer.Register("Highlighter2", DrawHierarchyItem, HierarchyToolOrder.Highlighter);
        }

        private static void DrawHierarchyItem(HierarchyItem item)
        {
            if (!Prefs.highlight) return;
            if (!(WindowsHelper.mouseOverWindow is SceneView)) return;

            Event e = Event.current;
            if (item.rect.Contains(e.mousePosition))
            {
                if (e.modifiers == Prefs.hierarchyIconsModifiers) SceneTools.Highlighter.Highlight(item.gameObject);
                else SceneTools.Highlighter.Highlight(null);
            }
            if (Prefs.highlightHierarchyRow && e.type == EventType.Repaint) HighlightRow(item.rect, item.gameObject);
        }

        private static void HighlightRow(Rect rect, GameObject go)
        {
            if (SceneTools.Highlighter.lastGameObject == null || go == null) return;
            if (SceneTools.Highlighter.lastGameObject != go) return;

            GUI.DrawTexture(new Rect(32, rect.y, 2, rect.height), hoveredTexture, ScaleMode.StretchToFill);
        }
    }
}