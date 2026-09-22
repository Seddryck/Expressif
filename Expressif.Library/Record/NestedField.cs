using Expressif.Values;

namespace Expressif.Library.Record;

/// <summary>Returns the value at a nested field path in the input record or object, or null when the path cannot be resolved.</summary>
[Function(prefix: "", DynamicReason = "Output depends on the value selected by the runtime field path.", SupportsValueSpread = true)]
[Scope("record")]
public sealed class NestedField : IFunction
{
    private Func<object?, object?[]> Path { get; }

    /// <param name="path">One or more literal field names in traversal order. Spread arguments expand arrays of names in place.</param>
    public NestedField(Func<object?, object?[]> path) => Path = path;

    public object? Evaluate(object? value)
    {
        var segments = Path.Invoke(value);
        if (segments.Length == 0)
            throw new ArgumentException("The nested-field path must contain at least one field name.", "path");
        if (segments.Any(segment => segment is not string))
            throw new ArgumentException("Every nested-field path segment must be text.", "path");

        foreach (var segment in segments.Cast<string>())
        {
            if (!NamedValueAccessor.TryGetValue(value, segment, out value))
                return null;
        }
        return value;
    }
}
