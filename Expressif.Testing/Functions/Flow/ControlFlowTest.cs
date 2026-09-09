using Expressif.Bindings;
using Expressif.Functions.Flow;
using Expressif.Serializers;

namespace Expressif.Testing.Functions.Flow;

public class ControlFlowTest
{
    [TestCase("switch(is-positive => 1, greater-than(10) => 2, _ => 3)", 20, 1)]
    [TestCase("try(absolute => greater-than(10), neutral => is-negative)", -5, -5)]
    [TestCase("try(absolute => is-positive, _ => 0) | add(2)", -5, 7)]
    [TestCase("switch(is-negative => absolute | add(2), _ => 0)", -5, 7)]
    [TestCase("switch(is-negative => try(absolute => greater-than(10), _ => 7), _ => 0)", -5, 7)]
    public void Evaluate_Branches(string source, int input, int expected)
        => Assert.That(Expression.Create(source).Evaluate(input), Is.EqualTo(expected));

    [TestCase("switch(is-positive => #null, _ => 42)")]
    [TestCase("try(#null => is-null, _ => 42)")]
    public void Evaluate_AcceptedNull(string source)
        => Assert.That(Expression.Create(source).Evaluate(1), Is.Null);

    [TestCase("switch(#true => 1, #true => coerce(:integer) | divide(0))")]
    [TestCase("try(1 => #true, divide(0) => #true)")]
    public void Evaluate_SkipsLaterBranch(string source)
        => Assert.That(Expression.Create(source).Evaluate(1), Is.EqualTo(1));

    [TestCase("switch(42 => 1, _ => 0)")]
    [TestCase("try(1 => 42, _ => 0)")]
    public void Evaluate_RequiresBoolean(string source)
        => Assert.Throws<PredicateEvaluationException>(() => Expression.Create(source).Evaluate(1));

    [TestCase("switch()")]
    [TestCase("switch(_ => 1)")]
    [TestCase("switch(_ => 1, #true => 2)")]
    [TestCase("switch(#true => 1, _ => 2, _ => 3)")]
    [TestCase("switch(#true, 1)")]
    [TestCase("switch(#true =>)")]
    [TestCase("switch(=> 1)")]
    [TestCase("switch(#true => 1 => 2)")]
    [TestCase("switch(#true => 1")]
    [TestCase("try(1 => #true)")]
    [TestCase("try(_ => 1, _ => 2)")]
    public void Bind_InvalidBranches(string source)
        => Assert.Catch(() => Expression.Create(source));

    [Test]
    public void Evaluate_CurrentObjectPreserved()
    {
        var expression = Expression.Create("{threshold := 10, value := -12} | .value | try(absolute => greater-than(^.threshold), _ => 0)");
        Assert.That(expression.Evaluate(null), Is.EqualTo(12));
    }

    [Test]
    public void Evaluate_CandidateRunsOnceAndErrorsPropagate()
    {
        var count = 0;
        var function = new Try([
            new ControlFlowBranch(value => { count++; return value; }, _ => true),
            new ControlFlowBranch(_ => throw new InvalidOperationException(), null),
        ]);
        Assert.That(function.Evaluate(7), Is.EqualTo(7));
        Assert.That(count, Is.EqualTo(1));
        var failing = new Try([
            new ControlFlowBranch(_ => throw new InvalidOperationException(), _ => false),
            new ControlFlowBranch(_ => 0, null),
        ]);
        Assert.Throws<InvalidOperationException>(() => failing.Evaluate(1));
    }

    [TestCase("switch(is-positive => 1, _ => 0)")]
    [TestCase("try(absolute => is-positive, _ => 0)")]
    public void Serialize_RoundTrip(string source)
    {
        var root = (OpenRootExpression)new ExpressifBinder().Bind(ExpressionParser.Parse(source));
        var serialized = new ExpressionSerializer().Serialize(root.Expression);
        Assert.That(Expression.Create(serialized).Evaluate(2), Is.EqualTo(Expression.Create(source).Evaluate(2)));
    }

    [Test]
    public void Evaluate_PredicateErrorDoesNotFallBack()
    {
        var function = new Try([
            new ControlFlowBranch(_ => 1, _ => throw new InvalidOperationException()),
            new ControlFlowBranch(_ => 0, null),
        ]);
        Assert.Throws<InvalidOperationException>(() => function.Evaluate(7));
    }

    [Test]
    public void Evaluate_ConcurrentReuse()
    {
        var expression = Expression.Create("try(absolute => greater-than(10), neutral => is-negative, _ => 0)");
        Parallel.For(1, 100, index =>
            Assert.That(expression.Evaluate(-index), Is.EqualTo(index > 10 ? index : -index)));
    }

    [TestCase("switch(#false => 1)")]
    [TestCase("try(1 => #false, 2 => #false)")]
    public void Evaluate_ExhaustedBranches(string source)
        => Assert.That(Expression.Create(source).Evaluate(1), Is.Null);

    [Test]
    public void Parse_QuotedDelimiters()
        => Assert.That(Expression.Create("""switch(#true => "a, => switch(x)", _ => "b")""").Evaluate(1), Is.EqualTo("a, => switch(x)"));
}
