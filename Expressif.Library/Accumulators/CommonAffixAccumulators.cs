using System;

namespace Expressif.Accumulators;

/// <summary>
/// Returns the longest prefix shared by all accumulated strings.
/// </summary>
[Accumulator(prefix: "", aliases: ["common-prefix"])]
public class CommonPrefixAccumulator : BaseAccumulator
{
    private string? value;

    public override void Initialize()
        => value = null;

    public override void Accumulate(object? item)
    {
        if (item is not string text)
            throw new InvalidCastException("Common-prefix aggregation requires string values.");

        if (value is null)
        {
            value = text;
            return;
        }

        var length = 0;
        var limit = Math.Min(value.Length, text.Length);
        while (length < limit && value[length] == text[length])
            length++;

        value = value[..length];
    }

    public override object? GetValue()
        => value;
}

/// <summary>
/// Returns the longest suffix shared by all accumulated strings.
/// </summary>
[Accumulator(prefix: "", aliases: ["common-suffix"])]
public class CommonSuffixAccumulator : BaseAccumulator
{
    private string? value;

    public override void Initialize()
        => value = null;

    public override void Accumulate(object? item)
    {
        if (item is not string text)
            throw new InvalidCastException("Common-suffix aggregation requires string values.");

        if (value is null)
        {
            value = text;
            return;
        }

        var length = 0;
        var limit = Math.Min(value.Length, text.Length);
        while (length < limit && value[value.Length - length - 1] == text[text.Length - length - 1])
            length++;

        value = value[(value.Length - length)..];
    }

    public override object? GetValue()
        => value;
}
