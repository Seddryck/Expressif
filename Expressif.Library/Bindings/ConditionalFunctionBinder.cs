using Expressif.Functions;
using Expressif.Functions.Flow;
using Expressif.Syntax;

namespace Expressif.Bindings;

internal sealed class ConditionalFunctionBinder : IFunctionBinder<Conditional>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
    {
        if (syntax.Arguments.Count != 2 || syntax.Arguments.Any(argument => argument is not PositionalArgumentSyntax))
            throw new BindingException("A conditional operator requires two positional operands.");
        return new Function(
            syntax.Name,
            syntax.Arguments.Select(argument => context.BindArgument(FunctionBinderSyntax.RequireValue(argument))).ToArray(),
            syntax.Name.Equals("conditional-forward", StringComparison.OrdinalIgnoreCase)
                ? FunctionSyntax.ConditionalForward
                : FunctionSyntax.ConditionalBackward);
    }
}
