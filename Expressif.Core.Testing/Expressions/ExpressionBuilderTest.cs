using Expressif.Hosting;
using Expressif.Library.Temporal;
using Expressif.Library.Text;
using Expressif.Library.Text.Selection;

namespace Expressif.Testing.Expressions;

public class ExpressionBuilderTest
{
    private static ExpressionBuilder CreateBuilder()
        => ExpressifEnvironment.Default.CreateExpressionBuilder();

    [Test]
    public void Create_WithoutParameter_CorrectlyEvaluates()
    {
        var expression = CreateBuilder().Create<Lower>().Build();
        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikola tesla"));
    }

    [Test]
    public void Create_WithParameter_PreservesLiteralRuntimeValue()
    {
        var instant = new DateTime(2026, 10, 4, 12, 30, 0, DateTimeKind.Utc);
        var expression = CreateBuilder().Create<DurationBetween>(instant).Build();
        Assert.That(expression.Evaluate(instant.AddHours(4)), Is.EqualTo(TimeSpan.FromHours(4)));
    }

    [Test]
    public void Then_ChainsMultipleFunctions()
    {
        var expression = CreateBuilder()
            .Create<Lower>()
            .Then<FirstChars>(5)
            .Then<PadRight>(7, '*')
            .Build();

        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
    }

    [Test]
    public void Then_TypeOverload_ChainsMultipleFunctions()
    {
        var expression = CreateBuilder()
            .Create(typeof(Lower))
            .Then(typeof(FirstChars), 5)
            .Then(typeof(PadRight), 7, '*')
            .Build();

        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("nikol**"));
    }

    [Test]
    public void Then_Pipeline_ComposesPipelinesFromSameBuilder()
    {
        var builder = CreateBuilder();
        var suffix = builder.Create<FirstChars>(5).Then<PadRight>(7, '*');
        var expression = builder.Create<Lower>().Then(suffix).Then<Upper>().Build();

        Assert.That(expression.Evaluate("Nikola Tesla"), Is.EqualTo("NIKOL**"));
    }

    [Test]
    public void Then_DoesNotMutateEarlierPipeline()
    {
        var lower = CreateBuilder().Create<Lower>();
        var shortened = lower.Then<FirstChars>(3);

        Assert.Multiple(() =>
        {
            Assert.That(lower.ToSource(), Is.EqualTo("lower"));
            Assert.That(shortened.ToSource(), Is.EqualTo("lower | first-chars(3)"));
        });
    }

    [Test]
    public void Build_CanBeRepeated()
    {
        var pipeline = CreateBuilder().Create<Lower>().Then<Length>();
        var first = pipeline.Build();
        var second = pipeline.Build();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(first.Evaluate("Nikola Tesla"), Is.EqualTo(12));
            Assert.That(second.Evaluate("Ada"), Is.EqualTo(3));
        });
    }

    [Test]
    public void ArgumentProvider_UsesEvaluationContext()
    {
        var pipeline = CreateBuilder().Create<FirstChars>(
            Argument.From<int>(scope => scope.GetVariable<int>("length")));
        var context = EvaluationContext.CreateBuilder().AddValue("length", 6).Build();

        Assert.That(pipeline.Build().WithContext(context).Evaluate("Nikola Tesla"), Is.EqualTo("Nikola"));
    }

    [Test]
    public void ToSource_SerializesPipeline()
    {
        var source = CreateBuilder()
            .Create<Lower>()
            .Then<FirstChars>(5)
            .Then<PadRight>(7, '*')
            .ToSource();

        Assert.That(source, Is.EqualTo("lower | first-chars(5) | pad-right(7, \"*\")"));
    }
}
