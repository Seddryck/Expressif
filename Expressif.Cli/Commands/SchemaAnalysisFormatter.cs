using Expressif.Planning;

namespace Expressif.Cli.Commands;

internal static class SchemaAnalysisFormatter
{
    public static string Format(AnalyzedLogicalPlan analyzed)
    {
        var annotations = analyzed.Analysis.Nodes.ToDictionary(node => node.Path, StringComparer.Ordinal);
        var root = new TreeDocument(
            $"Schema: {LogicalSchemaDisplay.Format(analyzed.Analysis.Input)} -> "
                + $"{LogicalSchemaDisplay.Format(analyzed.Analysis.Output)} "
                + $"({analyzed.Analysis.Completeness.ToString().ToLowerInvariant()})",
            new Dictionary<string, object?> { ["Kind"] = "schema" },
            [
                .. analyzed.Plan.Pipeline.Items.Select((item, index) =>
                    ToDocument(item, $"plan.items[{index}]", annotations, $"Step {index + 1}")),
                .. analyzed.Analysis.Diagnostics.Select(Diagnostic),
            ]);
        return TreeDocumentFormatter.Format(root, "tree");
    }

    private static TreeDocument ToDocument(
        LogicalValue value,
        string path,
        IReadOnlyDictionary<string, SchemaAnalysisNode> annotations,
        string? prefix = null)
    {
        annotations.TryGetValue(path, out var annotation);
        var label = value switch
        {
            LogicalPipeline => "Pipeline",
            LogicalCall call => call.Function.CanonicalName,
            LogicalLiteral literal => $"Literal ({literal.Type})",
            LogicalNamedExpressionInvocation invocation => $"Invoke: {invocation.Name}",
            _ => value.GetType().Name,
        };
        if (prefix is not null)
            label = $"{prefix}: {label}";
        if (annotation is not null)
        {
            label += $" [{LogicalSchemaDisplay.Format(annotation.Input)} -> "
                + $"{LogicalSchemaDisplay.Format(annotation.Output)}]";
        }

        var children = value switch
        {
            LogicalPipeline pipeline => pipeline.Items.Select((item, index) =>
                ToDocument(item, $"{path}.items[{index}]", annotations)).ToArray(),
            LogicalCall call => call.Arguments.Select((argument, index) =>
                Argument(argument, $"{path}.arguments[{index}]", annotations)).ToArray(),
            LogicalNamedExpressionInvocation invocation => invocation.Arguments.Select((argument, index) =>
                ToDocument(argument, $"{path}.arguments[{index}]", annotations)).ToArray(),
            _ => [],
        };
        return new TreeDocument(label, new Dictionary<string, object?> { ["Kind"] = label }, children);
    }

    private static TreeDocument Argument(
        LogicalArgument argument,
        string path,
        IReadOnlyDictionary<string, SchemaAnalysisNode> annotations)
        => new(
            $"Argument: {argument.Parameter.Name}",
            new Dictionary<string, object?> { ["Kind"] = $"Argument: {argument.Parameter.Name}" },
            argument.Value is null ? [] : [ToDocument(argument.Value, $"{path}.value", annotations)]);

    private static TreeDocument Diagnostic(SchemaAnalysisDiagnostic diagnostic)
        => new(
            $"Diagnostic: {diagnostic.Code} at {diagnostic.Path}: {diagnostic.Message}",
            new Dictionary<string, object?> { ["Kind"] = "diagnostic" },
            []);
}

internal static class LogicalSchemaDisplay
{
    public static string Format(LogicalSchema schema)
    {
        var value = schema switch
        {
            NoInputLogicalSchema => "none",
            AnyLogicalSchema => "any",
            ScalarLogicalSchema scalar => scalar.Type,
            RecordLogicalSchema record => FormatRecord(record),
            ArrayLogicalSchema array => $"array<{Format(array.Items)}>",
            TupleLogicalSchema tuple => $"tuple<{string.Join(", ", tuple.Items.Select(Format))}>",
            PairLogicalSchema pair => $"pair<{Format(pair.Key)}, {Format(pair.Value)}>",
            DictionaryLogicalSchema dictionary =>
                $"dictionary<{Format(dictionary.Keys)}, {Format(dictionary.Values)}>",
            GroupingLogicalSchema grouping => $"grouping<{Format(grouping.Keys)}, {Format(grouping.Items)}>",
            SortTableLogicalSchema table => $"sort-table<{Format(table.Items)}>",
            UnionLogicalSchema union => string.Join(" | ", union.Alternatives.Select(Format)),
            ConflictingLogicalSchema conflict => $"conflict<{Format(conflict.Left)}, {Format(conflict.Right)}>",
            _ => schema.GetType().Name,
        };
        return IsNullable(schema) ? $"{value}?" : value;
    }

    private static string FormatRecord(RecordLogicalSchema record)
    {
        var fields = record.Fields.Select(field =>
                $"{field.Key}{(field.Value.Optional ? "?" : string.Empty)}: {Format(field.Value.Schema)}")
            .ToList();
        if (record.AllowsAdditionalFields)
            fields.Add("...");
        return $"record{{{string.Join(", ", fields)}}}";
    }

    private static bool IsNullable(LogicalSchema schema) => schema switch
    {
        AnyLogicalSchema value => value.IsNullable,
        ScalarLogicalSchema value => value.IsNullable,
        RecordLogicalSchema value => value.IsNullable,
        ArrayLogicalSchema value => value.IsNullable,
        TupleLogicalSchema value => value.IsNullable,
        PairLogicalSchema value => value.IsNullable,
        DictionaryLogicalSchema value => value.IsNullable,
        GroupingLogicalSchema value => value.IsNullable,
        SortTableLogicalSchema value => value.IsNullable,
        UnionLogicalSchema value => value.IsNullable,
        ConflictingLogicalSchema value => value.IsNullable,
        _ => false,
    };
}
