using Expressif.Bindings;

namespace Expressif.Testing.Bindings;

public class FunctionBinderTest
{
    private static ExpressifBinder Binder { get; } = new();

    [Test]
    public void Bind_Function_PreservesArguments()
    {
        var syntax = SyntaxFactory.Open(SyntaxFactory.Function(
            "text-to-func",
            SyntaxFactory.Argument(SyntaxFactory.Text("foo")),
            SyntaxFactory.Argument(SyntaxFactory.Variable("bar"))));

        var function = Binder.BindFunction(syntax);

        Assert.Multiple(() =>
        {
            Assert.That(function.Name, Is.EqualTo("text-to-func"));
            Assert.That(function.Parameters, Has.Length.EqualTo(2));
            Assert.That(function.Parameters[0], Is.TypeOf<QuotedLiteralParameter>());
            Assert.That(function.Parameters[1], Is.TypeOf<VariableParameter>());
        });
    }

    [Test]
    public void Bind_NamedArguments_PreservesNamesAndOrder()
    {
        var syntax = SyntaxFactory.Open(SyntaxFactory.Function(
            "replace-slice",
            SyntaxFactory.Named("append", SyntaxFactory.Text("abc")),
            SyntaxFactory.Named("start", SyntaxFactory.Number(2)),
            SyntaxFactory.Named("length", SyntaxFactory.Number(4))));

        var function = Binder.BindFunction(syntax);

        Assert.That(function.Arguments.Select(argument => argument.Name),
            Is.EqualTo(new[] { "append", "start", "length" }));
    }

    [Test]
    public void Bind_PositionalAfterNamed_ThrowsSpecificException()
    {
        var syntax = SyntaxFactory.Open(SyntaxFactory.Function(
            "replace-slice",
            SyntaxFactory.Named("start", SyntaxFactory.Number(2)),
            SyntaxFactory.Argument(SyntaxFactory.Number(4))));

        Assert.That(() => Binder.BindFunction(syntax),
            Throws.TypeOf<PositionalArgumentAfterNamedArgumentException>());
    }

    [Test]
    public void Bind_DuplicateNamedArgument_ThrowsSpecificException()
    {
        var syntax = SyntaxFactory.Open(SyntaxFactory.Function(
            "replace-slice",
            SyntaxFactory.Named("start", SyntaxFactory.Number(2)),
            SyntaxFactory.Named("start", SyntaxFactory.Number(4))));

        Assert.That(() => Binder.BindFunction(syntax), Throws.TypeOf<DuplicateNamedArgumentException>());
    }

    [Test]
    public void Bind_ArraySpread_PreservesSpreadAndOrder()
    {
        var syntax = SyntaxFactory.Open(SyntaxFactory.Function(
            "array",
            SyntaxFactory.Argument(SyntaxFactory.Number(1)),
            SyntaxFactory.Spread(SyntaxFactory.Variable("values")),
            SyntaxFactory.Argument(SyntaxFactory.Number(4))));

        var function = Binder.BindFunction(syntax);

        Assert.Multiple(() =>
        {
            Assert.That(function.Arguments.Select(argument => argument.IsSpread),
                Is.EqualTo(new[] { false, true, false }));
            Assert.That(function.Arguments[1].Value, Is.TypeOf<VariableParameter>());
        });
    }

    [Test]
    public void Bind_ArrayImplicitSpread_UsesIncomingValue()
    {
        var syntax = SyntaxFactory.Open(SyntaxFactory.Function("array", SyntaxFactory.Spread()));

        var function = Binder.BindFunction(syntax);

        Assert.That(function.Arguments.Single().Value, Is.TypeOf<IncomingValueParameter>());
    }

    [Test]
    public void Bind_ArrayNamedArgument_ThrowsBindingError()
    {
        var syntax = SyntaxFactory.Open(
            SyntaxFactory.Function("array", SyntaxFactory.Named("value", SyntaxFactory.Number(1))));

        Assert.That(
            () => Binder.BindFunction(syntax),
            Throws.TypeOf<BindingException>()
                .With.Message.EqualTo("Function 'array' does not support named arguments."));
    }

    [Test]
    public void Bind_RegularFunctionSpread_ThrowsBindingError()
    {
        var syntax = SyntaxFactory.Open(
            SyntaxFactory.Function("add", SyntaxFactory.Spread(SyntaxFactory.Array(SyntaxFactory.Number(1)))));

        Assert.That(
            () => Binder.BindFunction(syntax),
            Throws.TypeOf<BindingException>()
                .With.Message.EqualTo("Function 'add' does not support spread arguments."));
    }

    [Test]
    public void Bind_FunctionWithExpressionParameter_CreatesOpenExpressionParameter()
    {
        var expression = SyntaxFactory.Parenthesized(SyntaxFactory.Open(
            SyntaxFactory.Function("upper"),
            SyntaxFactory.Function("first-chars", SyntaxFactory.Argument(SyntaxFactory.Number(2)))));
        var syntax = SyntaxFactory.Open(SyntaxFactory.Function("map", SyntaxFactory.Argument(expression)));

        var function = Binder.BindFunction(syntax);

        Assert.That(function.Parameters.Single(), Is.TypeOf<OpenExpressionParameter>());
        Assert.That(((OpenExpressionParameter)function.Parameters.Single()).Expression.Members.Select(member => member.Name),
            Is.EqualTo(new[] { "upper", "first-chars" }));
    }
}
