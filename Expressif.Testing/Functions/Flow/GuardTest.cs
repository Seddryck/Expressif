using Expressif.Bindings;
using Expressif.Functions.Flow;
using Expressif.Testing.Conformance;

namespace Expressif.Testing.Functions.Flow;

public class GuardTest
{
    [Conformance]
    public void Guard_Text(string value, string expression, string expected)
        => Assert.That(Expression.Create($"guard({expression})").Evaluate(value), Is.EqualTo(expected));

    [Conformance]
    public void Guard_Numeric(int value, string expression, int expected)
        => Assert.That(Expression.Create($"guard({expression})").Evaluate(value), Is.EqualTo(expected));

    [Test]
    public void Bind_Shorthand_CreatesGuardFunction()
    {
        var root = new ExpressifBinder().Bind(Expressif.Syntax.ExpressionParser.Parse("*trim"));
        var guard = ((OpenRootExpression)root).Expression.Members.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(guard.Name, Is.EqualTo("guard"));
            Assert.That(((OpenExpressionParameter)guard.Parameters.Single()).Expression.Members.Single().Name,
                Is.EqualTo("trim"));
        }
    }

    [TestCase("  Bob  ", "Bob")]
    [TestCase(42, 42)]
    public void Evaluate_Shorthand_GuardsDirectEntry(object input, object expected)
        => Assert.That(Expression.Create("*trim").Evaluate(input), Is.EqualTo(expected));

    [Test]
    public void Evaluate_ExplicitForm_IsEquivalentToShorthand()
        => Assert.That(Expression.Create("guard(trim)").Evaluate(42), Is.EqualTo(42));

    [Test]
    public void Evaluate_NumericFamily_AcceptsIntegerDirectly()
        => Assert.That(Expression.Create("*add(1)").Evaluate(5), Is.EqualTo(6));

    [Test]
    public void Evaluate_TextThatCouldBeCoercedToNumeric_IsPreserved()
        => Assert.That(Expression.Create("*add(1)").Evaluate("5"), Is.EqualTo("5"));

    [Test]
    public void Evaluate_GroupedExpression_GuardsCompletePipeline()
        => Assert.That(Expression.Create("*(trim | append-space)").Evaluate(42), Is.EqualTo(42));

    [Test]
    public void Evaluate_UngroupedExpression_ContinuesAfterGuard()
        => Assert.That(Expression.Create("*trim | append-space").Evaluate(42), Is.EqualTo("42 "));

    [Test]
    public void Evaluate_CompatibleExpressionReturningNull_PreservesNullResult()
        => Assert.That(Expression.Create("*square-root").Evaluate(-1), Is.Null);

    [Test]
    public void Evaluate_IncompatibleInput_PreservesExactOriginalReference()
    {
        var input = new object();
        Assert.That(Expression.Create("*trim").Evaluate(input), Is.SameAs(input));
    }

    [Test]
    public void Evaluate_CompatibleGroupedExpression_RunsLaterStagesNormally()
        => Assert.That(Expression.Create("*(trim | append-space)").Evaluate(" Bob "), Is.EqualTo("Bob "));
}
