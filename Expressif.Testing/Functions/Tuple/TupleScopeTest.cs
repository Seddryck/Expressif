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
    public void Parse_OverflowingPosition_ReportsDiagnostic(string source)
        => Assert.Throws<ExpressifSyntaxException>(() => Expression.Create(source));

    [TestCase("^$-1")]
    [TestCase("^^$^1")]
    [TestCase("^$")]
    [TestCase("^^$")]
    [TestCase("^ $1")]
    [TestCase("^$ 1")]
    [TestCase("^ ^$1")]
    [TestCase("^$01")]
    public void Parse_InvalidPosition_ReportsDiagnostic(string source)
        => Assert.Throws<ExpressifSyntaxException>(() => ExpressionParser.Parse(source));

    [TestCase("  ^^^$12  ")]
    [TestCase("lower | ^^^$12")]
    [TestCase("add(^^^$12)")]
    [TestCase("add(((^^^$12)))")]
    [TestCase("apply(T(1, 2) | add(^^^$12))")]
    public void Parse_NativeReference_PreservesAuthoredRoot(string source)
    {
        foreach (var syntax in new[] { ExpressifSyntax.Parse(source), ExpressionParser.Parse(source) })
        {
            var projection = Descendants(syntax).OfType<TupleProjectionSyntax>().Single();
            var offset = source.IndexOf("^^^$12", StringComparison.Ordinal);
            Assert.Multiple(() =>
            {
                Assert.That(projection.Text, Is.EqualTo("^^^$12"));
                Assert.That(projection.Span, Is.EqualTo(new SourceSpan(offset, 6)));
                Assert.That(projection.RootDepth, Is.EqualTo(3));
                Assert.That(projection.Index, Is.EqualTo(12));
                Assert.That(projection.Direction, Is.EqualTo(TupleProjectionDirection.FromStart));
                Assert.That(projection.Root!.Text, Is.EqualTo("^^^$"));
                Assert.That(projection.Root.Span, Is.EqualTo(new SourceSpan(offset, 4)));
                Assert.That(projection.Children, Is.EqualTo(new[] { projection.Root }));
            });
        }
    }

    [TestCase("^$1", 1)]
    [TestCase("^^^^$1", 4)]
    [TestCase("^^^^^^^^^^$1", 10)]
    public void Bind_NativeReference_PreservesDepth(string source, int depth)
    {
        var binder = new ExpressifBinder();
        var stage = binder.BindFunction(ExpressifSyntax.Parse(source));
        var argument = binder.BindFunction(ExpressifSyntax.Parse($"add(({source}))"));
        Assert.Multiple(() =>
        {
            Assert.That(stage.Syntax, Is.EqualTo(FunctionSyntax.ScopedTupleProjectionShorthand));
            Assert.That(stage.Parameters.Single(), Is.EqualTo(new ScopedTupleProjectionParameter(1, depth)));
            Assert.That(argument.Parameters.Single(), Is.EqualTo(new ScopedTupleProjectionParameter(1, depth)));
        });
    }

    [TestCase("$1", TupleProjectionDirection.FromStart, 1)]
    [TestCase("$^0", TupleProjectionDirection.FromEnd, 0)]
    [TestCase("$^1", TupleProjectionDirection.FromEnd, 1)]
    public void Parse_UnqualifiedReference_HasNoRoot(string source, TupleProjectionDirection direction, int index)
    {
        var projection = Descendants(ExpressionParser.Parse(source)).OfType<TupleProjectionSyntax>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(projection.RootDepth, Is.Zero);
            Assert.That(projection.Root, Is.Null);
            Assert.That(projection.Direction, Is.EqualTo(direction));
            Assert.That(projection.Index, Is.EqualTo(index));
        });
    }

    [Test]
    public void Bind_ScopedCoercionSelector_DoesNotDiscardScope()
        => Assert.Throws<BindingException>(() => Expression.Create("coerce(^$0 -> :numeric)"));

    [TestCase("append(\"^^$1\") /* ^^^$2 */")]
    [TestCase("append(`^^$1`) // ^^^$2")]
    public void Parse_Lookalikes_AreNotTupleReferences(string source)
        => Assert.That(Descendants(ExpressionParser.Parse(source)).OfType<TupleProjectionSyntax>(), Is.Empty);

    [Test]
    public void Evaluate_ConcurrentCalls_IsolatesScopes()
    {
        var expression = Expression.Create("apply(^$0 | add(^^$1))");
        Parallel.For(0, 100, index =>
            Assert.That(expression.Evaluate(new TupleValue(index, 10)), Is.EqualTo(index + 10)));
    }

    private static IEnumerable<SyntaxNode> Descendants(SyntaxNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }
    }
}
