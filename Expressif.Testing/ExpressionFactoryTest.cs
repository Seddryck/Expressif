using Expressif.Bindings;
using Expressif.Syntax;

namespace Expressif.Testing;

[TestFixture]
public class ExpressionFactoryTest
{
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
    public void CreateClosed_WithConfiguredComponents_UsesParserThenClosedBinder()
    {
        var parser = new TrackingParser();
        var binder = new TrackingBinder();
        var factory = new Expressif.ExpressionFactory(parser, binder);
        var expression = factory.CreateClosed("ignored");

        Assert.Multiple(() =>
        {
            Assert.That(parser.CallCount, Is.EqualTo(1));
            Assert.That(binder.ClosedCallCount, Is.EqualTo(1));
            Assert.That(binder.LastSyntax, Is.SameAs(parser.Syntax));
            Assert.That(expression, Is.SameAs(binder.Expression));
        });
    }

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
        public IExpression Expression { get; } = new StubExpression();
        public int CallCount { get; private set; }
        public int ClosedCallCount { get; private set; }
        public RootExpressionSyntax? LastSyntax { get; private set; }

        public IExpression Bind(RootExpressionSyntax syntax)
        {
            CallCount++;
            LastSyntax = syntax;
            return Expression;
        }

        public IExpression BindClosed(RootExpressionSyntax syntax)
        {
            ClosedCallCount++;
            LastSyntax = syntax;
            return Expression;
        }
    }

    private sealed class StubExpression : IExpression
    {
        public object? Evaluate(object? value) => value;
        public IExpression WithContext(EvaluationContext context) => this;
    }
}
