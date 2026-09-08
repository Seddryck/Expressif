namespace Expressif.Functions.Text;

/// <summary>
/// Splits text into consecutive nonempty segments while an operation over the current segment and next character returns true. Preserves every character. Returns an empty array for null or empty input and null for a non-Boolean operation result.
/// </summary>
[Function(prefix: "")]
[Scope("text/partitioning")]
public sealed class SplitWhile : BaseTextFunction<string[]>
{
    private Func<IFunction> Operation { get; }

    /// <param name="operation">The callable or open expression deciding whether the next character extends the current segment.</param>
    public SplitWhile(Func<IFunction> operation)
        => Operation = operation;

    protected override object? EvaluateHighLevelString(string value)
        => value is "(null)" or "(empty)" or "(blank)"
            ? base.EvaluateHighLevelString(value)
            : EvaluateString(value);

    protected override object EvaluateNull() => System.Array.Empty<string>();
    protected override object EvaluateEmpty() => System.Array.Empty<string>();
    protected override object EvaluateBlank() => new[] { " " };

    protected override object? EvaluateString(string value)
    {
        if (value.Length == 0)
            return System.Array.Empty<string>();
        if (value.Length == 1)
            return new[] { value };

        var operation = Operation.Invoke();
        var segments = new List<string>();
        var start = 0;
        for (var index = 1; index < value.Length; index++)
        {
            var segment = value[start..index];
            var candidate = value[index].ToString();
            if (operation.Evaluate(new Values.Tuple(segment, candidate)) is not bool continues)
                return null;
            if (!continues)
            {
                segments.Add(segment);
                start = index;
            }
        }
        segments.Add(value[start..]);
        return segments.ToArray();
    }
}
