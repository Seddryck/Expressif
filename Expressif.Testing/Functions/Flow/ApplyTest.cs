using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Flow;

public class ApplyTest
{
    [Conformance]
    public void Apply_Valid_Tuple(string value, string expression, int expected)
        => Assert.That(
            Expression.CreateClosed($"{value} | apply({expression})").Evaluate(null),
            Is.EqualTo(expected));

    [Conformance]
    public void Apply_Valid_Record(object? value, string expression, string expected)
        => Assert.That(Expression.Create($"apply({expression})").Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Apply_Valid_Array(object?[] value, string expression, object?[] expected)
        => Assert.That(Expression.Create($"apply({expression})").Evaluate(value), Is.EqualTo(expected));
}
