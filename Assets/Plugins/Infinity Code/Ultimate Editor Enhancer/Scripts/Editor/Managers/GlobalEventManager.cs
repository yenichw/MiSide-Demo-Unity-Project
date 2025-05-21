/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public class GlobalEventManager: BindingManager<GlobalEventManager.GlobalEvent>
    {
        static GlobalEventManager()
        {
            EditorApplication.CallbackFunction callback = EditorApplicationRef.GetGlobalEventHandler();
            callback = EditorGlobalEvent + callback;
            EditorApplicationRef.SetGlobalEventHandler(callback);
        }

        private static void EditorGlobalEvent()
        {
            for (int i = bindings.Count - 1; i >= 0; i--) bindings[i].TryInvoke();
        }

        public static GlobalEvent AddListener(Action action)
        {
            return Add(new GlobalEvent(action));
        }

        public class GlobalEvent
        {
            private Action action;

            public GlobalEvent(Action action)
            {
                this.action = action;
            }

            public void TryInvoke()
            {
                try
                {
                    if (action != null) action();
                }
                catch (Exception e)
                {
                    Log.Add(e);
                }
            }
        }
    }
}