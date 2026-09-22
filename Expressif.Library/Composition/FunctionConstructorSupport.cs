using System.Diagnostics.CodeAnalysis;
using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal static class FunctionConstructorSupport
{
    public static bool TryGetOpenExpression(
        IParameter parameter,
        [NotNullWhen(true)] out OpenExpressionParameter? expression)
    {
        expression = parameter switch
        {
            OpenExpressionParameter open => open,
            ScopedTupleProjectionParameter projection => new OpenExpressionParameter(new OpenExpression([
                new Bindings.Function(
                    "tuple-at",
                    [projection],
                    FunctionSyntax.ScopedTupleProjectionShorthand),
            ])),
            LiteralParameter { Value: string value } => new OpenExpressionParameter(
                new OpenExpression([new Bindings.Function(value, [])])),
            _ => null,
        };
        return expression is not null;
    }

    public static bool TryGetFieldName(IParameter[] parameters, out string fieldName)
    {
        fieldName = parameters switch
        {
            [LiteralParameter { Value: string value }] => value,
            [QuotedLiteralParameter quoted] => quoted.Value,
            _ => string.Empty,
        };
        return parameters is [LiteralParameter] or [QuotedLiteralParameter];
    }

    public static object? EvaluateNested(IFunction expression, object? input)
        => EvaluationRuntime.EvaluateNested(expression, input, input);

    public static IEnumerable<Func<object?, object?>> BuildExpressionEvaluators(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.Length == 0
            || function.Arguments.Any(argument => argument.Name is not null || argument.IsSpread))
        {
            throw new MissingOrUnexpectedParametersFunctionException(
                function.Name,
                function.Parameters.Length);
        }

        return function.Arguments
            .Select(argument => constructionContext.CreateValueEvaluator(argument.Value, context))
            .ToArray();
    }
}
