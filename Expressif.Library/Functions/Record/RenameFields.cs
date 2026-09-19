using Expressif.Predicates;
using ValueRecord = Expressif.Values.RecordValue;

namespace Expressif.Functions.Record;

/// <summary>Transforms selected field names while preserving field values and order. Duplicate resulting names cause an evaluation error.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class RenameFields : IFunction<ValueRecord, ValueRecord>
{
    private Func<IFunction> Transform { get; }
    private Func<IPredicate>? Filter { get; }

    /// <param name="transform">An expression transforming a field name from text to text.</param>
    public RenameFields(Func<IFunction> transform)
        : this(transform, null) { }

    /// <param name="transform">An expression transforming a field name from text to text.</param>
    /// <param name="filter">An optional predicate selecting field names to transform. Omission selects every field.</param>
    public RenameFields(Func<IFunction> transform, Func<IPredicate>? filter)
        => (Transform, Filter) = (transform, filter);

    public ValueRecord Evaluate(ValueRecord value) => EvaluateCore(value);

    object? IFunction.Evaluate(object? value) => EvaluateCore(value);

    private ValueRecord EvaluateCore(object? value)
    {
        var fields = RecordOperations.Enumerate(value);
        var transform = Transform.Invoke();
        var filter = Filter?.Invoke();
        var result = new ValueRecord();
        foreach (var field in fields)
        {
            var name = field.Key;
            if (filter is null || EvaluationRuntime.EvaluateNested(filter, name) is true)
            {
                name = EvaluationRuntime.EvaluateNested(transform, name) as string
                    ?? throw new InvalidOperationException("The rename-fields transformation must return text.");
            }
            if (result.ContainsKey(name))
                throw new InvalidOperationException($"The rename-fields result contains a duplicate field name '{name}'.");
            result.Set(name, field.Value);
        }
        return result;
    }
}
