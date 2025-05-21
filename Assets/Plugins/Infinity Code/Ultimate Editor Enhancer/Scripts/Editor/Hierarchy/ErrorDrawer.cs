/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [InitializeOnLoad]
    public static class ErrorDrawer
    {
        static ErrorDrawer()
        {
            HierarchyItemDrawer.Register("ErrorDrawer", DrawHierarchyItem, HierarchyToolOrder.Error);
        }

        private static void DrawHierarchyItem(HierarchyItem item)
        {
            if (!Prefs.hierarchyIcons || !Prefs.hierarchyErrorIcons) return;

            List<LogManager.Entry> entries = LogManager.GetEntries(item.id);
            if (entries == null || entries.Count <= 0) return;
            if (entries[0] == null || string.IsNullOrEmpty(entries[0].message)) return;

            GUIContent content = EditorIconContents.consoleErrorIconSmall;
            StringBuilder builder = StaticStringBuilder.Start();
            builder.Append(entries[0].message.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)[0]);
            if (entries.Count > 1) builder.Append("\n+").Append(entries.Count - 1).Append(" errors");

            content.tooltip = builder.ToString();
            Rect localRect = new Rect(item.rect);
            localRect.xMin = localRect.xMax - 20;
            item.rect.width -= 22;

            if (GUI.Button(localRect, content, GUIStyle.none)) entries[0].Open();
        }
    }
}