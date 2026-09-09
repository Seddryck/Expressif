namespace Expressif.Testing.Predicates;

public class OpenArgumentTest
{
    [TestCase("{value := 10, reference := #null} | .value | greater-than(.reference)", true)]
    [TestCase("{value := \"Alice\", reference := #null} | .value | starts-with(.reference)", false)]
    [TestCase("{value := #\"2025-01-01\", reference := #null} | .value | is-after(.reference)", true)]
    [TestCase("{value := 10, reference := \"9\"} | .value | greater-than(.reference)", true)]
    [TestCase("{value := \"123\", reference := 12} | .value | starts-with(.reference)", true)]
    [TestCase("{value := #\"2025-01-01\", reference := \"2024-01-01\"} | .value | is-after(.reference)", true)]
    [TestCase("{value := \"Alice\", reference := \" a \"} | .value | starts-with(.reference | trim)", true)]
    public void Evaluate_OpenArgument_PreservesCoercionAndNullContracts(string code, bool expected)
        => Assert.That(Expression.CreateClosed(code).Evaluate(null), Is.EqualTo(expected));

    [TestCase("{value := 10, reference := \"invalid\"} | .value | greater-than(.reference)")]
    [TestCase("{value := #\"2025-01-01\", reference := \"invalid\"} | .value | is-after(.reference)")]
    public void Evaluate_IncompatibleOpenArgument_UsesDeclaredTypeDiagnostic(string code)
    {
        var expression = Expression.CreateClosed(code);
        var literal = Expression.CreateClosed(code.Replace("(.reference)", "(\"invalid\")", StringComparison.Ordinal));
        var expected = Assert.Catch(() => literal.Evaluate(null));

        Assert.That(() => expression.Evaluate(null),
            Throws.TypeOf(expected!.GetType()).With.Message.EqualTo(expected.Message));
    }

    [TestCase("{{value := 10}} | filter(.value)")]
    [TestCase("{{value := #null}} | filter(.value)")]
    [TestCase("{{value := 10}} | filter(majority(.value))")]
    public void Evaluate_NonBooleanCondition_StillRejectsResult(string code)
        => Assert.That(() => Expression.CreateClosed(code).Evaluate(null),
            Throws.TypeOf<InvalidCastException>().With.Message.Contains("must return a Boolean"));

    [TestCase("{value := 10} | .value | greater-than(0) |OR greater-than(.value | divide(0))", true)]
    [TestCase("{value := 10} | .value | less-than(0) |AND greater-than(.value | divide(0))", false)]
    public void Evaluate_BooleanCombinator_ShortCircuits(string code, bool expected)
        => Assert.That(Expression.CreateClosed(code).Evaluate(null), Is.EqualTo(expected));
}
