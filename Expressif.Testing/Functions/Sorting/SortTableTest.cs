using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Sorting;

public class SortTableTest
{
    [Conformance]
    public void SortTable_Valid_Normalization(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void SortTable_DoesNotSortRows()
    {
        var table = (SortTableValue)Expression.Create("array((sort-key(sort-term(2, compare-numeric~)) => \"b\"), (sort-key(sort-term(1, compare-numeric~)) => \"a\")) | sort-table").Evaluate(null)!;
        Assert.That(table.Rows.Select(row => row.Value), Is.EqualTo(new[] { "b", "a" }));
    }

    [TestCase("array(1) | sort-table")]
    [TestCase("array((sort-key(sort-term(1, compare-numeric~)) => 1), (sort-key(sort-term(1, compare-ordinal~)) => 2)) | sort-table")]
    [TestCase("array((sort-key(sort-term(1, compare-numeric~)) => 1), (sort-key(sort-term(1, compare-numeric~), sort-term(2, compare-numeric~)) => 2)) | sort-table")]
    public void SortTable_InvalidShape_Throws(string source)
        => Assert.That(() => Expression.Create(source).Evaluate(null), Throws.Exception);
}
