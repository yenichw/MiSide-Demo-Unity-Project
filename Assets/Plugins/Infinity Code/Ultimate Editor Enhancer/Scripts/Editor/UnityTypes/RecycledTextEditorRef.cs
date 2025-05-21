/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class RecycledTextEditorRef
    {
        private static MethodInfo _endEditingMethod;
        private static MethodInfo _isEditingControlMethod;
        private static Type _type;

        private static MethodInfo endEditingMethod
        {
            get
            {
                if (_endEditingMethod == null) _endEditingMethod = type.GetMethod("EndEditing", Reflection.InstanceLookup);
                return _endEditingMethod;
            }
        }

        private static MethodInfo isEditingControlMethod
        {
            get
            {
                if (_isEditingControlMethod == null)
                {
                    _isEditingControlMethod = type.GetMethod(
                        "IsEditingControl",
                        Reflection.InstanceLookup,
                        null,
                        new[] { typeof(int) },
                        null);
                }
                return _isEditingControlMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("EditorGUI+RecycledTextEditor");
                return _type;
            }
        }

        public static void EndEditing(object editor)
        {
            endEditingMethod.Invoke(editor, null);
        }

        public static bool IsEditingControl(object recycledEditor, int id)
        {
            return (bool)isEditingControlMethod.Invoke(recycledEditor, new object[] { id });
        }
    }
}