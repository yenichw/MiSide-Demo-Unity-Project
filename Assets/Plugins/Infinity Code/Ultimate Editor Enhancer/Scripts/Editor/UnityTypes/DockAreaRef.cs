/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class DockAreaRef
    {
        private static MethodInfo _addTabMethod;
        private static FieldInfo _panesField;
        private static Type _type;

        private static MethodInfo addTabMethod
        {
            get
            {
                if (_addTabMethod == null) _addTabMethod = type.GetMethod("AddTab", Reflection.InstanceLookup, null, new[] { typeof(EditorWindow), typeof(bool) }, null);
                return _addTabMethod;
            }
        }
        
        private static FieldInfo panesField
        {
            get
            {
                if (_panesField == null) _panesField = type.GetField("m_Panes", Reflection.InstanceLookup);
                return _panesField;
            }
        }

        private static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("DockArea");
                return _type;
            }
        }
        
        public static void AddTab(object dockArea, EditorWindow pane, bool sendPaneEvents = true)
        {
            addTabMethod.Invoke(dockArea, new object[] { pane, sendPaneEvents });
        }
        
        public static List<EditorWindow> GetPanes(object dockArea)
        {
            return panesField.GetValue(dockArea) as List<EditorWindow>;
        }
    }
}