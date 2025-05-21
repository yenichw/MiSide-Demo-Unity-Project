/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class EditorUtilityRef
    {
        private static MethodInfo _displayObjectContextMenuMethod;

        private static MethodInfo displayObjectContextMenuMethod
        {
            get
            {
                if (_displayObjectContextMenuMethod == null)
                {
                    _displayObjectContextMenuMethod = Reflection.GetMethod(type, "DisplayObjectContextMenu",
                        new[]
                        {
                            typeof(Rect),
                            typeof(Object),
                            typeof(int)
                        }, Reflection.StaticLookup);
                }

                return _displayObjectContextMenuMethod;
            }
        }

        public static Type type
        {
            get => typeof(EditorUtility);
        }

        public static void DisplayObjectContextMenu(Rect position, Object context, int contextUserData)
        {
            displayObjectContextMenuMethod.Invoke(null, new object[] { position, context, contextUserData });
        }
    }
}