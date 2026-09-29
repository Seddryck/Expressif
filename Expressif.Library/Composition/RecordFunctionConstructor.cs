using Expressif.Bindings;
using Expressif.Library.Numeric;
using Expressif.Library.Record;
using Expressif.Values;

namespace Expressif.Library.Composition;

internal sealed class RecordFunctionConstructor : IFunctionConstructor<Expressif.Library.Record.Record>
{
    public IFunction Construct(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        if (function.Arguments.FirstOrDefault(argument => argument.Name is not null) is { } named)
            throw new UnknownParameterNameException(function.Name, named.Name!);
        if (function.Parameters.Length == 0)
            return new Expressif.Library.Record.Record();

        if (function.Parameters.Length != 1
            || function.Parameters[0] is not RecordDefinitionParameter definition)
        {
            throw new MissingOrUnexpectedParametersFunctionException(
                function.Name,
                function.Parameters.Length);
        }

        var explicitNames = new HashSet<string>(StringComparer.Ordinal);
        var duplicateEntry = definition.Entries
            .OfType<RecordNamedEntry>()
            .FirstOrDefault(entry => !explicitNames.Add(entry.Name));
        if (duplicateEntry is not null)
            throw new BindingException($"Duplicate explicit field '{duplicateEntry.Name}' in record(...).");

        var evaluators = definition.Entries.Select(entry => entry switch
        {
            RecordSpreadEntry spread => RecordEntryEvaluator.Spread(
                BuildValueEvaluator(spread.Value, context, constructionContext)),
            RecordNamedEntry named => RecordEntryEvaluator.Named(
                named.Name,
                BuildValueEvaluator(named.Value, context, constructionContext)),
            _ => throw new BindingException(
                $"Unsupported entry type '{entry.GetType().Name}' in record(...)."),
        }).ToArray();
        return new Expressif.Library.Record.Record(() => evaluators);
    }

    private static Func<object?, object?> BuildValueEvaluator(
        IParameter parameter,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        if (parameter is IncomingValueParameter)
            return input => input;
        if (parameter is QuotedLiteralParameter quoted)
            return _ => quoted.Value;
        if (parameter is LiteralParameter literal)
            return _ => literal.Value;
        if (parameter is not OpenExpressionParameter open)
        {
            var provider = constructionContext.CreateParameter(parameter, typeof(object), context);
            return _ => provider.DynamicInvoke();
        }

        if (open.Expression is InputBoundExpression)
            return constructionContext.CreateOpenExpression(open.Expression, context).Evaluate;
        if (TryBuildSingleTokenEvaluator(open, out var evaluator))
            return evaluator;
        try
        {
            var expression = constructionContext.CreateOpenExpression(open.Expression, context);
            return input => EvaluationRuntime.EvaluateNested(expression, input);
        }
        catch (NotImplementedFunctionException) when (IsSingleTokenExpression(open))
        {
            var literalToken = open.Expression.Members.First().Name;
            return RecordSyntax.TryParseTypedToken(literalToken, out var typed)
                ? _ => typed
                : _ => literalToken;
        }
    }

    private static bool TryBuildSingleTokenEvaluator(
        OpenExpressionParameter open,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
        out Func<object?, object?>? evaluator)
    {
        evaluator = null;
        if (!IsSingleTokenExpression(open))
            return false;

        var literalToken = open.Expression.Members.First().Name;
        if (!RecordSyntax.TryParseTypedToken(literalToken, out var typed))
            return false;
        evaluator = _ => typed;
        return true;
    }

    private static bool IsSingleTokenExpression(OpenExpressionParameter open)
        => open.Expression.Members.Count() == 1
            && open.Expression.Members.First().Parameters.Length == 0;
}
