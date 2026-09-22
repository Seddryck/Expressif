using System.Collections;
using Expressif.Values;

namespace Expressif.Library.Array.Grouping;

/// <summary>Converts selected present record fields into rows, preserving retained fields and source order.</summary>
[Function(prefix: "")]
[Scope("array/grouping")]
public sealed class Unpivot : BaseArrayFunction<RecordValue[]>
{
    private readonly Func<object?[]> fields;
    private readonly Func<string> nameField;
    private readonly Func<string> valueField;

    /// <param name="fields">An ordered array of distinct text field names to turn into rows.</param>
    /// <param name="nameField">The output field containing the selected source field name.</param>
    /// <param name="valueField">The output field containing the selected source field value.</param>
    public Unpivot(Func<object?[]> fields, Func<string> nameField, Func<string> valueField)
        => (this.fields, this.nameField, this.valueField) = (fields, nameField, valueField);

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var selected = fields.Invoke();
        var name = nameField.Invoke();
        var value = valueField.Invoke();
        if (selected is null || selected.Any(field => field is not string))
            throw new ArgumentException("Every unpivot field name must be text.", nameof(fields));
        var names = selected.Cast<string>().ToArray();
        var selection = new HashSet<string>(names, StringComparer.Ordinal);
        if (selection.Count != names.Length)
            throw new ArgumentException("Duplicate unpivot field name.", nameof(fields));
        if (name is null || value is null)
            throw new ArgumentException("Unpivot output field names must be non-null text.");
        if (name == value)
            throw new ArgumentException("Unpivot output field names must be distinct.");

        var result = new List<RecordValue>();
        IFunction pairs = new Record.Pairs();
        var fromPairs = new Record.FromPairs();
        foreach (var item in enumerable)
        {
            var source = (PairValue[])pairs.Evaluate(item)!;
            if (names.Length == 0)
                continue;
            var retained = source.Where(field => !selection.Contains((string)field.Key!)).ToArray();
            foreach (var field in retained)
            {
                if (Equals(field.Key, name) || Equals(field.Key, value))
                    throw new ArgumentException($"Duplicate record field name '{field.Key}' after key coercion.");
            }
            var lookup = source.ToDictionary(field => (string)field.Key!, field => field.Value, StringComparer.Ordinal);
            foreach (var field in names)
            {
                if (lookup.TryGetValue(field, out var fieldValue))
                    result.Add(fromPairs.Evaluate(retained.Concat([new PairValue(name, field), new PairValue(value, fieldValue)])));
            }
        }
        return result.ToArray();
    }
}
