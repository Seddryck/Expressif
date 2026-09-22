using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Sorting;

public class DenseRankByTest
{
    [Conformance]
    public void DenseRankBy_Valid_Criteria(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(TestExpression.Create(expression).Evaluate(input)), Is.EqualTo(expected));

    [TestCase(".v -> :numeric", "sort-term(.v | coerce(:numeric), compare-numeric~)")]
    [TestCase(".v -> :numeric | desc | nulls-first", "sort-term(.v | coerce(:numeric), compare-numeric~) | desc | nulls-first")]
    public void ExplicitLowering_MatchesSugar(string criterion, string term)
    {
        var source = "array({v := 1}, {v := 1}, {v := 2}, {v := \"bad\"}, {v := #null})";
        var sugar = TestExpression.Create($"{source} | dense-rank-by({criterion})").Evaluate(null);
        var lowered = TestExpression.Create($"{source} | map(neutral | pair(sort-key({term}), neutral)) | sort-table | dense-rank").Evaluate(null);
        Assert.That(ValueFormatter.Format(sugar), Is.EqualTo(ValueFormatter.Format(lowered)));
    }

    [Test]
    public void MissingCriteria_Throws()
        => Assert.That(() => TestExpression.Create("array() | dense-rank-by()").Evaluate(null), Throws.TypeOf<BindingException>());
}
