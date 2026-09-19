using Expressif.Predicates;

namespace Expressif.Functions.Flow;

/// <summary>Raises an evaluation exception when the input is rejected; otherwise, passes the input through.</summary>
[Function(prefix: "", DynamicReason = "Output preserves the input type when the value is accepted.")]
[Scope("flow")]
public sealed class Throw : IFunction
{
    private readonly Func<IPredicate> predicate;

    /// <summary>Creates a check that rejects null values.</summary>
    public Throw()
        : this(() => new Predicates.Special.Null()) { }

    /// <param name="predicate">Predicate that rejects the input when true; defaults to is-null when omitted.</param>
    public Throw(Func<IPredicate> predicate) => this.predicate = predicate;

    public object? Evaluate(object? value)
    {
        if (ControlFlowBranch.RequireBoolean(EvaluationRuntime.EvaluateNested(predicate.Invoke(), value)))
            throw new EvaluationException();
        return value;
    }
}

public sealed class EvaluationException() : ExpressifException("The value was rejected by throw.");
