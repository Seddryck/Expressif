using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Sorting;

public class SortTest
{
    [Conformance]
    public void Sort_Valid_Rows(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [Test]
    public void Sort_EqualKeys_IsStable()
    {
        var result = Expression.Create("array((sort-key(sort-term(1, compare-numeric~)) => \"first\"), (sort-key(sort-term(1, compare-numeric~)) => \"second\")) | sort-table | sort").Evaluate(null);
        Assert.That(result, Is.EqualTo(new[] { "first", "second" }));
    }

    [Test]
    public void Sort_InvalidNonNullComparison_Throws()
    {
        var result = Expression.Create("array((sort-key(sort-term(\"bad\", compare-numeric~)) => 1), (sort-key(sort-term(2, compare-numeric~)) => 2)) | sort-table | sort");
        Assert.That(() => result.Evaluate(null), Throws.TypeOf<InvalidOperationException>());
    }
}
