using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Testing.Conformance;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;
using JoinFunction = Expressif.Library.Array.Combination.Join;

namespace Expressif.Testing.Array.Combination;

public class JoinTest
{
    [Conformance]
    public void Join_Valid_Array(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Join_Valid_Grouping(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Join_Valid_Dictionary(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Join_Valid_Empty(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void Join_UsesStructuralKeysAndKeepsMatchOrder()
    {
        var right = new object?[] { new TupleValue(1, "A"), new TupleValue(1, "B") };
        var left = new object?[] { new TupleValue(1, "left") };
        var join = new JoinFunction(() => right, value => ((TupleValue)value!).GetPosition(0));

        var pairs = (PairValue[])join.Evaluate(left)!;

        Assert.That(pairs.Select(pair => ((TupleValue)pair.Value!).GetPosition(1)), Is.EqualTo(new[] { "A", "B" }));
    }

    [Test]
    public void Join_EvaluatesRightOnceAndSharedKeyForBothSides()
    {
        var rightCalls = 0;
        var keyCalls = 0;
        var join = new JoinFunction(
            () =>
            {
                rightCalls++;
                return new object?[] { 1, 1, 2 };
            },
            value =>
            {
                keyCalls++;
                return value;
            });

        var result = (PairValue[])join.Evaluate(new object?[] { 1, 3 })!;

        Assert.Multiple(() =>
        {
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(rightCalls, Is.EqualTo(1));
            Assert.That(keyCalls, Is.EqualTo(5));
        });
    }

    [Test]
    public void Join_SkipsRightKeyForAlreadyKeyedValues()
    {
        var grouping = new GroupingValue([new PairValue(1, new object?[] { "A", "B" })]);
        var join = new JoinFunction(() => grouping, value => value, _ => throw new AssertionException("Right-key should be skipped."));

        Assert.That(((PairValue[])join.Evaluate(new object?[] { 1 })!).Length, Is.EqualTo(2));
    }

    [Test]
    public void Join_AmbientRightSourceAndNestedSelectorsKeepSeparateScopes()
    {
        var left = new RecordValue();
        left.Set("id", 1m);
        var right = new RecordValue();
        right.Set("id", 1m);
        var input = new RecordValue();
        input.Set("left", new object?[] { left });
        input.Set("right", new object?[] { right });

        var result = TestExpression.Create(".left | join(^.right, .id)").Evaluate(input);

        Assert.That(ValueFormatter.Format(result), Is.EqualTo("{({id := 1} => {id := 1})}"));
    }

    [Test]
    public void Join_OmittedRightKeyDiffersFromExplicitNullExpression()
    {
        var omitted = TestExpression.CreateClosed("{1} | join({1}, @_)").Evaluate(null);
        var explicitNull = TestExpression.CreateClosed("{1} | join({1}, @_, #null)").Evaluate(null);

        Assert.Multiple(() =>
        {
            Assert.That(ValueFormatter.Format(omitted), Is.EqualTo("{(1 => 1)}"));
            Assert.That(ValueFormatter.Format(explicitNull), Is.EqualTo("{}"));
        });
    }
}
