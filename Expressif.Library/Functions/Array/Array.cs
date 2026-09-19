using System.Collections;

namespace Expressif.Functions.Array;

/// <summary>
/// Constructs a new array by evaluating zero or more positional expressions from left to right against the same input.
/// Spread arguments expand array values in place. This is the runtime-expression counterpart of array literal syntax.
/// </summary>
[Function(prefix: "", aliases: ["array"])]
public class Array : IFunction<object?, object?[]>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Values { get; }

    /// <summary>Creates an empty array constructor.</summary>
    public Array()
        : this(() => []) { }

    /// <param name="values">Zero or more expressions whose evaluated values become the elements of the resulting array.</param>
    public Array(Func<ValueArgumentEvaluator[]> values)
        => Values = values;

    public object?[] Evaluate(object? value)
        => ValueArguments.Evaluate(Values.Invoke(), value).ToArray();

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

internal static class SpreadValues
{
    public static IEnumerable<object?> Enumerate(object? value)
    {
        if (value is null)
            throw new SpreadArgumentException("Spread argument cannot be null.");

        if (value is string || value is not IEnumerable enumerable)
            throw new SpreadArgumentException("Spread argument must evaluate to an array.");

        foreach (var item in enumerable)
            yield return item;
    }

    public static void Append(object? value, ICollection<object?> target)
    {
        foreach (var item in Enumerate(value))
            target.Add(item);
    }
}
