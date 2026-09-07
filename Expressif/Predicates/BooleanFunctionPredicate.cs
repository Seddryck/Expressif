using Expressif.Functions;

namespace Expressif.Predicates;

internal sealed class BooleanFunctionPredicate(IFunction function, bool preserveCurrentInput = false) : IPredicate
{
    public bool Evaluate(object? value)
    {
        using var scope = EvaluationRuntime.Derive(value, preserveCurrentInput ? EvaluationRuntime.Frame?.Current : value);
        var result = function.Evaluate(value);
        return result is bool boolean
            ? boolean
            : throw new InvalidCastException(
                $"A predicate expression must return a Boolean value, but returned '{result?.GetType().Name ?? "null"}'.");
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
