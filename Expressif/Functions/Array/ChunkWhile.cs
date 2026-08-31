using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Values;

namespace Expressif.Functions.Array;

/// <summary>
/// Groups consecutive values while an operation over each previous and current pair evaluates to `true`. Returns `null` when the operation does not produce a Boolean value or the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: [])]
[Scope("array/partitioning")]
public sealed class ChunkWhile : BaseArrayFunction
{
    public Func<IFunction> Operation { get; }

    /// <param name="operation">Specifies the callable or open expression that decides whether the current value continues the preceding chunk.</param>
    public ChunkWhile(Func<IFunction> operation)
        => Operation = operation;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var chunks = new List<object?[]>();
        var chunk = new List<object?>();
        var enumerator = enumerable.GetEnumerator();
        if (!enumerator.MoveNext())
            return chunks.ToArray();

        var previous = enumerator.Current;
        chunk.Add(previous);

        if (!enumerator.MoveNext())
        {
            chunks.Add(chunk.ToArray());
            return chunks.ToArray();
        }

        var operation = Operation.Invoke();

        do
        {
            var current = enumerator.Current;
            if (operation.Evaluate(new Values.Tuple(previous, current)) is not bool continues)
                return null;

            if (!continues)
            {
                chunks.Add(chunk.ToArray());
                chunk = [];
            }

            chunk.Add(current);
            previous = current;
        }
        while (enumerator.MoveNext());

        chunks.Add(chunk.ToArray());
        return chunks.ToArray();
    }
}
