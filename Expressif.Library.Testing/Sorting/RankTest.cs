using Expressif.Functions;
using Expressif.Library.Record;
using Expressif.Library.Sorting;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Sorting;

public class RankTest
{
    [Conformance]
    public void Rank_Valid_Rows(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void Rank_InvalidNonNullComparison_Throws()
    {
        var expression = TestExpression.Create("array((sort-key(sort-term(\"bad\", compare-numeric~)) => 1), (sort-key(sort-term(2, compare-numeric~)) => 2)) | sort-table | rank");
        Assert.That(() => expression.Evaluate(null), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Rank_InvalidRowArity_Throws()
    {
        var table = new SortTableValue([], [new SortRow([1], "invalid")]);
        Assert.That(() => new Rank().Evaluate(table), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Rank_UnsupportedInput_Throws()
        => Assert.That(() => ((IFunction)new Rank()).Evaluate(new object()), Throws.TypeOf<ArgumentException>());

    [Test]
    public void Rank_HeaderlessRows_AllTieAndPreserveOriginalValues()
    {
        var first = new object();
        var table = new SortTableValue([], [new SortRow([], first), new SortRow([], null)]);
        IFunction<SortTableValue, Expressif.Values.Grouping> function = new Rank();
        var result = function.Evaluate(table);
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Key, Is.EqualTo(1));
            Assert.That(result[0][0], Is.SameAs(first));
            Assert.That(result[0][1], Is.Null);
        });
    }

    [Test]
    public void Rank_ComparerEqualDistinctRuntimeValues_ShareRank()
    {
        var comparer = new SortComparer("numeric", typeof(CompareNumeric), (left, right) =>
            new CompareNumeric(() => right).Evaluate(left));
        var table = new SortTableValue([new SortHeader(comparer, true, false)],
            [new SortRow([0], "integer"), new SortRow([0m], "decimal")]);
        var result = new Rank().Evaluate(table);
        Assert.Multiple(() =>
        {
            Assert.That(table.Rows[0].Keys[0], Is.TypeOf<int>());
            Assert.That(table.Rows[1].Keys[0], Is.TypeOf<decimal>());
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].Values, Is.EqualTo(new[] { "integer", "decimal" }));
        });
    }

    [Test]
    public void Rank_PostSortScan_UsesAdjacentOrderedKeys()
    {
        var comparisons = new List<(object? Left, object? Right)>();
        var comparer = new SortComparer("numeric", typeof(CompareNumeric), (left, right) =>
        {
            comparisons.Add((left, right));
            var comparison = Convert.ToInt32(left).CompareTo(Convert.ToInt32(right));
            return comparison < 0 ? OrderingValue.Less : comparison > 0 ? OrderingValue.Greater : OrderingValue.Equal;
        });
        var table = new SortTableValue([new SortHeader(comparer, true, false)],
            [new SortRow([3], "three"), new SortRow([1], "one"), new SortRow([2], "two-first"), new SortRow([2], "two-second")]);
        new Rank().Evaluate(table);
        Assert.That(comparisons.TakeLast(3), Is.EqualTo(new (object?, object?)[] { (2, 1), (2, 2), (3, 2) }));
    }

    [Test]
    public void Rank_InvalidComparisonDuringTieDetection_Throws()
    {
        var comparisons = 0;
        var comparer = new SortComparer("invalid", typeof(CompareNumeric), (_, _) =>
            ++comparisons == 1 ? OrderingValue.Equal : null);
        var table = new SortTableValue([new SortHeader(comparer, true, false)],
            [new SortRow([1], "first"), new SortRow([1], "second")]);
        Assert.That(() => new Rank().Evaluate(table), Throws.TypeOf<InvalidOperationException>()
            .With.Message.Contains("returned null for two non-null values"));
    }
}
