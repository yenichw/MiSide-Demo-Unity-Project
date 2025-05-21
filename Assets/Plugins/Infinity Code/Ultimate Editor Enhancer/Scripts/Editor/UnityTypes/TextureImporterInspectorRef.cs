/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Reflection;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class TextureImporterInspectorRef
    {
        private static MethodInfo _onApplyRevertGUIMethod;
        private static Type _type;

        private static MethodInfo onApplyRevertGUIMethod
        {
            get
            {
                if (_onApplyRevertGUIMethod == null) _onApplyRevertGUIMethod = type.GetMethod("OnApplyRevertGUI", Reflection.InstanceLookup);
                return _onApplyRevertGUIMethod;
            }
        }

        public static Type type
        {
            get
            {
                if (_type == null) _type = Reflection.GetEditorType("TextureImporterInspector");
                return _type;
            }
        }

        public static bool OnApplyRevertGUI(object instance)
        {
            return (bool)onApplyRevertGUIMethod.Invoke(instance, null);
        }
    }
}