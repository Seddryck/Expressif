using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Flow;

public class TransformAsTest
{
    [Conformance]
    public void TransformAs_Valid_FieldAccess(object? value, string[] expressions, string expected)
        => AssertResult(value, expressions, expected);

    [Conformance]
    public void TransformAs_Valid_ArbitraryExpressions(object? value, string[] expressions, string expected)
        => AssertResult(value, expressions, expected);

    [Conformance]
    public void TransformAs_Valid_FieldNamingAndOrder(object? value, string[] expressions, string expected)
        => AssertResult(value, expressions, expected);

    [Conformance]
    public void TransformAs_Valid_SingleField(object? value, string[] expressions, string expected)
        => AssertResult(value, expressions, expected);

    [Test]
    public void Create_UnnamedExpressionAfterOperation_Throws()
        => Assert.That(
            () => Expression.Create("transform-as(trim, .name)"),
            Throws.TypeOf<BindingException>());

    private static void AssertResult(object? value, string[] expressions, string expected)
    {
        var actual = Expression.Create($"transform-as({string.Join(", ", expressions)})").Evaluate(value);
        Assert.That(ValueFormatter.Format(actual), Is.EqualTo(expected));
    }
}
