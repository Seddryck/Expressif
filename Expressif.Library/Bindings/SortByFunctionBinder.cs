using Expressif.Functions;
using Expressif.Functions.Sorting;
using Expressif.Syntax;
using Expressif.Types;

namespace Expressif.Bindings;

internal sealed class SortByFunctionBinder :
    IFunctionBinder<SortBy>,
    IFunctionBinder<RankBy>,
    IFunctionBinder<DenseRankBy>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
    {
        if (syntax.Arguments.Count == 0 || syntax.Arguments.Any(argument => argument is not PositionalArgumentSyntax))
            throw new BindingException($"Function '{syntax.Name}' requires one or more positional typed criteria.");
        return new Function(syntax.Name.ToLowerInvariant(), syntax.Arguments
            .Select(argument => BindCriterion(FunctionBinderSyntax.RequireValue(argument), context))
            .Cast<IParameter>()
            .ToArray());
    }

    private static SortCriterionParameter BindCriterion(ExpressionSyntax syntax, IFunctionBindingContext context)
    {
        BinaryExpressionSyntax? mapping = null;
        IReadOnlyList<ExpressionSyntax> modifiers = [];
        if (syntax is BinaryExpressionSyntax direct)
        {
            mapping = direct;
        }
        else if (syntax is OpenExpressionSyntax { Source: BinaryExpressionSyntax source } open)
        {
            mapping = source;
            modifiers = open.Pipeline.ToArray();
        }
        else if (syntax is OpenExpressionSyntax { Source: null } openWithoutSource
            && openWithoutSource.Pipeline.FirstOrDefault() is BinaryExpressionSyntax first)
        {
            mapping = first;
            modifiers = openWithoutSource.Pipeline.Skip(1).ToArray();
        }

        if (mapping is not { Operator.Text: "->", Right: TypeLiteralSyntax type })
            throw new BindingException("A sort-by criterion must use the form expression -> :type.");

        var ascending = true;
        var nullsFirst = false;
        foreach (var modifier in modifiers)
        {
            if (modifier is not FunctionCallSyntax { Arguments.Count: 0 } call)
                throw new BindingException("Sort-by criteria accept only direction and null-placement modifiers.");
            switch (call.Name.ToLowerInvariant())
            {
                case "ascending" or "asc": ascending = true; break;
                case "descending" or "desc": ascending = false; break;
                case "nulls-first": nullsFirst = true; break;
                case "nulls-last": nullsFirst = false; break;
                default: throw new BindingException($"Unsupported sort-by modifier '{call.Name}'.");
            }
        }

        return new SortCriterionParameter(context.BindArgument(mapping.Left), TypeRegistry.Resolve(type.Name), ascending, nullsFirst);
    }
}
