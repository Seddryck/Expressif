using Expressif.Bindings;
using Expressif.Library.Array;
using Expressif.Semantics;

namespace Expressif.Library.Composition;

internal static class DirectionalMapConstruction
{
    public static IFunction Build(
        Bindings.Function function,
        IContext context,
        IFunctionConstructionContext constructionContext,
        bool mapOver)
    {
        var type = mapOver ? typeof(Expressif.Library.Array.MapOver) : typeof(Expressif.Library.Array.MapWith);
        var binding = ParameterArgumentBinder.Bind(type, function.Arguments);
        if (binding.Parameters is not [OpenExpressionParameter expression, var values])
            throw new MissingOrUnexpectedParametersFunctionException(function.Name, function.Parameters.Length);

        var valuesEvaluator = constructionContext.CreateValueEvaluator(values, context);
        Func<System.Collections.IEnumerable?> valuesProvider = () =>
            valuesEvaluator.Invoke(EvaluationRuntime.Frame?.Current) is { } evaluated
                && AggregationEnumerable.TryGetEnumerable(evaluated, out var enumerable)
                    ? enumerable
                    : null;
        Func<IFunction> operationProvider = () => BuildOperation(
            expression, context, constructionContext, mapOver);

        return mapOver
            ? new Expressif.Library.Array.MapOver(operationProvider, valuesProvider)
            : new Expressif.Library.Array.MapWith(operationProvider, valuesProvider);
    }

    private static IFunction BuildOperation(
        OpenExpressionParameter expression,
        IContext context,
        IFunctionConstructionContext constructionContext,
        bool mapOver)
    {
        var members = expression.Expression.Members.ToArray();
        if (TupleBindingOperations.LeadingLength(expression.Expression) > 0)
            return BuildExplicitOperation(expression.Expression, context, constructionContext, mapOver);
        var isBareCallable = LegacyTupleBindingRules.IsCandidate(
            mapOver ? "map-over" : "map-with", expression.Expression);
        if (isBareCallable)
        {
            var name = members[0].Name;
            return new DelegatedFunction(value =>
            {
                var invocation = GetInput(value);
                var inputs = DirectionalScope<object?>.Create(mapOver, invocation.Outer, invocation.Item);
                var arguments = mapOver
                    ? GetMapOverArguments(invocation.Item)
                    : [new LiteralParameter(invocation.Outer)];
                var callable = constructionContext.CreateFunction(
                    new Bindings.Function(name, arguments), context);
                using var scope = EvaluationRuntime.Derive(inputs.Arguments);
                return callable.Evaluate(inputs.Input);
            });
        }

        var normalized = mapOver && expression.Expression.InputBinding is null
            ? NormalizeMapOverProjections(expression.Expression)
            : expression.Expression;
        var operation = constructionContext.CreateOpenExpression(normalized, context);
        return new DelegatedFunction(value =>
        {
            var invocation = GetInput(value);
            var inputs = DirectionalScope<object?>.Create(mapOver, invocation.Outer, invocation.Item);
            if (operation is InputBoundFunction)
                return operation.Evaluate(inputs.Input);
            using var scope = EvaluationRuntime.Derive(inputs.Arguments);
            return operation.Evaluate(inputs.Input);
        });
    }

    private static IFunction BuildExplicitOperation(
        OpenExpression expression,
        IContext context,
        IFunctionConstructionContext constructionContext,
        bool mapOver)
    {
        var explicitOperation = constructionContext.CreateOpenExpression(
            mapOver ? NormalizeMapOverProjections(expression) : expression,
            context);
        return new DelegatedFunction(value =>
        {
            var invocation = GetInput(value);
            var inputs = DirectionalScope<object?>.Create(mapOver, invocation.Outer, invocation.Item);
            var arguments = mapOver && invocation.Item is Values.Tuple tuple
                ? tuple.ToArray()
                : [invocation.Item];
            var prepared = new Values.Tuple([invocation.Outer, .. arguments]);
            using var scope = EvaluationRuntime.Derive(inputs.Arguments);
            return explicitOperation.Evaluate(prepared);
        });
    }

    private static IParameter[] GetMapOverArguments(object? item)
        => item is Values.Tuple tuple
            ? tuple.Select(value => (IParameter)new LiteralParameter(value)).ToArray()
            : [new LiteralParameter(item)];

    private static DirectionalMapInput GetInput(object? value)
        => value as DirectionalMapInput
            ?? throw new InvalidOperationException(
                "Directional map operations require a directional map input.");

    private static OpenExpression NormalizeMapOverProjections(OpenExpression expression)
        => new(expression.Members.Select(member =>
        {
            var normalized = Bindings.Function.FromArguments(
                member.Name,
                member.Arguments.Select(argument => argument with
                {
                    Value = NormalizeMapOverProjection(argument.Value),
                }).ToArray(),
                member.Syntax);
            normalized.SourceSpan = member.SourceSpan;
            return normalized;
        }));

    private static IParameter NormalizeMapOverProjection(IParameter parameter)
        => parameter switch
        {
            TupleProjectionParameter { Index: > 0, FromEnd: false } projection
                => projection with { Index = projection.Index - 1 },
            ArrayParameter array => new ArrayParameter(array.Elements.Select(element
                => element with { Value = NormalizeMapOverProjection(element.Value) }).ToArray()),
            TupleParameter tuple => new TupleParameter(tuple.Elements.Select(element
                => element with { Value = NormalizeMapOverProjection(element.Value) }).ToArray()),
            _ => parameter,
        };
}
