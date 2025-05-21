/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    public class ProjectBookmarkComparer : IComparer<ProjectBookmark>
    {
        public int Compare(ProjectBookmark x, ProjectBookmark y)
        {
            bool f1 = x.target is DefaultAsset;
            bool f2 = y.target is DefaultAsset;
            if (f1 != f2) return f1 ? -1 : 1;
                
            return string.Compare(x.title, y.title, StringComparison.InvariantCulture);
        }
    }
}