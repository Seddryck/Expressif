using Expressif.Bindings;
using Expressif.Syntax;
using RuntimeExpression = Expressif.IExpression;

namespace Expressif.Testing;

[TestFixture]
public class ExpressionApiTest
{
    [Test]
    public void ParseThenBind_UsesExplicitStages()
    {
        var syntax = ExpressionParser.Parse("upper");
        var expression = ExpressionBinder.Bind(syntax);

        Assert.That(expression.Evaluate("foo"), Is.EqualTo("FOO"));
    }

    [Test]
    public void Create_WithConfiguredComponents_UsesParserThenBinder()
    {
        var parser = new TrackingParser();
        var binder = new TrackingBinder();
        var factory = new Expressif.ExpressionFactory(parser, binder);

        var expression = factory.Create("ignored");

        Assert.Multiple(() =>
        {
            Assert.That(parser.CallCount, Is.EqualTo(1));
            Assert.That(binder.CallCount, Is.EqualTo(1));
            Assert.That(binder.LastSyntax, Is.SameAs(parser.Syntax));
            Assert.That(expression, Is.SameAs(binder.Expression));
        });
    }

    [Test]
    public void Create_WithSyntax_DoesNotReparse()
    {
        var parser = new TrackingParser();
        var binder = new TrackingBinder();
        var factory = new Expressif.ExpressionFactory(parser, binder);

        var expression = factory.Create(parser.Syntax);

        Assert.Multiple(() =>
        {
            Assert.That(parser.CallCount, Is.Zero);
            Assert.That(binder.CallCount, Is.EqualTo(1));
            Assert.That(expression, Is.SameAs(binder.Expression));
        });
    }

    [Test]
    public void Create_WithOnlyCustomParser_UsesStandardBinder()
    {
        var parser = new TrackingParser();
        var factory = new Expressif.ExpressionFactory(parser: parser);

        var expression = factory.Create("ignored");

        Assert.Multiple(() =>
        {
            Assert.That(parser.CallCount, Is.EqualTo(1));
            Assert.That(expression.Evaluate("foo"), Is.EqualTo("FOO"));
        });
    }

    [Test]
    public void Create_WithOnlyCustomBinder_UsesStandardParser()
    {
        var binder = new TrackingBinder();
        var factory = new Expressif.ExpressionFactory(binder: binder);

        var expression = factory.Create("upper");

        Assert.Multiple(() =>
        {
            Assert.That(binder.CallCount, Is.EqualTo(1));
            Assert.That(binder.LastSyntax, Is.Not.Null);
            Assert.That(expression, Is.SameAs(binder.Expression));
        });
    }

    [Test]
    public void ExpressionCreate_UsesDefaultFactory()
        => Assert.That(Expression.Create("upper").Evaluate("foo"), Is.EqualTo("FOO"));

    private sealed class TrackingParser : IExpressionParser
    {
        public RootExpressionSyntax Syntax { get; } = ExpressionParser.Parse("upper");
        public int CallCount { get; private set; }

        public RootExpressionSyntax Parse(string text)
        {
            CallCount++;
            return Syntax;
        }
    }

    private sealed class TrackingBinder : IExpressionBinder
    {
        public RuntimeExpression Expression { get; } = new StubExpression();
        public int CallCount { get; private set; }
        public RootExpressionSyntax? LastSyntax { get; private set; }

        public RuntimeExpression Bind(RootExpressionSyntax syntax)
        {
            CallCount++;
            LastSyntax = syntax;
            return Expression;
        }
    }

    private sealed class StubExpression : RuntimeExpression
    {
        public object? Evaluate(object? value) => value;
    }
}
