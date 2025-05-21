/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class EditorWindowRef
    {
        private static FieldInfo _parentField;

        private static FieldInfo parentField
        {
            get
            {
                if (_parentField == null)
                {
                    _parentField = type.GetField("m_Parent", Reflection.InstanceLookup);
                }

                return _parentField;
            }
        }

        public static Type type
        {
            get => typeof(EditorWindow);
        }

        public static object GetParent(EditorWindow window)
        {
            return parentField.GetValue(window);
        }
    }
}