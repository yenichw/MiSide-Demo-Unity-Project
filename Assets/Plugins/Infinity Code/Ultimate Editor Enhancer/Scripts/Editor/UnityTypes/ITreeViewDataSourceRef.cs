/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ITreeViewDataSourceRef
    {
        private static MethodInfo _isExpandedMethod;
        public static Type _type;

        public static MethodInfo isExpandedMethod
        {
            get
            {
                if (_isExpandedMethod == null) _isExpandedMethod = type.GetMethod("IsExpanded", Reflection.InstanceLookup, null, new []{typeof(int)}, null);
                return _isExpandedMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("IMGUI.Controls.ITreeViewDataSource"); ;
                return _type;
            }
        }

        public static bool IsExpanded(object instance, int id)
        {
            return (bool)isExpandedMethod.Invoke(instance, new object[] {id});
        }
    }
}