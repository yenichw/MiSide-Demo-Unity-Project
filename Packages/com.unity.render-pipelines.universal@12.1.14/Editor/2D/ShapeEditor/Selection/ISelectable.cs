// based on the original game.Yen Chezky(yenichw)
namespace UnityEditor.Rendering.Universal.Path2D
{
    internal interface ISelectable<T>
    {
        bool Select(ISelector<T> selector);
    }
}
