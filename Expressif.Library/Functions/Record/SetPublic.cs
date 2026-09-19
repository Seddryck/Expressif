using Expressif.Values;

namespace Expressif.Functions.Record;

/// <summary>Returns a new record by renaming selected private fields by removing one leading underscore, preserving field order and values. Omitting names converts all applicable fields. Existing destination names cause an evaluation error. Unlike public, this function does not remove fields.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class SetPublic : IFunction<RecordValue, RecordValue>
{
    private Func<object?[]>? Names { get; }

    /// <summary>Converts all applicable fields.</summary>
    public SetPublic() { }

    /// <param name="names">Field names without the private underscore prefix. Missing, inapplicable, and duplicate names are ignored. An empty array changes no fields; omission selects all applicable fields.</param>
    public SetPublic(Func<object?[]> names) => Names = names;

    public RecordValue Evaluate(RecordValue value) => EvaluateCore(value);

    object? IFunction.Evaluate(object? value) => EvaluateCore(value);

    private RecordValue EvaluateCore(object? value)
        => RecordVisibility.Rename(value, Names is null ? null : RecordOperations.ResolveNames(Names.Invoke()), true);
}
