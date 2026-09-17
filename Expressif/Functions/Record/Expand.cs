using Expressif.Bindings;
using Expressif.Values;
using ValueRecord = Expressif.Values.RecordValue;

namespace Expressif.Functions.Record;

/// <summary>Flattens a selected nested record into its parent, qualifying conflicts or every expanded field when a label is supplied.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class Expand : IFunction<ValueRecord, ValueRecord?>
{
    private RecordExpansionSelector Selector { get; }
    private Func<object?, string>? Label { get; }
    private string? ConsumedField { get; }

    /// <param name="selector">An expression selecting the nested record to expand.</param>
    public Expand(RecordExpansionSelector selector)
        : this(selector, null) { }

    /// <param name="selector">An expression selecting the nested record to expand.</param>
    /// <param name="label">An optional qualifier for every expanded field. Omission derives the qualifier from a direct field selector and qualifies only conflicts.</param>
    public Expand(RecordExpansionSelector selector, Func<object?, string>? label)
        => (Selector, Label, ConsumedField) = (selector, label, selector.Field);

    public ValueRecord? Evaluate(ValueRecord value) => EvaluateCore(value);

    object? IFunction.Evaluate(object? value) => EvaluateCore(value);

    private ValueRecord? EvaluateCore(object? value)
    {
        if (!RecordEntryEvaluator.IsRecord(value))
            throw new ArgumentException("Input value must be a record.", nameof(value));
        var parent = RecordOperations.Enumerate(value);
        var selected = Selector.Evaluate.Invoke(value);
        var qualifier = Label?.Invoke(value) ?? ConsumedField
            ?? throw new InvalidOperationException("An expand selector without a field name requires an explicit label.");
        IReadOnlyList<KeyValuePair<string, object?>> nested = [];
        if (selected is not null)
        {
            if (!RecordEntryEvaluator.IsRecord(selected) || !NamedValueAccessor.TryEnumerate(selected, out var fields))
                return null;
            nested = fields;
        }

        var occupied = parent.Where(field => field.Key != ConsumedField)
            .Select(field => field.Key).ToHashSet(StringComparer.Ordinal);
        var names = nested.Select(field => Label is null && !occupied.Contains(field.Key)
            ? field.Key : $"{qualifier}.{field.Key}").ToArray();
        // Reserve unique unqualified names before resolving qualified collisions.
        if (Label is null)
        {
            for (var index = 0; index < nested.Count; index++)
            {
                if (names[index] == nested[index].Key)
                    occupied.Add(names[index]);
            }
        }
        for (var index = 0; index < nested.Count; index++)
        {
            if (Label is null && names[index] == nested[index].Key)
                continue;
            while (!occupied.Add(names[index]))
                names[index] = $"{qualifier}.{names[index]}";
        }

        var result = new ValueRecord();
        var inserted = false;
        foreach (var field in parent)
        {
            if (field.Key == ConsumedField)
            {
                Insert();
                inserted = true;
            }
            else
            {
                result.Set(field.Key, field.Value);
            }
        }
        if (!inserted)
            Insert();
        return result;

        void Insert()
        {
            for (var index = 0; index < nested.Count; index++)
                result.Set(names[index], nested[index].Value);
        }
    }
}
