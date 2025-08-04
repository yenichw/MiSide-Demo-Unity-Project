// based on the original game.Yen Chezky(yenichw)
using System;

namespace Unity.Burst.CompilerServices
{
    /// <summary>
    /// Skip zero-initialization of local variables.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SkipLocalsInitAttribute : Attribute
    {
    }
}
