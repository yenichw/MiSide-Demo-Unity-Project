/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools
{
    public static class SceneViewAlignDrawer
    {
        private static ToolbarAlign align;
        private static Texture2D passiveTexture;
        private static Texture2D activeTexture;

        public static void Hide()
        {
            SceneViewManager.RemoveListener(OnSceneGUI);
        }

        public static ToolbarAlign Show(Rect sceneRect, Rect wndRect, out Vector2 offset)
        {
            SceneViewManager.AddListener(OnSceneGUI, float.MaxValue, true);
            align = Update(sceneRect, wndRect, out offset);
            return align;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (Event.current.type != EventType.Repaint) return;

            if (passiveTexture == null) passiveTexture = Resources.CreateSinglePixelTexture(255, 0, 0, 128);
            if (activeTexture == null) activeTexture = Resources.CreateSinglePixelTexture(0, 255, 0, 128);

            Rect viewRect = SceneViewManager.GetRect(sceneView);
            Rect freeRect = QuickAccess.GetFreeRect(viewRect);

            float width = freeRect.width;
            float height = freeRect.height;
            float left = freeRect.x;

            Handles.BeginGUI();

            // Top left 
            Texture2D texture = align == ToolbarAlign.topLeft ? activeTexture : passiveTexture;
            GUI.DrawTexture(new Rect(left, 0, 5, 20), texture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(left + 5, 0, 15, 5), texture, ScaleMode.StretchToFill);

            // Bottom left
            texture = align == ToolbarAlign.bottomLeft ? activeTexture : passiveTexture;
            GUI.DrawTexture(new Rect(left, height - 20, 5, 20), texture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(left + 5, height - 5, 15, 5), texture, ScaleMode.StretchToFill);

            // Top right
            texture = align == ToolbarAlign.topRight ? activeTexture : passiveTexture;
            GUI.DrawTexture(new Rect(left + width - 5, 0, 5, 20), texture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(left + width - 20, 0, 15, 5), texture, ScaleMode.StretchToFill);

            // Bottom right
            texture = align == ToolbarAlign.bottomRight ? activeTexture : passiveTexture;
            GUI.DrawTexture(new Rect(left + width - 5, height - 20, 5, 20), texture, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(left + width - 20, height - 5, 15, 5), texture, ScaleMode.StretchToFill);

            // Top Center
            texture = align == ToolbarAlign.top ? activeTexture : passiveTexture;
            GUI.DrawTexture(new Rect((width - left) / 2 - 20, 0, 40, 5), texture, ScaleMode.StretchToFill);

            // Bottom Center
            texture = align == ToolbarAlign.bottom ? activeTexture : passiveTexture;
            GUI.DrawTexture(new Rect((width - left) / 2 - 20, height - 5, 40, 5), texture, ScaleMode.StretchToFill);

            // Left Center
            texture = align == ToolbarAlign.left ? activeTexture : passiveTexture;
            GUI.DrawTexture(new Rect(left, height / 2 - 20, 5, 40), texture, ScaleMode.StretchToFill);

            // Right Center
            texture = align == ToolbarAlign.right ? activeTexture : passiveTexture;
            GUI.DrawTexture(new Rect(width - 5, height / 2 - 20, 5, 40), texture, ScaleMode.StretchToFill);

            Handles.EndGUI();
        }

        public static ToolbarAlign Update(Rect sceneRect, Rect wndRect, out Vector2 offset)
        {
            align = ToolbarAlign.topLeft;

            sceneRect = new Rect(Vector2.zero, sceneRect.size);

            sceneRect = QuickAccess.GetFreeRect(sceneRect);

            // Init Top Left
            offset = wndRect.min - sceneRect.min;
            float minDistance = offset.sqrMagnitude;

            // Left
            Vector2 off = new Vector2(wndRect.x, wndRect.center.y) - new Vector2(sceneRect.x, sceneRect.center.y);
            float d = off.sqrMagnitude;
            if (d < minDistance)
            {
                offset = off;
                minDistance = d;
                align = ToolbarAlign.left;
            }

            // Bottom Left
            off = new Vector2(wndRect.x, wndRect.max.y) - new Vector2(sceneRect.x, sceneRect.max.y);
            d = off.sqrMagnitude;
            if (d < minDistance)
            {
                offset = off;
                minDistance = d;
                align = ToolbarAlign.bottomLeft;
            }

            // Bottom
            off = new Vector2(wndRect.center.x, wndRect.max.y) - new Vector2(sceneRect.center.x, sceneRect.max.y);
            d = off.sqrMagnitude;
            if (d < minDistance)
            {
                offset = off;
                minDistance = d;
                align = ToolbarAlign.bottom;
            }

            // Bottom Right
            off = new Vector2(wndRect.max.x, wndRect.max.y) - new Vector2(sceneRect.max.x, sceneRect.max.y);
            d = off.sqrMagnitude;
            if (d < minDistance)
            {
                offset = off;
                minDistance = d;
                align = ToolbarAlign.bottomRight;
            }

            // Right
            off = new Vector2(wndRect.max.x, wndRect.center.y) - new Vector2(sceneRect.max.x, sceneRect.center.y);
            d = off.sqrMagnitude;
            if (d < minDistance)
            {
                offset = off;
                minDistance = d;
                align = ToolbarAlign.right;
            }

            // Top Right
            off = new Vector2(wndRect.max.x, wndRect.min.y) - new Vector2(sceneRect.max.x, sceneRect.min.y);
            d = off.sqrMagnitude;
            if (d < minDistance)
            {
                offset = off;
                minDistance = d;
                align = ToolbarAlign.topRight;
            }

            // Top
            off = new Vector2(wndRect.center.x, wndRect.min.y) - new Vector2(sceneRect.center.x, sceneRect.min.y);
            d = off.sqrMagnitude;
            if (d < minDistance)
            {
                offset = off;
                align = ToolbarAlign.top;
            }

            return align;
        }
    }
}