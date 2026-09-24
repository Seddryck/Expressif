using Expressif.Values;
using Expressif.Functions;

namespace Expressif.Library.Tuple;

/// <summary>Returns a flat record by assigning one positional label to each tuple item and qualifying every field expanded from a record item.</summary>
[Function(prefix: "")]
[Scope("tuple")]
public sealed class Label : IFunction<IPositionalValue, RecordValue>
{
    private Func<string[]> Names { get; }

    /// <summary>Creates a tuple-labeling function with no positional labels.</summary>
    public Label()
        : this(() => []) { }

    /// <param name="names">One label for each tuple position, in positional order.</param>
    public Label([ArgumentPacking(ArgumentPackingMode.Variadic)] Func<string[]> names) => Names = names;

    public RecordValue Evaluate(IPositionalValue value)
        => TupleLabeling.Create(value, Names.Invoke(), qualifyConflictsOnly: false);

    object? IFunction.Evaluate(object? value)
        => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

/// <summary>Returns a flat record whose expanded record fields receive positional labels only when their unqualified names conflict.</summary>
[Function(prefix: "")]
[Scope("tuple")]
public sealed class LabelConflicts : IFunction<IPositionalValue, RecordValue>
{
    private Func<string[]> Names { get; }

    /// <summary>Creates a conflict-aware tuple-labeling function with no positional labels.</summary>
    public LabelConflicts()
        : this(() => []) { }

    /// <param name="names">One label for each tuple position, in positional order.</param>
    public LabelConflicts([ArgumentPacking(ArgumentPackingMode.Variadic)] Func<string[]> names) => Names = names;

    public RecordValue Evaluate(IPositionalValue value)
        => TupleLabeling.Create(value, Names.Invoke(), qualifyConflictsOnly: true);

    object? IFunction.Evaluate(object? value)
        => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

internal static class TupleLabeling
{
    public static RecordValue Create(IPositionalValue value, string[] labels, bool qualifyConflictsOnly)
    {
        ArgumentNullException.ThrowIfNull(labels);
        if (labels.Length != value.Arity)
            throw new ArgumentException("Number of labels must match tuple arity.", nameof(labels));
        if (labels.Any(label => label is null))
            throw new ArgumentException("Labels must not be null.", nameof(labels));

        var conflicts = qualifyConflictsOnly ? CountUnqualifiedNames(value, labels) : null;
        var result = new RecordValue();
        for (var index = 0; index < value.Arity; index++)
        {
            var item = value.GetPosition(index);
            if (item is RecordValue record)
            {
                foreach (var field in record)
                {
                    var name = conflicts is not null && conflicts[field.Key] == 1
                        ? field.Key
                        : $"{labels[index]}.{field.Key}";
                    SetUnique(result, name, field.Value);
                }
            }
            else
            {
                SetUnique(result, labels[index], item);
            }
        }

        return result;
    }

    private static Dictionary<string, int> CountUnqualifiedNames(IPositionalValue value, string[] labels)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < value.Arity; index++)
        {
            var item = value.GetPosition(index);
            if (item is RecordValue record)
            {
                foreach (var field in record)
                    Increment(field.Key);
            }
            else
            {
                Increment(labels[index]);
            }
        }

        return counts;

        void Increment(string name)
            => counts[name] = counts.GetValueOrDefault(name) + 1;
    }

    private static void SetUnique(RecordValue target, string name, object? value)
    {
        if (target.ContainsKey(name))
            throw new ArgumentException($"Tuple labels produce duplicate field '{name}'.");
        target.Set(name, value);
    }
}
