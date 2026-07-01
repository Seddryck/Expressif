using Expressif.Values.Casters;
using System;

namespace Expressif.Accumulators;

/// <summary>
/// Counts the number of accumulated items, including <see langword="null"/> values.
/// </summary>
[Accumulator(prefix: "", aliases: ["count"])]
public class CountAccumulator : BaseAccumulator
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
[Accumulator(prefix: "", aliases: ["sum"])]
public class SumAccumulator : BaseAccumulator
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
[Accumulator(prefix: "", aliases: ["min"])]
public class MinAccumulator : BaseAccumulator
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
[Accumulator(prefix: "", aliases: ["max"])]
public class MaxAccumulator : BaseAccumulator
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
[Accumulator(prefix: "", aliases: ["first"])]
public class FirstAccumulator : BaseAccumulator
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
[Accumulator(prefix: "", aliases: ["last"])]
public class LastAccumulator : BaseAccumulator
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
