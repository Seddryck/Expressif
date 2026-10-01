namespace Expressif.Planning;

/// <summary>
/// Supplies vocabulary metadata to the Core logical planner.
/// </summary>
public interface ILogicalPlanningContext
{
    PlannerFunctionMetadata? FindFunction(
        string name,
        string? expectedKind = null,
        int? argumentCount = null);

    string? FindType(string name);
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
