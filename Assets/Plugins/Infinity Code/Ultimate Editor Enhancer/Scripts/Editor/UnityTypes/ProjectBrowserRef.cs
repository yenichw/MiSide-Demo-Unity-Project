/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class ProjectBrowserRef
    {
        #region FIELDS
        
        private static FieldInfo _assetTreeStateField;
        private static FieldInfo _folderTreeStateField;
        private static MethodInfo _isTwoColumnsMethod;
        private static Type _type;
        
        #endregion

        #region PROPERTIES

        private static FieldInfo assetTreeStateField
        {
            get
            {
                if (_assetTreeStateField == null) _assetTreeStateField = type.GetField("m_AssetTreeState", Reflection.InstanceLookup);
                return _assetTreeStateField;
            }
        }

        private static FieldInfo folderTreeStateField
        {
            get
            {
                if (_folderTreeStateField == null) _folderTreeStateField = type.GetField("m_FolderTreeState", Reflection.InstanceLookup);
                return _folderTreeStateField;
            }
        }

        private static MethodInfo isTwoColumnsMethod
        {
            get
            {
                if (_isTwoColumnsMethod == null) _isTwoColumnsMethod = type.GetMethod("IsTwoColumns", Reflection.InstanceLookup);
                return _isTwoColumnsMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("ProjectBrowser");
                return _type;
            }
        }
        
        #endregion

        #region METHODS

        public static TreeViewState GetAssetTreeViewState(Object projectWindow)
        {
            return assetTreeStateField.GetValue(projectWindow) as TreeViewState;
        }

        public static TreeViewState GetFolderTreeViewState(Object projectWindow)
        {
            return folderTreeStateField.GetValue(projectWindow) as TreeViewState;
        }

        public static bool IsTwoColumns(Object projectWindow)
        {
            return (bool) isTwoColumnsMethod.Invoke(projectWindow, null);
        }
        
        #endregion
    }
}