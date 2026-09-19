using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Functions.Grouping;

/// <summary>Constructs a grouping from zero or more pairs. Spread arguments expand arrays of pairs in place.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class Grouping : IFunction<object?, GroupingValue>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Values { get; }

    /// <summary>Creates an empty grouping constructor.</summary>
    public Grouping()
        : this(() => []) { }

    /// <param name="values">Zero or more pairs whose keys and grouped value collections become groups.</param>
    public Grouping(Func<ValueArgumentEvaluator[]> values)
        => Values = values;

    public GroupingValue Evaluate(object? value)
    {
        var evaluated = ValueArguments.Evaluate(Values.Invoke(), value).ToArray();
        if (evaluated.Any(item => item is not PairValue))
            throw new ArgumentException("Every grouping argument must evaluate to a pair.", nameof(value));
        return new GroupingValue(evaluated.Cast<PairValue>());
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
