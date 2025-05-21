/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Linq;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class TreeDrawer
    {
        private static Texture2D _endIcon;
        private static Texture2D _lineIcon;
        private static Texture2D _middleIcon;

        public static Texture2D endIcon
        {
            get
            {
                if (_endIcon == null) _endIcon = Resources.LoadIcon("Hierarchy_Tree_End");

                return _endIcon;
            }
        }

        public static Texture2D lineIcon
        {
            get
            {
                if (_lineIcon == null) _lineIcon = Resources.LoadIcon("Hierarchy_Tree_Line");
                return _lineIcon;
            }
        }

        public static Texture2D middleIcon
        {
            get
            {
                if (_middleIcon == null) _middleIcon = Resources.LoadIcon("Hierarchy_Tree_Middle");
                return _middleIcon;
            }
        }

        static TreeDrawer()
        {
            HierarchyItemDrawer.Register("TreeDrawer", DrawTree, HierarchyToolOrder.Tree);
        }

        private static void DrawTree(HierarchyItem item)
        {
            if (!Prefs.hierarchyTree || Event.current.type != EventType.Repaint) return;
            if (item == null || item.gameObject == null) return;
            if (!string.IsNullOrEmpty(HierarchyItemDrawer.searchFilter)) return;

            Transform transform = item.gameObject.transform;
            
            Rect rect = item.rect;

            rect.width = 36;
            rect.x -= 32;
            
            Rect tooltipRect = rect;
            Vector2 mousePosition = Event.current.mousePosition;

            if (transform.childCount > 0)
            {
                tooltipRect.width -= 16;
                Rect arrowRect = rect;
                arrowRect.xMin += 16;
                bool isArrowHovered = arrowRect.Contains(mousePosition);
                int arrowId = GUIUtility.GetControlID(FocusType.Passive, arrowRect);
                GUIStyle.none.Draw(arrowRect, TempContent.Get("", $"The number of children is {transform.childCount}"), arrowId, isArrowHovered);
            }
            
            Transform parent = transform.parent;
            if (parent == null) return;

            Vector4 borderWidths = new Vector4(transform.childCount > 0 ? 8 : 0, 0, 0, 0);

            Color color = GetColor(item, transform);

            string tooltip = parent.gameObject.name;
            int id = GUIUtility.GetControlID(FocusType.Passive, tooltipRect);
            bool isHovered = tooltipRect.Contains(mousePosition);

            if (parent.childCount == 1 || transform.GetSiblingIndex() == parent.childCount - 1)
            {
                if (endIcon != null)
                {
                    GUIStyle.none.Draw(tooltipRect, TempContent.Get("", tooltip), id, isHovered); 
                    GUI.DrawTexture(rect, endIcon, ScaleMode.ScaleToFit, true, 0, color, borderWidths, Vector4.zero);
                }
            }
            else
            {
                if (middleIcon != null)
                {
                    GUIStyle.none.Draw(tooltipRect, TempContent.Get("", tooltip), id, isHovered);
                    GUI.DrawTexture(rect, middleIcon, ScaleMode.ScaleToFit, true, 0, color, borderWidths, Vector4.zero);
                }
            }

            if (lineIcon != null)
            {
                while (parent != null && parent.parent != null)
                {
                    rect.x -= 14;
                    id = GUIUtility.GetControlID(FocusType.Passive, rect);
                    isHovered = rect.Contains(mousePosition);

                    if (parent.GetSiblingIndex() < parent.parent.childCount - 1)
                    {
                        color = GetColor(item, parent);

                        GUI.DrawTexture(rect, lineIcon, ScaleMode.ScaleToFit, true, 0, color, borderWidths, Vector4.zero);
                        GUIStyle.none.Draw(rect, TempContent.Get("", parent.parent.gameObject.name), id, isHovered);
                    }

                    parent = parent.parent;
                }
            }
        }

        public static Color GetBackground(SceneReferences reference, GameObject target)
        {
            foreach (SceneReferences.HierarchyBackground b in reference.hierarchyBackgrounds)
            {
                if (b.gameObject == target) return b.color;
            }

            Transform t = target.transform.parent;
            Texture texture = AssetPreview.GetMiniThumbnail(target);
            string textureName = texture.name;
            if (textureName.StartsWith("sv_icon_"))
            {
                return GetColorFromTexture(textureName);
            }
            
            while (t != null)
            {
                GameObject go = t.gameObject;

                foreach (SceneReferences.HierarchyBackground b in reference.hierarchyBackgrounds)
                {
                    if (b.gameObject == go) return b.color;
                }

                texture = AssetPreview.GetMiniThumbnail(go);
                textureName = texture.name;
                if (textureName.StartsWith("sv_icon_"))
                {
                    return GetColorFromTexture(textureName);
                }

                t = t.parent;
            }

            return Color.gray;
        }

        public static Color GetBackground(GameObject target)
        {
            Transform t = target.transform.parent;

            while (t != null)
            {
                GameObject go = t.gameObject;

                Texture texture = AssetPreview.GetMiniThumbnail(go);
                string textureName = texture.name;
                if (textureName.StartsWith("sv_icon_"))
                {
                    return GetColorFromTexture(textureName);
                }

                t = t.parent;
            }

            return Color.gray;
        }

        public static Color GetColor(HierarchyItem item, Transform parent)
        {
            SceneReferences r = SceneReferences.Get(item.gameObject.scene, false);

            if (r == null) return GetBackground(parent.gameObject);
            Color color = GetBackground(r, parent.gameObject);
            if (parent != null && color == Color.gray) GetSelectionColor(parent, ref color);
            return color;
        }

        private static Color GetColorFromTexture(string textureName)
        {
            if (textureName[8] == 'n')
            {
                int index = textureName[12] - '0';
                return GameObjectHierarchySettings.colors[index];
            }

            if (textureName[8] == 'd')
            {
                int index;
                if (textureName.Length == 16) index = textureName[11] - '0';
                else if (textureName.Length == 17) index = (textureName[11] - '0') * 10 + (textureName[12] - '0');
                else return Color.gray;

                return GameObjectHierarchySettings.colors[index % 8];
            }

            return Color.gray;
        }

        private static void GetSelectionColor(Transform parent, ref Color color)
        {
            GameObject[] gameObjects = Selection.gameObjects;
            if (gameObjects == null || gameObjects.Length == 0) return;
            
            Transform t = parent.parent;
            while (t != null)
            {
                if (gameObjects.Any(g => g.transform == t))
                {
                    color = new Color(0.172f, 0.364f, 0.529f);
                }

                t = t.parent;
            }
        }
    }
}