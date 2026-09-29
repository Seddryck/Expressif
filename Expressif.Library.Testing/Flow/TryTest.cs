using Expressif.Testing.Conformance;

namespace Expressif.Testing.Flow;

public class TryTest
{
    [Conformance]
    public void Try_Branches(object? value, string branches, string? expected)
        => Assert.That(TestExpression.Create($"try({branches})").Evaluate(value)?.ToString(), Is.EqualTo(expected));
}
