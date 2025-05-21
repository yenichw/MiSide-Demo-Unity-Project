/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEngine.UIElements;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class StyleSheets
    {
        private static StyleSheet _flatSelector;

        public static StyleSheet flatSelector
        {
            get
            {
                if (_flatSelector == null)
                {
                    _flatSelector = Resources.Load<StyleSheet>("Stylesheets/FlatSelector.uss");
                }

                return _flatSelector;
            }
        }
    }
}