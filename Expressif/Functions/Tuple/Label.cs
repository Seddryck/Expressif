using System;
using System.Collections.Generic;
using System.Linq;
using Expressif.Values;

namespace Expressif.Functions.Tuple;

/// <summary>Returns a flat record by applying one positional label to each tuple item and qualifying every field expanded from a record item.</summary>
[Function(prefix: "")]
[Scope("tuple")]
public sealed class Label : IFunction<IPositionalValue, RecordValue>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Names { get; }

    /// <summary>Creates a label function with no supplied labels.</summary>
    public Label()
        : this(() => []) { }

    /// <param name="names">One label expression for each tuple position, in positional order. Spread arrays expand labels in place.</param>
    public Label(Func<ValueArgumentEvaluator[]> names) => Names = names;

    public RecordValue Evaluate(IPositionalValue value)
        => TupleLabeling.Create(
            value,
            ValueArguments.Evaluate(Names.Invoke(), value).ToArray(),
            qualifyConflictsOnly: false);

    object? IFunction.Evaluate(object? value)
        => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

/// <summary>Returns a flat record by applying positional labels only to record fields whose unqualified names would conflict.</summary>
[Function(prefix: "")]
[Scope("tuple")]
public sealed class LabelConflicts : IFunction<IPositionalValue, RecordValue>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Names { get; }

    /// <summary>Creates a label-conflicts function with no supplied labels.</summary>
    public LabelConflicts()
        : this(() => []) { }

    /// <param name="names">One label expression for each tuple position, in positional order. Spread arrays expand labels in place.</param>
    public LabelConflicts(Func<ValueArgumentEvaluator[]> names) => Names = names;

    public RecordValue Evaluate(IPositionalValue value)
        => TupleLabeling.Create(
            value,
            ValueArguments.Evaluate(Names.Invoke(), value).ToArray(),
            qualifyConflictsOnly: true);

    object? IFunction.Evaluate(object? value)
        => value is IPositionalValue tuple ? Evaluate(tuple) : null;
}

internal static class TupleLabeling
{
    public static RecordValue Create(IPositionalValue value, object?[] labels, bool qualifyConflictsOnly)
    {
        ArgumentNullException.ThrowIfNull(labels);
        if (labels.Length != value.Arity)
            throw new ArgumentException("Number of labels must match tuple arity.", nameof(labels));
        if (labels.Any(label => label is not string))
            throw new ArgumentException("Every label must be text.", nameof(labels));

        var names = labels.Cast<string>().ToArray();
        var conflicts = qualifyConflictsOnly ? CountUnqualifiedNames(value, names) : null;
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
                        : $"{names[index]}.{field.Key}";
                    SetUnique(result, name, field.Value);
                }
            }
            else
            {
                SetUnique(result, names[index], item);
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
