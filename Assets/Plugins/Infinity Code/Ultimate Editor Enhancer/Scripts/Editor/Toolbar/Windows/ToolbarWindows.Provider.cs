/*           INFINITY CODE          */
/*     https://infinity-code.com    */

namespace InfinityCode.UltimateEditorEnhancer.Tools
{
    public static partial class ToolbarWindows
    {
        public abstract class Provider
        {
            public abstract float order { get; }

            public abstract void GenerateMenu(GenericMenuEx menu, ref bool hasItems);
        }
    }
}