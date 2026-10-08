using Expressif.Functions;
using Expressif.Library.Numeric;
using Expressif.Library.Record;
using Expressif.Library.Operators;
using Expressif.Syntax;

namespace Expressif.Bindings;

internal sealed class FieldFunctionBinder :
    IFunctionBinder<Field>,
    IFunctionBinder<IsPresent>,
    IFunctionBinder<IsAbsent>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
    {
        if (syntax.Arguments is [PositionalArgumentSyntax positional]
            && FunctionBinderSyntax.TryGetBareFunctionName(positional.Value, out var fieldName))
            return new Function(syntax.Name, [new LiteralParameter(fieldName)]);
        return new Function(syntax.Name, syntax.Arguments
            .Select(argument => context.BindArgument(FunctionBinderSyntax.RequireValue(argument)))
            .ToArray());
    }
}
