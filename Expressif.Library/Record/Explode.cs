using System.Collections;
using System.Data;
using Expressif.Bindings;
using Expressif.Values;
using Expressif.Values.Types;
using ValueRecord = Expressif.Values.RecordValue;

namespace Expressif.Library.Record;

/// <summary>Emits one record per element of a selected collection-valued field, preserving other fields and field order.</summary>
[Function(prefix: "")]
[Scope("record")]
public class Explode : IFunction<ValueRecord, ValueRecord[]>, IFunction<IEnumerable, ValueRecord[]>
{
    private readonly NamedFieldSelector selector;
    private readonly bool preserveParent;

    /// <param name="selector">A direct field selector identifying the collection-valued field to replace.</param>
    public Explode([AcceptedExpressionShape(AcceptedExpressionShape.DirectFieldSelector)] NamedFieldSelector selector) => this.selector = selector;

    protected Explode(NamedFieldSelector selector, bool preserveParent)
    {
        this.selector = selector;
        this.preserveParent = preserveParent;
    }

    public ValueRecord[] Evaluate(ValueRecord value) => ExplodeParent(value).ToArray();

    public ValueRecord[] Evaluate(IEnumerable value)
    {
        if (!IsCollection(value))
            throw new ArgumentException("Input value must be a record or an array of records.", nameof(value));
        return value.Cast<object?>().SelectMany(ExplodeParent).ToArray();
    }

    object? IFunction.Evaluate(object? value)
        => value is null or DBNull ? null
            : IsRecord(value) ? ExplodeParent(value).ToArray()
            : value is IEnumerable enumerable ? Evaluate(enumerable)
            : throw new ArgumentException("Input value must be a record or an array of records.", nameof(value));

    private IEnumerable<ValueRecord> ExplodeParent(object? parent)
    {
        if (!IsRecord(parent))
            throw new ArgumentException("Every parent supplied to explode must be a record.", nameof(parent));
        var fields = RecordOperations.Enumerate(parent);
        var children = selector.Evaluate(parent);
        if (children is null or DBNull)
        {
            if (preserveParent)
                yield return Replace(null);
            yield break;
        }
        if (!IsCollection(children))
            throw new ArgumentException("The field selected by explode must contain an array or supported enumerable.", nameof(parent));

        var any = false;
        foreach (var child in (IEnumerable)children)
        {
            any = true;
            yield return Replace(child);
        }
        if (!any && preserveParent)
            yield return Replace(null);

        ValueRecord Replace(object? child)
        {
            var result = new ValueRecord();
            foreach (var field in fields)
                result.Set(field.Key, field.Key == selector.Name ? child : field.Value);
            if (!result.ContainsKey(selector.Name))
                result.Set(selector.Name, child);
            return result;
        }
    }

    internal static bool IsRecord(object? value)
        => value is ValueRecord or IReadOnlyDictionary<string, object?> or IDictionary<string, object?>
            or IDictionary or DataRow or IReadOnlyDataRow;

    private static bool IsCollection(object? value)
        => value is IEnumerable and not string and not IDictionary
            and not IReadOnlyDictionary<string, object?> and not IExpressifValueType;
}
