using Expressif.Functions.Sorting;
using Expressif.Values;

namespace Expressif.Testing.Functions.Sorting;

public class SortSelectionTest
{
    [TestCase("top", "{1, 2}")]
    [TestCase("bottom", "{2, 3}")]
    [TestCase("top-with-ties", "{1, 2}")]
    [TestCase("bottom-with-ties", "{2, 3}")]
    public void CountExpression_ReadsEnclosingRecord(string function, string expected)
    {
        var expression = "{limit := 2} | array((sort-key(sort-term(3, compare-numeric~)) => 3), "
            + "(sort-key(sort-term(1, compare-numeric~)) => 1), (sort-key(sort-term(2, compare-numeric~)) => 2))"
            + $" | sort-table | {function}(.limit)";
        Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(null)), Is.EqualTo(expected));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public void PartialSelection_MatchesStableOrderedBoundaries(bool ascending, bool nullsFirst)
    {
        var comparer = new SortComparer("numeric", typeof(CompareNumeric), (left, right) => new CompareNumeric(() => right).Evaluate(left));
        var random = new Random(42);
        var rows = Enumerable.Range(0, 100).Select(index => new SortRow(
            [index % 7 == 0 ? null : random.Next(5), random.Next(3)], index)).ToArray();
        var table = new SortTableValue(
            [new SortHeader(comparer, ascending, nullsFirst), new SortHeader(comparer, false, false)], rows);
        var ordered = new Sort().Evaluate(table);
        foreach (var count in new[] { 0, 1, 2, 9, 50, 100, 101 })
        {
            Assert.That(new Top(() => count).Evaluate(table), Is.EqualTo(ordered.Take(count)));
            Assert.That(new Bottom(() => count).Evaluate(table), Is.EqualTo(ordered.TakeLast(count)));
            var top = ordered.Take(count).ToList();
            var bottom = ordered.TakeLast(count).ToList();
            if (count > 0 && count < ordered.Length)
            {
                var topKey = rows[(int)top[^1]!].Keys;
                var bottomKey = rows[(int)bottom[0]!].Keys;
                top.AddRange(ordered.Skip(count).TakeWhile(value => rows[(int)value!].Keys.SequenceEqual(topKey)));
                bottom.InsertRange(0, ordered.Take(ordered.Length - count).Reverse()
                    .TakeWhile(value => rows[(int)value!].Keys.SequenceEqual(bottomKey)).Reverse());
            }
            Assert.That(new TopWithTies(() => count).Evaluate(table), Is.EqualTo(top));
            Assert.That(new BottomWithTies(() => count).Evaluate(table), Is.EqualTo(bottom));
        }
    }

    [Test]
    public void SmallSelection_UsesLinearComparisonBudget()
    {
        var comparisons = 0;
        var comparer = new SortComparer("numeric", typeof(CompareNumeric), (left, right) =>
        {
            comparisons++;
            return new CompareNumeric(() => right).Evaluate(left);
        });
        var table = new SortTableValue([new SortHeader(comparer, true, false)],
            Enumerable.Range(0, 1000).Reverse().Select(value => new SortRow([value], value)));
        Assert.That(new Top(() => 1).Evaluate(table), Is.EqualTo(new[] { 0 }));
        Assert.That(comparisons, Is.LessThanOrEqualTo(2 * table.Rows.Count));
    }
}
