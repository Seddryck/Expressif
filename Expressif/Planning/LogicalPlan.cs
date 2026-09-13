using Expressif.Functions.Catalog;

namespace Expressif.Planning;

/// <summary>
/// A language-independent logical representation of an Expressif expression.
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
    ParameterOmissionDocumentation? Omission = null);

/// <summary>
/// The planner-relevant part of a catalog operator.
/// </summary>
public sealed record PlannerFunctionDescriptor(
    string Name,
    string Input,
    string Output,
    FunctionTraversalDocumentation? Traversal = null);

/// <summary>
/// The planner-relevant part of a catalog parameter.
/// </summary>
public sealed record PlannerParameterDescriptor(
    string Name,
    string Type,
    bool Optional,
    bool Variadic,
    int MinimumCardinality,
    ParameterEvaluationDocumentation? Evaluation = null);
