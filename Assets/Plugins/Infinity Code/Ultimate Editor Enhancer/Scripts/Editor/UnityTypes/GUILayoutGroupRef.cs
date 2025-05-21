/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class GUILayoutGroupRef
    {
        private static MethodInfo _peekNextMethod;
        private static Type _type;

        private static MethodInfo peekNextMethod
        {
            get
            {
                if (_peekNextMethod == null)
                {
                    _peekNextMethod = Reflection.GetMethod(type, "PeekNext", new Type[0], Reflection.InstanceLookup);
                }

                return _peekNextMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorTypeFromAssembly("GUILayoutGroup", "UnityEngine", "UnityEngine");
                return _type;
            }
        }

        public static Rect PeekNext(object group)
        {
            return (Rect)peekNextMethod.Invoke(group, null);
        }
    }
}