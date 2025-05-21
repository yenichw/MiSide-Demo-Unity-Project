/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class AssetsTreeViewDataSourceRef
    {
        private static FieldInfo _rootInstanceIDField;
        private static Type _type;

        public static FieldInfo rootInstanceIDField
        {
            get
            {
                if (_rootInstanceIDField == null) _rootInstanceIDField = type.GetField("m_rootInstanceID", Reflection.InstanceLookup);
                return _rootInstanceIDField;
            }
        }

        private static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("AssetsTreeViewDataSource");
                return _type;
            }
        }

        public static int GetRootInstanceID(object data)
        {
            return (int)rootInstanceIDField.GetValue(data);
        }

        public static void SetRootInstanceID(object data, int id)
        {
            rootInstanceIDField.SetValue(data, id);
        }
    }
}