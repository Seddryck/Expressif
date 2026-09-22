using System;
using Expressif.Functions;

namespace Expressif.Library.Array.Aggregation;

/// <summary>
/// Returns the longest prefix shared by all accumulated strings.
/// </summary>
[Function(prefix: "", Name = "common-prefix")]
public class CommonPrefixAccumulator : BaseArrayAccumulator
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
[Function(prefix: "", Name = "common-suffix")]
public class CommonSuffixAccumulator : BaseArrayAccumulator
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
