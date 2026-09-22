using Expressif.Library.Record;
using Expressif.Library.Sorting;
using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Sorting;

public class BottomTest
{
    [Conformance]
    public void Bottom_Valid_Rows(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void NegativeCount_Throws()
        => Assert.That(() => new Bottom(() => -1).Evaluate(new SortTableValue([], [])), Throws.TypeOf<ArgumentOutOfRangeException>());

    [Test]
    public void InvalidComparer_Throws()
    {
        var comparer = new SortComparer("invalid", typeof(CompareNumeric), (_, _) => null);
        var table = new SortTableValue([new SortHeader(comparer, true, false)],
            [new SortRow([1], "first"), new SortRow([2], "second")]);
        Assert.That(() => new Bottom(() => 1).Evaluate(table), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void ComparerEqualDistinctValues_CutStably()
    {
        var comparer = new SortComparer("numeric", typeof(CompareNumeric), (left, right) => new CompareNumeric(() => right).Evaluate(left));
        var table = new SortTableValue([new SortHeader(comparer, true, false)],
            [new SortRow([0], "integer"), new SortRow([0m], "decimal")]);
        Assert.That(new Bottom(() => 1).Evaluate(table), Is.EqualTo(new[] { "decimal" }));
    }
}
