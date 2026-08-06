using System;
using Expressif.Values;

namespace Expressif.Functions.Special;

/// <summary>
/// Returns the tuple field at the specified zero-based position. Returns `null` when the input is not a tuple or the position is out of range.
/// </summary>
[Function(prefix: "", aliases: ["tuple-at"])]
public class TupleAt : IFunction
{
    public Func<int> Position { get; }

    /// <param name="position">Specifies the zero-based position of the tuple field to return.</param>
    public TupleAt(Func<int> position)
        => Position = position;

    public object? Evaluate(object? value)
    {
        var position = Position.Invoke();
        return value is TupleValue tuple && position >= 0 && position < tuple.Count
            ? tuple[position]
            : null;
    }
}

/// <summary>
/// Returns the first field of a tuple. Returns `null` when the input is not a tuple.
/// </summary>
[Function(prefix: "", aliases: ["tuple-first"])]
public class TupleFirst : TupleAt
{
    public TupleFirst() : base(() => 0) { }
}

/// <summary>
/// Returns the second field of a tuple. Returns `null` when the input is not a tuple.
/// </summary>
[Function(prefix: "", aliases: ["tuple-second"])]
public class TupleSecond : TupleAt
{
    public TupleSecond() : base(() => 1) { }
}
