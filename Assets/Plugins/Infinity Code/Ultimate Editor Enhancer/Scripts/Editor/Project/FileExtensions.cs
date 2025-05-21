/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class FileExtensions
    {
        private static Dictionary<string, (string, float)> widthCache = new Dictionary<string, (string, float)>();
        
        static FileExtensions()
        {
            ProjectItemDrawer.Register("FILE_EXTENSION", DrawButton, ProjectToolOrder.FileExtension);
        }

        private static void DrawButton(ProjectItem item)
        {
            if (!Prefs.projectFileExtension) return;
            if (item.isFolder) return;
            if (item.rect.width < 128) return;
            string path = item.path;
            if (!path.StartsWith("Assets")) return;
            
            GUIStyle style = Styles.smallLabel;
            (string, float) data;
            float width;
            string extension;
            
            if (!widthCache.TryGetValue(item.guid, out data))
            {
                extension = new FileInfo(path).Extension;
                GUIContent content = TempContent.Get(extension);
                width = style.CalcSize(content).x;
                widthCache[item.guid] = (extension, width);
            }
            else
            {
                extension = data.Item1;
                width = data.Item2;
            }

            Rect r = item.rect;
            r.xMin = r.xMax - width;
            r.height = 16;
            
            item.rect.xMax -= width;
            
            Event e = Event.current;
            if (e.type != EventType.Repaint) return;
            
            GUI.Label(r, TempContent.Get(extension), style);
        }
    }
}