namespace Expressif.Planning;

/// <summary>
/// Makes statically resolvable implicit conversions explicit in a logical plan.
/// </summary>
internal sealed class LogicalPlanCoercionNormalizer(ILogicalPlanningContext context)
{
    public LogicalPipeline Normalize(LogicalPipeline pipeline)
    {
        var normalized = new List<LogicalValue>();
        string? sourceType = null;
        foreach (var item in pipeline.Items.Select(Normalize))
        {
            if (sourceType is not null
                && item is LogicalCall call
                && Coercion(sourceType, call.Function.Input) is { } coercion)
            {
                normalized.Add(coercion);
            }

            normalized.Add(item);
            sourceType = OutputType(item);
        }

        return pipeline with { Items = normalized };
    }

    private LogicalValue Normalize(LogicalValue value) => value switch
    {
        LogicalPipeline pipeline => Normalize(pipeline),
        LogicalCall call => Normalize(call),
        _ => value,
    };

    private LogicalCall Normalize(LogicalCall call)
        => call with
        {
            Arguments = call.Arguments.Select(Normalize).ToArray(),
        };

    private LogicalArgument Normalize(LogicalArgument argument)
    {
        if (!argument.IsExplicit || argument.Value is null)
            return argument;

        return argument with { Value = Normalize(argument.Value) };
    }

    private LogicalCall? Coercion(string sourceType, string targetType)
    {
        if (sourceType.Equals(targetType, StringComparison.OrdinalIgnoreCase))
            return null;

        var metadata = context.FindCoercion(sourceType, targetType);
        return metadata is null ? null : new LogicalCall(metadata.Function, []);
    }

    private static string? OutputType(LogicalValue value) => value switch
    {
        LogicalLiteral literal => literal.Type,
        LogicalCall call => call.Function.Output,
        LogicalPipeline { Items.Count: > 0 } pipeline => OutputType(pipeline.Items[^1]),
        _ => null,
    };
}
