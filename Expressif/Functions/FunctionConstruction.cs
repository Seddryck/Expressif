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
    Record,
    With,
    Coalesce,
    Coerce,
    Adjacent,
    ChunkWhile,
    Generate,
    Closest,
    Implode,
    MapOver,
    MapWith,
    Reduce,
    Extend,
    Pair,
    Put,
    PutPath,
    Key,
    GroupBy,
    Pick,
    Apply,
    TransformWith,
    TransformAs,
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
        => name.ToKebabCase() switch
        {
            "bind" => FunctionConstructionKind.TupleBind,
            "conditional-forward" or "conditional-backward" => FunctionConstructionKind.Conditional,
            "switch" or "try" => FunctionConstructionKind.ControlFlow,
            "record" => FunctionConstructionKind.Record,
            "with" => FunctionConstructionKind.With,
            "coalesce" => FunctionConstructionKind.Coalesce,
            "coerce" => FunctionConstructionKind.Coerce,
            "adjacent" => FunctionConstructionKind.Adjacent,
            "chunk-while" => FunctionConstructionKind.ChunkWhile,
            "generate" => FunctionConstructionKind.Generate,
            "closest" => FunctionConstructionKind.Closest,
            "implode" => FunctionConstructionKind.Implode,
            "map-over" => FunctionConstructionKind.MapOver,
            "map-with" => FunctionConstructionKind.MapWith,
            "reduce" => FunctionConstructionKind.Reduce,
            "extend" => FunctionConstructionKind.Extend,
            "pair" => FunctionConstructionKind.Pair,
            "put" or "put-present" or "put-absent" => FunctionConstructionKind.Put,
            "put-path" or "put-present-path" or "put-absent-path" => FunctionConstructionKind.PutPath,
            "key" => FunctionConstructionKind.Key,
            "group-by" => FunctionConstructionKind.GroupBy,
            "pick" => FunctionConstructionKind.Pick,
            "apply" => FunctionConstructionKind.Apply,
            "transform-with" => FunctionConstructionKind.TransformWith,
            "transform-as" => FunctionConstructionKind.TransformAs,
            _ => FunctionConstructionKind.Standard,
        };
}
