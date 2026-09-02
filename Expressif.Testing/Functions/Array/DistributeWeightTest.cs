using Expressif.Functions;
using Expressif.Functions.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class DistributeWeightTest
{
    [Conformance]
    public void DistributeWeight_Valid_Weight(object? input, string weight, string? expected)
    {
        if (input is "(null)")
        {
            Assert.That(Expression.Create($"distribute-weight({weight})").Evaluate(null), Is.Null);
            return;
        }

        var value = input is string text ? new ParameterValueConverter().Parse(text) : input;
        var actual = Expression.Create($"distribute-weight({weight})").Evaluate(value);
        Assert.That(actual is null ? null : ValueFormatter.Format(actual), Is.EqualTo(expected));
    }

    [TestCase("null-to-value")]
    [TestCase("prepend(\"invalid\")")]
    [TestCase("oppose")]
    public void Expression_InvalidWeight_ReturnsNull(string weight)
        => Assert.That(Expression.Create($"distribute-weight({weight})").Evaluate(new[] { 1, 2 }), Is.Null);

    [Test]
    public void Expression_WeightProjection_BalancesTuples()
        => Assert.That(
            Expression.Create("distribute-weight(tuple-second)")
                .Evaluate(new[] { new Expressif.Values.Tuple("A", 8), new Expressif.Values.Tuple("B", 7) }),
            Is.EqualTo(new object?[][]
            {
                [new Expressif.Values.Tuple("A", 8)],
                [new Expressif.Values.Tuple("B", 7)],
            }));

    [Test]
    public void Evaluate_WeightExpression_ExecutesOncePerItem()
    {
        var evaluations = 0;
        var function = new DistributeWeight(() => new DelegatedFunction(value =>
        {
            evaluations++;
            return value;
        }));

        _ = function.Evaluate(new[] { 1, 2, 3, 4 });

        Assert.That(evaluations, Is.EqualTo(4));
    }

    private sealed class DelegatedFunction(Func<object?, object?> implementation) : IFunction
    {
        public object? Evaluate(object? value) => implementation(value);
    }
}
