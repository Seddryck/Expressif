using System.Collections;
using Expressif.Bindings;
using Expressif.Values;
using Expressif.Types;
using ValueRecord = Expressif.Values.RecordValue;

namespace Expressif.Functions.Record;

/// <summary>Groups records by all non-selected fields and collects selected values in source order.</summary>
public abstract class StructuralImplode : IFunction<IEnumerable, ValueRecord[]>
{
    private readonly NamedFieldSelector selector;
    private readonly bool ignoreNull;

    protected StructuralImplode(NamedFieldSelector selector, bool ignoreNull)
    {
        this.selector = selector;
        this.ignoreNull = ignoreNull;
    }

    public ValueRecord[] Evaluate(IEnumerable value)
    {
        if (value is string or IDictionary or IReadOnlyDictionary<string, object?> or IExpressifValueType)
            throw new ArgumentException("Input value must be an array of records.", nameof(value));

        var groups = new Dictionary<ValueRecord, (ValueRecord Parent, List<object?> Children)>(StructuralValueComparer.Instance);
        var ordered = new List<(ValueRecord Parent, List<object?> Children)>();
        foreach (var item in value)
        {
            if (!Explode.IsRecord(item))
                throw new ArgumentException("Every parent supplied to implode-inner must be a record.", nameof(value));
            var fields = RecordOperations.Enumerate(item);
            var key = new ValueRecord();
            foreach (var field in fields)
            {
                if (field.Key != selector.Name)
                    key.Set(field.Key, field.Value);
            }
            if (!groups.TryGetValue(key, out var group))
            {
                group = (RecordOperations.Copy(item), []);
                groups.Add(key, group);
                ordered.Add(group);
            }
            var child = selector.Evaluate(item);
            if (!ignoreNull || child is not (null or DBNull))
                group.Children.Add(child);
        }
        foreach (var group in ordered)
            group.Parent.Set(selector.Name, group.Children.ToArray());
        return ordered.Select(group => group.Parent).ToArray();
    }

    object? IFunction.Evaluate(object? value)
        => value is null or DBNull ? null
            : value is IEnumerable enumerable ? Evaluate(enumerable)
            : throw new ArgumentException("Input value must be an array of records.", nameof(value));
}
