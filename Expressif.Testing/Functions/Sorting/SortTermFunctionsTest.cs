using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Sorting;

public class SortTermFunctionsTest
{
    [Conformance]
    public void SortTerm_Valid_Construction(object? input, string expression, string expected) => Verify(input, expression, expected);
    [Conformance]
    public void Ascending_Valid_Modifier(object? input, string expression, string expected) => Verify(input, expression, expected);
    [Conformance]
    public void Descending_Valid_Modifier(object? input, string expression, string expected) => Verify(input, expression, expected);
    [Conformance]
    public void NullsFirst_Valid_Modifier(object? input, string expression, string expected) => Verify(input, expression, expected);
    [Conformance]
    public void NullsLast_Valid_Modifier(object? input, string expression, string expected) => Verify(input, expression, expected);

    [Test]
    public void SortTerm_NonComparerReference_Throws()
        => Assert.That(() => Expression.Create("sort-term(1, add~)").Evaluate(null), Throws.TypeOf<BindingException>());

    [Test]
    public void SortTerm_ComparerIsNotInvokedDuringConstruction()
        => Assert.That(Expression.Create("sort-term(1, compare-numeric~)").Evaluate(null), Is.TypeOf<Expressif.Values.SortTerm>());

    private static void Verify(object? input, string expression, string expected)
        => Assert.That(ValueFormatter.Format(Expression.Create(expression).Evaluate(input)), Is.EqualTo(expected));
}
