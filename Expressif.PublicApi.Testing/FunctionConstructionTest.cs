using Expressif.Bindings;
using Expressif.Discovery;
using Expressif.Functions;
using Expressif.Syntax;
using NUnit.Framework;

namespace Expressif.PublicApi.Testing;

public class FunctionConstructionTest
{
    [Test]
    public void FromArguments_PreservesNamedAndSpreadArguments()
    {
        FunctionArgument[] arguments =
        [
            new("selector", new LiteralParameter("name")),
            new("values", new ArrayParameter([new LiteralParameter(1)]), IsSpread: true),
        ];

        var function = Function.FromArguments(new OperatorIdentity("sample", "project"), arguments);
        arguments[0] = new FunctionArgument("replacement", new LiteralParameter("other"));

        Assert.Multiple(() =>
        {
            Assert.That(function.Identity, Is.EqualTo(new OperatorIdentity("sample", "project")));
            Assert.That(function.Arguments[0].Name, Is.EqualTo("selector"));
            Assert.That(function.Arguments[1].IsSpread, Is.True);
            Assert.That(function.Parameters, Is.EqualTo(function.Arguments.Select(argument => argument.Value)));
        });
    }

    [Test]
    public void ExternalBinder_CanUseCompletePublicConstructionModel()
        => Assert.That(new ExtensionBinder(), Is.InstanceOf<IFunctionBinder<ExtensionFunction>>());

    private sealed class ExtensionBinder : IFunctionBinder<ExtensionFunction>
    {
        public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
            => Function.FromArguments(
                syntax.Name,
                [
                    new FunctionArgument("selector", new LiteralParameter("name")),
                    new FunctionArgument("values", new ArrayParameter(Array.Empty<IParameter>()), IsSpread: true),
                ]);
    }

    private sealed class ExtensionFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }
}
