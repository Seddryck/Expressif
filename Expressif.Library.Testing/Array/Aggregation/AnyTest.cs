using Expressif.Library.Array.Aggregation;
using Expressif.Functions.Accumulation;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Array.Aggregation;

[TestFixture]
public class AnyTest
{
    [Conformance]
    public void Any_Valid(object? value, bool? expected)
        => Assert.That(Evaluate(value), Is.EqualTo(expected));

    private static object? Evaluate(object? value)
        => value switch
        {
            "(null)" => null,
            "(empty)" => new Fold(() => new AnyAccumulator()).Evaluate(System.Array.Empty<object>()),
            _ => new Fold(() => new AnyAccumulator()).Evaluate(value),
        };
}
