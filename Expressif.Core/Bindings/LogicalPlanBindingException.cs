namespace Expressif.Bindings;

/// <summary>
/// Represents a failure to bind a logical plan to an executable expression.
/// </summary>
public sealed class LogicalPlanBindingException : Exception
{
    public LogicalPlanBindingException(string message)
        : base(message) { }

    public LogicalPlanBindingException(string message, Exception innerException)
        : base(message, innerException) { }
}
