using Expressif.Bindings;
using Expressif.Cli.Commands;

namespace Expressif.Cli.Tests;

public class BoundTreeFormatterTests
{
    [Test]
    public void Format_ClosedExpression_RendersEveryParameterShape()
    {
        var parameters = new IParameter[]
        {
            new ArrayParameter([new LiteralParameter(true), new QuotedLiteralParameter("text")]),
            new TupleParameter([new ObjectIndexParameter(2), new TupleProjectionParameter(1, true)]),
            new ScopedTupleProjectionParameter(1, 3),
            new RecordLiteralParameter([new RecordLiteralField("name", new ObjectPropertyParameter("source"))]),
            new RecordDefinitionParameter([
                new RecordSpreadEntry(new IncomingValueParameter()),
                new RecordNamedEntry("amount", new LiteralParameter(12.5m)),
                new UnknownRecordEntry(),
            ]),
            new OpenExpressionParameter(new OpenExpression([new Function("trim", [])])),
            new InputExpressionParameter(new ClosedExpression(
                new VariableParameter("input"),
                [new Function("upper", [])])),
            new IntervalParameter(new IntervalBinding(
                new IntervalBoundBinding(IntervalBoundBindingKind.NegativeInfinity),
                new IntervalBoundBinding(IntervalBoundBindingKind.Finite, 10),
                false,
                true)),
            new PredicationParameter(new SinglePredication(new Function("even", []))),
            new PredicationParameter(new UnknownPredication()),
            new LiteralParameter(new[] { 1, 2 }),
            new LiteralParameter(new DisplayValue()),
        };
        var root = new ClosedRootExpression(new ClosedExpression(
            new VariableParameter("source"),
            [new Function("project", parameters, FunctionSyntax.MapShorthand)]));

        var result = BoundTreeFormatter.Format(root, "tree");

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("ClosedExpression"));
            Assert.That(result, Does.Contain("Function: project (from MapShorthand)"));
            Assert.That(result, Does.Contain("Spread: IncomingValue"));
            Assert.That(result, Does.Contain("UnknownRecordEntry"));
            Assert.That(result, Does.Contain("LowerBound: NegativeInfinity"));
            Assert.That(result, Does.Contain("UnknownPredication"));
            Assert.That(result, Does.Contain("1, 2"));
            Assert.That(result, Does.Contain("display-value"));
            Assert.That(result, Does.Contain("^^^$1"));
        });
    }

    [Test]
    public void Format_UnknownRoot_RendersItsRuntimeType()
        => Assert.That(
            BoundTreeFormatter.Format(new UnknownRootExpression(), "tree"),
            Does.Contain("UnknownRootExpression"));

    [Test]
    public void Format_ControlFlow_ExposesBranchOperands()
    {
        var root = new ExpressifBinder().Bind(Expressif.Syntax.ExpressionParser.Parse("try(absolute => is-positive, _ => 0)"));
        var tree = BoundTreeFormatter.Format(root, "tree");
        Assert.That(tree, Does.Contain("Predicate").And.Contain("Expression").And.Contain("Fallback"));
    }

    private sealed class UnknownRootExpression : IRootExpression;
    private sealed class UnknownRecordEntry : IRecordDefinitionEntry;
    private sealed class UnknownPredication : IPredication;
    private sealed class DisplayValue
    {
        public override string ToString() => "display-value";
    }
}
