using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Functions.Special;

/// <summary>
/// Coerces a scalar value or selected tuple and record values to requested Expressif types.
/// </summary>
[Function]
public sealed class Coerce : IFunction<object?, object?>
{
    private readonly Type[]? positionalTypes;
    private readonly CoercionMapping[]? mappings;
    private readonly Caster caster = new();

    /// <summary>Creates a type-directed coercion.</summary>
    /// <param name="specifications">One or more positional type descriptors or selector-to-type mappings.</param>
    public Coerce(params Type[] specifications)
        => positionalTypes = specifications;

    /// <summary>Creates a type-directed coercion.</summary>
    /// <param name="specifications">One or more positional type descriptors or selector-to-type mappings.</param>
    public Coerce(params CoercionMapping[] specifications)
        => mappings = specifications;

    public object? Evaluate(object? value)
        => value switch
        {
            TupleValue tuple => CoerceTuple(tuple),
            RecordValue record => CoerceRecord(record),
            _ => CoerceScalar(value),
        };

    object? IFunction.Evaluate(object? value) => Evaluate(value);

    private object? CoerceScalar(object? value)
    {
        if (positionalTypes is not [var type])
            throw new StructuralValidationException("Scalar input requires exactly one positional type descriptor.");
        return Convert(value, type);
    }

    private TupleValue CoerceTuple(TupleValue tuple)
    {
        var values = tuple.ToArray();
        if (positionalTypes is not null)
        {
            for (var index = 0; index < Math.Min(positionalTypes.Length, values.Length); index++)
                values[index] = Convert(values[index], positionalTypes[index]);
        }
        else
        {
            if (mappings is null || mappings.Any(mapping => mapping.Selector is not TupleCoercionSelector))
                throw new StructuralValidationException("Tuple input requires tuple-position selectors.");
            foreach (var mapping in mappings)
            {
                var index = ((TupleCoercionSelector)mapping.Selector).Position;
                if (index < 0 || index >= values.Length)
                    continue;
                values[index] = Convert(values[index], mapping.TargetType);
            }
        }
        return new Values.Tuple(values);
    }

    private RecordValue CoerceRecord(RecordValue record)
    {
        if (mappings is null || mappings.Any(mapping => mapping.Selector is not FieldCoercionSelector))
            throw new StructuralValidationException("Record input requires field selector mappings.");
        var result = new RecordValue();
        foreach (var field in record)
            result.Set(field.Key, field.Value);
        foreach (var mapping in mappings)
        {
            var field = ((FieldCoercionSelector)mapping.Selector).Field;
            if (record.TryGetValue(field, out var value))
                result.Set(field, Convert(value, mapping.TargetType));
        }
        return result;
    }

    private object? Convert(object? value, Type targetType)
        => caster.TryCast(value, targetType, out var result) ? result : null;
}

public interface ICoercionSelector;
public sealed record FieldCoercionSelector(string Field) : ICoercionSelector;
public sealed record TupleCoercionSelector(int Position) : ICoercionSelector;
public sealed record CoercionMapping(ICoercionSelector Selector, Type TargetType);
