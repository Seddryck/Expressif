using Expressif.Functions;
using Expressif.Functions.Record;
using Expressif.Syntax;

namespace Expressif.Bindings;

internal sealed class WithFunctionBinder : IFunctionBinder<With>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
    {
        if (syntax.Arguments.Count < 2
            || syntax.Arguments[^1] is not PositionalArgumentSyntax { Value: { } body }
            || syntax.Arguments.Take(syntax.Arguments.Count - 1).Any(argument => argument is not NamedArgumentSyntax))
            throw new BindingException("Function 'with' expects one or more named projections followed by a body expression.");

        var names = new HashSet<string>(StringComparer.Ordinal);
        var projections = syntax.Arguments
            .Take(syntax.Arguments.Count - 1)
            .Cast<NamedArgumentSyntax>()
            .Select(named =>
            {
                if (!names.Add(named.Name.Value))
                    throw new BindingException($"Duplicate projection '{named.Name.Value}' in with(...).");
                return new WithProjection(named.Name.Value, context.BindArgument(named.Value));
            })
            .ToArray();
        return new Function(syntax.Name, [new WithDefinitionParameter(projections, context.BindArgument(body))]);
    }
}
