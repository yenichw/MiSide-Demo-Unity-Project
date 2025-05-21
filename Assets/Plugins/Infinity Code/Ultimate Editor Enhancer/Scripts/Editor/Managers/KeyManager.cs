/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    [InitializeOnLoad]
    public class KeyManager : BindingManager<KeyManager.KeyBinding>
    {
        private static Dictionary<KeyCode, bool> _isDown = new Dictionary<KeyCode, bool>();

        static KeyManager()
        {
            GlobalEventManager.AddListener(OnGlobalEvent);
        }

        public static KeyBinding AddBinding()
        {
            return Add(new KeyBinding());
        }

        public static KeyBinding AddBinding(KeyCode keyCode, bool shift = false, bool control = false)
        {
            return Add(new KeyBinding(keyCode, shift, control));
        }

        public static bool IsKeyDown(KeyCode key)
        {
            bool v;
            if (_isDown.TryGetValue(key, out v)) return v;
            return false;
        }

        public static bool IsModifier(KeyCode key)
        {
            if (key == KeyCode.LeftControl) return true;
            if (key == KeyCode.RightControl) return true;
            if (key == KeyCode.LeftShift) return true;
            if (key == KeyCode.RightShift) return true;
            if (key == KeyCode.LeftAlt) return true;
            if (key == KeyCode.RightAlt) return true;
            if (key == KeyCode.LeftCommand) return true;
            if (key == KeyCode.RightCommand) return true;

            return false;
        }

        private static void OnGlobalEvent()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                _isDown[Event.current.keyCode] = true;
                for (int i = bindings.Count - 1; i >= 0; i--) bindings[i].TryInvokePress();
            }
            else if (Event.current.type == EventType.KeyUp)
            {
                _isDown[Event.current.keyCode] = false;
                for (int i = bindings.Count - 1; i >= 0; i--) bindings[i].TryInvokeRelease();
            }
        }

        public static void RemoveBinding(KeyBinding keyBinding)
        {
            bindings.Remove(keyBinding);
            keyBinding.Dispose();
        }

        public class KeyBinding
        {
            public Action OnPress;
            public Action OnPressOnRelease;
            public Action OnRelease;
            public Func<bool> OnValidate;
            private bool useValidate;
            private KeyCode keyCode;
            private bool shift;
            private bool control;

            internal KeyBinding()
            {
                useValidate = true;
            }

            internal KeyBinding(KeyCode keyCode, bool shift, bool control)
            {
                this.keyCode = keyCode;
                this.shift = shift;
                this.control = control;
            }

            public void Dispose()
            {
                OnPress = null;
                OnRelease = null;
            }

            public void Remove()
            {
                RemoveBinding(this);
            }

            public void TryInvokePress()
            {
                if (useValidate)
                {
                    try
                    {
                        if (OnValidate != null && !OnValidate()) return;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    Event e = Event.current;
                    if (e.keyCode != keyCode || e.shift != shift || e.control != control) return;
                }
                
                try
                {
                    if (OnPress != null) OnPress();
                    if (OnPressOnRelease != null) OnPressOnRelease();
                }
                catch
                {
                }
            }

            public void TryInvokeRelease()
            {
                if (useValidate)
                {
                    try
                    {
                        if (OnValidate != null && !OnValidate()) return;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    Event e = Event.current;
                    if (e.keyCode != keyCode || e.shift != shift || e.control != control) return;
                }

                try
                {
                    if (OnRelease != null) OnRelease();
                    if (OnPressOnRelease != null) OnPressOnRelease();
                }
                catch
                {
                }
            }
        }
    }
}