using Expressif.Functions;
using Expressif.Library.Array;
using Expressif.Library.Flow;
using Expressif.Syntax;
using TryFunction = Expressif.Library.Flow.Try;

namespace Expressif.Bindings;

internal sealed class ControlFlowFunctionBinder :
    IFunctionBinder<Switch>,
    IFunctionBinder<TryFunction>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
    {
        var isTry = syntax.Name.Equals("try", StringComparison.OrdinalIgnoreCase);
        if (syntax.Arguments.Count < (isTry ? 2 : 1))
            throw new BindingException($"Function '{syntax.Name}' has too few branches.");
        var branches = new List<IParameter>();
        for (var index = 0; index < syntax.Arguments.Count; index++)
        {
            var argument = syntax.Arguments[index];
            if (argument is NamedArgumentSyntax { Name.Value: "fallback" } fallback)
            {
                if (index == 0 || index != syntax.Arguments.Count - 1)
                    throw new BindingException("A catch-all fallback must follow ordinary branches and be final.");
                branches.Add(new ControlFlowBranchParameter(context.BindArgument(fallback.Value), null));
            }
            else if (argument is PositionalArgumentSyntax
                {
                    Value: OpenExpressionSyntax
                    {
                        Source: null,
                        Pipeline: [FunctionCallSyntax { Name: "branch", Arguments.Count: 2 } pair],
                    },
                })
            {
                branches.Add(new ControlFlowBranchParameter(
                    context.BindArgument(FunctionBinderSyntax.RequireValue(pair.Arguments[isTry ? 0 : 1])),
                    context.BindArgument(FunctionBinderSyntax.RequireValue(pair.Arguments[isTry ? 1 : 0]))));
            }
            else
            {
                throw new BindingException("Invalid control-flow branch.");
            }
        }
        return new Function(syntax.Name.ToLowerInvariant(), branches.ToArray());
    }
}
