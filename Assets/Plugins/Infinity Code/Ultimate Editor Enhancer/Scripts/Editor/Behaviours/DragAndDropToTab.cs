/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.Behaviors
{
    [InitializeOnLoad]
    public static class DragAndDropToTab
    {
        static DragAndDropToTab()
        {
            EditorApplication.update += Update;
        }

        private static EditorWindow CreatePropertyEditorWindow(Object obj)
        {
            EditorWindow wnd = ScriptableObject.CreateInstance(PropertyEditorRef.type) as EditorWindow;
            ActiveEditorTracker tracker = PropertyEditorRef.GetTracker(wnd);
            ActiveEditorTrackerRef.SetObjectsLockedByThisTracker(tracker, new List<Object>(){obj});
            return wnd;
        }

        private static void CreateWindow(object parent, Object obj)
        {
            EditorWindow wnd = CreatePropertyEditorWindow(obj);
            
            DockAreaRef.AddTab(parent, wnd);
            Texture icon = AssetPreview.GetMiniThumbnail(obj);
            wnd.titleContent = new GUIContent(obj.name, icon);
            wnd.Focus();
        }
        
        private static void ProcessDragAndDrop()
        {
            Event e = EventRef.s_Current;
            
            if (e == null) return;
            if (e.type != EventType.DragPerform && e.type != EventType.DragUpdated) return;
            if (DragAndDrop.objectReferences.Length != 1) return;

            EditorWindow wnd = WindowsHelper.mouseOverWindow;
            if (wnd == null) return;

            int height = wnd.GetType() == SceneHierarchyWindowRef.type ? 10 : 20;
            Rect rect = new Rect(0, 0, wnd.position.width, height);
            if (!rect.Contains(e.mousePosition)) return;
            
            Object obj = DragAndDrop.objectReferences[0];
            if (obj is DefaultAsset) return;
            
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            
            if (e.type != EventType.DragPerform) return;
            
            DragAndDrop.AcceptDrag();

            object parent = EditorWindowRef.GetParent(wnd);
            if (parent == null) return;

            CreateWindow(parent, obj);
        }

        private static void Update()
        {
            if (Prefs.dragAndDropToTab) ProcessDragAndDrop();
        }
    }
}