/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.PopupWindows
{
    public class Hierarchy : PopupWindowItem
    {
        public override Texture icon
        {
            get { return EditorIconContents.hierarchyWindow.image; }
        }

        protected override string label
        {
            get { return "Hierarchy"; }
        }

        public override float order
        {
            get { return -80; }
        }

        private static void FinalizeWindow(EditorWindow window)
        {
            HierarchyHelper.ExpandHierarchy(window, Selection.activeGameObject);
        }

        protected override void ShowTab(Vector2 mousePosition)
        {
            Type windowType = SceneHierarchyWindowRef.type;
            Vector2 windowSize = Prefs.defaultWindowSize;
            Rect rect = new Rect(GUIUtility.GUIToScreenPoint(mousePosition) - windowSize / 2, Vector2.zero);

            Rect windowRect = new Rect(rect.position, windowSize);

            EditorWindow window = ScriptableObject.CreateInstance(windowType) as EditorWindow;
            window.Show();
            window.Focus();
            window.position = windowRect;

            FinalizeWindow(window);
        }

        protected override void ShowUtility(Vector2 mousePosition)
        {
            Type windowType = SceneHierarchyWindowRef.type;
            Vector2 windowSize = Prefs.defaultWindowSize;
            Rect rect = new Rect(GUIUtility.GUIToScreenPoint(mousePosition) - windowSize / 2, Vector2.zero);

            Rect windowRect = new Rect(rect.position, windowSize);

            EditorWindow window = ScriptableObject.CreateInstance(windowType) as EditorWindow;
            window.ShowUtility();
            window.Focus();
            window.position = windowRect;

            FinalizeWindow(window);
        }

        protected override void ShowPopup(Vector2 mousePosition)
        {
            Type windowType = SceneHierarchyWindowRef.type;
            Vector2 windowSize = Prefs.defaultWindowSize;
            Rect rect = new Rect(GUIUtility.GUIToScreenPoint(mousePosition) - windowSize / 2, Vector2.zero);

            Rect windowRect = new Rect(rect.position, windowSize);
            if (windowRect.y < 40) windowRect.y = 40;

            EditorWindow window = ScriptableObject.CreateInstance(windowType) as EditorWindow;
            window.position = windowRect;
            window.ShowPopup();
            window.Focus();
            EventManager.AddBinding(EventManager.ClosePopupEvent).OnInvoke += b =>
            {
                window.Close();
                b.Remove();
            };
            PinAndClose.Show(window, windowRect, window.Close, () =>
            {
                EditorWindow wnd = Object.Instantiate(window);
                wnd.Show();
                Rect wRect = window.position;
                wRect.yMin -= PinAndClose.Height;
                wnd.position = wRect;
                wnd.maxSize = new Vector2(4000f, 4000f);
                wnd.minSize = new Vector2(100f, 100f);
                window.Close();
            }, "Hierarchy");

            FinalizeWindow(window);
        }
    }
}