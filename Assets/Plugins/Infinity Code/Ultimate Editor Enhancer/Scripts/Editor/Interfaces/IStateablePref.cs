// based on the original game.Yen Chezky(yenichw)
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