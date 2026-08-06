using System.Collections;
using Expressif.Functions.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Array;

[TestFixture]
public class WithPositionTest
{
    [Conformance]
    public void WithPosition_Valid(object? input, object?[]? expected)
        => Assert.That(new WithPosition().Evaluate(input), Is.EqualTo(ToTuples(expected)));

    private static object? ToTuples(object?[]? expected)
        => expected?.Select(pair =>
        {
            var values = (object?[])pair!;
            return new TupleValue(int.Parse((string)values[0]!), values[1]);
        }).ToArray();

    [Test]
    public void Evaluate_IsProgressiveAndUsesOneEnumerator()
    {
        var source = new TrackingEnumerable("a", "b", "c");
        var result = ((IEnumerable)new WithPosition().Evaluate(source)!).GetEnumerator();

        Assert.That(source.MoveNextCalls, Is.Zero);
        Assert.That(result.MoveNext(), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(source.GetEnumeratorCalls, Is.EqualTo(1));
            Assert.That(source.MoveNextCalls, Is.EqualTo(1));
            Assert.That(result.Current, Is.EqualTo(new TupleValue(0, "a")));
        });
    }

    [Test]
    public void Evaluate_PositionUsesInt32()
    {
        var result = ((IEnumerable)new WithPosition().Evaluate(new[] { "a" })!).Cast<TupleValue>().Single();

        Assert.That(result[0], Is.TypeOf<int>());
    }

    private sealed class TrackingEnumerable(params object?[] values) : IEnumerable
    {
        public int GetEnumeratorCalls { get; private set; }
        public int MoveNextCalls { get; private set; }

        public IEnumerator GetEnumerator()
        {
            GetEnumeratorCalls++;
            return Enumerate().GetEnumerator();
        }

        private IEnumerable Enumerate()
        {
            foreach (var value in values)
            {
                MoveNextCalls++;
                yield return value;
            }
        }
    }
}

[TestFixture]
public class PositionOfTest
{
    [Conformance]
    public void PositionOf_Valid_Value(object? input, object? value, int? expected)
        => Assert.That(new PositionOf(() => value).Evaluate(input), Is.EqualTo(expected));

    [Test]
    public void Evaluate_StopsAfterFirstMatch()
    {
        var source = new ThrowAfterMatchEnumerable();

        Assert.That(new PositionOf(() => "match").Evaluate(source), Is.EqualTo(0));
    }

    private sealed class ThrowAfterMatchEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return "match";
            throw new InvalidOperationException("The source should not be enumerated after the first match.");
        }
    }
}

[TestFixture]
public class ValueAtTest
{
    [Conformance]
    public void ValueAt_Valid_Position(object? input, int position, object? expected)
        => Assert.That(new ValueAt(() => position).Evaluate(input), Is.EqualTo(expected));

    [Test]
    public void Evaluate_StopsAtRequestedPosition()
    {
        var source = new ThrowAfterRequestedPositionEnumerable();

        Assert.That(new ValueAt(() => 0).Evaluate(source), Is.EqualTo("first"));
    }

    private sealed class ThrowAfterRequestedPositionEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return "first";
            throw new InvalidOperationException("The source should not be enumerated past the requested position.");
        }
    }
}
