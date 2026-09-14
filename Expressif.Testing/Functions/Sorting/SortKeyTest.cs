using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Sorting;

public class SortKeyTest
{
    [Conformance]
    public void SortKey_Valid_Construction(object? input, string expression, string expected)
    {
        var value = Expression.Create(expression).Evaluate(input);
        var serialized = ValueFormatter.Format(value);
        Assert.That(serialized, Is.EqualTo(expected));
        Assert.That(Expression.Create(serialized).Evaluate(input), Is.EqualTo(value));
    }

    [TestCase("sort-key()")]
    [TestCase("sort-key(1)")]
    [TestCase("sort-key(sort-key(sort-term(1, compare-numeric~)))")]
    public void SortKey_InvalidValues_Throw(string source)
        => Assert.That(() => Expression.Create(source).Evaluate(null), Throws.Exception);
}
