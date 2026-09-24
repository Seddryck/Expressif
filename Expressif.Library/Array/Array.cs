namespace Expressif.Library.Array;

/// <summary>
/// Constructs a new array by evaluating zero or more positional expressions from left to right against the same input.
/// Spread arguments expand array values in place. This is the runtime-expression counterpart of array literal syntax.
/// </summary>
[Function(prefix: "", aliases: ["array"])]
public class Array : IFunction<object?, object?[]>
{
    private Func<object?, object?[]> Values { get; }

    /// <summary>Creates an empty array constructor.</summary>
    public Array()
        : this(_ => []) { }

    /// <param name="values">Zero or more expressions whose evaluated values become the elements of the resulting array.</param>
    public Array([ArgumentPacking(ArgumentPackingMode.Variadic, AllowSpread = true)] [ArgumentOmission(ArgumentOmissionMode.EmptyVariadic)] Func<object?, object?[]> values)
        => Values = values;

    public object?[] Evaluate(object? value)
        => Values.Invoke(value);

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
