// based on the original game.Yen Chezky(yenichw)
namespace UnityEditor.Rendering.Universal.Path2D
{
    internal interface ISelector<T>
    {
        bool Select(T element);
    }
}
