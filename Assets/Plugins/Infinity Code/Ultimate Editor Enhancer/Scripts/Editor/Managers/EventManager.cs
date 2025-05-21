/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public class EventManager: BindingManager<EventManager.EventBinding>
    {
        public const string ClosePopupEvent = "ClosePopup";
        public const string ToggleWindowsEvent = "ToggleWindows";

        static EventManager()
        {
            KeyManager.AddBinding(KeyCode.Escape).OnPress += OnEscape;
            KeyManager.AddBinding(KeyCode.Space, true, true).OnPress += OnToggle;

            EditorApplication.wantsToQuit += OnQuitting;
        }

        public static EventBinding AddBinding(string eventName)
        {
            return Add(new EventBinding(eventName));
        }

        public static void Broadcast(string eventName)
        {
            for (int i = bindings.Count - 1; i >= 0; i--) bindings[i].TryInvoke(eventName);
        }

        public static void BroadcastClosePopup()
        {
            Broadcast(ClosePopupEvent);
        }

        private static void OnEscape()
        {
            BroadcastClosePopup();
        }

        private static bool OnQuitting()
        {
            BroadcastClosePopup();
            return true;
        }

        private static void OnToggle()
        {
            Broadcast(ToggleWindowsEvent);
        }

        public static void RemoveBinding(EventBinding keyBinding)
        {
            bindings.Remove(keyBinding);
            keyBinding.Dispose();
        }

        public class EventBinding
        {
            public Action<EventBinding> OnInvoke;
            private string eventName;

            internal EventBinding(string eventName)
            {
                this.eventName = eventName;
            }

            public void Dispose()
            {
                OnInvoke = null;
            }

            public void Remove()
            {
                RemoveBinding(this);
            }

            public void TryInvoke(string eventName)
            {
                if (eventName == this.eventName)
                {
                    if (OnInvoke != null) OnInvoke(this);
                }
            }
        }
    }
}