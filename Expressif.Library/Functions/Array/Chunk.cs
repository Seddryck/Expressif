using System;
using System.Collections;
using System.Collections.Generic;

namespace Expressif.Functions.Array;

/// <summary>
/// Splits an array into consecutive, non-overlapping chunks of at most the specified size, preserving a final partial chunk. It resembles a count-based tumbling window but, unlike general sliding or hopping windows, has no separate step and always keeps the final partial chunk. It does not group items by inactivity or time. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["chunk"])]
[Scope("array/partitioning")]
public class Chunk : BaseArrayFunction
{
    public Func<int> Size { get; }

    /// <param name="size">The strictly positive number of items in each chunk.</param>
    public Chunk(Func<int> size)
        => Size = size;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var size = Size.Invoke();
        if (size <= 0)
            return null;

        var chunks = new List<object?[]>();
        var chunk = new List<object?>(size);
        foreach (var item in enumerable)
        {
            chunk.Add(item);
            if (chunk.Count != size)
                continue;

            chunks.Add(chunk.ToArray());
            chunk = new List<object?>(size);
        }

        if (chunk.Count > 0)
            chunks.Add(chunk.ToArray());

        return chunks.ToArray();
    }
}
