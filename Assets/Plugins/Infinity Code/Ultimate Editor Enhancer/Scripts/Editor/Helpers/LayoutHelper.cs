/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class LayoutHelper
    {
        private static void AddSplitView(Object view, List<Object> itemToSave)
        {
            itemToSave.Add(view);

            Object[] children = ViewRef.GetChildren(view);

            foreach (Object child in children)
            {
                if (child.GetType().Name == "SplitView")
                {
                    AddSplitView(child, itemToSave);
                    continue;
                }
                
                itemToSave.Add(child);
                
                List<EditorWindow> windows = DockAreaRef.GetPanes(child);

                foreach (EditorWindow wnd in windows)
                {
                    itemToSave.Add(wnd);
                }
            }
        }

        public static void LoadLayout(string path)
        {
            if (!File.Exists(path)) return;
            
            Object[] objects = InternalEditorUtility.LoadSerializedFileAndForget(path);
            if (objects == null || objects.Length == 0) return;
            
            Object containerWindow = objects[0];
            if (containerWindow == null) return;
            
            int showMode = ContainerWindowRef.GetShowMode(containerWindow);
            
            ContainerWindowRef.Show(containerWindow, showMode, true, true, true);
        }
        
        public static void LoadLayoutFromText(string text)
        {
            string filename = DateTime.Now.Ticks + ".tmp";
            File.WriteAllText(filename, text);
            LoadLayout(filename);
            File.Delete(filename);
        }

        public static string SaveLayout(EditorWindow window)
        {
            return SaveLayout(window, out EditorWindow[] _);
        }

        public static string SaveLayout(EditorWindow window, out EditorWindow[] windows)
        {
            string filename = DateTime.Now.Ticks + ".tmp";
            SaveLayout(window, filename, out windows);
            if (!File.Exists(filename)) return null;
            
            string text = File.ReadAllText(filename);
            File.Delete(filename);
            return text;

        }

        public static void SaveLayout(EditorWindow window, string path)
        {
            SaveLayout(window, path, out EditorWindow[] _);
        }

        public static void SaveLayout(EditorWindow window, string path, out EditorWindow[] windows)
        {
            Object parent = EditorWindowRef.GetParent(window) as Object;
            windows = null;
            if (parent == null) return;

            if (parent == null || parent.GetType().Name != "DockArea") return;
            
            Object containerWindow = ViewRef.GetWindow(parent);
            if (containerWindow == null) return;

            try
            {
                List<Object> itemsToSave = new List<Object>();
                itemsToSave.Add(containerWindow);

                Object rootView = ContainerWindowRef.GetRootView(containerWindow);
                AddSplitView(rootView, itemsToSave);

                windows = itemsToSave.Where(i => i is EditorWindow).Cast<EditorWindow>().ToArray();
                    
                InternalEditorUtility.SaveToSerializedFileAndForget(itemsToSave.ToArray(), path, true);
            }
            catch
            {
            }
        }
    }
}