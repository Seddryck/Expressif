using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Sorting;

public class ComparisonFunctionsTest
{
    [Conformance]
    public void CompareNumeric_Valid_Ordering(object? input, string expression, string expected)
        => Assert.That(Format(expression, input), Is.EqualTo(expected));

    [Conformance]
    public void CompareOrdinal_Valid_Ordering(object? input, string expression, string expected)
        => Assert.That(Format(expression, input), Is.EqualTo(expected));

    [Conformance]
    public void CompareDate_Valid_Ordering(object? input, string expression, string expected)
        => Assert.That(Format(expression, input), Is.EqualTo(expected));

    [Conformance]
    public void CompareTime_Valid_Ordering(object? input, string expression, string expected)
        => Assert.That(Format(expression, input), Is.EqualTo(expected));

    [Conformance]
    public void CompareDatetime_Valid_Ordering(object? input, string expression, string expected)
        => Assert.That(Format(expression, input), Is.EqualTo(expected));

    [Conformance]
    public void Compare_Valid_Routing(object? input, string expression, string expected)
        => Assert.That(Format(expression, input), Is.EqualTo(expected));

    [Test]
    public void Compare_UnsupportedType_Throws()
        => Assert.That(
            () => Expression.Create("compare(2, :boolean)").Evaluate(1),
            Throws.TypeOf<ArgumentException>()
                .With.Message.Contains(":boolean"));

    [Test]
    public void Compare_Arguments_AreEvaluatedAgainstIncomingValue()
        => Assert.That(
            Expression.Create(".left | compare(.right, :numeric)")
                .Evaluate(new { left = "2", right = 1 }),
            Is.SameAs(OrderingValue.Greater));

    [Test]
    public void CompareNumeric_TupleBinding_UsesLeftAsInputAndRightAsParameter()
        => Assert.That(
            Expression.Create("T(10, 20) | compare-numeric~").Evaluate(null),
            Is.SameAs(OrderingValue.Less));

    private static string Format(string expression, object? input)
        => ValueFormatter.Format(Expression.Create(expression).Evaluate(input));
}
