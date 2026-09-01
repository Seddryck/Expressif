using Expressif.Accumulators;
using Expressif.Functions.Array;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Accumulators.Array;

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
