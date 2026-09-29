using Expressif.Functions;
using Expressif.Library.Special;
using Expressif.Syntax;
using Expressif.Values.Types;

namespace Expressif.Bindings;

internal sealed class CoerceFunctionBinder : IFunctionBinder<Coerce>
{
    public Function Bind(FunctionCallSyntax syntax, IFunctionBindingContext context)
    {
        if (syntax.Arguments.Count == 0)
            throw new BindingException("Function 'coerce' expects one or more coercion specifications.");
        if (syntax.Arguments.Any(argument => argument is not PositionalArgumentSyntax))
            throw new BindingException("Function 'coerce' accepts positional coercion specifications only.");

        var specifications = syntax.Arguments
            .Select(argument => BindSpecification(FunctionBinderSyntax.RequireValue(argument), context))
            .ToArray();
        ValidateModes(specifications);
        ValidateDuplicateSelectors(specifications);
        return new Function(syntax.Name, specifications);
    }

    private static void ValidateModes(CoercionSpecificationParameter[] specifications)
    {
        if (specifications.OfType<PositionalCoercionParameter>().Any()
            && specifications.Any(specification => specification is not PositionalCoercionParameter))
            throw new BindingException("Function 'coerce' cannot mix positional type descriptors and selector mappings.");
        if (specifications.Any(specification => specification is FieldCoercionParameter)
            && specifications.Any(specification => specification is TupleCoercionParameter))
            throw new BindingException("Function 'coerce' cannot mix field and tuple-position selector mappings.");
    }

    private static void ValidateDuplicateSelectors(CoercionSpecificationParameter[] specifications)
    {
        var duplicateField = specifications.OfType<FieldCoercionParameter>()
            .GroupBy(specification => specification.Field, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateField is not null)
            throw new BindingException($"Duplicate coercion selector '{duplicateField.Key}'.");

        var duplicatePosition = specifications.OfType<TupleCoercionParameter>()
            .GroupBy(specification => specification.Position)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicatePosition is not null)
            throw new BindingException($"Duplicate coercion selector '${duplicatePosition.Key}'.");
    }

    private static CoercionSpecificationParameter BindSpecification(
        ExpressionSyntax syntax,
        IFunctionBindingContext context)
        => syntax switch
        {
            TypeLiteralSyntax type => new PositionalCoercionParameter(ResolveType(type, context)),
            BinaryExpressionSyntax { Operator.Text: "->", Right: TypeLiteralSyntax type, Left: TupleProjectionSyntax selector }
                when selector.Direction is TupleProjectionDirection.FromStart && selector.RootDepth == 0
                => new TupleCoercionParameter(selector.Index, ResolveType(type, context)),
            BinaryExpressionSyntax { Operator.Text: "->", Right: TypeLiteralSyntax type, Left: FunctionCallSyntax selector }
                when selector.Arguments.Count == 0
                => new FieldCoercionParameter(selector.Name, ResolveType(type, context)),
            _ => throw new BindingException("A coerce specification must be ':type' or 'selector -> :type'."),
        };

    private static Type ResolveType(TypeLiteralSyntax syntax, IFunctionBindingContext context)
        => context.ResolveRuntimeType(syntax.Name)
            ?? throw new BindingException($"Expressif type ':{syntax.Name}' cannot be used as a coercion target.");
}
