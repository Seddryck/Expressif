using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Functions;

namespace Expressif.Functions.Array;

/// <summary>
/// Returns up to the requested number of elements from the start of the input enumerable.
/// Returns <see langword="null"/> when the input is not an enumerable, is a string, or the count is negative.
/// </summary>
[Function(prefix: "", aliases: ["first"])]
[Scope("array/selection")]
public class FirstElements : BaseArrayFunction
{
    public Func<int> Count { get; }

    /// <param name="count">Number of elements to return from the start of the input.</param>
    public FirstElements(Func<int> count)
        => Count = count;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var count = Count.Invoke();
        if (count < 0)
            return null;

        var output = new List<object?>();
        var index = 0;
        foreach (var item in enumerable!)
        {
            if (index >= count)
                break;

            output.Add(item);
            index++;
        }

        return output.ToArray();
    }
}

/// <summary>
/// Omits the requested number of elements from the start of the input enumerable and returns the remainder.
/// Returns <see langword="null"/> when the input is not an enumerable, is a string, or the count is negative.
/// </summary>
[Function(prefix: "", aliases: ["skip-first"])]
[Scope("array/selection")]
public class SkipFirstElements : BaseArrayFunction
{
    public Func<int> Count { get; }

    /// <param name="count">Number of elements to omit from the start of the input.</param>
    public SkipFirstElements(Func<int> count)
        => Count = count;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var count = Count.Invoke();
        if (count < 0)
            return null;

        var output = new List<object?>();
        var index = 0;
        foreach (var item in enumerable!)
        {
            if (index >= count)
                output.Add(item);

            index++;
        }

        return output.ToArray();
    }
}

/// <summary>
/// Returns up to the requested number of elements from the end of the input enumerable, preserving their order.
/// Returns <see langword="null"/> when the input is not an enumerable, is a string, or the count is negative.
/// </summary>
[Function(prefix: "", aliases: ["last"])]
[Scope("array/selection")]
public class LastElements : BaseArrayFunction
{
    public Func<int> Count { get; }

    /// <param name="count">Number of elements to return from the end of the input.</param>
    public LastElements(Func<int> count)
        => Count = count;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var count = Count.Invoke();
        if (count < 0)
            return null;

        if (count == 0)
            return System.Array.Empty<object?>();

        var buffer = new Queue<object?>();
        foreach (var item in enumerable!)
        {
            if (buffer.Count == count)
                buffer.Dequeue();

            buffer.Enqueue(item);
        }

        return buffer.ToArray();
    }
}

/// <summary>
/// Omits the requested number of elements from the end of the input enumerable and returns the remainder.
/// Returns <see langword="null"/> when the input is not an enumerable, is a string, or the count is negative.
/// </summary>
[Function(prefix: "", aliases: ["skip-last"])]
[Scope("array/selection")]
public class SkipLastElements : BaseArrayFunction
{
    public Func<int> Count { get; }

    /// <param name="count">Number of elements to omit from the end of the input.</param>
    public SkipLastElements(Func<int> count)
        => Count = count;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var count = Count.Invoke();
        if (count < 0)
            return null;

        if (count == 0)
        {
            var passthrough = new List<object?>();
            foreach (var item in enumerable!)
                passthrough.Add(item);

            return passthrough.ToArray();
        }

        var output = new List<object?>();
        var delay = new Queue<object?>();
        foreach (var item in enumerable!)
        {
            delay.Enqueue(item);
            if (delay.Count > count)
                output.Add(delay.Dequeue());
        }

        return output.ToArray();
    }
}

/// <summary>
/// Returns the elements in the zero-based half-open range from <c>start</c>, inclusive, to <c>end</c>, exclusive.
/// Returns <see langword="null"/> when the input is not an enumerable, is a string, or either bound is negative.
/// </summary>
[Function(prefix: "", aliases: ["slice"])]
[Scope("array/selection")]
public class SliceElements : BaseArrayFunction
{
    public Func<int> Start { get; }
    public Func<int> End { get; }

    /// <param name="start">Zero-based index of the first element to return.</param>
    /// <param name="end">Zero-based exclusive index at which to stop returning elements.</param>
    public SliceElements(Func<int> start, Func<int> end)
        => (Start, End) = (start, end);

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var start = Start.Invoke();
        var end = End.Invoke();
        if (start < 0 || end < 0)
            return null;

        if (start >= end)
            return System.Array.Empty<object?>();

        var output = new List<object?>();
        var index = 0;
        foreach (var item in enumerable!)
        {
            if (index >= end)
                break;

            if (index >= start)
                output.Add(item);

            index++;
        }

        return output.ToArray();
    }
}
