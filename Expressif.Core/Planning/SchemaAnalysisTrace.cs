namespace Expressif.Planning;

internal sealed class SchemaAnalysisTrace
{
    private readonly Dictionary<string, SchemaAnalysisNode> nodes = new(StringComparer.Ordinal);

    public IReadOnlyList<SchemaAnalysisNode> GetNodes()
        => nodes.Values.OrderBy(node => node.Path, StringComparer.Ordinal).ToArray();

    public void Capture(LogicalValue value, string path, LogicalSchema input, LogicalSchema output)
    {
        var (kind, operation) = value switch
        {
            LogicalPipeline => ("pipeline", null),
            LogicalCall call => ("call", call.Function.Name),
            LogicalLiteral => ("literal", null),
            LogicalNamedExpressionInvocation invocation => ("named-expression-invocation", invocation.Name),
            _ => (value.GetType().Name, null),
        };
        nodes[path] = new SchemaAnalysisNode(path, kind, operation, input, output);
    }
}
