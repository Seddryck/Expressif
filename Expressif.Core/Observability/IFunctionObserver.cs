using Expressif.Discovery;

namespace Expressif.Observability;

/// <summary>
/// Passively observes values crossing bound-function boundaries.
/// </summary>
/// <remarks>
/// Observers may be shared by concurrent evaluations and must therefore be thread-safe. Callbacks
/// run after the function has returned or thrown, in registration order. Exceptions thrown by an
/// observer are isolated and never replace the authoritative function result or exception.
/// Implementations must not mutate observed values. Inputs and outputs can contain sensitive data;
/// observers are responsible for appropriate access, retention, and disposal policies.
/// </remarks>
public interface IFunctionObserver
{
    void OnCompleted(FunctionObservationContext context, object? input, object? output);

    void OnFailed(FunctionObservationContext context, object? input, Exception exception);
}

/// <summary>
/// Identifies one function node in a bound expression.
/// </summary>
public sealed class FunctionObservationContext
{
    internal FunctionObservationContext(string id, OperatorIdentity function)
        => (Id, Function) = (id, function);

    public string Id { get; }

    public OperatorIdentity Function { get; }

    public string Name => Function.Name;

    public string Namespace => Function.Namespace;

    public string CanonicalName => Function.CanonicalName;
}
