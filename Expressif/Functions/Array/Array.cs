using System.Collections;

namespace Expressif.Functions.Array;

/// <summary>
/// Constructs a new array by evaluating zero or more positional expressions from left to right against the same input.
/// Spread arguments expand array values in place. This is the runtime-expression counterpart of array literal syntax.
/// </summary>
[Function(prefix: "", aliases: ["array"])]
public class Array : IFunction<object?, object?[]>
{
    private Func<ArrayArgumentEvaluator[]> Values { get; }

    /// <summary>Creates an empty array constructor.</summary>
    public Array()
        : this(() => []) { }

    /// <param name="values">Specifies zero or more positional expressions whose evaluated values become array elements.</param>
    public Array(Func<ArrayArgumentEvaluator[]> values)
        => Values = values;

    public object?[] Evaluate(object? value)
    {
        var result = new List<object?>();
        foreach (var argument in Values.Invoke())
        {
            var evaluated = argument.Evaluator.Invoke(value);
            if (argument.IsSpread)
                SpreadValues.Append(evaluated, result);
            else
                result.Add(evaluated);
        }
        return result.ToArray();
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

public sealed record ArrayArgumentEvaluator(
    Func<object?, object?> Evaluator,
    bool IsSpread = false);

internal static class SpreadValues
{
    public static void Append(object? value, ICollection<object?> target)
    {
        if (value is null)
            throw new SpreadArgumentException("Spread argument cannot be null.");

        if (value is string || value is not IEnumerable enumerable)
            throw new SpreadArgumentException("Spread argument must evaluate to an array.");

        foreach (var item in enumerable)
            target.Add(item);
    }
}
