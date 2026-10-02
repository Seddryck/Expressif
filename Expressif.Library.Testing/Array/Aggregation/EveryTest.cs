using Expressif.Library.Array.Aggregation;
using Expressif.Functions.Accumulation;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array.Aggregation;

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
