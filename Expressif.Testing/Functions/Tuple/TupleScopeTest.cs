using Expressif.Bindings;
using Expressif.Serializers;
using Expressif.Syntax;
using Expressif.Testing.Conformance;
using NUnit.Framework;
using TupleValue = Expressif.Values.Tuple;

namespace Expressif.Testing.Functions.Tuple;

public class TupleScopeTest
{
    [Conformance]
    public void TupleScope_References(string source, string expression, decimal? expected)
    {
        var input = Expression.CreateClosed(source).Evaluate(null);
        Assert.That(Expression.Create(expression).Evaluate(input), Is.EqualTo(expected));
    }

    [TestCase("^$1")]
    [TestCase("^^$1")]
    [TestCase("^^^^$12")]
    public void Serialize_Reference_PreservesScope(string source)
    {
        var function = new ExpressifBinder().BindFunction(ExpressionParser.Parse(source));
        Assert.That(new FunctionSerializer().Serialize(function), Is.EqualTo(source));
        var call = new ExpressifBinder().BindFunction(ExpressionParser.Parse($"add({source})"));
        Assert.That(new FunctionSerializer().Serialize(call), Is.EqualTo($"add({source})"));
    }

    [Test]
    public void Parse_QuotedReference_RemainsLiteral()
        => Assert.That(Expression.CreateClosed("\"^^$1\"").Evaluate(null), Is.EqualTo("^^$1"));

    [Test]
    public void Evaluate_Failure_RestoresEnclosingFrame()
    {
        var parent = new EvaluationFrame(new TupleValue(10, 20), new TupleValue(10, 20));
        var expression = Expression.Create("apply(T(1, 2) | tuple(...#null))");
        using var scope = EvaluationRuntime.Enter(parent, EvaluationContext.Empty);
        Assert.Throws<SpreadArgumentException>(() => expression.Evaluate(new TupleValue(30, 40)));
        Assert.That(EvaluationRuntime.Frame, Is.SameAs(parent));
    }

    [TestCase("^$2147483648")]
    [TestCase("add(^^$2147483648)")]
    public void Bind_OverflowingPosition_ReportsDiagnostic(string source)
        => Assert.Throws<BindingException>(() => Expression.Create(source));

    [TestCase("^$-1")]
    [TestCase("^^$^1")]
    public void Parse_InvalidPosition_ReportsDiagnostic(string source)
        => Assert.Throws<ExpressifSyntaxException>(() => ExpressionParser.Parse(source));

    [Test]
    public void Evaluate_ConcurrentCalls_IsolatesScopes()
    {
        var expression = Expression.Create("apply(^$0 | add(^^$1))");
        Parallel.For(0, 100, index =>
            Assert.That(expression.Evaluate(new TupleValue(index, 10)), Is.EqualTo(index + 10)));
    }
}
