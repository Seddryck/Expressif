using Expressif.Hosting;
using Expressif.Library.SemVer;
using Expressif.Library.Text;
using Expressif.Library.Text.Casing;
using Expressif.Library.Text.Concatenation;
using NUnit.Framework;

namespace Expressif.PublicApi.Testing;

public class ExpressifEnvironmentTest
{
    [Test]
    public void Default_CreatesEveryCompositionSurface()
    {
        var environment = ExpressifEnvironment.Default;

        var expression = environment.CreateExpression("trim | upper");
        var closedExpression = environment.CreateClosedExpression("\"alice\" | upper");
        var predication = environment.CreatePredication("lower-case");
        var builtExpression = environment.CreateExpressionBuilder().Create<Lower>().Build();
        var builtPredication = environment.CreatePredicationBuilder().Create<StartsWith>("Nik").Build();

        Assert.Multiple(() =>
        {
            Assert.That(environment.CreateFunctionFactory(), Is.Not.Null);
            Assert.That(environment.CreateExpressionBinder(), Is.Not.Null);
            Assert.That(environment.CreateExpressionFactory(), Is.Not.Null);
            Assert.That(expression.Evaluate("  Alice  "), Is.EqualTo("ALICE"));
            Assert.That(closedExpression.Evaluate(null), Is.EqualTo("ALICE"));
            Assert.That(predication.Evaluate("alice"), Is.True);
            Assert.That(builtExpression.Evaluate("ALICE"), Is.EqualTo("alice"));
            Assert.That(builtPredication.Evaluate("Nikola Tesla"), Is.True);
        });
    }

    [Test]
    public void RegisteredEnvironment_UsesExtensionAcrossTheSameCompositionPath()
    {
        var environment = ExpressifEnvironment.Default.RegisterLibrary<SemVerLibrary>();

        var expression = environment.CreateClosedExpression("#\"1.2.3\":semver | bump-patch");
        var factoryExpression = environment.CreateExpressionFactory()
            .CreateClosed("#\"2.3.4\":semver | bump-minor");
        var builtExpression = environment.CreateExpressionBuilder().Create<BumpPatch>().Build();

        Assert.Multiple(() =>
        {
            Assert.That(expression.Evaluate(null)?.ToString(), Is.EqualTo("1.2.4"));
            Assert.That(factoryExpression.Evaluate(null)?.ToString(), Is.EqualTo("2.4.0"));
            Assert.That(builtExpression.Evaluate(new SemanticVersion(3, 4, 5))?.ToString(), Is.EqualTo("3.4.6"));
            Assert.That(
                () => ExpressifEnvironment.Default.CreateClosedExpression("#\"1.2.3\":semver | bump-patch"),
                Throws.Exception);
        });
    }

    [Test]
    public void EvaluationContext_ConfiguresBuiltAndParsedExpressions()
    {
        var built = ExpressifEnvironment.Default.CreateExpressionBuilder()
            .Create<Append>(Argument.From<string>(ctx => ctx.GetVariable<string>("suffix")))
            .Build();

        var reusable = ExpressifEnvironment.Default.CreateExpression("append(@suffix)");
        var builtContext = EvaluationContext.CreateBuilder().AddValue("suffix", "!").Build();
        var evaluatedContext = EvaluationContext.CreateBuilder().AddValue("suffix", "?").Build();
        var predicateContext = EvaluationContext.CreateBuilder().AddValue("prefix", "Nik").Build();
        var evaluated = reusable.WithContext(evaluatedContext);
        var predication = ExpressifEnvironment.Default.CreatePredication("starts-with(@prefix)")
            .WithContext(predicateContext);

        Assert.Multiple(() =>
        {
            Assert.That(built.WithContext(builtContext).Evaluate("Hello"), Is.EqualTo("Hello!"));
            Assert.That(evaluated.Evaluate("Hello"), Is.EqualTo("Hello?"));
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        });
    }
}
