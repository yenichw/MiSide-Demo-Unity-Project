/*           INFINITY CODE          */
/*     https://infinity-code.com    */

namespace InfinityCode.UltimateEditorEnhancer
{
    public interface IStateablePref
    {
        string GetMenuName();
        void SetState(bool state);
    }
}