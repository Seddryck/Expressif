using Expressif.Values;
using GroupingValue = Expressif.Values.Grouping;

namespace Expressif.Library.Grouping;

/// <summary>Constructs a grouping from zero or more pairs. Spread arguments expand arrays of pairs in place.</summary>
[Function(prefix: "")]
[Scope("grouping")]
public sealed class Grouping : IFunction<object?, GroupingValue>
{
    private Func<object?, object?[]> Values { get; }

    /// <summary>Creates an empty grouping constructor.</summary>
    public Grouping()
        : this(_ => []) { }

    /// <param name="values">Zero or more pairs whose keys and grouped value collections become groups.</param>
    public Grouping([ArgumentPacking(ArgumentPackingMode.Variadic, AllowSpread = true)] [ArgumentOmission(ArgumentOmissionMode.EmptyVariadic)] Func<object?, object?[]> values)
        => Values = values;

    public GroupingValue Evaluate(object? value)
    {
        var evaluated = Values.Invoke(value);
        if (evaluated.Any(item => item is not PairValue))
            throw new ArgumentException("Every grouping argument must evaluate to a pair.", nameof(value));
        return new GroupingValue(evaluated.Cast<PairValue>());
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
