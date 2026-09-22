using Expressif.Functions;
using Expressif.Syntax;
using SortTermFunction = Expressif.Library.Sorting.SortTerm;

namespace Expressif.Bindings;

internal sealed class SortTermFunctionBinder : IFunctionBinder<SortTermFunction>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
    {
        if (syntax.Arguments.Count is not (2 or 4))
            throw new BindingException("Function 'sort-term' expects a value and a tuple-bound comparer reference.");
        if (syntax.Arguments.Count == 4
            && syntax.Arguments.Skip(2).Select(FunctionBinderSyntax.RequireValue).Any(value => value is not BooleanLiteralSyntax))
            throw new BindingException("The canonical four-position SortTerm form requires literal direction and null-placement flags.");

        var arguments = new List<FunctionArgument>();
        for (var index = 0; index < syntax.Arguments.Count; index++)
        {
            var argument = syntax.Arguments[index];
            var name = argument is NamedArgumentSyntax named ? named.Name.Value : null;
            var value = FunctionBinderSyntax.RequireValue(argument);
            var isComparer = name?.Equals("comparer", StringComparison.OrdinalIgnoreCase) == true
                || (name is null && index == 1);
            if (isComparer)
            {
                var reference = value switch
                {
                    TupleBindingShorthandSyntax direct => direct,
                    OpenExpressionSyntax { Source: null, Pipeline: [TupleBindingShorthandSyntax nested] } => nested,
                    _ => null,
                };
                if (reference is null || reference.Direction == TupleBindingDirection.Prefix)
                    throw new BindingException("The comparer for 'sort-term' must be a tuple-bound callable reference such as compare-numeric~.");
                arguments.Add(new FunctionArgument(name, new CallableReferenceParameter(reference.Name)));
            }
            else
            {
                arguments.Add(new FunctionArgument(name, context.BindArgument(value)));
            }
        }
        return Function.FromArguments(syntax.Name, arguments.ToArray());
    }
}
