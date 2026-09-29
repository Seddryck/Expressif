using Expressif.Bindings;
using Expressif.Values;

namespace Expressif.Library.Composition;

internal sealed class CoalesceFunctionConstructor : IFunctionConstructor<Expressif.Library.Special.Coalesce>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.FirstOrDefault(argument => argument.Name is not null) is { } named)
            throw new UnknownParameterNameException(function.Name, named.Name!);
        if (function.Parameters.Length < 2)
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var expressions = function.Parameters
            .Select(parameter => BuildEvaluator(parameter, context, constructionContext));
        return new Expressif.Library.Special.Coalesce(expressions);
    }

    private static Func<object?, object?> BuildEvaluator(
        IParameter parameter,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        if (parameter is IncomingValueParameter)
            return input => input;
        if (parameter is OpenExpressionParameter open)
        {
            return TryBuildFieldEvaluator(open, context, constructionContext, out var evaluator)
                ? evaluator
                : constructionContext.CreateOpenExpressionValueEvaluator(open, context);
        }

        var provider = constructionContext.CreateParameter(parameter, typeof(object), context);
        return _ => provider.DynamicInvoke();
    }

    private static bool TryBuildFieldEvaluator(
        OpenExpressionParameter open,
        IContext context,
        IFunctionConstructionContext constructionContext,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Func<object?, object?>? evaluator)
    {
        evaluator = null;
        var members = open.Expression.Members.ToArray();
        if (members.Length == 0
            || !members[0].Name.Equals("field", StringComparison.OrdinalIgnoreCase)
            || !FunctionConstructorSupport.TryGetFieldName(members[0].Parameters, out var fieldName))
        {
            return false;
        }

        var remainder = new ChainFunction(
            members.Skip(1)
                .Select(member => constructionContext.CreateFunction(member, context))
                .ToArray());
        evaluator = input => NamedValueAccessor.TryGetValue(input, fieldName, out var fieldValue)
            ? remainder.Evaluate(fieldValue)
            : null;
        return true;
    }
}
