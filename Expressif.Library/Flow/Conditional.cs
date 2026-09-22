namespace Expressif.Library.Flow;

[Function(prefix: "", aliases: ["conditional-backward"], Name = "conditional-forward")]
internal sealed class ConditionalForward(
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
