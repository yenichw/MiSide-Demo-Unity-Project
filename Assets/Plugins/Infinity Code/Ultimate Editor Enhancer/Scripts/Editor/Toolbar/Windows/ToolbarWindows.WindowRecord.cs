/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    public static partial class ToolbarWindows
    {
        public class WindowRecord
        {
            public Type type;
            public string title;
            public bool used;

            public WindowRecord(EditorWindow window)
            {
                type = window.GetType();
                title = window.titleContent.text;
                used = true;
            }
        }
    }
}