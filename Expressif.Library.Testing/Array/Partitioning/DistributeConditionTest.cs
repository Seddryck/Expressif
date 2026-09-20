using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Library.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Array.Partitioning;

[TestFixture]
public class DistributeConditionTest
{
    [Conformance]
    public void DistributeCondition_Valid_Condition(object? input, string condition, string? expected)
    {
        if (input is "(null)")
        {
            Assert.That(TestExpression.Create($"distribute-condition({condition})").Evaluate(null), Is.Null);
            return;
        }

        var value = input is string text ? new ParameterValueConverter().Parse(text) : input;
        var actual = TestExpression.Create($"distribute-condition({condition})").Evaluate(value);
        Assert.That(actual is null ? null : ValueFormatter.Format(actual), Is.EqualTo(expected));
    }

    [Test]
    public void Evaluate_NonEnumerable_ReturnsNull()
        => Assert.That(TestExpression.Create("distribute-condition(is-even)").Evaluate(42), Is.Null);
}
