using Expressif.Bindings;
using Expressif.Library.Record;
using Expressif.Library.Sorting;
using Expressif.Values;

namespace Expressif.Library.Composition;

internal sealed class SortTermFunctionConstructor : IFunctionConstructor<Expressif.Library.Sorting.SortTerm>
{
    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Parameters.Length is not (2 or 4)
            || function.Parameters[1] is not CallableReferenceParameter reference)
        {
            throw new BindingException(
                "The comparer for 'sort-term' must be a tuple-bound callable reference.");
        }

        var target = constructionContext.ResolveTupleTarget(reference.Name, function.SourceSpan);
        var outputs = target.GetInterfaces()
            .Where(contract => contract.IsGenericType
                && contract.GetGenericTypeDefinition() == typeof(IFunction<,>))
            .Select(contract => Nullable.GetUnderlyingType(contract.GetGenericArguments()[1])
                ?? contract.GetGenericArguments()[1])
            .ToArray();
        if (!outputs.Contains(typeof(OrderingValue)))
            throw new BindingException($"Callable '{reference.Name}' does not return an ordering value.");

        var canonicalName = target.Name.ToKebabCase();
        var comparer = new SortComparer(
            canonicalName,
            target,
            (left, right) => constructionContext.InvokeTuple(
                canonicalName,
                new Values.Tuple(left, right)) as OrderingValue);
        var evaluator = constructionContext.CreateValueEvaluator(function.Parameters[0], context);
        if (function.Parameters is [_, _, var ascendingParameter, var nullsFirstParameter])
        {
            var ascending = (Func<bool>)constructionContext.CreateParameter(
                ascendingParameter, typeof(bool), context);
            var nullsFirst = (Func<bool>)constructionContext.CreateParameter(
                nullsFirstParameter, typeof(bool), context);
            return new DelegatedFunction(input => new Values.SortTerm(
                evaluator.Invoke(input),
                comparer,
                ascending.Invoke(),
                nullsFirst.Invoke()));
        }

        return new Expressif.Library.Sorting.SortTerm(
            () => evaluator.Invoke(EvaluationRuntime.Frame?.Current ?? context.CurrentObject.Value),
            () => comparer);
    }
}
