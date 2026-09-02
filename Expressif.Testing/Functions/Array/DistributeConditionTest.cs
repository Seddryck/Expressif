using Expressif.Functions.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class DistributeConditionTest
{
    [Conformance]
    public void DistributeCondition_Valid_Condition(object? input, string condition, string? expected)
    {
        if (input is "(null)")
        {
            Assert.That(Expression.Create($"distribute-condition({condition})").Evaluate(null), Is.Null);
            return;
        }

        var value = input is string text ? new ParameterValueConverter().Parse(text) : input;
        var actual = Expression.Create($"distribute-condition({condition})").Evaluate(value);
        Assert.That(actual is null ? null : ValueFormatter.Format(actual), Is.EqualTo(expected));
    }

    [Test]
    public void Evaluate_NonEnumerable_ReturnsNull()
        => Assert.That(Expression.Create("distribute-condition(is-even)").Evaluate(42), Is.Null);
}
