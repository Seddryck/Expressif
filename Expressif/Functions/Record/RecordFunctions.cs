using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Expressif.Values;
using ValueRecord = Expressif.Values.RecordValue;

namespace Expressif.Functions.Record;

/// <summary>
/// Returns the value of the named field from the input record or object.
/// Returns <see langword="null"/> when the field does not exist or the input does not expose named values.
/// </summary>
[Function(prefix: "")]
public class Field : IFunction
{
    private Func<string> Name { get; }

    /// <param name="name">Name of the field to retrieve from the input.</param>
    public Field(Func<string> name)
        => Name = name;

    public object? Evaluate(object? value)
        => NamedValueAccessor.TryGetValue(value, Name.Invoke(), out var result) ? result : null;
}

/// <summary>
/// Creates a record by evaluating its named and spread entries against the input value.
/// Later entries overwrite fields with the same name created by earlier entries.
/// </summary>
[Function(prefix: "")]
public class Record : IFunction<object?, ValueRecord>, IValueSpreadAware
{
    private Func<RecordEntryEvaluator[]> Entries { get; }

    /// <summary>Creates an empty record constructor.</summary>
    public Record()
        : this(() => []) { }

    /// <param name="entries">Zero or more named or spread entries used to construct the resulting record. Each entry is evaluated against the input value.</param>
    public Record(Func<RecordEntryEvaluator[]> entries)
        => Entries = entries;

    public object? Evaluate(object? value)
    {
        var record = new ValueRecord();
        foreach (var entry in Entries.Invoke())
            entry.Apply(value, record);

        return record;
    }

    ValueRecord IFunction<object?, ValueRecord>.Evaluate(object? value) => (ValueRecord)Evaluate(value)!;
}

/// <summary>
/// Evaluates named projections against the same input and evaluates a body expression against their temporary record.
/// </summary>
[Function(prefix: "")]
public class With : IFunction
{
    private Func<RecordEntryEvaluator[]> Projections { get; }
    private Func<object?, object?> Body { get; }

    /// <param name="projections">One or more named projections evaluated independently against the input value.</param>
    /// <param name="body">The final expression evaluated against the temporary projection record.</param>
    public With(Func<RecordEntryEvaluator[]> projections, Func<object?, object?> body)
        => (Projections, Body) = (projections, body);

    public object? Evaluate(object? value)
    {
        var temporary = new ValueRecord();
        foreach (var projection in Projections.Invoke())
            projection.Apply(value, temporary);

        return Body.Invoke(temporary);
    }
}

public class RecordEntryEvaluator
{
    private string? Name { get; }
    private Func<object?, object?> Evaluator { get; }

    private bool IsSpread => Name == null;

    private RecordEntryEvaluator(string? name, Func<object?, object?> evaluator)
        => (Name, Evaluator) = (name, evaluator);

    public static RecordEntryEvaluator Spread(Func<object?, object?> evaluator)
        => new(null, evaluator);

    public static RecordEntryEvaluator Named(string name, Func<object?, object?> evaluator)
        => new(name, evaluator);

    public void Apply(object? input, ValueRecord target)
    {
        if (IsSpread)
        {
            ApplySpread(Evaluator.Invoke(input), target);
            return;
        }

        target.Set(Name!, Evaluator.Invoke(input));
    }

    private static void ApplySpread(object? input, ValueRecord target)
    {
        if (IsRecord(input)
            && NamedValueAccessor.TryEnumerate(input, out IReadOnlyList<KeyValuePair<string, object?>>? fields))
        {
            foreach (var field in fields)
                target.Set(field.Key, field.Value);

            return;
        }

        throw new SpreadArgumentException("Spread argument must evaluate to a record.");
    }

    private static bool IsRecord(object? value)
        => value is ValueRecord
            or IReadOnlyDictionary<string, object?>
            or IDictionary<string, object?>
            or IDictionary
            or DataRow
            or ILiteDataRow;
}
