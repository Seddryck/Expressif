using System;
using System.Collections;
using System.Collections.Generic;

namespace Expressif.Functions.Array;

/// <summary>
/// Distributes successive array values cyclically among a requested number of output arrays. Returns `null` when the count is not strictly positive or the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/partitioning")]
public sealed class DistributeRoundRobin : BaseArrayFunction
{
    public Func<int> Count { get; }

    /// <param name="count">Specifies the strictly positive number of output arrays.</param>
    public DistributeRoundRobin(Func<int> count)
        => Count = count;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var count = Count.Invoke();
        if (enumerable is null || count <= 0)
            return null;

        var outputs = new List<object?>[count];
        for (var index = 0; index < outputs.Length; index++)
            outputs[index] = [];

        var outputIndex = 0;
        foreach (var item in enumerable)
        {
            outputs[outputIndex].Add(item);
            outputIndex = outputIndex == count - 1 ? 0 : outputIndex + 1;
        }

        var result = new object?[count][];
        for (var index = 0; index < outputs.Length; index++)
            result[index] = outputs[index].ToArray();

        return result;
    }
}
