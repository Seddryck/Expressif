using Expressif.Values;
using PairValueType = Expressif.Values.Pair;

namespace Expressif.Library.Pair;

/// <summary>
/// Constructs a pair by evaluating a key expression and a value expression against the same input.
/// </summary>
[Function(prefix: "")]
[Scope("pair")]
public sealed class Pair : IFunction<object?, PairValueType>
{
    private Func<object?, object?> Key { get; }
    private Func<object?, object?> Value { get; }

    /// <param name="key">The expression whose evaluated result becomes the key.</param>
    /// <param name="value">The expression whose evaluated result becomes the value.</param>
    public Pair(
        [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] Func<object?, object?> key,
        [ArgumentEvaluation(ArgumentEvaluationMode.Incoming)] Func<object?, object?> value)
        => (Key, Value) = (key, value);

    public PairValueType Evaluate(object? input)
        => new Expressif.Values.Pair(Key.Invoke(input), Value.Invoke(input));

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}

/// <summary>Returns the key component of the input pair.</summary>
[Function(prefix: "")]
[Scope("pair")]
public sealed class PairKey : IFunction<PairValueType, object?>
{
    public object? Evaluate(PairValueType value) => value.Key;
    object? IFunction.Evaluate(object? value) => value switch
    {
        PairValueType pair => Evaluate(pair),
        Group group => group.Key,
        _ => null,
    };
}

/// <summary>Returns the value component of the input pair.</summary>
[Function(prefix: "")]
[Scope("pair")]
public sealed class PairValue : IFunction<PairValueType, object?>
{
    public object? Evaluate(PairValueType value) => value.Value;
    object? IFunction.Evaluate(object? value) => value switch
    {
        PairValueType pair => Evaluate(pair),
        Group group => group.Value,
        _ => null,
    };
}
