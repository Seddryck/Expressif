using Expressif.Bindings;
using Expressif.Library.Record;
using Expressif.Library.Sorting;
using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Library.Composition;

internal sealed class SortByFunctionConstructor :
    IFunctionConstructor<Expressif.Library.Sorting.SortBy>,
    IFunctionConstructor<Expressif.Library.Sorting.RankBy>,
    IFunctionConstructor<Expressif.Library.Sorting.DenseRankBy>
{
    private readonly Caster caster = new();

    public IFunction Construct(Bindings.Function function, IContext context, IFunctionConstructionContext constructionContext)
    {
        if (function.Parameters.Length == 0
            || function.Parameters.Any(parameter => parameter is not SortCriterionParameter))
        {
            throw new BindingException("Function 'sort-by' requires one or more typed criteria.");
        }
        var criteria = function.Parameters.Cast<SortCriterionParameter>().Select(criterion =>
        {
            var selector = constructionContext.CreateValueEvaluator(criterion.Selector, context);
            object? EvaluateCriterion(object? input)
            {
                using var scope = EvaluationRuntime.Derive(input);
                var selected = selector.Invoke(input);
                var targetType = criterion.Type.RuntimeType
                    ?? throw new BindingException($"Type ':{criterion.Type.Name}' has no coercion target.");
                return caster.TryCast(selected, targetType, out var converted) ? converted : null;
            }
            var comparerName = criterion.Type.Name.ToLowerInvariant() switch
            {
                "text" => "compare-ordinal",
                "integer" or "decimal" or "numeric" => "compare-numeric",
                "date" => "compare-date",
                "time" => "compare-time",
                "datetime" or "date-time" => "compare-datetime",
                var name => throw new BindingException($"Type ':{name}' is not supported by sort-by."),
            };
            var target = constructionContext.ResolveTupleTarget(comparerName, function.SourceSpan);
            var comparer = new SortComparer(
                comparerName,
                target,
                (left, right) => constructionContext.InvokeTuple(
                    comparerName,
                    new Values.Tuple(left, right)) as OrderingValue);
            return new SortByCriterion(
                EvaluateCriterion,
                comparer,
                criterion.Ascending,
                criterion.NullsFirst);
        }).ToArray();
        return function.Name.ToKebabCase() switch
        {
            "rank-by" => new Expressif.Library.Sorting.RankBy(criteria),
            "dense-rank-by" => new Expressif.Library.Sorting.DenseRankBy(criteria),
            _ => new Expressif.Library.Sorting.SortBy(criteria),
        };
    }
}
