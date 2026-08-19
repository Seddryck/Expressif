using Expressif.Values;
using System;
using System.Collections.Generic;
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
        => NamedValueAccessor.Get(value, Name.Invoke());
}

/// <summary>
/// Creates a record by evaluating its named and spread entries against the input value.
/// Later entries overwrite fields with the same name created by earlier entries.
/// </summary>
[Function(prefix: "")]
public class Record : IFunction
{
    private Func<RecordEntryEvaluator[]> Entries { get; }

    public Record()
        : this(() => []) { }

    /// <param name="entries">Factory that creates the named and spread entries used to build the record.</param>
    public Record(Func<RecordEntryEvaluator[]> entries)
        => Entries = entries;

    public object? Evaluate(object? value)
    {
        var record = new ValueRecord();
        foreach (var entry in Entries.Invoke())
            entry.Apply(value, record);

        return record;
    }
}

public class RecordEntryEvaluator
{
    private string? Name { get; }
    private Func<object?, object?>? Evaluator { get; }

    private bool IsSpread => Name == null;

    private RecordEntryEvaluator(string? name, Func<object?, object?>? evaluator)
        => (Name, Evaluator) = (name, evaluator);

    public static RecordEntryEvaluator Spread()
        => new(null, null);

    public static RecordEntryEvaluator Named(string name, Func<object?, object?> evaluator)
        => new(name, evaluator);

    public void Apply(object? input, ValueRecord target)
    {
        if (IsSpread)
        {
            ApplySpread(input, target);
            return;
        }

        target.Set(Name!, Evaluator!.Invoke(input));
    }

    private static void ApplySpread(object? input, ValueRecord target)
    {
        if (NamedValueAccessor.TryEnumerate(input, out IReadOnlyList<KeyValuePair<string, object?>>? fields))
        {
            foreach (var field in fields)
                target.Set(field.Key, field.Value);

            return;
        }

        target.Set(GenerateUnnamedFieldName(target), input);
    }

    private static string GenerateUnnamedFieldName(ValueRecord target)
    {
        var index = 0;
        while (target.ContainsKey($"__NONAME_{index}"))
            index++;

        return $"__NONAME_{index}";
    }
}
