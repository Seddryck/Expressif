using Expressif.Bindings;
using Expressif.Functions;
using Expressif.Syntax;

namespace Expressif.Testing.Bindings;

public sealed class FunctionBinderExtensionTest
{
    [TestCase("custom")]
    [TestCase("custom-alias")]
    public void Bind_RegisteredFunction_UsesBinderAssociatedWithImplementationType(string name)
    {
        var binder = new ExpressifBinder(
            [new CustomTypeMapper()],
            new FunctionBinderRegistry([new CustomFunctionBinder()]));

        var function = binder.BindFunction(ExpressionParser.Parse($"{name}(ignored := 1)"));

        Assert.That(function.Parameters, Is.EqualTo(new[] { new LiteralParameter("specialized") }));
    }

    private sealed class CustomFunctionBinder : IFunctionBinder<CustomFunction>
    {
        public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
            => new(syntax.Name, [new LiteralParameter("specialized")]);
    }

    private sealed class CustomTypeMapper : BaseTypeMapper
    {
        protected override IDictionary<string, Type> Initialize()
            => new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                ["custom"] = typeof(CustomFunction),
                ["custom-alias"] = typeof(CustomFunction),
            };
    }

    private sealed class CustomFunction : IFunction
    {
        public object? Evaluate(object? value) => value;
    }
}
