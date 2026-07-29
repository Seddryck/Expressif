using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Functions;

namespace Expressif.Functions.Array;

[Function(prefix: "", aliases: ["first"])]
public class FirstElements : BaseArrayFunction
{
    public Func<int> Count { get; }

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

[Function(prefix: "", aliases: ["skip-first"])]
public class SkipFirstElements : BaseArrayFunction
{
    public Func<int> Count { get; }

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

[Function(prefix: "", aliases: ["last"])]
public class LastElements : BaseArrayFunction
{
    public Func<int> Count { get; }

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

[Function(prefix: "", aliases: ["skip-last"])]
public class SkipLastElements : BaseArrayFunction
{
    public Func<int> Count { get; }

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

[Function(prefix: "", aliases: ["slice"])]
public class SliceElements : BaseArrayFunction
{
    public Func<int> Start { get; }
    public Func<int> End { get; }

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
