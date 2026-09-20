using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Flow;

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
            () => TestExpression.Create("transform-as(trim, .name)"),
            Throws.TypeOf<BindingException>());

    private static void AssertResult(object? value, string[] expressions, string expected)
    {
        var actual = TestExpression.Create($"transform-as({string.Join(", ", expressions)})").Evaluate(value);
        Assert.That(ValueFormatter.Format(actual), Is.EqualTo(expected));
    }
}
