/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public static class WindowManager
    {
        public static Action<EditorWindow> OnMaximizedChanged;
        public static Action<EditorWindow> OnWindowFocused;

        private static EditorWindow focusedWindow;
        private static bool isMaximized;

        static WindowManager()
        {
            EditorApplication.update += Update;
            focusedWindow = EditorWindow.focusedWindow;
            isMaximized = focusedWindow != null && focusedWindow.maximized;
        }

        private static void Update()
        {
            if (focusedWindow != EditorWindow.focusedWindow)
            {
                focusedWindow = EditorWindow.focusedWindow;
                if (OnWindowFocused != null) OnWindowFocused(focusedWindow);
            }

            bool maximized = focusedWindow != null && focusedWindow.maximized;
            if (maximized != isMaximized)
            {
                isMaximized = maximized;
                if (OnMaximizedChanged != null) OnMaximizedChanged(focusedWindow);
            }
        }
    }
}