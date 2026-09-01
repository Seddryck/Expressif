using Expressif.Accumulators;
using Expressif.Functions.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Accumulators.Array;

[TestFixture]
public class EveryTest
{
    [Conformance]
    public void Every_Valid(object? value, bool? expected)
        => Assert.That(Evaluate(value), Is.EqualTo(expected));

    private static object? Evaluate(object? value)
        => value switch
        {
            "(null)" => null,
            "(empty)" => new Fold(() => new EveryAccumulator()).Evaluate(System.Array.Empty<object>()),
            _ => new Fold(() => new EveryAccumulator()).Evaluate(value),
        };
}
