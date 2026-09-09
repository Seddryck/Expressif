using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Flow;

public class TryTest
{
    [Conformance]
    public void Try_Branches(object? value, string branches, string? expected)
        => Assert.That(Expression.Create($"try({branches})").Evaluate(value)?.ToString(), Is.EqualTo(expected));
}
