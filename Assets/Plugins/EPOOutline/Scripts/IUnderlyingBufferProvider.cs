// based on the original game.Yen Chezky(yenichw)
using UnityEngine.Rendering;

namespace EPOOutline
{
    public interface IUnderlyingBufferProvider
    {
        CommandBuffer UnderlyingBuffer { get; }
    }
}