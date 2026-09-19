using Expressif.Values;
using DictionaryValueType = Expressif.Values.DictionaryValue;

namespace Expressif.Functions.Dictionary;

/// <summary>Constructs a dictionary from zero or more pairs. Spread arguments expand arrays of pairs in place.</summary>
[Function(prefix: "")]
[Scope("dictionary")]
public sealed class Dictionary : IFunction<object?, DictionaryValueType>, IValueSpreadAware
{
    private Func<ValueArgumentEvaluator[]> Values { get; }

    /// <summary>Creates an empty dictionary constructor.</summary>
    public Dictionary()
        : this(() => []) { }

    /// <param name="values">Zero or more pairs whose unique keys and values become dictionary entries.</param>
    public Dictionary(Func<ValueArgumentEvaluator[]> values) => Values = values;

    public DictionaryValueType Evaluate(object? value)
    {
        var evaluated = ValueArguments.Evaluate(Values.Invoke(), value).ToArray();
        if (evaluated.Any(item => item is not PairValue))
            throw new ArgumentException("Every dictionary argument must evaluate to a pair.", nameof(value));
        return new Expressif.Values.Dictionary(evaluated.Cast<PairValue>());
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
