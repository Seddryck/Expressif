using Expressif.Bindings;

namespace Expressif.Functions;

// Factory-integrated calls and ordinary constructor-bound calls must be
// classified identically for runtime construction and semantic analysis.
internal enum FunctionConstructionKind
{
    Standard,
    TupleBind,
    Conditional,
    ControlFlow,
    Catch,
    Throw,
    Record,
    With,
    Let,
    Coalesce,
    Coerce,
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
        => Accumulators.AccumulatorNames.Resolve(name.ToKebabCase()) switch
        {
            "bind" => FunctionConstructionKind.TupleBind,
            "conditional-forward" or "conditional-backward" => FunctionConstructionKind.Conditional,
            "switch" or "try" => FunctionConstructionKind.ControlFlow,
            "catch" => FunctionConstructionKind.Catch,
            "throw" => FunctionConstructionKind.Throw,
            "record" => FunctionConstructionKind.Record,
            "with" => FunctionConstructionKind.With,
            "let" => FunctionConstructionKind.Let,
            "coalesce" => FunctionConstructionKind.Coalesce,
            "coerce" => FunctionConstructionKind.Coerce,
            "adjacent" => FunctionConstructionKind.Adjacent,
            "chunk-while" => FunctionConstructionKind.ChunkWhile,
            "generate" => FunctionConstructionKind.Generate,
            "only" => FunctionConstructionKind.Only,
            "closest" => FunctionConstructionKind.Closest,
            "concat" => FunctionConstructionKind.Concat,
            "map-over" => FunctionConstructionKind.MapOver,
            "map-with" => FunctionConstructionKind.MapWith,
            "reduce" => FunctionConstructionKind.Reduce,
            "extend" => FunctionConstructionKind.Extend,
            "pair" => FunctionConstructionKind.Pair,
            "put" or "put-present" or "put-absent" => FunctionConstructionKind.Put,
            "put-path" or "put-present-path" or "put-absent-path" => FunctionConstructionKind.PutPath,
            "expand" => FunctionConstructionKind.Expand,
            "rename-fields" => FunctionConstructionKind.RenameFields,
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
