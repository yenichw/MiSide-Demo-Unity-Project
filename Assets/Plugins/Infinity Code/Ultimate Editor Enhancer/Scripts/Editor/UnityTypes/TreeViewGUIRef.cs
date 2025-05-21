/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class TreeViewGUIRef
    {
        private static FieldInfo _iconWidthField;
        private static FieldInfo _spaceBetweenIconAndTextField;
        private static Type _type;

        private static FieldInfo iconWidthField
        {
            get
            {
                if (_iconWidthField == null) _iconWidthField = type.GetField("k_IconWidth", Reflection.InstanceLookup);
                return _iconWidthField;
            }
        }

        private static FieldInfo spaceBetweenIconAndTextField
        {
            get
            {
                if (_spaceBetweenIconAndTextField == null) _spaceBetweenIconAndTextField = type.GetField("k_SpaceBetweenIconAndText", Reflection.InstanceLookup);
                return _spaceBetweenIconAndTextField;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("IMGUI.Controls.TreeViewGUI");
                return _type;
            }
        }

        public static void SetIconWidth(object instance, float size)
        {
            iconWidthField.SetValue(instance, size);
        }

        public static void SetSpaceBetweenIconAndText(object instance, float space)
        {
            spaceBetweenIconAndTextField.SetValue(instance, space);
        }
    }
}