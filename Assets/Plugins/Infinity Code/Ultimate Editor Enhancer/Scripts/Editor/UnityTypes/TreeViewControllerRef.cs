/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class TreeViewControllerRef
    {
        private static MethodInfo _changeExpandedStateMethod;
        private static PropertyInfo _dataProp;
        private static PropertyInfo _guiProp;
        private static MethodInfo _onGUIMethod;
        private static MethodInfo _reloadDataMethod;
        private static Type _type;
        private static MethodInfo _userInputChangedExpandedStateMethod;
        private static FieldInfo _useExpansionAnimation;

        public static FieldInfo useExpansionAnimationField
        {
            get
            {
                if (_useExpansionAnimation == null) _useExpansionAnimation = type.GetField("m_UseExpansionAnimation", Reflection.InstanceLookup);
                return _useExpansionAnimation;
            }
        }

        private static MethodInfo changeExpandedStateMethod
        {
            get
            {
                if (_changeExpandedStateMethod == null) _changeExpandedStateMethod = type.GetMethod("ChangeExpandedState", Reflection.InstanceLookup);
                return _changeExpandedStateMethod;
            }
        }

        private static PropertyInfo dataProp
        {
            get
            {
                if (_dataProp == null) _dataProp = type.GetProperty("data", Reflection.InstanceLookup);
                return _dataProp;
            }
        }

        private static PropertyInfo guiProp
        {
            get
            {
                if (_guiProp == null) _guiProp = type.GetProperty("gui", Reflection.InstanceLookup);
                return _guiProp;
            }
        }

        private static MethodInfo onGUIMethod
        {
            get
            {
                if (_onGUIMethod == null) _onGUIMethod = type.GetMethod("OnGUI", Reflection.InstanceLookup, null, new[] { typeof(Rect), typeof(int) }, null); 
                return _onGUIMethod;
            }
        }

        private static MethodInfo reloadDataMethod
        {
            get
            {
                if (_reloadDataMethod == null) _reloadDataMethod = type.GetMethod("ReloadData", Reflection.InstanceLookup);
                return _reloadDataMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("IMGUI.Controls.TreeViewController");
                return _type;
            }
        }

        public static MethodInfo userInputChangedExpandedStateMethod
        {
            get
            {
                if (_userInputChangedExpandedStateMethod == null) _userInputChangedExpandedStateMethod = type.GetMethod("UserInputChangedExpandedState", Reflection.InstanceLookup);
                return _userInputChangedExpandedStateMethod;
            }
        }

        public static void ChangeExpandedState(object instance, TreeViewItem item, bool expanded, bool includeChildren)
        {
            changeExpandedStateMethod.Invoke(instance, new object[] { item, expanded, includeChildren });
        }

        public static object GetData(object instance)
        {
            return dataProp.GetValue(instance);
        }

        public static object GetGUI(object instance)
        {
            return guiProp.GetValue(instance);
        }

        public static bool GetUseExpansionAnimation(object instance)
        {
            return (bool)useExpansionAnimationField.GetValue(instance);
        }

        public static void OnGUI(object instance, Rect rect, int keyboardControlID)
        {
            onGUIMethod.Invoke(instance, new object[] { rect, keyboardControlID });
        }

        public static void ReloadData(object instance)
        {
            reloadDataMethod.Invoke(instance, null);
        }

        public static void SetUseExpansionAnimation(object instance, bool value)
        {
            useExpansionAnimationField.SetValue(instance, value);
        }
    }
}