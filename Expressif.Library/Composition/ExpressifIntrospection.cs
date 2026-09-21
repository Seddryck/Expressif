using Expressif.Functions.Coercions;
using Expressif.Discovery;
using Expressif.Introspection;
using Expressif.Values.Types;

namespace Expressif.Library.Composition;

/// <summary>
/// Composes Core introspection with the official built-in Expressif vocabulary.
/// </summary>
public static class ExpressifIntrospection
{
    private static readonly ITypeSource Source = new AssemblyTypeSource([typeof(ExpressifIntrospection).Assembly]);
    private static readonly ICoercionRegistry BuiltInCoercions = new CoercionRegistry(Source);
    private static readonly IntrospectionOptions Options = new(
        ExpressifTypeRegistry.Instance,
        BuiltInCoercions,
        BuildParameterTypes(),
        BuildParameterNames(),
        BuildVariadicParameters(),
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["coalesce"] = "Output depends on the first non-null expression result at runtime.",
            ["field"] = "Output depends on the selected field in the runtime record shape.",
            ["guard"] = "Output preserves the input type when entry is incompatible and otherwise depends on the guarded expression.",
            ["neutral"] = "Identity semantics require an open TIn -> TIn contract rather than one closed contract.",
            ["walk"] = "Output preserves each container kind while leaf output types depend on the supplied expression.",
        },
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["datetime-to-date"] = "date",
        },
        TupleBindingCapabilities.Describe);

    public static FunctionIntrospector Functions { get; } = new(Source, Options);
    public static PredicateIntrospector Predicates { get; } = new(Source, Options);
    public static CoercionIntrospector Coercions { get; } = new(BuiltInCoercions);

    private static IReadOnlyDictionary<ParameterIntrospectionKey, string> BuildParameterTypes()
        => new Dictionary<ParameterIntrospectionKey, string>
        {
            [Key<Array.Aggregation.Broadcast>("accumulator")] = "accumulator",
            [Key<Array.Aggregation.Fold>("accumulator")] = "accumulator",
            [Key<Array.Aggregation.Scan>("accumulator")] = "accumulator",
            [Key<Temporal.DurationBetween>("previous")] = "date | date-time | year-month",
            [Key<Array.Array>("values")] = "any",
            [Key<Tuple.Tuple>("values")] = "any",
            [Key<Pair.Pair>("key")] = "any",
            [Key<Pair.Pair>("value")] = "any",
            [Key<Grouping.Grouping>("values")] = "pair",
            [Key<Grouping.GroupingSets>("values")] = "tuple",
            [Key<Dictionary.Dictionary>("values")] = "pair",
            [Key<Array.Grouping.Key>("expressions")] = "expression",
            [Key<Array.Grouping.GroupBy>("expressions")] = "expression",
            [Key<Record.Expand>("selector")] = "expression",
            [Key<Record.Expand>("label")] = "text",
            [Key<Record.Explode>("selector")] = "expression",
            [Key<Record.ExplodeOuter>("selector")] = "expression",
            [Key<Record.ImplodeInner>("selector")] = "expression",
            [Key<Array.Grouping.Pivot>("row")] = "expression",
            [Key<Array.Grouping.Pivot>("column")] = "expression",
            [Key<Array.Grouping.Pivot>("summary")] = "expression",
            [Key<Array.Combination.Exists>("right")] = "array | grouping",
            [Key<Array.Combination.Exists>("leftKey")] = "expression",
            [Key<Array.Combination.Exists>("rightKey")] = "expression",
            [Key<Array.Combination.Join>("right")] = "array | grouping | dictionary",
            [Key<Array.Combination.Join>("leftKey")] = "expression",
            [Key<Array.Combination.Join>("rightKey")] = "expression",
            [Key<Array.Combination.JoinLeft>("right")] = "array | grouping | dictionary",
            [Key<Array.Combination.JoinLeft>("leftKey")] = "expression",
            [Key<Array.Combination.JoinLeft>("rightKey")] = "expression",
            [Key<Array.Combination.JoinRight>("right")] = "array | grouping | dictionary",
            [Key<Array.Combination.JoinRight>("leftKey")] = "expression",
            [Key<Array.Combination.JoinRight>("rightKey")] = "expression",
            [Key<Array.Combination.JoinFull>("right")] = "array | grouping | dictionary",
            [Key<Array.Combination.JoinFull>("leftKey")] = "expression",
            [Key<Array.Combination.JoinFull>("rightKey")] = "expression",
            [Key<Grouping.DrillDown>("expressions")] = "expression",
            [Key<Grouping.DrillUp>("expression")] = "expression",
            [Key<Text.Concatenation.Text>("values")] = "expression",
            [Key<Text.Partitioning.SplitLengths>("lengths")] = "integer",
            [Key<Tuple.Pick>("positions")] = "integer",
            [Key<Tuple.Label>("names")] = "text",
            [Key<Tuple.LabelConflicts>("names")] = "text",
            [Key<Record.Record>("entries")] = "entry",
            [Key<Record.NestedField>("path")] = "text",
            [Key<Record.Put>("assignments")] = "entry",
            [Key<Record.PutPresent>("assignments")] = "entry",
            [Key<Record.PutAbsent>("assignments")] = "entry",
            [Key<Record.PutPath>("path")] = "expression",
            [Key<Record.PutPath>("value")] = "expression",
            [Key<Record.PutPresentPath>("path")] = "expression",
            [Key<Record.PutPresentPath>("value")] = "expression",
            [Key<Record.PutAbsentPath>("path")] = "expression",
            [Key<Record.PutAbsentPath>("value")] = "expression",
            [Key<Record.With>("projections")] = "entry",
            [Key<Flow.Let>("bindings")] = "entry",
            [Key<Record.With>("body")] = "expression",
            [Key<Flow.Switch>("branches")] = "entry",
            [Key<Flow.Try>("branches")] = "entry",
            [Key<Special.Coalesce>("expressions")] = "expression",
            [Key<Special.Coerce>("specifications")] = "type | mapping",
            [Key<Flow.TransformWith>("operation")] = "expression",
            [Key<Flow.TransformWith>("expressions")] = "expression",
            [Key<Flow.TransformAs>("operation")] = "expression",
            [Key<Flow.TransformAs>("expressions")] = "entry",
            [Key<Boolean.Majority>("predicates")] = "predicate",
            [Key<Boolean.SatisfiesExactly>("predicates")] = "predicate",
            [Key<Boolean.SatisfiesAtLeast>("predicates")] = "predicate",
            [Key<Boolean.SatisfiesAtMost>("predicates")] = "predicate",
            [Key<Array.MapOver>("expression")] = "expression",
            [Key<Array.MapOver>("values")] = "array",
            [Key<Array.MapWith>("expression")] = "expression",
            [Key<Array.MapWith>("values")] = "array",
            [Key<Sorting.CompareNumeric>("right")] = "numeric",
            [Key<Sorting.CompareOrdinal>("right")] = "text",
            [Key<Sorting.CompareDate>("right")] = "date",
            [Key<Sorting.CompareTime>("right")] = "time",
            [Key<Sorting.CompareDateTime>("right")] = "date-time",
            [Key<Sorting.SortTerm>("comparer")] = "expression",
            [Key<Sorting.SortKey>("values")] = "sort-term",
            [Key<Sorting.SortBy>("criteria")] = "expression",
            [Key<Sorting.RankBy>("criteria")] = "expression",
            [Key<Sorting.DenseRankBy>("criteria")] = "expression",
        };

    private static IReadOnlyDictionary<ParameterIntrospectionKey, string> BuildParameterNames()
    {
        var types = new[]
        {
            typeof(Array.Combination.Exists),
            typeof(Array.Combination.Join),
            typeof(Array.Combination.JoinLeft),
            typeof(Array.Combination.JoinRight),
            typeof(Array.Combination.JoinFull),
            typeof(Array.Grouping.Unpivot),
        };
        return types
            .SelectMany(type => type.GetConstructors().SelectMany(constructor => constructor.GetParameters()
                .Select(parameter => new KeyValuePair<ParameterIntrospectionKey, string>(
                    new(type, parameter.Name!),
                    parameter.Name!.ToKebabCase()))))
            .DistinctBy(pair => pair.Key)
            .ToDictionary();
    }

    private static IReadOnlyDictionary<ParameterIntrospectionKey, int> BuildVariadicParameters()
        => new Dictionary<ParameterIntrospectionKey, int>
        {
            [Key<Record.NestedField>("path")] = 1,
            [Key<Record.Record>("entries")] = 0,
            [Key<Record.Put>("assignments")] = 1,
            [Key<Record.PutPresent>("assignments")] = 1,
            [Key<Record.PutAbsent>("assignments")] = 1,
            [Key<Record.With>("projections")] = 1,
            [Key<Flow.Let>("bindings")] = 1,
            [Key<Special.Coalesce>("expressions")] = 2,
            [Key<Flow.Switch>("branches")] = 1,
            [Key<Flow.Try>("branches")] = 2,
            [Key<Special.Coerce>("specifications")] = 1,
            [Key<Flow.TransformWith>("expressions")] = 1,
            [Key<Flow.TransformAs>("expressions")] = 1,
            [Key<Tuple.Pick>("positions")] = 1,
            [Key<Text.Partitioning.SplitLengths>("lengths")] = 0,
            [Key<Array.Grouping.Key>("expressions")] = 1,
            [Key<Array.Grouping.GroupBy>("expressions")] = 1,
            [Key<Sorting.SortBy>("criteria")] = 1,
            [Key<Sorting.RankBy>("criteria")] = 1,
            [Key<Sorting.DenseRankBy>("criteria")] = 1,
            [Key<Grouping.DrillDown>("expressions")] = 1,
            [Key<Tuple.Label>("names")] = 0,
            [Key<Tuple.LabelConflicts>("names")] = 0,
            [Key<Boolean.Majority>("predicates")] = 0,
            [Key<Boolean.SatisfiesExactly>("predicates")] = 0,
            [Key<Boolean.SatisfiesAtLeast>("predicates")] = 0,
            [Key<Boolean.SatisfiesAtMost>("predicates")] = 0,
            [Key<Sorting.SortKey>("values")] = 1,
        };

    private static ParameterIntrospectionKey Key<T>(string parameterName)
        => new(typeof(T), parameterName);
}
