using System.Collections;
using System.Data;
using Expressif.Values;
using Expressif.Values.Casters;
using ValueRecord = Expressif.Values.RecordValue;

namespace Expressif.Functions.Record;

/// <summary>Converts all record fields to pairs, preserving field names, values, and order.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class Pairs : IFunction<ValueRecord, PairValue[]>
{
    public PairValue[] Evaluate(ValueRecord value) => EvaluateCore(value);

    object? IFunction.Evaluate(object? value) => EvaluateCore(value);

    private static PairValue[] EvaluateCore(object? value)
    {
        if (value is not ValueRecord
            and not IReadOnlyDictionary<string, object?>
            and not IDictionary<string, object?>
            and not IDictionary
            and not DataRow
            and not ILiteDataRow)
            throw new ArgumentException("Input value must be a record.", nameof(value));

        return RecordOperations.Enumerate(value)
            .Select(field => new PairValue(field.Key, field.Value))
            .ToArray();
    }
}

/// <summary>Converts an array of pairs to a record, coercing keys to text and preserving values and order.</summary>
[Function(prefix: "")]
[Scope("record")]
public sealed class FromPairs : IFunction<IEnumerable, ValueRecord>
{
    public ValueRecord Evaluate(IEnumerable value)
    {
        if (value is null or string or IReadOnlyDictionary<string, object?> or IDictionary<string, object?> or IDictionary or TupleValue)
            throw new ArgumentException("Input value must be an array or supported enumerable of pairs.", nameof(value));

        var result = new ValueRecord();
        var caster = new TextCaster();
        foreach (var item in value)
        {
            if (item is not PairValue pair)
                throw new ArgumentException("Every element supplied to from-pairs must be a pair.", nameof(value));
            if (pair.Key is null || !caster.TryCast(pair.Key, out var name) || name is null)
                throw new ArgumentException("Every pair key must be coercible to text.", nameof(value));
            if (result.ContainsKey(name))
                throw new ArgumentException($"Duplicate record field name '{name}' after key coercion.", nameof(value));

            result.Set(name, pair.Value);
        }
        return result;
    }

    object? IFunction.Evaluate(object? value)
        => value is IEnumerable enumerable
            ? Evaluate(enumerable)
            : throw new ArgumentException("Input value must be an array or supported enumerable of pairs.", nameof(value));
}
