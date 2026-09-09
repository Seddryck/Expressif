namespace Expressif.Functions.Flow;

internal sealed class Conditional(
    Func<object?, object?> expression,
    Func<object?, object?> predicate,
    bool testCandidate) : IFunction
{
    public object? Evaluate(object? value)
    {
        if (!testCandidate)
            return ControlFlowBranch.RequireBoolean(predicate.Invoke(value)) ? expression.Invoke(value) : value;
        var candidate = expression.Invoke(value);
        return ControlFlowBranch.RequireBoolean(predicate.Invoke(candidate)) ? candidate : value;
    }
}
