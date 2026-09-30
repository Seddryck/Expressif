using Expressif.Values;

namespace Expressif.Library.Sorting;

/// <summary>Creates a non-empty ordered sort key from one or more sort terms.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class SortKey : IFunction<object?, SortKeyValue>
{
    private readonly Func<object?, object?[]> values;

    /// <param name="values">One or more sort terms in lexicographic comparison order.</param>
    public SortKey([ArgumentPacking(ArgumentPackingMode.Variadic, AllowSpread = true)] Func<object?, object?[]> values) => this.values = values;

    public SortKeyValue Evaluate(object? input)
    {
        var evaluated = values.Invoke(input);
        if (evaluated.Length == 0)
            throw new ArgumentException("SortKey requires at least one SortTerm.");
        if (evaluated.Any(value => value is not SortTermValue))
            throw new ArgumentException("SortKey values must all be SortTerm values.");
        return new Values.SortKey(evaluated.Cast<SortTermValue>().ToArray());
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
