using Expressif.Functions;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Accumulators.Array;

[TestFixture]
public class ReduceTest
{
    [Conformance]
    public void Reduce_WithoutInitial(object? value, string operation, decimal? expected)
        => Assert.That(Evaluate(value, $"reduce({operation})"), Is.EqualTo(expected));

    [Conformance]
    public void Reduce_WithInitial(object? value, string operation, decimal initial, decimal expected)
        => Assert.That(Evaluate(value, $"reduce({operation}, {initial})"), Is.EqualTo(expected));

    [Test]
    public void Evaluate_NamedInitial_Valid()
        => Assert.That(
            Expression.Create("reduce(operation := add($0, $1), initial := 10)").Evaluate(new object[] { 1, 2, 3 }),
            Is.EqualTo(16m));

    private static object? Evaluate(object? value, string expression)
    {
        var source = value switch
        {
            "(empty)" => "{}",
            string text => text,
            _ => throw new ArgumentException("Conformance input must use Expressif array syntax.", nameof(value)),
        };
        return Expression.Create($"{source} | {expression}").Evaluate(null);
    }
}
