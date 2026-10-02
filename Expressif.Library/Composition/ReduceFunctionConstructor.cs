using Expressif.Functions.Accumulation;
using Expressif.Bindings;

namespace Expressif.Library.Composition;

internal sealed class ReduceFunctionConstructor :
    IFunctionConstructor<ReduceAccumulator>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        var bound = ParameterArgumentBinder.Bind(typeof(ReduceAccumulator), function.Arguments).Parameters;
        if (!FunctionConstructorSupport.TryGetOpenExpression(bound[0], out var operation))
        {
            throw new ArgumentException(
                $"The accumulator named '{function.Name}' expects parameter 'operation' to be an open expression.",
                nameof(function));
        }

        var operationProvider = BuildOperationProvider(operation, context, constructionContext);
        if (bound.Length == 1)
            return new ReduceAccumulator(operationProvider);

        var initial = constructionContext.CreateValueEvaluator(bound[1], context);
        return new ReduceAccumulator(
            operationProvider,
            () => initial.Invoke(EvaluationRuntime.Frame?.Current));
    }

    private static Func<IFunction> BuildOperationProvider(
        OpenExpressionParameter operation,
        IContext context,
        IFunctionConstructionContext constructionContext)
    {
        if (operation.Expression.InputBinding is not null)
            return () => constructionContext.CreateOpenExpression(operation.Expression, context);

        var members = operation.Expression.Members.ToArray();
        if (members is [var first, ..]
            && first.Arguments is [
                { Name: null, Value: TupleProjectionParameter { Index: 0, FromEnd: false } },
                .. var remaining])
        {
            members[0] = Bindings.Function.FromArguments(first.Name, remaining);
            members = [
                new Bindings.Function("tuple-at", [new LiteralParameter("0")]),
                .. members,
            ];
        }

        var normalized = new OpenExpression(members);
        return () => constructionContext.CreateOpenExpression(normalized, context);
    }
}
