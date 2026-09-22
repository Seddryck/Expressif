using System;
using Expressif.Functions;
using Expressif.Values.Casters;

namespace Expressif.Library.Array.Aggregation;

/// <summary>
/// Counts the number of accumulated items, including <see langword="null"/> values.
/// </summary>
[Function(prefix: "", Name = "count")]
public class CountAccumulator : BaseArrayAccumulator
{
    private int count;

    public override void Initialize()
        => count = 0;

    public override void Accumulate(object? item)
        => count++;

    public override object GetValue()
        => count;
}

/// <summary>
/// Computes the sum of all accumulated numeric values.
/// </summary>
/// <remarks>
/// Each item is converted using <see cref="NumericCaster"/>.
/// A <see cref="InvalidCastException"/> is thrown when a <see langword="null"/> value is accumulated.
/// </remarks>
[Function(prefix: "", Name = "sum")]
public class SumAccumulator : BaseArrayAccumulator
{
    private decimal sum;
    private NumericCaster Caster { get; } = new();

    public override void Initialize()
        => sum = 0;

    public override void Accumulate(object? item)
        => sum += Caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to numeric for sum aggregation."));

    public override object GetValue()
        => sum;
}

/// <summary>
/// Tracks the smallest numeric value found during accumulation.
/// </summary>
/// <remarks>
/// Returns <see langword="null"/> when no value has been accumulated.
/// </remarks>
[Function(prefix: "", Name = "min")]
public class MinAccumulator : BaseArrayAccumulator
{
    private decimal? min;
    private NumericCaster Caster { get; } = new();

    public override void Initialize()
        => min = null;

    public override void Accumulate(object? item)
    {
        var numeric = Caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to numeric for min aggregation."));
        min = min.HasValue ? Math.Min(min.Value, numeric) : numeric;
    }

    public override object? GetValue()
        => min;
}

/// <summary>
/// Tracks the greatest numeric value found during accumulation.
/// </summary>
/// <remarks>
/// Returns <see langword="null"/> when no value has been accumulated.
/// </remarks>
[Function(prefix: "", Name = "max")]
public class MaxAccumulator : BaseArrayAccumulator
{
    private decimal? max;
    private NumericCaster Caster { get; } = new();

    public override void Initialize()
        => max = null;

    public override void Accumulate(object? item)
    {
        var numeric = Caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to numeric for max aggregation."));
        max = max.HasValue ? Math.Max(max.Value, numeric) : numeric;
    }

    public override object? GetValue()
        => max;
}

/// <summary>
/// Stores the first accumulated item and ignores all subsequent items.
/// </summary>
/// <remarks>
/// Returns <see langword="null"/> when no value has been accumulated.
/// </remarks>
[Function(prefix: "", Name = "first")]
public class FirstAccumulator : BaseArrayAccumulator
{
    private object? first;
    private bool hasValue;

    public override void Initialize()
    {
        first = null;
        hasValue = false;
    }

    public override void Accumulate(object? item)
    {
        if (hasValue)
            return;

        first = item;
        hasValue = true;
    }

    public override object? GetValue()
        => hasValue ? first : null;
}

/// <summary>
/// Stores the most recently accumulated item.
/// </summary>
/// <remarks>
/// Returns <see langword="null"/> when no value has been accumulated.
/// </remarks>
[Function(prefix: "", Name = "last")]
public class LastAccumulator : BaseArrayAccumulator
{
    private object? last;
    private bool hasValue;

    public override void Initialize()
    {
        last = null;
        hasValue = false;
    }

    public override void Accumulate(object? item)
    {
        last = item;
        hasValue = true;
    }

    public override object? GetValue()
        => hasValue ? last : null;
}

/// <summary>
/// Returns <see langword="true"/> only when every accumulated boolean value is <see langword="true"/>.
/// </summary>
/// <remarks>
/// The neutral value is <see langword="true"/>. Each item is converted using <see cref="BooleanCaster"/>.
/// A <see cref="InvalidCastException"/> is thrown when a <see langword="null"/> value is accumulated.
/// </remarks>
[Function(prefix: "", Name = "every")]
public class EveryAccumulator : BaseArrayAccumulator
{
    private bool every;
    private BooleanCaster Caster { get; } = new();

    public override void Initialize()
        => every = true;

    public override void Accumulate(object? item)
        => every &= Caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to boolean for every aggregation."));

    public override object GetValue()
        => every;
}

/// <summary>
/// Returns <see langword="true"/> when at least one accumulated boolean value is <see langword="true"/>.
/// </summary>
/// <remarks>
/// The neutral value is <see langword="false"/>. Each item is converted using <see cref="BooleanCaster"/>.
/// A <see cref="InvalidCastException"/> is thrown when a <see langword="null"/> value is accumulated.
/// </remarks>
[Function(prefix: "", Name = "any")]
public class AnyAccumulator : BaseArrayAccumulator
{
    private bool any;
    private BooleanCaster Caster { get; } = new();

    public override void Initialize()
        => any = false;

    public override void Accumulate(object? item)
        => any |= Caster.Cast(item ?? throw new InvalidCastException("Cannot cast null value to boolean for any aggregation."));

    public override object GetValue()
        => any;
}

/// <summary>
/// Combines accumulated items in source order by evaluating an expression against the accumulated value and current item.
/// </summary>
/// <remarks>
/// The expression receives a two-element tuple where <c>$0</c> is the accumulated value and <c>$1</c> is the current item.
/// Without an initial value, the first item becomes the accumulated value. An empty input then returns <see langword="null"/>.
/// </remarks>
[Function(prefix: "", Name = "reduce")]
public class ReduceAccumulator : BaseArrayAccumulator
{
    private readonly Func<IFunction> operationProvider;
    private readonly Func<object?>? initialProvider;
    private IFunction? operation;
    private object? value;
    private bool hasValue;

    /// <param name="operation">Specifies the expression evaluated against each accumulated-value/current-item tuple.</param>
    public ReduceAccumulator(Func<IFunction> operation)
        => operationProvider = operation;

    /// <param name="operation">Specifies the expression evaluated against each accumulated-value/current-item tuple.</param>
    /// <param name="initial">Specifies the initial accumulated value. It is returned unchanged for an empty input.</param>
    public ReduceAccumulator(Func<IFunction> operation, Func<object?> initial)
        => (operationProvider, initialProvider) = (operation, initial);

    public override void Initialize()
    {
        operation = operationProvider.Invoke();
        hasValue = initialProvider is not null;
        value = initialProvider?.Invoke();
    }

    public override void Accumulate(object? item)
    {
        if (!hasValue)
        {
            value = item;
            hasValue = true;
            return;
        }

        var pair = new Expressif.Values.Tuple(value, item);
        value = EvaluationRuntime.EvaluateNested(operation!, pair);
    }

    public override object? GetValue()
        => hasValue ? value : null;
}
