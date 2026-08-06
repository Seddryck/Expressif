using System;
using System.Collections;
using System.Collections.Generic;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns each input item paired with its zero-based position as a tuple in `(position, value)` order. Preserves input order and cardinality. Position terminology distinguishes sequence locations from indexes used to accelerate searches. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["with-position"])]
[Scope("array")]
public sealed class WithPosition : BaseArrayFunction
{
    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        return Enumerate(enumerable);
    }

    private static IEnumerable<Expressif.Values.Tuple> Enumerate(IEnumerable source)
    {
        var position = 0;
        foreach (var item in source)
            yield return new Expressif.Values.Tuple(position++, item);
    }
}

/// <summary>
/// Returns the zero-based position of the first input item equal to the specified value. Returns `null` when no item matches or the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["position-of"])]
[Scope("array")]
public sealed class PositionOf : BaseArrayFunction<int>
{
    public Func<object?> Value { get; }

    /// <param name="value">Specifies the value to locate.</param>
    public PositionOf(Func<object?> value)
        => Value = value;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var value = Value.Invoke();
        var position = 0;
        foreach (var item in enumerable)
        {
            if (EqualityComparer<object?>.Default.Equals(item, value))
                return position;

            position++;
        }

        return null;
    }
}

/// <summary>
/// Returns the input item at the specified zero-based position. Returns `null` when the position is negative or out of range, or the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["value-at"])]
[Scope("array")]
public sealed class ValueAt : BaseArrayFunction<object>
{
    public Func<int> Position { get; }

    /// <param name="position">Specifies the zero-based position of the item to return.</param>
    public ValueAt(Func<int> position)
        => Position = position;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var requestedPosition = Position.Invoke();
        if (requestedPosition < 0)
            return null;

        var position = 0;
        foreach (var item in enumerable)
        {
            if (position++ == requestedPosition)
                return item;
        }

        return null;
    }
}
