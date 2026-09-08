using Expressif.Values.Casters;

namespace Expressif.Functions.Text;

/// <summary>
/// Splits text into consecutive nonempty segments of the requested lengths, preserving any remaining text as a final segment. Returns an empty array for null or empty input and null for invalid lengths.
/// </summary>
[Function(prefix: "")]
[Scope("text/partitioning")]
public sealed class SplitLengths : BaseTextFunction<string[]>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Lengths { get; }

    /// <summary>Creates a splitter with no requested lengths.</summary>
    public SplitLengths()
        : this(() => []) { }

    /// <param name="lengths">Zero or more strictly positive character counts, consumed in order. Spread arrays expand lengths in place.</param>
    public SplitLengths(Func<ValueArgumentEvaluator[]> lengths)
        => Lengths = lengths;

    protected override object? EvaluateHighLevelString(string value)
        => value is "(null)" or "(empty)" or "(blank)"
            ? base.EvaluateHighLevelString(value)
            : EvaluateString(value);

    protected override object? EvaluateNull() => EvaluateString(string.Empty);
    protected override object? EvaluateEmpty() => EvaluateString(string.Empty);
    protected override object? EvaluateBlank() => EvaluateString(" ");

    protected override object? EvaluateString(string value)
    {
        var lengths = new List<int>();
        var caster = new IntegerCaster();
        foreach (var argument in ValueArguments.Evaluate(Lengths.Invoke(), value))
        {
            if (argument is null || !caster.TryCast(argument, out var length) || length <= 0)
                return null;
            lengths.Add(length);
        }

        var segments = new List<string>();
        var offset = 0;
        foreach (var length in lengths)
        {
            if (offset == value.Length)
                break;
            var count = Math.Min(length, value.Length - offset);
            segments.Add(value.Substring(offset, count));
            offset += count;
        }

        if (offset < value.Length)
            segments.Add(value[offset..]);
        return segments.ToArray();
    }
}
