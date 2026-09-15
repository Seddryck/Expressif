using Expressif.Testing.Conformance;
using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;
using JoinFunction = Expressif.Functions.Array.Join;

namespace Expressif.Testing.Functions.Array;

public class JoinTest
{
    [Conformance]
    public void Join_Valid_Array(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Join_Valid_Grouping(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Join_Valid_Dictionary(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void Join_Valid_Empty(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

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
}
