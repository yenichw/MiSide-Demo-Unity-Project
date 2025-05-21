/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System.Reflection;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public class DrawPresetButtonInterceptor: StatedInterceptor<DrawPresetButtonInterceptor>
    {
        protected override InitType initType
        {
            get => InitType.gui;
        }

        protected override MethodInfo originalMethod
        {
            get => PresetSelectorRef.drawPresetButtonMethod;
        }

        protected override string prefixMethodName
        {
            get => nameof(DrawPresetButtonPrefix);
        }

        public override bool state
        {
            get => Prefs.hidePresetButton;
        }

        private static bool DrawPresetButtonPrefix()
        {
            return !Prefs.hidePresetButton;
        }
    }
}