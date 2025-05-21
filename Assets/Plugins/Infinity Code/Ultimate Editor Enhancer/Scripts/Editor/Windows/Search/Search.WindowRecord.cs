/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class Search
    {
        internal class WindowRecord : Record
        {
            private static Texture2D windowIcon;

            protected override Texture2D assetPreview
            {
                get
                {
                    if (windowIcon == null) windowIcon = Icons.winBtnWinMax as Texture2D;
                    return windowIcon;
                }
            }

            public override Object target
            {
                get => null;
            }

            public override string tooltip
            {
                get => _tooltip;
            }

            public override string type
            {
                get => "window";
            }

            public WindowRecord(string submenu, string label)
            {
                _tooltip = submenu;
                _label = submenu;
                search = new[] { label };
            }

            protected override void ShowContextMenu(int index)
            {
                GenericMenuEx menu = GenericMenuEx.Start();
                menu.Add("Open Window", () => SelectRecord(index, 1));
                menu.Show();
            }

            public override void Select(int state)
            {
                EditorApplication.ExecuteMenuItem(tooltip);
            }
        }
    }
}