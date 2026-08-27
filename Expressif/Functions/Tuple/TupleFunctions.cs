using System;
using Expressif.Values;

namespace Expressif.Functions.Tuple;

/// <summary>
/// Returns the number of positional elements in the input tuple.
/// </summary>
[Function(prefix: "", aliases: ["arity"])]
[Scope("tuple")]
public sealed class Arity : IFunction<TupleValue, int>
{
    public int Evaluate(TupleValue value) => value.Count;

    object? IFunction.Evaluate(object? value) => value is TupleValue tuple ? Evaluate(tuple) : null;
}

/// <summary>Returns a tuple with two positions exchanged, defaulting to the first and last positions.</summary>
[Function(prefix: "", aliases: ["swap"])]
[Scope("tuple")]
public sealed class Swap : IFunction<TupleValue, TupleValue>
{
    private Func<int>? First { get; }
    private Func<int>? Second { get; }
    public Swap() { }
    /// <param name="first">Specifies the first zero-based position.</param>
    /// <param name="second">Specifies the second zero-based position.</param>
    public Swap(Func<int> first, Func<int> second) => (First, Second) = (first, second);
    public TupleValue Evaluate(TupleValue value)
    {
        if (value.Count == 0) return new Expressif.Values.Tuple();
        var first = First?.Invoke() ?? 0;
        var second = Second?.Invoke() ?? value.Count - 1;
        if (first < 0 || first >= value.Count || second < 0 || second >= value.Count)
            throw new IndexOutOfRangeException("Tuple swap position is out of range.");
        var values = value.ToArray();
        (values[first], values[second]) = (values[second], values[first]);
        return new Expressif.Values.Tuple(values);
    }
    object? IFunction.Evaluate(object? value) => value is TupleValue tuple ? Evaluate(tuple) : null;
}

/// <summary>Returns a new tuple with a value appended, expanding tuple values into their positions.</summary>
[Function(prefix: "", aliases: ["extend"])]
[Scope("tuple")]
public sealed class Extend : IFunction<TupleValue, TupleValue>
{
    private Func<TupleValue, object?> Extension { get; }
    /// <param name="value">Specifies the value to append; tuple values are expanded into their positions.</param>
    public Extend(Func<TupleValue, object?> value) => Extension = value;
    public TupleValue Evaluate(TupleValue value)
    {
        var extension = Extension.Invoke(value);
        return extension is TupleValue tuple
            ? new Expressif.Values.Tuple(value.Concat(tuple).ToArray())
            : new Expressif.Values.Tuple(value.Append(extension).ToArray());
    }
    object? IFunction.Evaluate(object? value) => value is TupleValue tuple ? Evaluate(tuple) : null;
}

/// <summary>
/// Returns the tuple field at the specified zero-based position. Returns `null` when the input is not a tuple or the position is out of range.
/// </summary>
[Function(prefix: "", aliases: ["tuple-at"])]
[Scope("tuple")]
public class TupleAt : IFunction<TupleValue, object?>
{
    public Func<int> Position { get; }

    /// <param name="position">Specifies the zero-based position of the tuple field to return.</param>
    public TupleAt(Func<int> position)
        => Position = position;

    public object? Evaluate(object? value)
    {
        var position = Position.Invoke();
        var index = position == int.MinValue ? -1 : position < 0 ? tupleIndexFromEnd(position) : position;
        return value is TupleValue tuple && index >= 0 && index < tuple.Count
            ? tuple[index]
            : null;

        int tupleIndexFromEnd(int offset)
            => value is TupleValue tuple ? tuple.Count + offset : -1;
    }

    public object? Evaluate(TupleValue value) => Evaluate((object?)value);
}

/// <summary>
/// Returns the first field of a tuple. Returns `null` when the input is not a tuple.
/// </summary>
[Function(prefix: "", aliases: ["tuple-first"])]
[Scope("tuple")]
public class TupleFirst : TupleAt
{
    public TupleFirst()
        : base(() => 0) { }
}

/// <summary>
/// Returns the second field of a tuple. Returns `null` when the input is not a tuple.
/// </summary>
[Function(prefix: "", aliases: ["tuple-second"])]
[Scope("tuple")]
public class TupleSecond : TupleAt
{
    public TupleSecond()
        : base(() => 1) { }
}
