using System;
using System.Collections;
using System.Collections.Generic;
using Expressif.Functions;

namespace Expressif.Functions.Array;

/// <summary>
/// Evaluates an expression once for every supplied value while preserving the pipeline input as the expression input.
/// Tuple values are expanded into positional arguments for a bare callable. Returns `null` when values is not enumerable or is text.
/// </summary>
[Function(prefix: "", aliases: [])]
public sealed class MapOver : DirectionalMap
{
    /// <param name="expression">Expression evaluated with the outer pipeline input and each supplied value as its argument context.</param>
    /// <param name="values">Values iterated as argument contexts in declaration order.</param>
    public MapOver(Func<IFunction> expression, Func<IEnumerable?> values)
        : base(expression, values) { }
}

/// <summary>
/// Evaluates an expression once for every supplied value, using that value as the pipeline input and the outer input as its argument.
/// Tuple values remain ordinary pipeline values and are not expanded. Returns `null` when values is not enumerable or is text.
/// </summary>
[Function(prefix: "", aliases: [])]
public sealed class MapWith : DirectionalMap
{
    /// <param name="expression">Expression evaluated with each supplied value as input and the outer pipeline input as its argument.</param>
    /// <param name="values">Values iterated as pipeline inputs in declaration order.</param>
    public MapWith(Func<IFunction> expression, Func<IEnumerable?> values)
        : base(expression, values) { }
}

public abstract class DirectionalMap : IFunction<object?, IEnumerable?>
{
    private Func<IFunction> Expression { get; }
    private Func<IEnumerable?> Values { get; }

    private protected DirectionalMap(Func<IFunction> expression, Func<IEnumerable?> values)
        => (Expression, Values) = (expression, values);

    public IEnumerable? Evaluate(object? value)
    {
        var values = Values.Invoke();
        if (values is null || values is string)
            return null;

        var expression = Expression.Invoke();
        var output = new List<object?>();
        foreach (var item in values)
            output.Add(expression.Evaluate(new DirectionalMapInput(value, item)));
        return output.ToArray();
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

internal sealed record DirectionalMapInput(object? Outer, object? Item);
