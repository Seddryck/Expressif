using Expressif.Predicates;
using System;
using System.Collections.Generic;

namespace Expressif.Functions.Array;

/// <summary>
/// Generates an array by repeatedly transforming a seed while a condition is satisfied.
/// </summary>
[Function(prefix: "", aliases: ["generate"])]
public class Generate : IFunction<object?, object?[]>
{
    public Func<IPredicate> While { get; }
    public Func<IFunction> Next { get; }
    public Func<IFunction>? Result { get; }

    /// <param name="while">Specifies the predicate that determines whether the current seed is included.</param>
    /// <param name="next">Specifies the expression that produces the next seed.</param>
    public Generate(Func<IPredicate> @while, Func<IFunction> next)
        : this(@while, next, null) { }

    /// <param name="while">Specifies the predicate that determines whether the current seed is included.</param>
    /// <param name="next">Specifies the expression that produces the next seed.</param>
    /// <param name="result">Specifies the expression that produces the value appended for the current seed.</param>
    public Generate(Func<IPredicate> @while, Func<IFunction> next, Func<IFunction>? result)
        => (While, Next, Result) = (@while, next, result);

    public object?[] Evaluate(object? value)
    {
        var condition = While.Invoke();
        var next = Next.Invoke();
        var result = Result?.Invoke();
        var output = new List<object?>();
        var seed = value;

        while (condition.Evaluate(seed))
        {
            output.Add(result is null ? seed : result.Evaluate(seed));
            seed = next.Evaluate(seed);
        }

        return output.ToArray();
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
