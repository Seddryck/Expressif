namespace Expressif.Library.Array;

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
