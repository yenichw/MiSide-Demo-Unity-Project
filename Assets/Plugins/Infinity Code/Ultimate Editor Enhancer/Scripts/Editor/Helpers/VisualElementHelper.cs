/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class VisualElementHelper
    {
        public static VisualElement GetMainContainer(EditorWindow wnd)
        {
            return wnd != null ? GetVisualElementByClass(wnd.rootVisualElement, "unity-inspector-main-container") : null;
        }

        public static VisualElement GetVisualElementByClass(VisualElement element, string className)
        {
            for (int i = 0; i < element.childCount; i++)
            {
                VisualElement el = element[i];
                if (el.ClassListContains(className)) return el;
                el = GetVisualElementByClass(el, className);
                if (el != null) return el;
            }

            return null;
        }
    }
}