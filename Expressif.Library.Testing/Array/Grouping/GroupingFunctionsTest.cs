using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Testing.Conformance;
using Expressif.Values;
using GroupByFunction = Expressif.Library.Array.Grouping.GroupBy;
using GroupFunction = Expressif.Library.Array.Grouping.Group;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Testing.Array.Grouping;

public class GroupingFunctionsTest
{
    [Conformance]
    public void Key_Valid_Scalar(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Key_Valid_Tuple(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Group_Valid_Stable(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void GroupBy_Valid_Scalar(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void GroupBy_Valid_Tuple(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void GroupBy_Valid_Empty(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void Group_RejectsTupleAndRecordValues()
    {
        var group = new GroupFunction();

        Assert.Multiple(() =>
        {
            Assert.That(() => group.Evaluate(new object?[] { new TupleValue(1, 2) }), Throws.ArgumentException);
            Assert.That(() => group.Evaluate(new object?[] { new RecordValue() }), Throws.ArgumentException);
        });
    }

    [Test]
    public void GroupBy_EvaluatesEverySelectorOncePerValue()
    {
        var evaluations = 0;
        var groupBy = new GroupByFunction([value =>
        {
            evaluations++;
            return value;
        }]);

        var result = groupBy.Evaluate(new object?[] { 1, 1, 2 });

        Assert.Multiple(() =>
        {
            Assert.That(evaluations, Is.EqualTo(3));
            Assert.That(result, Is.TypeOf<GroupingValue>());
        });
    }
}
