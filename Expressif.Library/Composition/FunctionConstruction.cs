using Expressif.Bindings;

namespace Expressif.Library.Composition;

// Specialized calls remain classified for semantic analysis and tuple-binding
// eligibility even though runtime construction is provided by constructor registrations.
internal enum FunctionConstructionKind
{
    Standard,
    Catch,
    Throw,
    Let,
    Coalesce,
    Adjacent,
    ChunkWhile,
    Generate,
    Only,
    Closest,
    Concat,
    MapOver,
    MapWith,
    Reduce,
    Extend,
    Pair,
    Put,
    PutPath,
    RenameFields,
    Expand,
    Key,
    GroupBy,
    Join,
    DrillDown,
    DrillUp,
    TopGroups,
    Pick,
    Label,
    Apply,
    TransformWith,
    TransformAs,
    SortTerm,
    SortBy,
}

internal static class FunctionConstruction
{
    public static bool UsesInputValueEvaluator(IParameter parameter)
        => parameter is IncomingValueParameter or ArrayParameter or TupleParameter
            or PairParameter or GroupingParameter or DictionaryParameter
            or RecordLiteralParameter or InputExpressionParameter;

    public static bool IsPredicatePipeline<T>(IReadOnlyList<T> members, Func<T, bool> isPredicate)
        => members.Count == 1 && isPredicate(members[0]);

    public static FunctionConstructionKind Classify(string name)
        => ImplementationRegistry.NormalizeName(name) switch
        {
            "catch" => FunctionConstructionKind.Catch,
            "throw" => FunctionConstructionKind.Throw,
            "let" => FunctionConstructionKind.Let,
            "coalesce" => FunctionConstructionKind.Coalesce,
            "adjacent" => FunctionConstructionKind.Adjacent,
            "chunk-while" => FunctionConstructionKind.ChunkWhile,
            "generate" => FunctionConstructionKind.Generate,
            "only" => FunctionConstructionKind.Only,
            "closest" => FunctionConstructionKind.Closest,
            "concat" or "implode" => FunctionConstructionKind.Concat,
            "map-over" => FunctionConstructionKind.MapOver,
            "map-with" => FunctionConstructionKind.MapWith,
            "reduce" => FunctionConstructionKind.Reduce,
            "extend" => FunctionConstructionKind.Extend,
            "pair" => FunctionConstructionKind.Pair,
            "put" or "put-present" or "put-absent" => FunctionConstructionKind.Put,
            "put-path" or "put-present-path" or "put-absent-path" => FunctionConstructionKind.PutPath,
            "rename-fields" => FunctionConstructionKind.RenameFields,
            "expand" => FunctionConstructionKind.Expand,
            "key" => FunctionConstructionKind.Key,
            "group-by" => FunctionConstructionKind.GroupBy,
            "exists" or "join" or "join-left" or "join-right" or "join-full" => FunctionConstructionKind.Join,
            "drill-down" => FunctionConstructionKind.DrillDown,
            "drill-up" => FunctionConstructionKind.DrillUp,
            "top-groups" => FunctionConstructionKind.TopGroups,
            "pick" => FunctionConstructionKind.Pick,
            "label" or "label-conflicts" => FunctionConstructionKind.Label,
            "apply" => FunctionConstructionKind.Apply,
            "transform-with" => FunctionConstructionKind.TransformWith,
            "transform-as" => FunctionConstructionKind.TransformAs,
            "sort-term" => FunctionConstructionKind.SortTerm,
            "sort-by" or "rank-by" or "dense-rank-by" => FunctionConstructionKind.SortBy,
            _ => FunctionConstructionKind.Standard,
        };
}
