using Expressif.Values.Types;
using Expressif.Values;
using Expressif.Values.Casters;

namespace Expressif.Library.Sorting;

[Function(prefix: "")]
[Scope("sorting")]
public abstract class BaseComparison<T> : IFunction<object?, OrderingValue?>
    where T : IComparable<T>
{
    private readonly Func<object?> right;

    protected BaseComparison(Func<object?> right)
        => this.right = right;

    public OrderingValue? Evaluate(object? value)
    {
        var rightValue = right.Invoke();
        if (!TryCoerce(value, out var coercedLeft) || !TryCoerce(rightValue, out var coercedRight))
            return null;

        return ToOrdering(CompareValues(coercedLeft, coercedRight));
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);

    protected virtual bool TryCoerce(object? value, out T result)
    {
        if (new Caster().TryCast<T>(value, out var converted))
        {
            result = converted;
            return true;
        }

        result = default!;
        return false;
    }

    protected virtual int CompareValues(T left, T right) => left.CompareTo(right);

    protected static OrderingValue ToOrdering(int comparison)
        => comparison < 0 ? OrderingValue.Less
            : comparison > 0 ? OrderingValue.Greater
            : OrderingValue.Equal;
}

/// <summary>Compares two values after numeric coercion.</summary>
public sealed class CompareNumeric : BaseComparison<decimal>, IFunction<decimal?, OrderingValue?>
{
    /// <param name="right">The value compared with the input value.</param>
    public CompareNumeric(Func<object?> right)
        : base(right) { }

    OrderingValue? IFunction<decimal?, OrderingValue?>.Evaluate(decimal? value) => Evaluate(value);
}

/// <summary>Compares two values as text using ordinal ordering.</summary>
public sealed class CompareOrdinal : BaseComparison<string>, IFunction<string?, OrderingValue?>
{
    /// <param name="right">The value compared with the input value.</param>
    public CompareOrdinal(Func<object?> right)
        : base(right) { }

    OrderingValue? IFunction<string?, OrderingValue?>.Evaluate(string? value) => Evaluate(value);

    protected override int CompareValues(string left, string right)
        => StringComparer.Ordinal.Compare(left, right);
}

/// <summary>Compares two values after date coercion.</summary>
public sealed class CompareDate : BaseComparison<DateOnly>, IFunction<DateOnly?, OrderingValue?>
{
    /// <param name="right">The value compared with the input value.</param>
    public CompareDate(Func<object?> right)
        : base(right) { }

    OrderingValue? IFunction<DateOnly?, OrderingValue?>.Evaluate(DateOnly? value) => Evaluate(value);
}

/// <summary>Compares two values after time coercion.</summary>
public sealed class CompareTime : BaseComparison<TimeOnly>, IFunction<TimeOnly?, OrderingValue?>
{
    /// <param name="right">The value compared with the input value.</param>
    public CompareTime(Func<object?> right)
        : base(right) { }

    OrderingValue? IFunction<TimeOnly?, OrderingValue?>.Evaluate(TimeOnly? value) => Evaluate(value);
}

/// <summary>Compares two values after datetime coercion.</summary>
public sealed class CompareDateTime : BaseComparison<DateTime>, IFunction<DateTime?, OrderingValue?>
{
    /// <param name="right">The value compared with the input value.</param>
    public CompareDateTime(Func<object?> right)
        : base(right) { }

    OrderingValue? IFunction<DateTime?, OrderingValue?>.Evaluate(DateTime? value) => Evaluate(value);
}

/// <summary>Compares two values in a selected supported domain.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class Compare : IFunction<object?, OrderingValue?>
{
    private readonly Func<object?> right;
    private readonly Func<TypeDescriptor> type;

    /// <param name="right">The value compared with the input value.</param>
    public Compare(Func<object?> right)
        : this(right, () => ExpressifTypeRegistry.Instance.Resolve("text")) { }

    /// <param name="right">The value compared with the input value.</param>
    /// <param name="type">The comparison domain.</param>
    public Compare(Func<object?> right, Func<TypeDescriptor> type)
        => (this.right, this.type) = (right, type);

    public OrderingValue? Evaluate(object? value)
    {
        BaseComparisonAdapter comparison = type.Invoke().Name.ToLowerInvariant() switch
        {
            "text" => new((left, right) => new CompareOrdinal(() => right).Evaluate(left)),
            "integer" or "decimal" or "numeric" => new((left, right) => new CompareNumeric(() => right).Evaluate(left)),
            "date" => new((left, right) => new CompareDate(() => right).Evaluate(left)),
            "time" => new((left, right) => new CompareTime(() => right).Evaluate(left)),
            "datetime" or "date-time" => new((left, right) => new CompareDateTime(() => right).Evaluate(left)),
            var name => throw new ArgumentException($"Type ':{name}' is not supported by compare.", nameof(type)),
        };

        return comparison.Evaluate(value, right.Invoke());
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);

    private sealed record BaseComparisonAdapter(Func<object?, object?, OrderingValue?> Evaluate);
}
