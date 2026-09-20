using Expressif.Library.Array.Aggregation;
using Expressif.Library.Array.Combination;
using Expressif.Library.Array.Grouping;
using Expressif.Library.Array.Partitioning;
using Expressif.Library.Array.Selection;
using Expressif.Library.Array.Sequencing;
using Expressif.Library.Array.Set;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Array.Combination;

public class OuterJoinTest
{
    [Conformance]
    public void JoinLeft_Valid(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void JoinRight_Valid(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Conformance]
    public void JoinFull_Valid(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void OuterJoin_EvaluatesSelectorsOnceAndReusesWithoutState()
    {
        var rightCalls = 0;
        var leftCalls = 0;
        var rightKeyCalls = 0;
        var join = new Expressif.Library.Array.Combination.JoinFull(
            () => { rightCalls++; return new object?[] { 2, 3, 2 }; },
            value => { leftCalls++; return value; },
            value => { rightKeyCalls++; return value; });
        Assert.That(ValueFormatter.Format(join.Evaluate(new object?[] { 2 })), Is.EqualTo("{(2 => 2), (2 => 2), (null => 3)}"));
        Assert.That(ValueFormatter.Format(join.Evaluate(new object?[] { 1 })), Is.EqualTo("{(1 => null), (null => 2), (null => 3), (null => 2)}"));
        Assert.Multiple(() =>
        {
            Assert.That(rightCalls, Is.EqualTo(2));
            Assert.That(leftCalls, Is.EqualTo(2));
            Assert.That(rightKeyCalls, Is.EqualTo(6));
        });
    }

    [Test]
    public void OuterJoin_SkipsRightSelectorForKeyedSource()
    {
        var right = new Expressif.Values.Grouping([new PairValue(1, new object?[] { "A" }), new PairValue(2, new object?[] { "B" })]);
        var join = new Expressif.Library.Array.Combination.JoinFull(() => right, value => value,
            _ => throw new AssertionException("A keyed source must skip the right selector."));
        Assert.That(ValueFormatter.Format(join.Evaluate(new object?[] { 1 })), Is.EqualTo("{(1 => \"A\"), (null => \"B\")}"));
    }

    [Test]
    public void OuterJoin_ReturnsNullForInvalidCollections()
    {
        var join = new Expressif.Library.Array.Combination.JoinFull(() => 42, value => value);
        Assert.That(join.Evaluate(new object?[] { 1 }), Is.Null);
        Assert.That(join.Evaluate(null), Is.Null);
    }

    [Test]
    public void OuterJoin_IsReusableAcrossConcurrentTypedEvaluations()
    {
        Expressif.Functions.IFunction<System.Collections.IEnumerable, System.Collections.IEnumerable?> join =
            new Expressif.Library.Array.Combination.JoinFull(() => new object?[] { 1, 2 }, value => value);
        Parallel.For(0, 100, i =>
        {
            var result = join.Evaluate(new object?[] { (i % 2) + 1 })!.Cast<PairValue>().ToArray();
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0].Key, Is.EqualTo((i % 2) + 1));
            Assert.That(result[1].Key, Is.Null);
        });
    }
}
