namespace Expressif.Testing;

[TestFixture]
public class EvaluationContextTest
{
    [Test]
    public void Constructor_CopiesVariables()
    {
        var variables = new Dictionary<string, object?> { ["suffix"] = "!" };
        var context = new EvaluationContext(variables);

        variables["suffix"] = "?";

        Assert.That(context.Variables["suffix"], Is.EqualTo("!"));
    }

    [Test]
    public void WithContext_ReturnsNewExpressionWithoutChangingOriginal()
    {
        var legacy = new Context(new Dictionary<string, object?> { ["suffix"] = "?" });
        var expression = Expression.Create("append(@suffix)", legacy);
        var contextual = expression.WithContext(
            new EvaluationContext(new Dictionary<string, object?> { ["suffix"] = "!" }));

        Assert.Multiple(() =>
        {
            Assert.That(expression.Evaluate("hello"), Is.EqualTo("hello?"));
            Assert.That(contextual.Evaluate("hello"), Is.EqualTo("hello!"));
        });
    }

    [Test]
    public void WithContext_NullVariable_DoesNotFallBackToBindingContext()
    {
        var legacy = new Context(new Dictionary<string, object?> { ["value"] = "fallback" });
        var expression = Expression.Create("append(@value)", legacy).WithContext(
            new EvaluationContext(new Dictionary<string, object?> { ["value"] = null }));

        Assert.That(expression.Evaluate("input"), Is.EqualTo("input"));
    }

    [Test]
    public void WithContext_IsSafeForConcurrentEvaluation()
    {
        var expression = Expression.Create("append(@suffix)").WithContext(
            new EvaluationContext(new Dictionary<string, object?> { ["suffix"] = "!" }));

        var results = ParallelEnumerable.Range(0, 100)
            .Select(index => expression.Evaluate(index.ToString()))
            .ToArray();

        Assert.That(results, Is.EquivalentTo(Enumerable.Range(0, 100).Select(index => $"{index}!")));
    }
}
