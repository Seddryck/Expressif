using Expressif.Functions;
using Expressif.Predicates.Array;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Predicates.Array;

public class ExistsTest
{
    [Conformance]
    public void Exists_Array(object? input, string expression, bool expected)
        => Assert.That(Expression.Create(expression).Evaluate(input), Is.EqualTo(expected));

    [Conformance]
    public void Exists_Grouping(object? input, string expression, bool expected)
        => Assert.That(Expression.Create(expression).Evaluate(input), Is.EqualTo(expected));

    [Test]
    public void Exists_StopsEnumerationAndDisposesAfterFirstMatch()
    {
        var disposed = false;
        var calls = new List<string>();
        IEnumerable<object?> Right()
        {
            try
            {
                yield return 1;
                yield return 2;
                throw new AssertionException("Must stop at the match.");
            }
            finally
            {
                disposed = true;
            }
        }
        IFunction<object, bool> predicate = new Exists(
            () => { calls.Add("right"); return Right(); },
            value => { calls.Add("left"); return value; },
            value => { calls.Add($"key:{value}"); return value; });

        Assert.That(predicate.Evaluate(2), Is.True);
        Assert.That(calls, Is.EqualTo(new[] { "right", "left", "key:1", "key:2" }));
        Assert.That(disposed, Is.True);
    }

    [Test]
    public void Exists_GroupingSkipsRightKeyAndAcceptsEmptyBuckets()
    {
        var grouping = new Expressif.Values.Grouping([new PairValue(1, System.Array.Empty<object?>())]);
        var predicate = new Exists(() => grouping, value => value,
            _ => throw new AssertionException("Grouping lookup must skip right-key."));
        Assert.That(predicate.Evaluate(1), Is.True);
    }

    [TestCase("exists({1})")]
    [TestCase("exists({1}, @_, @_, @_)")]
    [TestCase("exists(unknown := {1}, left-key := @_)")]
    public void Exists_RejectsInvalidArguments(string expression)
        => Assert.That(() => Expression.Create(expression), Throws.Exception);

    [TestCase(null)]
    [TestCase(42)]
    public void Exists_UnsupportedRightReturnsFalse(object? right)
        => Assert.That(new Exists(() => right, value => value).Evaluate(1), Is.False);

    [Test]
    public void Exists_ConcurrentEvaluationsKeepKeyContextsIsolated()
    {
        var expression = Expression.Create("exists({{id := \"1\"}, {id := \"3\"}}, .id)");
        Parallel.For(0, 100, index =>
        {
            var input = new RecordValue();
            input.Set("id", (index % 4).ToString(System.Globalization.CultureInfo.InvariantCulture));
            Assert.That(expression.Evaluate(input), Is.EqualTo(index % 4 is 1 or 3));
        });
    }

    [Test]
    public void Exists_FilterUsesEachCustomerAsLeftContext()
    {
        var result = Expression.Create(
            "{{id := 1}, {id := 2}} | filter(exists({{customer-id := 1}}, .id, .customer-id))").Evaluate(null);
        Assert.That(ValueFormatter.Format(result), Is.EqualTo("{{id := 1}}"));
    }
}
