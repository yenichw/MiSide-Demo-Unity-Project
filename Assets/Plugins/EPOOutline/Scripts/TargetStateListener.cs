using System;
using System.Collections.Generic;
using UnityEngine;

namespace EPOOutline
{
    [ExecuteAlways]
    public class TargetStateListener : MonoBehaviour
    {
        public struct Callback
        {
            public readonly Outlinable Target;
            public readonly Action Action;

            public Callback(Outlinable target, Action action)
            {
                Target = target;
                Action = action;
            }
        }

        private List<Callback> callbacks = new List<Callback>();

        public void AddCallback(Outlinable outlinable, Action action)
        {
            callbacks.Add(new Callback(outlinable, action));
        }

        public void RemoveCallback(Outlinable outlinable, Action callback)
        {
            var found = callbacks.FindIndex(x => x.Target == outlinable && x.Action == callback);
            if (found == -1)
                return;
            
            callbacks.RemoveAt(found);
        }

        private void Awake()
        {
            hideFlags = HideFlags.HideInInspector;
        }

        public void ForceUpdate()
        {
            callbacks.RemoveAll(x => x.Target == null);
            foreach (var callback in callbacks)
                callback.Action();
        }

        private void OnBecameVisible()
        {
            ForceUpdate();
        }

        private void OnBecameInvisible()
        {
            ForceUpdate();
        }
    }
}