using Expressif.Values;

namespace Expressif.Functions.Sorting;

/// <summary>Creates a non-empty ordered sort key from one or more sort terms.</summary>
[Function(prefix: "")]
[Scope("sorting")]
public sealed class SortKey : IFunction<object?, SortKeyValue>, IValueSpreadAware
{
    private readonly Func<ValueArgumentEvaluator[]> values;

    /// <param name="values">One or more sort terms in lexicographic comparison order.</param>
    public SortKey(Func<ValueArgumentEvaluator[]> values) => this.values = values;

    public SortKeyValue Evaluate(object? input)
    {
        var evaluated = ValueArguments.Evaluate(values.Invoke(), input).ToArray();
        if (evaluated.Length == 0)
            throw new ArgumentException("SortKey requires at least one SortTerm.");
        if (evaluated.Any(value => value is not SortTermValue))
            throw new ArgumentException("SortKey values must all be SortTerm values.");
        return new Values.SortKey(evaluated.Cast<SortTermValue>().ToArray());
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
