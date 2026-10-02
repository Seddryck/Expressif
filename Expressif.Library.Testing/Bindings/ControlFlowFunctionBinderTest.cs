using Expressif.Library.Flow;
using System.Reflection;

namespace Expressif.Testing.Bindings;

public sealed class ControlFlowFunctionBinderTest
{
    [TestCase("switch")]
    [TestCase("try")]
    public void BindCreatesConditionalAndFallbackBranches(string name)
    {
        var context = CreateContext();
        var conditional = Open(Call("branch", Argument(Number(1)), Argument(Number(2))));
        var syntax = Call(name, Argument(conditional), Named("fallback", Number(3)));

        var function = new ControlFlowFunctionBinder().Bind(syntax, context.Object);
        var branches = function.Parameters.Cast<ControlFlowBranchParameter>().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(function.Name, Is.EqualTo(name));
            Assert.That(branches, Has.Length.EqualTo(2));
            Assert.That(branches[0].Predicate, Is.Not.Null);
            Assert.That(branches[1].Predicate, Is.Null);
            Assert.That(context.Invocations, Has.Count.EqualTo(3));
        });
    }

    [TestCase("switch", 0)]
    [TestCase("try", 1)]
    public void BindRejectsTooFewBranches(string name, int branchCount)
    {
        var arguments = Enumerable.Range(0, branchCount)
            .Select(index => Argument(Open(Call("branch", Argument(Number(index)), Argument(Number(index))))))
            .ToArray();

        Assert.That(
            () => new ControlFlowFunctionBinder().Bind(Call(name, arguments), CreateContext().Object),
            Throws.TypeOf<BindingException>().With.Message.Contains("too few branches"));
    }

    [Test]
    public void BindRejectsFallbackBeforeFinalPosition()
    {
        var syntax = Call(
            "switch",
            Named("fallback", Number(0)),
            Argument(Open(Call("branch", Argument(Number(1)), Argument(Number(2))))));

        Assert.That(
            () => new ControlFlowFunctionBinder().Bind(syntax, CreateContext().Object),
            Throws.TypeOf<BindingException>().With.Message.Contains("must follow ordinary branches"));
    }

    [Test]
    public void BindRejectsInvalidBranchShape()
        => Assert.That(
            () => new ControlFlowFunctionBinder().Bind(
                Call("switch", Argument(Number(1))),
                CreateContext().Object),
            Throws.TypeOf<BindingException>().With.Message.EqualTo("Invalid control-flow branch."));

    private static Mock<IFunctionBindingContext> CreateContext()
    {
        var context = new Mock<IFunctionBindingContext>();
        context.Setup(candidate => candidate.BindArgument(It.IsAny<ExpressionSyntax>()))
            .Returns((ExpressionSyntax syntax) => new LiteralParameter(syntax.Text));
        return context;
    }

    private static SourceSpan Span(string text) => new(0, text.Length);

    private static FunctionCallSyntax Call(string name, params ArgumentSyntax[] arguments)
        => Create<FunctionCallSyntax>(Span(name), name, name, arguments.Length > 0, arguments);

    private static PositionalArgumentSyntax Argument(ExpressionSyntax value)
        => Create<PositionalArgumentSyntax>(value.Span, value.Text, value);

    private static NamedArgumentSyntax Named(string name, ExpressionSyntax value)
        => Create<NamedArgumentSyntax>(
            Span(name),
            name,
            Create<ArgumentNameSyntax>(Span(name), name, name, false, null),
            value);

    private static OpenExpressionSyntax Open(params ExpressionSyntax[] pipeline)
        => Create<OpenExpressionSyntax>(new SourceSpan(0, 0), string.Empty, null, pipeline);

    private static NumericLiteralSyntax Number(decimal value)
    {
        var text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return Create<NumericLiteralSyntax>(Span(text), text);
    }

    private static T Create<T>(params object?[] arguments)
        => (T)(Activator.CreateInstance(
            typeof(T),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            arguments,
            null) ?? throw new InvalidOperationException($"Could not create {typeof(T).Name}."));
}
