using Expressif.Values;

namespace Expressif.Functions.Record;

/// <summary>Returns a new record without fields whose names start with an underscore, preserving public field order and values. Unlike set-public and set-private, this function removes fields rather than renaming them.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class Public : IFunction<RecordValue, RecordValue>
{
    public RecordValue Evaluate(RecordValue value) => EvaluateCore(value);

    object? IFunction.Evaluate(object? value) => EvaluateCore(value);

    private static RecordValue EvaluateCore(object? value)
        => RecordOperations.Filter(value, field => !field.Key.StartsWith('_'));
}
