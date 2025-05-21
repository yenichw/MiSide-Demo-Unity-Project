/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.JSON;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [Serializable]
    public class ProjectFolderRule
    {
        public string folderName;
        public string icon;
        public Color color = Color.white;
        public bool rootOnly = false;
        public bool recursive = true;

        [NonSerialized]
        private bool isDirty = true;
        
        [NonSerialized]
        private Texture _iconTexture;
        
        [NonSerialized]
        private string[] _parts;
        
        public string[] parts
        {
            get
            {
                if (_parts == null) _parts = folderName.ToUpperInvariant().Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
                return _parts;
            }
        }

        public JsonItem json
        {
            get
            { 
                return Json.Serialize(this) as JsonObject;
            }
        }
        
        public Texture iconTexture
        {
            get
            {
                if (isDirty)
                {
                    isDirty = false;

                    if (!string.IsNullOrEmpty(icon))
                    {
                        GUIContent content = EditorGUIUtility.IconContent(icon);
                        if (content != null) _iconTexture = content.image;
                        else _iconTexture = null;
                    }
                    else _iconTexture = null;
                }

                return _iconTexture;
            }
        }

        public void Draw(Rect rect, bool expanded, bool isEmpty)
        {
            Rect r2 = new Rect();
            bool drawIcon = false;
            Texture folderTexture = Icons.folder;
            
            Rect r = GetRect(rect, expanded, isEmpty, ref r2, ref drawIcon, ref folderTexture);

            if (Prefs.projectFolderIconsDrawColors)
            {
                Color clr = GUI.color;
                GUI.color = color;
                GUI.DrawTexture(r, folderTexture);
                GUI.color = clr;
            }

            if (drawIcon && Prefs.projectFolderIconsDrawIcons)
            {
                Texture img = iconTexture;

                if (img != null)
                {
                    GUI.DrawTexture(r2, img);
                }
            }
        }

        private Rect GetRect(Rect rect, bool expanded, bool isEmpty, ref Rect r2, ref bool drawIcon, ref Texture folderTexture)
        {
            if (rect.height > 20)
            {
                r2 = new Rect(rect.x + rect.width / 2, rect.y + rect.width / 2.2f, rect.width / 3, rect.width / 3);
                drawIcon = true;
                return new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.width + 2);
            }
            
            if (isEmpty) folderTexture = Icons.folderEmpty;
            else if (expanded) folderTexture = Icons.folderOpen;

            if (rect.x == 16 && rect.height == 16)
            {
                return new Rect(rect.x, rect.y - 1, rect.height + 2, rect.height + 2);
            }
            
            if (rect.x > 20)
            {
                return new Rect(rect.x - 1, rect.y - 1, rect.height + 2, rect.height + 2);
            }

            return new Rect(rect.x + 2, rect.y - 1, rect.height + 2, rect.height + 2);
        }

        public void SetDirty()
        {
            isDirty = true;
            _parts = null;
        }
    }
}