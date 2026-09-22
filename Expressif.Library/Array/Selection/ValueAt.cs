using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Library.Array;

namespace Expressif.Library.Array.Selection;

/// <summary>
/// Returns the input item at the specified zero-based position. Returns `null` when the position is negative or out of range, or the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["value-at"])]
[Scope("array/selection")]
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
