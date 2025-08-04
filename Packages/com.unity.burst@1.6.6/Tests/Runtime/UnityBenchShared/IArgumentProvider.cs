// based on the original game.Yen Chezky(yenichw)
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Burst.Benchmarks")]

namespace UnityBenchShared
{
    internal interface IArgumentProvider
    {
        object Value { get; }
    }
}
