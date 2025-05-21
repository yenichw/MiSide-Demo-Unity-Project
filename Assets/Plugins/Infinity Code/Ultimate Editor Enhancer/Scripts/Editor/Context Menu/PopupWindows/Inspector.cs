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
    public class Inspector : PopupWindowItem
    {
        public override Texture icon
        {
            get { return EditorIconContents.inspectorWindow.image; }
        }

        protected override string label
        {
            get { return "Inspector"; }
        }

        public override float order
        {
            get { return 90; }
        }

        protected override void ShowTab(Vector2 mousePosition)
        {
            Vector2 windowSize = Prefs.defaultWindowSize;
            Rect rect = new Rect(GUIUtility.GUIToScreenPoint(mousePosition) - new Vector2(150, 150), Vector2.zero);

            EditorWindow window = ScriptableObject.CreateInstance(InspectorWindowRef.type) as EditorWindow;
            window.Show();
            window.Focus();
            window.position = new Rect(rect.position, windowSize);
        }

        protected override void ShowUtility(Vector2 mousePosition)
        {
            Vector2 windowSize = Prefs.defaultWindowSize;
            Rect rect = new Rect(GUIUtility.GUIToScreenPoint(mousePosition) - new Vector2(150, 150), Vector2.zero);

            EditorWindow window = ScriptableObject.CreateInstance(InspectorWindowRef.type) as EditorWindow;
            window.ShowUtility();
            window.Focus();
            window.position = new Rect(rect.position, windowSize);
        }

        protected override void ShowPopup(Vector2 mousePosition)
        {
            Vector2 windowSize = Prefs.defaultWindowSize;
            Rect rect = new Rect(GUIUtility.GUIToScreenPoint(mousePosition) - new Vector2(150, 150), Vector2.zero);

            EditorWindow window = ScriptableObject.CreateInstance(InspectorWindowRef.type) as EditorWindow;
            Rect windowRect = new Rect(rect.position, windowSize);
            if (windowRect.y < 40) windowRect.y = 40;

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
            }, "Inspector");
        }
    }
}