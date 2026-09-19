using Expressif.Functions;

namespace Expressif.Predicates;

internal sealed class BooleanFunctionPredicate(IFunction function, bool preserveCurrentInput = false) : IPredicate, IInputBoundFunction
{
    public bool IsInputBound => function is IInputBoundFunction { IsInputBound: true };

    public bool Evaluate(object? value)
    {
        var result = EvaluationRuntime.EvaluateNested(function, value, preserveCurrentInput ? EvaluationRuntime.Frame?.Current : value);
        return result is bool boolean
            ? boolean
            : throw new InvalidCastException(
                $"A predicate expression must return a Boolean value, but returned '{result?.GetType().Name ?? "null"}'.");
    }

    object? IFunction.Evaluate(object? value) => Evaluate(value);
}
