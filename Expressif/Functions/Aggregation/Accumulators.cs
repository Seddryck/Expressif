using Expressif.Values.Casters;
using System;

namespace Expressif.Functions.Array;

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
