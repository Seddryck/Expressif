using System.Diagnostics.CodeAnalysis;
using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal static class FunctionConstructorSupport
{
    public static bool TryGetOpenExpression(
        IParameter parameter,
        [NotNullWhen(true)] out OpenExpressionParameter? expression)
        => ExpressionShapeNormalizer.TryGetOpenExpression(parameter, out expression);

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
        if (!constructionContext.TryResolveImplementation(function.Name, out var type))
            throw new NotImplementedFunctionException(function.Name);
        var layout = ParameterArgumentBinder.BindLayout(type, function.Arguments);
        return layout.Positional
            .Select(argument => constructionContext.CreateValueEvaluator(argument.Value, context))
            .ToArray();
    }
}
