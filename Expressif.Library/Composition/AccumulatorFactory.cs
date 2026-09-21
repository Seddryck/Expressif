using Expressif.Functions;
using Expressif.Discovery;

namespace Expressif.Library.Composition;

internal static class AccumulatorFactory
{
    private static readonly AccumulatorRegistry Registry = new(
        new AssemblyTypeSource([typeof(AccumulatorFactory).Assembly]));

    public static IAccumulator Instantiate(string? name)
        => Registry.Create(name ?? string.Empty);
}
