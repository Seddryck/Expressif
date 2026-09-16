using Expressif.Values;

namespace Expressif.Functions.Tuple;

internal static class TupleLabeling
{
    public static RecordValue Evaluate(
        IPositionalValue value,
        Func<ValueArgumentEvaluator[]> labelsProvider,
        bool qualifyOnlyConflicts)
    {
        var labels = ValueArguments.Evaluate(labelsProvider.Invoke(), value).ToArray();
        if (labels.Any(label => label is not string))
            throw new ArgumentException("Every label must be text.", "labels");
        if (labels.Length != value.Arity)
            throw new ArgumentException(
                $"The number of labels ({labels.Length}) must match the tuple arity ({value.Arity}).",
                "labels");

        var fields = new List<LabeledField>();
        for (var position = 0; position < value.Arity; position++)
        {
            var label = (string)labels[position]!;
            var item = value.GetPosition(position);
            if (item is RecordValue record)
            {
                foreach (var field in record)
                    fields.Add(new(label, field.Key, field.Value, IsRecordField: true));
            }
            else
            {
                fields.Add(new(label, label, item, IsRecordField: false));
            }
        }

        IReadOnlyDictionary<string, int>? occurrences = null;
        if (qualifyOnlyConflicts)
        {
            occurrences = fields
                .GroupBy(field => field.Name, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        }

        var result = new RecordValue();
        foreach (var field in fields)
        {
            var name = field.IsRecordField
                && (!qualifyOnlyConflicts || occurrences![field.Name] > 1)
                    ? $"{field.Label}.{field.Name}"
                    : field.Name;

            if (result.ContainsKey(name))
                throw new InvalidOperationException($"Labeling would duplicate field '{name}'.");
            result.Set(name, field.Value);
        }
        return result;
    }

    private readonly record struct LabeledField(string Label, string Name, object? Value, bool IsRecordField);
}

/// <summary>Turns tuple positions into an ordered record, qualifying fields expanded from record positions with their position label.</summary>
[Function(prefix: "")]
[Scope("tuple")]
public sealed class Label : IFunction<IPositionalValue, RecordValue>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Labels { get; }

    /// <param name="labels">One text label per tuple position. Spread arguments expand arrays of labels in place.</param>
    public Label(Func<ValueArgumentEvaluator[]> labels)
        => Labels = labels;

    public RecordValue Evaluate(IPositionalValue value)
        => TupleLabeling.Evaluate(value, Labels, qualifyOnlyConflicts: false);

    object? IFunction.Evaluate(object? value)
        => value is IPositionalValue positional ? Evaluate(positional) : null;
}

/// <summary>Turns tuple positions into an ordered record, qualifying expanded record fields only when their unqualified names conflict.</summary>
[Function(prefix: "")]
[Scope("tuple")]
public sealed class LabelConflicts : IFunction<IPositionalValue, RecordValue>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Labels { get; }

    /// <param name="labels">One text label per tuple position. Spread arguments expand arrays of labels in place.</param>
    public LabelConflicts(Func<ValueArgumentEvaluator[]> labels)
        => Labels = labels;

    public RecordValue Evaluate(IPositionalValue value)
        => TupleLabeling.Evaluate(value, Labels, qualifyOnlyConflicts: true);

    object? IFunction.Evaluate(object? value)
        => value is IPositionalValue positional ? Evaluate(positional) : null;
}
