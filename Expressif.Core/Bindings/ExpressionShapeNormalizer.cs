using System.Diagnostics.CodeAnalysis;

namespace Expressif.Bindings;

/// <summary>Recognizes reusable bound-expression shapes without constructing runtime functions.</summary>
internal static class ExpressionShapeNormalizer
{
    public static bool TryGetDirectFieldName(IParameter parameter, [NotNullWhen(true)] out string? name)
    {
        name = null;
        return parameter is OpenExpressionParameter open
            && open.Expression.Members.Count() == 1
            && TryGetLeadingFieldName(parameter, out name);
    }

    public static bool TryGetLeadingFieldName(IParameter parameter, [NotNullWhen(true)] out string? name)
    {
        name = parameter is OpenExpressionParameter open
            && open.Expression.Members.FirstOrDefault() is
                { Syntax: FunctionSyntax.FieldShorthand, Parameters: [LiteralParameter { Value: string field }] }
                    ? field
                    : null;
        return name is not null;
    }

    public static string RequireDirectFieldName(IParameter parameter, string function, string parameterName)
        => TryGetDirectFieldName(parameter, out var name)
            ? name
            : throw new BindingException(
                $"Function '{function}' parameter '{parameterName}' must be a direct field selector such as .tags; computed expressions and nested paths are not supported.");

    public static CallableReferenceParameter RequireCallableReference(
        IParameter parameter, string function, string parameterName)
        => parameter as CallableReferenceParameter
            ?? throw new BindingException(
                $"Function '{function}' parameter '{parameterName}' must be a tuple-bound callable reference.");

    public static bool TryGetOpenExpression(
        IParameter parameter,
        [NotNullWhen(true)] out OpenExpressionParameter? expression)
    {
        expression = parameter switch
        {
            OpenExpressionParameter open => open,
            ScopedTupleProjectionParameter projection => new OpenExpressionParameter(new OpenExpression([
                new Function("tuple-at", [projection], FunctionSyntax.ScopedTupleProjectionShorthand),
            ])),
            LiteralParameter { Value: string value } => new OpenExpressionParameter(
                new OpenExpression([new Function(value, [])])),
            _ => null,
        };
        return expression is not null;
    }
}
