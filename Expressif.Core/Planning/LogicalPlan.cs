using System.Text.Json;

namespace Expressif.Planning;

/// <summary>
/// A portable logical representation of an Expressif expression.
/// </summary>
public sealed record LogicalPlan(LogicalPipeline Pipeline);

/// <summary>
/// A value that can appear in a logical plan.
/// </summary>
public abstract record LogicalValue;

/// <summary>
/// A sequence whose output flows from one item to the next.
/// </summary>
public sealed record LogicalPipeline(IReadOnlyList<LogicalValue> Items) : LogicalValue;

/// <summary>
/// A canonical operator invocation.
/// </summary>
public sealed record LogicalCall(
    PlannerFunctionDescriptor Function,
    IReadOnlyList<LogicalArgument> Arguments,
    int ContextDepth = 0) : LogicalValue;

/// <summary>
/// A scalar value identified by its Expressif semantic type.
/// </summary>
public sealed record LogicalLiteral(string Type, object? Value) : LogicalValue;

/// <summary>
/// An argument associated with its canonical catalog parameter.
/// </summary>
public sealed record LogicalArgument(
    PlannerParameterDescriptor Parameter,
    LogicalValue? Value,
    bool IsSpread,
    bool IsExplicit,
    PlannerOmissionDescriptor? Omission = null);

/// <summary>
/// Describes how an omitted argument obtains its value.
/// </summary>
public sealed record PlannerOmissionDescriptor(
    PlannerOmissionMode Mode,
    JsonElement Value = default,
    string? Source = null);

/// <summary>
/// Identifies the source of an omitted argument value.
/// </summary>
public enum PlannerOmissionMode
{
    Constant,
    EmptyVariadic,
    Absent,
    EnvironmentDerived,
}

/// <summary>
/// The planner-relevant part of a catalog operator.
/// </summary>
public sealed record PlannerFunctionDescriptor(
    string Name,
    string Input,
    string Output,
    PlannerTraversalDescriptor? Traversal = null,
    PlannerSemanticsDescriptor? Semantics = null);

/// <summary>
/// The machine-readable traversal contract of a planned operator.
/// </summary>
public sealed record PlannerTraversalDescriptor(string Source, string Selection);

/// <summary>
/// The machine-readable structural effect of a planned operator.
/// </summary>
public sealed record PlannerSemanticsDescriptor(string Cardinality, string Dependency, string Ordering);

/// <summary>
/// The planner-relevant part of a catalog parameter.
/// </summary>
public sealed record PlannerParameterDescriptor(
    string Name,
    string Type,
    bool Optional,
    bool Variadic,
    int MinimumCardinality,
    PlannerEvaluationDescriptor? Evaluation = null);

/// <summary>
/// The machine-readable evaluation contract of a planned argument.
/// </summary>
public sealed record PlannerEvaluationDescriptor(
    string Frequency,
    string? Source = null,
    string? Context = null);
