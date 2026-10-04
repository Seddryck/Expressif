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
        var builtExpression = environment.CreateExpressionBuilder().Chain<Lower>().Build();
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
        var builtExpression = environment.CreateExpressionBuilder().Chain<BumpPatch>().Build();

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
    public void ContextAndEvaluationContext_HaveSeparateLifetimes()
    {
        var context = new Context();
        context.Variables.Add<string>("suffix", "!");
        var built = ExpressifEnvironment.Default.CreateExpressionBuilder(context)
            .Chain<Append>(ctx => ctx.Variables["suffix"])
            .Build();

        var reusable = ExpressifEnvironment.Default.CreateExpression("append(@suffix)");
        var evaluated = reusable.WithContext(new EvaluationContext(
            new Dictionary<string, object?> { ["suffix"] = "?" }));
        var predication = ExpressifEnvironment.Default.CreatePredication("starts-with(@prefix)")
            .WithContext(new EvaluationContext(
                new Dictionary<string, object?> { ["prefix"] = "Nik" }));

        Assert.Multiple(() =>
        {
            Assert.That(built.Evaluate("Hello"), Is.EqualTo("Hello!"));
            Assert.That(evaluated.Evaluate("Hello"), Is.EqualTo("Hello?"));
            Assert.That(predication.Evaluate("Nikola Tesla"), Is.True);
        });
    }
}
