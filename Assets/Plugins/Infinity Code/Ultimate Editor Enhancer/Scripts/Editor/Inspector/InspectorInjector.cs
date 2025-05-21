/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.InspectorTools
{
    public abstract class InspectorInjector
    {
        private double initTime;
        private List<EditorWindow> failedWindows;

        protected void InitInspector()
        {
            failedWindows = new List<EditorWindow>();

            InitWindowType(InspectorWindowRef.type);
            InitWindowType(PropertyEditorRef.type);

            if (failedWindows.Count > 0)
            {
                initTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += TryReinit;
            }
        }

        private void InitWindowType(Type type)
        {
            Object[] windows = UnityEngine.Resources.FindObjectsOfTypeAll(type);
            foreach (EditorWindow wnd in windows)
            {
                if (wnd == null) continue;
                if (!InjectBar(wnd))
                {
                    failedWindows.Add(wnd);
                }
            }
        }

        protected bool InjectBar(EditorWindow wnd)
        {
            VisualElement mainContainer = VisualElementHelper.GetMainContainer(wnd);
            if (mainContainer == null) return false;
            if (mainContainer.childCount < 2) return false;
            VisualElement editorsList = VisualElementHelper.GetVisualElementByClass(mainContainer, "unity-inspector-editors-list");

            return OnInject(wnd, mainContainer, editorsList);
        }

        protected abstract bool OnInject(EditorWindow wnd, VisualElement mainContainer, VisualElement editorsList);

        protected void OnMaximizedChanged(EditorWindow w)
        {
            Object[] windows = UnityEngine.Resources.FindObjectsOfTypeAll(InspectorWindowRef.type);
            foreach (EditorWindow wnd in windows)
            {
                if (wnd != null) InjectBar(wnd);
            }
        }

        private void TryReinit()
        {
            if (EditorApplication.timeSinceStartup - initTime <= 0.5) return;
            EditorApplication.update -= TryReinit;
            if (failedWindows != null)
            {
                TryReinit(failedWindows);
                failedWindows = null;
            }
        }

        private void TryReinit(List<EditorWindow> windows)
        {
            foreach (EditorWindow wnd in windows)
            {
                if (wnd == null) continue;
                InjectBar(wnd);
            }
        }
    }
}