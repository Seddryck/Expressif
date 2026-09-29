using System.Globalization;
using Expressif.Planning;

namespace Expressif.Cli.Commands;

internal static class LogicalPlanFormatter
{
    public static string Format(LogicalPlan plan)
        => TreeDocumentFormatter.Format(ToDocument(plan.Pipeline, "plan"), "tree");

    public static string Format(AnalyzedLogicalPlan analyzed)
    {
        var annotations = analyzed.Analysis.Nodes.ToDictionary(node => node.Path, StringComparer.Ordinal);
        var document = ToDocument(analyzed.Plan.Pipeline, "plan", annotations);
        return TreeDocumentFormatter.Format(new TreeDocument(
            $"{document.Label} ({analyzed.Analysis.Completeness.ToString().ToLowerInvariant()})",
            document.Properties,
            [.. document.Children, .. Diagnostics(analyzed.Analysis.Diagnostics)]), "tree");
    }

    private static TreeDocument ToDocument(
        LogicalValue value,
        string path,
        IReadOnlyDictionary<string, SchemaAnalysisNode>? annotations = null)
    {
        var annotation = Annotation(path, annotations);
        return value switch
        {
            LogicalPipeline pipeline => Node(
                $"Pipeline{annotation}",
                pipeline.Items.Select((item, index) =>
                    ToDocument(item, $"{path}.items[{index}]", annotations))),
            LogicalCall call => Node(
                (call.ContextDepth == 0
                    ? $"Call: {call.Function.Name}"
                    : $"Call: {call.Function.Name} (context depth {call.ContextDepth})") + annotation,
                call.Arguments.Select((argument, index) =>
                    Argument(argument, $"{path}.arguments[{index}]", annotations))),
            LogicalLiteral literal => Node(
                $"Literal: {literal.Type} = {Format(literal.Value)}{annotation}"),
            _ => Node($"{value.GetType().Name}{annotation}"),
        };
    }

    private static TreeDocument Argument(
        LogicalArgument argument,
        string path,
        IReadOnlyDictionary<string, SchemaAnalysisNode>? annotations)
    {
        var suffix = argument.IsExplicit
            ? argument.IsSpread ? " (spread)" : string.Empty
            : $" (omitted: {argument.Omission?.Mode.ToString() ?? "unspecified"})";
        return Node(
            $"Argument: {argument.Parameter.Name}{suffix}",
            argument.Value is null ? [] : [ToDocument(argument.Value, $"{path}.value", annotations)]);
    }

    private static string Annotation(
        string path,
        IReadOnlyDictionary<string, SchemaAnalysisNode>? annotations)
        => annotations?.TryGetValue(path, out var node) == true
            ? $" [{LogicalSchemaDisplay.Format(node.Input)} -> {LogicalSchemaDisplay.Format(node.Output)}]"
            : string.Empty;

    private static IEnumerable<TreeDocument> Diagnostics(IReadOnlyList<SchemaAnalysisDiagnostic> diagnostics)
        => diagnostics.Select(diagnostic => Node(
            $"Diagnostic: {diagnostic.Code} at {diagnostic.Path}: {diagnostic.Message}"));

    private static string Format(object? value) => value switch
    {
        null => "null",
        string text => $"\"{text}\"",
        bool boolean => boolean.ToString().ToLowerInvariant(),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static TreeDocument Node(string label, IEnumerable<TreeDocument>? children = null)
        => new(label, new Dictionary<string, object?> { ["Kind"] = label }, children?.ToArray() ?? []);
}
