namespace Expressif.Planning;

/// <summary>
/// Discovers input requirements and output schema from a portable logical plan without executing it.
/// </summary>
public static class LogicalSchemaAnalyzer
{
    public static AnalyzedLogicalPlan AnalyzePlan(LogicalPlan plan, LogicalSchema? declaredInput = null)
        => new(plan, Analyze(plan, declaredInput));

    public static SchemaAnalysis Analyze(LogicalPlan plan, LogicalSchema? declaredInput = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        LogicalNamedExpressionValidator.Validate(plan);
        var session = new LogicalSchemaAnalysisSession(plan.Definitions);
        for (var index = 0; index < plan.Definitions.Count; index++)
            session.AnalyzeDefinition(plan.Definitions[index], $"definitions[{index}]");
        var requirement = session.RequirePipeline(plan.Pipeline, new AnyLogicalSchema(), "plan");
        var input = declaredInput is null
            ? requirement
            : session.Intersect(declaredInput, requirement, "plan.input");
        var output = session.InferPipeline(plan.Pipeline, input, input, "plan");
        var completeness = session.Completeness(input, output);
        return new SchemaAnalysis(input, output, completeness, session.Diagnostics, session.Nodes);
    }
}
