using System;
using System.Collections;
using System.Linq;
using Expressif.Values;

namespace Expressif.Functions.Array;

/// <summary>
/// Splits an array on a zero-based boundary and returns the elements before and from that position as a tuple. Returns `null` when the position is invalid or the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/partitioning")]
public sealed class ChunkOn : BaseArrayFunction<TupleValue>
{
    public Func<int> Position { get; }

    /// <param name="position">The zero-based boundary position; the element at this position belongs to the right chunk.</param>
    public ChunkOn(Func<int> position)
        => Position = position;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var values = enumerable.Cast<object?>().ToArray();
        var position = Position.Invoke();
        if (position < 0 || position > values.Length)
            return null;

        return new Values.Tuple(values[..position], values[position..]);
    }
}

/// <summary>
/// Separates the element at a zero-based position from the elements before and after it, returning the three parts as a tuple. Returns `null` when the position is invalid or the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/partitioning")]
public sealed class ChunkAround : BaseArrayFunction<TupleValue>
{
    public Func<int> Position { get; }

    /// <param name="position">The zero-based position of the element to separate.</param>
    public ChunkAround(Func<int> position)
        => Position = position;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var values = enumerable.Cast<object?>().ToArray();
        var position = Position.Invoke();
        if (position < 0 || position >= values.Length)
            return null;

        return new Values.Tuple(values[..position], values[position], values[(position + 1)..]);
    }
}
