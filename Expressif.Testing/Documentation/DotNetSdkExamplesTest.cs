using Expressif.Functions.Text;
using Expressif.Predicates.Text;

namespace Expressif.Testing.Documentation;

public class DotNetSdkExamplesTest
{
    [Test]
    [Category("documentation")]
    public void FirstEvaluation_ReturnsUppercaseTrimmedText()
    {
        var expression = Expression.Create("trim | upper");
        var result = expression.Evaluate("  Alice  ");

        Assert.That(result, Is.EqualTo("ALICE"));
    }

    [Test]
    [Category("documentation")]
    public void InstalledPackage_EvaluatesExpression()
    {
        var expression = Expression.Create("lower");
        var result = expression.Evaluate("Nikola Tesla");

        Assert.That(result, Is.EqualTo("nikola tesla"));
    }

    [Test]
    [Category("documentation")]
    public void Expression_EvaluatesMultipleInputs()
    {
        var normalizeName = Expression.Create("trim | upper");

        var firstResult = normalizeName.Evaluate("  Nikola Tesla  ");
        var secondResult = normalizeName.Evaluate("  Ada Lovelace  ");

        Assert.Multiple(() =>
        {
            Assert.That(firstResult, Is.EqualTo("NIKOLA TESLA"));
            Assert.That(secondResult, Is.EqualTo("ADA LOVELACE"));
        });
    }

    [Test]
    [Category("documentation")]
    public void Expression_ResultCanBePatternMatched()
    {
        var firstResult = Expression.Create("trim | upper").Evaluate("  Nikola Tesla  ");
        string? normalizedName = null;

        if (firstResult is string value)
            normalizedName = value;

        Assert.That(normalizedName, Is.EqualTo("NIKOLA TESLA"));
    }

    [Test]
    [Category("documentation")]
    public void Expression_UsesVariablesFromEvaluationContext()
    {
        var expression = Expression.Create("append(@suffix)");

        var context = new EvaluationContext(
            new Dictionary<string, object?>
            {
                ["suffix"] = " Nikola!",
            });

        var configuredExpression = expression.WithContext(context);
        var result = configuredExpression.Evaluate("Hello");

        Assert.That(result, Is.EqualTo("Hello Nikola!"));
    }

    [Test]
    [Category("documentation")]
    public void Expression_CanBeReusedWithDifferentContexts()
    {
        var expression = Expression.Create("append(@suffix)");

        var excited = expression.WithContext(new EvaluationContext(
            new Dictionary<string, object?> { ["suffix"] = "!" }));

        var questioning = expression.WithContext(new EvaluationContext(
            new Dictionary<string, object?> { ["suffix"] = "?" }));

        var first = excited.Evaluate("Really");
        var second = questioning.Evaluate("Really");

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo("Really!"));
            Assert.That(second, Is.EqualTo("Really?"));
        });
    }

    [Test]
    [Category("documentation")]
    public void Expression_EvaluatesStructuredValue()
    {
        var formatName = Expression.Create(".name | trim | append(^.suffix)");

        var input = new Dictionary<string, object?>
        {
            ["name"] = "Ada Lovelace  ",
            ["suffix"] = " (mathematician)",
        };

        var result = formatName.Evaluate(input);

        Assert.That(result, Is.EqualTo("Ada Lovelace (mathematician)"));
    }

    [Test]
    [Category("documentation")]
    public void Predication_ReturnsBoolean()
    {
        var predication = new Predication("lower-case");

        bool first = predication.Evaluate("Nikola Tesla");
        bool second = predication.Evaluate("nikola tesla");

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.False);
            Assert.That(second, Is.True);
        });
    }

    [Test]
    [Category("documentation")]
    public void Predication_HasStronglyTypedResult()
    {
        object? expressionResult = Expression.Create("lower-case")
            .Evaluate("nikola tesla");

        bool predicationResult = new Predication("lower-case")
            .Evaluate("nikola tesla");

        Assert.Multiple(() =>
        {
            Assert.That(expressionResult, Is.EqualTo(true));
            Assert.That(predicationResult, Is.True);
        });
    }

    [Test]
    [Category("documentation")]
    public void ExpressionBuilder_CreatesPipeline()
    {
        var expression = new ExpressionBuilder()
            .Chain<Lower>()
            .Chain<FirstChars>(5)
            .Build();

        var result = expression.Evaluate("Nikola Tesla");

        Assert.That(result, Is.EqualTo("nikol"));
    }

    [Test]
    [Category("documentation")]
    public void ExpressionBuilder_SerializesBeforeBuild()
    {
        var builder = new ExpressionBuilder()
            .Chain<Lower>()
            .Chain<Length>();

        var source = builder.Serialize();
        var expression = builder.Build();

        Assert.Multiple(() =>
        {
            Assert.That(source, Is.EqualTo("lower | length"));
            Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo(12));
        });
    }

    [Test]
    [Category("documentation")]
    public void PredicationBuilder_ReadsParametersFromContext()
    {
        var context = new Context();
        context.Variables.Add<string>("prefix", "Nik");

        var builder = new PredicationBuilder(context)
            .Create<StartsWith>(ctx => ctx.Variables["prefix"]);

        var predicate = builder.Build();

        Assert.That(predicate.Evaluate("Nikola Tesla"), Is.True);
    }
}
