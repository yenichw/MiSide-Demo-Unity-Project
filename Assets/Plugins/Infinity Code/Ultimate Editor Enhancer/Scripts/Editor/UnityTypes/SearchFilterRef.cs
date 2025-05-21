/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class SearchFilterRef
    {
        private static FieldInfo _foldersField;
        private static Type _type;

        private static FieldInfo foldersField
        {
            get
            {
                if (_foldersField == null) _foldersField = type.GetField("m_Folders", Reflection.InstanceLookup);
                return _foldersField;
            }
        }

        private static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("SearchFilter");
                return _type;
            }
        }

        public static string[] GetFolders(object searchFilter)
        {
            return (string[])foldersField.GetValue(searchFilter);
        }

        public static void SetFolders(object searchFilter, string[] folders)
        {
            foldersField.SetValue(searchFilter, folders);
        }
    }
}