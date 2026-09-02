using Expressif.Testing.Conformance;
using Expressif.Values;

namespace Expressif.Testing.Functions.Flow;

public class TransformWithTest
{
    [Conformance]
    public void TransformWith_Valid_FieldAccess(object? value, string[] expressions, string expected)
        => AssertResult(value, expressions, expected);

    [Conformance]
    public void TransformWith_Valid_ArbitraryExpressions(object? value, string[] expressions, string expected)
        => AssertResult(value, expressions, expected);

    [Conformance]
    public void TransformWith_Valid_Ordering(object? value, string[] expressions, string expected)
        => AssertResult(value, expressions, expected);

    [Conformance]
    public void TransformWith_Valid_SingleExpression(object? value, string[] expressions, string expected)
        => AssertResult(value, expressions, expected);

    private static void AssertResult(object? value, string[] expressions, string expected)
    {
        var actual = Expression.Create($"transform-with({string.Join(", ", expressions)})").Evaluate(value);
        Assert.That(ValueFormatter.Format(actual), Is.EqualTo(expected));
    }
}
