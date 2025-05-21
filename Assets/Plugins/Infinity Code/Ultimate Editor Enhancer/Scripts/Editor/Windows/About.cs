/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public class About: EditorWindow
    {
        private string years = DateTime.Now.Year.ToString();

        [MenuItem(WindowsHelper.MenuPath + "About", false, MenuItemOrder.About)]
        public static void OpenWindow()
        {
            About window = GetWindow<About>(true, "About", true);
            window.minSize = new Vector2(200, 100);
            window.maxSize = new Vector2(200, 100);
        }

        public void OnGUI()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle textStyle = new GUIStyle(EditorStyles.label);
            textStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("Ultimate Editor Enhancer", titleStyle);
            GUILayout.Label("version " + Version.version, textStyle);
            GUILayout.Label("created by Infinity Code", textStyle);
            GUILayout.Label(years, textStyle);
        }
    }
}