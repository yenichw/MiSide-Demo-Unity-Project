using UnityEngine.Rendering;

namespace EPOOutline
{
    public interface IUnderlyingBufferProvider
    {
        CommandBuffer UnderlyingBuffer { get; }
    }
}