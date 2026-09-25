using Expressif.Bindings;
using Expressif.Syntax;

namespace Expressif.Planning;

/// <summary>
/// Supplies syntax binding and vocabulary metadata to the Core logical planner.
/// </summary>
public interface ILogicalPlanningContext
{
    IRootExpression Bind(RootExpressionSyntax syntax);

    PlannerFunctionMetadata? FindFunction(string name);

    string? FindTypeName(Type runtimeType);
}

/// <summary>
/// Describes an operator and its parameters for logical planning.
/// </summary>
public sealed record PlannerFunctionMetadata(
    PlannerFunctionDescriptor Function,
    IReadOnlyList<PlannerParameterMetadata> Parameters);

/// <summary>
/// Describes one operator parameter and its omission behavior.
/// </summary>
public sealed record PlannerParameterMetadata(
    PlannerParameterDescriptor Descriptor,
    PlannerOmissionDescriptor? Omission = null);
