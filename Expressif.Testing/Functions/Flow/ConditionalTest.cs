using Expressif.Bindings;
using Expressif.Functions.Flow;
using Expressif.Serializers;

namespace Expressif.Testing.Functions.Flow;

public class ConditionalTest
{
    [TestCase("is-negative ?> absolute", -12, 12)]
    [TestCase("is-negative ?> absolute", 12, 12)]
    [TestCase("absolute <? greater-than(10)", -12, 12)]
    [TestCase("absolute <? greater-than(10)", -5, -5)]
    [TestCase("(absolute | add(5)) <? greater-than(20)", -18, 23)]
    [TestCase("(absolute | add(5)) <? greater-than(20)", -10, -10)]
    [TestCase("absolute | add(5) <? greater-than(20)", -10, 10)]
    [TestCase("absolute |\n add(5) <? greater-than(20)", -10, 10)]
    [TestCase("is-negative ?> (absolute | add(5)) | add(2)", -10, 17)]
    [TestCase("is-negative ?> (absolute | add(5)) | add(2)", 10, 12)]
    [TestCase("absolute <? greater-than(10) | add(2)", -5, -3)]
    [TestCase("(is-negative ?> absolute) <? greater-than(10)", -5, -5)]
    [TestCase("is-negative ?> (absolute <? greater-than(10))", -5, -5)]
    [TestCase("try((absolute | add(5)) <? greater-than(20) => is-positive, _ => 0)", -10, 0)]
    [TestCase("switch(is-negative => absolute <? greater-than(10), _ => 0)", -5, -5)]
    [TestCase("(try(absolute => greater-than(10), _ => 1)) <? greater-than(5)", -5, -5)]
    [TestCase("#false ?> divide(0)", 5, 5)]
    [TestCase("is-within-interval(I]0, 10[) ?> add(2)", 5, 7)]
    [TestCase("absolute <? is-within-interval(I[0, 10[)", -5, 5)]
    [TestCase("switch(is-within-interval(I]0, 10[) => add(2), _ => 0)", 5, 7)]
    [TestCase("switch(#false => 1, _ /* fallback */ => 7)", 5, 7)]
    [TestCase("(is-positive |AND is-negative) ?> add(2)", 5, 5)]
    public void Evaluate_Conditional(string source, int input, int expected)
        => Assert.That(Expression.Create(source).Evaluate(input), Is.EqualTo(expected));

    [TestCase("conditional-forward", FunctionSyntax.ConditionalForward)]
    [TestCase("CONDITIONAL-FORWARD", FunctionSyntax.ConditionalForward)]
    [TestCase("Conditional-Forward", FunctionSyntax.ConditionalForward)]
    [TestCase("conditional-backward", FunctionSyntax.ConditionalBackward)]
    [TestCase("CONDITIONAL-BACKWARD", FunctionSyntax.ConditionalBackward)]
    [TestCase("Conditional-Backward", FunctionSyntax.ConditionalBackward)]
    public void Bind_ConditionalName_IgnoresCase(string name, FunctionSyntax expected)
    {
        var function = new ExpressifBinder().BindFunction(ExpressionParser.Parse($"{name}(#true, #false)"));
        Assert.That(function.Syntax, Is.EqualTo(expected));
    }

    [TestCase("is-negative ?> absolute // trailing comment", -12, 12)]
    [TestCase("is-negative ?> absolute // trailing comment", 12, 12)]
    [TestCase("absolute <? greater-than(10) // trailing comment", -12, 12)]
    [TestCase("absolute <? greater-than(10) // trailing comment", -5, -5)]
    public void Evaluate_TrailingLineComment(string source, int input, int expected)
        => Assert.That(Expression.Create(source).Evaluate(input), Is.EqualTo(expected));

    [TestCase("#null <? is-null")]
    [TestCase("#true ?> #null")]
    public void Evaluate_AcceptsNull(string source)
        => Assert.That(Expression.Create(source).Evaluate(1), Is.Null);

    [TestCase("#null <? #false")]
    [TestCase("#false ?> #null")]
    public void Evaluate_RejectionPreservesInput(string source)
        => Assert.That(Expression.Create(source).Evaluate(7), Is.EqualTo(7));

    [TestCase("42 ?> absolute")]
    [TestCase("absolute <? 42")]
    public void Evaluate_RequiresBoolean(string source)
        => Assert.Throws<PredicateEvaluationException>(() => Expression.Create(source).Evaluate(-1));

    [TestCase("#true ?> absolute <? is-positive")]
    [TestCase("switch(#true => 1]")]
    [TestCase("absolute <? is-positive <? #true")]
    [TestCase("#true ?> #true ?> absolute")]
    [TestCase("?> absolute")]
    [TestCase("absolute <?")]
    public void Parse_RejectsInvalidOperators(string source)
        => Assert.Catch(() => Expression.Create(source));

    [TestCase("is-negative ?> (absolute | add(5))")]
    [TestCase("(absolute | add(5)) <? greater-than(20)")]
    [TestCase("#null <? is-null")]
    [TestCase("switch(#true => (absolute | add(5)) <? greater-than(20), _ => 0)")]
    [TestCase("TRY(absolute => is-positive, _ => 0)")]
    [TestCase("SWITCH(is-positive => 1, _ => 0)")]
    public void Serialize_RoundTrip(string source)
    {
        var root = (OpenRootExpression)new ExpressifBinder().Bind(ExpressionParser.Parse(source));
        var serialized = new ExpressionSerializer().Serialize(root.Expression);
        Assert.That(Expression.Create(serialized).Evaluate(-10), Is.EqualTo(Expression.Create(source).Evaluate(-10)));
    }

    [Test]
    public void Evaluate_CurrentObjectPreserved()
        => Assert.That(Expression.Create("{threshold := 10, value := -12} | .value | absolute <? greater-than(^.threshold)").Evaluate(null), Is.EqualTo(12));

    [Test]
    public void Evaluate_CandidateRunsOnce()
    {
        var calls = 0;
        var expression = new Conditional(value => { calls++; return value; }, _ => false, true);
        Assert.That(expression.Evaluate(7), Is.EqualTo(7));
        Assert.That(calls, Is.EqualTo(1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Evaluate_PredicateErrorPropagates(bool backward)
    {
        var expression = new Conditional(value => value, _ => throw new InvalidOperationException(), backward);
        Assert.Throws<InvalidOperationException>(() => expression.Evaluate(1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Evaluate_ExpressionErrorPropagates(bool backward)
    {
        var expression = new Conditional(_ => throw new InvalidOperationException(), _ => true, backward);
        Assert.Throws<InvalidOperationException>(() => expression.Evaluate(1));
    }

    [Test]
    public void Evaluate_ForwardSkipsExpression()
    {
        var expression = new Conditional(_ => throw new InvalidOperationException(), _ => false, false);
        Assert.That(expression.Evaluate(7), Is.EqualTo(7));
    }

    [Test]
    public void Evaluate_ConcurrentReuse()
    {
        var expression = Expression.Create("absolute <? greater-than(10)");
        Parallel.For(1, 100, index =>
            Assert.That(expression.Evaluate(-index), Is.EqualTo(index > 10 ? index : -index)));
    }

    [Test]
    public void Serialize_FieldPipelineRoundTrip()
    {
        const string source = "try(^.value | absolute => greater-than(^.threshold), _ => 0)";
        var root = (OpenRootExpression)new ExpressifBinder().Bind(ExpressionParser.Parse(source));
        var serialized = new ExpressionSerializer().Serialize(root.Expression);
        var value = new Dictionary<string, object?> { ["value"] = -12, ["threshold"] = 10 };
        Assert.That(Expression.Create(serialized).Evaluate(value), Is.EqualTo(12));
    }

    [Test]
    public void Parse_QuotedOperators()
        => Assert.That(Expression.Create("""#true ?> "a ?> b <? c" | neutral""").Evaluate(1), Is.EqualTo("a ?> b <? c"));
}
