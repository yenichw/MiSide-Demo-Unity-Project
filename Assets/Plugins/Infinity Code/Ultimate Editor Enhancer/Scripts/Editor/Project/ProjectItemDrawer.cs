/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public class ProjectItemDrawer
    {
        public static Action<ProjectItem> OnStopped;
        private static List<Listener> listeners;
        private static bool isDirty;
        private static bool isStopped;
        private static ProjectItem item;
        private static string lastHoveredGUID;

        static ProjectItemDrawer()
        {
            if (Prefs.projectTools) Enable();
            item = new ProjectItem();
        }

        private static int CompareListeners(Listener i1, Listener i2)
        {
            if (i1.order == i2.order) return 0;
            if (i1.order > i2.order) return 1;
            return -1;
        }

        public static void Disable()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectItemGUI;
        }

        public static void Enable()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;
        }

        private static void OnProjectItemGUI(string guid, Rect rect)
        {
            if (listeners == null) return;

            if (isDirty)
            {
                listeners.Sort(CompareListeners);
                isDirty = false;
            }

            EditorWindow mouseOverWindow = WindowsHelper.mouseOverWindow;
            if (mouseOverWindow != null && mouseOverWindow.GetType() == ProjectBrowserRef.type && mouseOverWindow.wantsMouseMove == false)
            {
                mouseOverWindow.wantsMouseMove = true;
            }

            item.Set(guid, rect);
            bool needRepaint = false;

            if (item.hovered && lastHoveredGUID != item.guid)
            {
                lastHoveredGUID = item.guid;
                needRepaint = true;
            }

            foreach (Listener listener in listeners)
            {
                if (listener.action != null)
                {
                    try
                    {
                        listener.action(item);
                    }
                    catch (Exception e)
                    {
                        Log.Add(e);
                    }
                }
                if (isStopped) break;
            }

            if (needRepaint && mouseOverWindow != null) mouseOverWindow.Repaint();

            isStopped = false;
        }

        public static void Register(string id, Action<ProjectItem> action, float order = 0)
        {
            if (string.IsNullOrEmpty(id)) throw new Exception("ID cannot be empty");
            if (listeners == null) listeners = new List<Listener>();

            int hash = id.GetHashCode();
            foreach (Listener listener in listeners)
            {
                if (listener.hash == hash && listener.id == id)
                {
                    listener.action = action;
                    listener.order = order;
                    return;
                }
            }
            listeners.Add(new Listener
            {
                id = id,
                hash = hash,
                action = action,
                order = order
            });

            isDirty = true;
        }

        public static void StopCurrentRowGUI()
        {
            isStopped = true;
            if (OnStopped != null) OnStopped(item);
        }

        private class Listener
        {
            public int hash;
            public string id;
            public Action<ProjectItem> action;
            public float order;
        }
    }
}