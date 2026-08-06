using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Values;

namespace Expressif.Functions.Array;

/// <summary>
/// Evaluates an operation against every consecutive pair of input values. Returns `null` when the input cannot be evaluated.
/// </summary>
[Function(prefix: "", aliases: ["adjacent"])]
public class Adjacent : BaseArrayFunction
{
    public Func<IFunction> Operation { get; }

    /// <param name="operation">Specifies the callable or open expression evaluated against each consecutive pair.</param>
    public Adjacent(Func<IFunction> operation)
        => Operation = operation;

    protected override object? EvaluateArray(IEnumerable enumerable)
    {
        var pairs = new Pairwise().Evaluate(enumerable) as IEnumerable;
        return pairs is null ? null : Enumerate(pairs, Operation.Invoke());
    }

    private static IEnumerable<object?> Enumerate(IEnumerable pairs, IFunction operation)
    {
        foreach (var pair in pairs)
            yield return operation.Evaluate(pair);
    }
}
