namespace Expressif.Testing.Expressions.Evaluation;

[TestFixture]
public class EvaluationContextTest
{
    [Test]
    public void Build_CreatesImmutableRegistrationSnapshot()
    {
        var builder = EvaluationContext.CreateBuilder().AddValue("suffix", "!");
        var context = builder.Build();
        builder.AddValue("other", "?");

        var expression = TestExpression.Create("append(@suffix)").WithContext(context);
        Assert.That(expression.Evaluate("hello"), Is.EqualTo("hello!"));
    }

    [Test]
    public void WithContext_ReturnsNewExpressionWithoutChangingOriginal()
    {
        var expression = TestExpression.Create("append(@suffix)");
        var question = EvaluationContext.CreateBuilder().AddValue("suffix", "?").Build();
        var exclamation = EvaluationContext.CreateBuilder().AddValue("suffix", "!").Build();

        Assert.Multiple(() =>
        {
            Assert.That(expression.WithContext(question).Evaluate("hello"), Is.EqualTo("hello?"));
            Assert.That(expression.WithContext(exclamation).Evaluate("hello"), Is.EqualTo("hello!"));
        });
    }

    [Test]
    public void WithContext_NullValue_IsAvailableWithoutFallback()
    {
        var context = EvaluationContext.CreateBuilder().AddValue<string?>("value", null).Build();
        var expression = TestExpression.Create("append(@value)").WithContext(context);

        Assert.That(expression.Evaluate("input"), Is.EqualTo("input"));
    }

    [Test]
    public void Provider_RunsOnceForEachTopLevelEvaluation()
    {
        var calls = 0;
        var context = EvaluationContext.CreateBuilder()
            .AddProvider("suffix", _ => { calls++; return "!"; })
            .Build();
        var expression = TestExpression.Create("append(@suffix) | append(@suffix)").WithContext(context);

        Assert.Multiple(() =>
        {
            Assert.That(expression.Evaluate("hello"), Is.EqualTo("hello!!"));
            Assert.That(expression.Evaluate("again"), Is.EqualTo("again!!"));
            Assert.That(calls, Is.EqualTo(2));
        });
    }

    [Test]
    public void Provider_ReceivesTopLevelInput()
    {
        var context = EvaluationContext.CreateBuilder()
            .AddProvider("suffix", start => $"-{start.Input}")
            .Build();
        var expression = TestExpression.Create("append(@suffix)").WithContext(context);

        Assert.That(expression.Evaluate("value"), Is.EqualTo("value-value"));
    }

    [Test]
    public void WithContext_IsSafeForConcurrentEvaluation()
    {
        var context = EvaluationContext.CreateBuilder().AddValue("suffix", "!").Build();
        var expression = TestExpression.Create("append(@suffix)").WithContext(context);

        var results = ParallelEnumerable.Range(0, 100)
            .Select(index => expression.Evaluate(index.ToString()))
            .ToArray();

        Assert.That(results, Is.EquivalentTo(Enumerable.Range(0, 100).Select(index => $"{index}!")));
    }

    [Test]
    public void PredicationWithContext_IsSafeForConcurrentEvaluation()
    {
        var context = EvaluationContext.CreateBuilder().AddValue("prefix", "value-").Build();
        var predication = TestPredication.Create("starts-with(@prefix)").WithContext(context);

        var results = ParallelEnumerable.Range(0, 100)
            .Select(index => predication.Evaluate($"value-{index}"))
            .ToArray();

        Assert.That(results, Is.All.True);
    }
}
