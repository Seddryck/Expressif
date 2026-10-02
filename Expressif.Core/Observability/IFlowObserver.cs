namespace Expressif.Observability;

/// <summary>Observes semantic decisions made by flow functions.</summary>
public interface IFlowObserver
{
    void OnDecision(FunctionObservationContext function, FlowDecision decision);
}

/// <summary>Describes one flow decision without exposing evaluated values.</summary>
public readonly struct FlowDecision
{
    public FlowDecision(
        FlowDecisionOutcome outcome,
        int index = -1,
        int evaluated = 0,
        bool isFallback = false)
    {
        Outcome = outcome;
        Index = index;
        Evaluated = evaluated;
        IsFallback = isFallback;
    }

    public FlowDecisionOutcome Outcome { get; }

    public int Index { get; }

    public int Evaluated { get; }

    public bool IsFallback { get; }
}

public enum FlowDecisionOutcome
{
    PassThrough,
    Recovery,
    ExpressionSelected,
    OriginalInputRetained,
    CandidateSelected,
    AllCandidatesNull,
    BranchSelected,
    NoBranchMatched,
    NoCandidateAccepted,
    GuardedExpressionSelected,
    IncompatibleInputRetained,
    InputAccepted,
    InputRejected,
}
