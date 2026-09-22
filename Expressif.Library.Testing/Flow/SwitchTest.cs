using Expressif.Testing.Conformance;

namespace Expressif.Testing.Flow;

public class SwitchTest
{
    [Conformance]
    public void Switch_Branches(object? value, string branches, string? expected)
        => Assert.That(TestExpression.Create($"switch({branches})").Evaluate(value)?.ToString(), Is.EqualTo(expected));
}
