namespace Expressif.Functions.Flow;

public sealed record ControlFlowBranch(Func<object?, object?> Expression, Func<object?, object?>? Predicate)
{
    internal bool Accepts(object? value)
        => Predicate is null || RequireBoolean(Predicate.Invoke(value));

    internal static bool RequireBoolean(object? value)
        => value is bool result ? result : throw new PredicateEvaluationException();
}

public sealed class PredicateEvaluationException()
    : Exception("A control-flow predicate must return a Boolean value.");
